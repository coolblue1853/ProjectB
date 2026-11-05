# Loc 누락 문자열 · 성능/GC 점검 (11-05, AUTO-29)

1회 grep/스크립트 결과이며 코드는 수정하지 않았다. 판정은 정적 추정이고 프로파일러 측정이 아니다. Unity 미사용.

## 1. Loc 키 대조

방법: `Assets/Script`(Localization 제외)의 `Loc.T/F/Region("리터럴")` 키 187종을 `LocTerms`·`LocItems`·`LocUi` 번역 항목 804종과 대조(I2 용어 정규화 근사). 호출은 `Loc.T` 286·`Loc.F` 26·`Region` 3·`Toggle` 1, 이 중 변수·보간 키 77건은 대조 불가.

### 1-1. 번역 표에 없는 리터럴 키 (12)

| 키(원문) | 위치 | 비고 |
|---|---|---|
| ` complete! Report to ` / `Report to ` / `Report back to ` | `Quest/QuestBoard.cs:130~132` | 표에는 전체 문장(`...the quest board.`)만 있어 조각 키와 불일치, 영어 표시 위험 |
| `Craft cost : ` | `UI/CraftUI/CraftingManager.cs:167` | 번역 항목 없음 |
| `Explorer Seal acquired ({0}/{1})` | `Item/BossReward.cs:100` | 신규(FT-SEAL) 문자열, 항목 없음 |
| `Locked: complete the Floor 1 bestiary or first-clear a Floor 1 boss` 외 3줄(Floor 2·3·4) | `UI/ExtraEquip.cs:63~66` | 신규 장비 슬롯 잠금 문구 4종 |
| `New equipment slot unlocked: ` | `UI/ExtraEquip.cs:235` | 항목 없음 |
| `Trial readiness` | `Enemy/EnemyHealth.cs:74` | 항목 없음 |
| `건너편` | `Map/PortalLabel.cs:65` | 키가 한국어 — 영어 키 규약(`Loc.T` 키=영어 원문) 위반, 영어 모드에서 한국어 노출 |

### 1-2. 코드 안 한국어 하드코딩(Loc 미경유, 샘플)

`Tracking/NestHuntUI.cs:61~62`(`"둥지 습격 {0}/{1}차례"`, `"남은 몬스터 N마리 ..."`), `Floor1Extra/BestiaryMilestones.cs:132`(`"도감 보상 - "`). 로그용(`GameLog`·`Debug`)은 제외.

### 1-3. 코드·CSV 어디에서도 찾지 못한 번역 항목 (21, 사용 후보 아님 확인 필요)

`LocTerms` 16 · `LocUi` 5. 지역명 4(Forest Outskirts·Outskirts - Side Road·Village at the Tower's Foot·Undercloud Cliffs), 퀘스트 보고 문구 6(Report back to ...), 스토리 대사 5문장(Lua/대화 DB 에서 쓰일 수 있어 미사용 단정 불가). 번역 표 804종 중 대부분(아이템·몹 이름)은 CSV 에서 로드되어 리터럴 대조로는 미사용 판정 불가라 "미사용 키" 목록은 만들지 않았다.

## 2. Update/FixedUpdate/LateUpdate 안 할당·탐색 (75건 중 상위 10)

`Find*`·LINQ 는 **0건**(AUTO-20 정리 반영). `new Vector/Color` 51건은 구조체라 GC 없음으로 제외. 남은 후보:

| # | 위치 | 내용 | 비용 판단 |
|---|---|---|---|
| 1 ✅ | `NpcQuest/NpcQuestTracker.cs:114,123` | `new StringBuilder()` + 문자열 결합 | 0.5초 주기였음. 이제 결과 문구가 같으면 TMP 높이 재계산·대입 생략(LP-3) |
| 2 ✅(습격 구간) | `Tracking/NestHuntUI.cs:61,62,83` | `string.Format`·결합 후 `Set(ref last...)` 비교 | 습격 단계는 차례·남은 수·언어가 같으면 문자열 생성 생략. 추적 단계의 Format/퍼센트 문구는 미적용 |
| 3 ✅ | `UI/ItemCheck.cs:169` | `have + "/" + needCount` | 이미 `_shownHave/_shownNeed` 숫자 비교 뒤에만 생성(코드 확인) |
| 4 ✅ | `Combat/ContactDamage.cs:107` | 접촉 적마다 `GetComponent<Collider2D>()` | 이미 딕셔너리 캐시(적당 1회). `TryGetComponent` 로 교체(LP-4) |
| 5 ✅ | `Weapon/SummonRunner.cs:65` | 타격마다 `GetComponent<DamageObject>()` | 소환수 있을 때만 |
| 6 ✅ | `Weapon/OrbitWisps.cs:72` | 타격마다 `GetComponent<DamageObject>()` | 낮음 |
| 7 ✅ | `Weapon/HomingOrb.cs:57` | 대상 `GetComponent<Collider2D>()` | 낮음(대상 갱신 시) |
| 8 ✅ | `Enemy/EnemyHealth.cs:598` | 몹마다 `Camera.main` 조회 | 정적 `MainCam()` 캐시(파괴 시 재조회, 서브시스템 리셋 시 null) |
| 9 ✅(유지) | `Enemy/StatusTint.cs:43,44` | `GetComponentInChildren` 2회 | 독/출혈 공격 컴포넌트가 자식으로 런타임에 붙고 떨어지므로 초기화 캐시 불가. 0.2초류 `CheckInterval` 호출만 유지(AUTO-70, 코드 변경 없음) |
| 10 | `Player/PlayerController.cs:525` | 사다리 처리 `GetComponent<BoxCollider2D>()` | 사다리 상태에서만 |

참고(낮음): `Floor1Extra/BestiaryMilestones.cs:129` `new List`+`ES3.Save` 는 보상 수령 시 1회, `Tracking/NestHuntSite.cs:102` 상태 생성은 조건부로 보임(미확인).

## 3. 수정 후보 큐 ID

- LP-1 (P1): Loc 누락 키 12종 번역 추가·`PortalLabel` 키를 영어 원문으로 교체·QuestBoard 조각 키를 항목에 맞춤 (§1-1)
- LP-2 (P2): `NestHuntUI`·`BestiaryMilestones` 하드코딩 한국어를 `Loc.T/F` 로 (§1-2)
- LP-3 (P2): `NpcQuestTracker`·`NestHuntUI`·`ItemCheck` 문자열 생성을 값 변경 때만 하도록 캐시 (§2 #1~3)
- LP-4 (P3): `EnemyHealth` `Camera.main` 캐시, `ContactDamage` 접촉 컴포넌트 캐시 (§2 #4,#8)
- LP-5 (P3): 미확인 번역 21종 사용처 확인 후 정리 (§1-3)

## 4. LP-1 적용 (11-05)
`LocUi` 에 8종 추가(Craft cost·Explorer Seal·잠금 문구 4·슬롯 해금·Trial readiness), `PortalLabel` 키 `건너편`→`Other side`(번역 `건너편`). QuestBoard 조각 키는 동적 키(`"Report to " + ReportTo`)로 표에 전체 문장이 이미 있어 **오탐 → 변경 없음**. 검증: `LocUiKeysTests` + EditMode 741 통과.

## 5. LP-2·LP-3 적용 (11-05)
`NestHuntUI`·`BestiaryMilestones` 한국어를 `Loc` 키 5종으로 교체(LocUi). §2 #1~3 위 표기. 검증: 컴파일 에러 0 + EditMode 741 통과.

## 5. LP-5 번역 미사용 22종 분류 (11-05, AUTO-37)

방법: `LocTerms`·`LocItems`·`LocUi` 번역 818종을 `Assets/Script`(Localization 제외)+`Assets/Resources/**/*.csv` 에서 문자열 검색 → 22종 미검출(§1-3 의 21종 + `LocUi` 1종 추가분). 이어서 개별 grep(`*.cs`·씬·에셋)으로 사용처 재확인. 코드·CSV 수정 없음. 삭제는 `I2Languages.asset` 재동기화(`sync_i2_terms`)가 필요해 별도 작업.

| # | 키 | 분류 | 근거 |
|---|---|---|---|
| 1~4 | Forest Outskirts / Outskirts - Side Road / Village at the Tower's Foot / Undercloud Cliffs | 보존(로드 전용) | 지역명은 `SampleScene.unity`·`I2Languages.asset` 에 문자열로 존재, 표시 때 `Loc` 경유 |
| 5~6 | Report back to the quest board. / ...the Village Elder. | 보존(동적 키) | `QuestBoard.cs:130` `Loc.T("Report back to " + ReportTo + ".")` |
| 7~8 | Report to the quest board / ...the Village Elder | 보존(동적 키) | `QuestBoard.cs:131` |
| 9~10 | ` complete! Report to the quest board.` / `...Village Elder.` | 보존(동적 키) | `QuestBoard.cs:132` |
| 11~16 | 스토리 대사 6문장(Hunters found...·The crest was carved...·Beyond the swamp...·The same crest...·An armored giant...·It wore his crest...) | 보존(퀘스트 본문) | `Quest/QuestCatalog.cs:99~125` 의 `offer`/`report` 리터럴 |
| 17 | Crafting material | 보존(오탐) | `Localization/ItemText.cs:46` 에서 사용(검색 범위가 Localization 제외라 누락) |
| 18 | TrapNormal | 유지 | 삭제 안 함: `TrackingUI.cs:179` 가 `Loc.T("Trap" + s.Quality)` 로 동적 조회(Normal/Good) |
| 19 ✅ | Crystal refund on 9 | 삭제함 | 사용처 없음 |
| 20 ✅ | HAMMER! | 삭제함 | 사용처 없음 |
| 21 ✅ | U / Pad RT: buy back top | 삭제함 | `Buyback.cs` 는 `Nothing to buy back` 만 사용, 안내 문구 호출 없음 |
| 22 ✅ | U: buy back last sold item | 삭제함 | 위와 동일 |

요약: 보존 17(오탐 1 포함) · 삭제 후보 5 · **번역 누락 0**(반대 방향 누락은 §1-1 LP-1 로 처리 완료). 삭제 후보 5종은 위험이 낮지만 수혜도 작아(문자열 5개) 우선순위 P4, 필요 시 `LocUi.cs` 5행 제거 + `sync_i2_terms` 재실행 + `LocUiKeysTests` 확인.

## 9. 7차 큐 상태 (AUTO-75, 11-05)
- ✅ LP-1~LP-4 적용, LP-5 미사용 번역 4종 삭제(AUTO-55), 타격 GetComponent 3곳 TryGetComponent(AUTO-51), UiText.SetIfChanged(AUTO-56).
- 보류: LP-6 StatusTint(AUTO-70, 캐시 불가 사유 기록), §2 #10 사다리 GetComponent(사다리 상태에서만, 영향 작음).

## 10. 후속 재점검: 신규 코드 Loc 누락 (11-05, AUTO-89b)

방법: `Assets/Script`(Localization 제외)의 `Loc.T/F/Region("리터럴")` 키를 `Localization/*.cs` 번역 항목과 재대조, 그리고 `Loc.` 없이 한글 리터럴을 쓰는 줄(로그·Tooltip 제외)을 grep. 코드·CSV 수정 없음.

### 10-1. 리터럴 키 누락
신규 누락 **0**. 남은 4건은 동적 조합 키라 정상: `QuestBoard.cs:130~132`(`Report back to `·`Report to `·` complete! Report to `)와 `TrackingUI.cs:179`(`Trap`+품질). EquipSlotExpansion(`ExtraEquip.cs` 6곳)·도감(`BestiaryMilestones.cs`)은 LP-1/LP-2 로 이미 키 등록됨.

### 10-2. 한국어 하드코딩 후보 (건별 1줄, 호출부는 `Loc.` 미경유. 한국어 모드 영향 없음, 영어 모드에서 한글 노출)
우선순위: P1 = 전투 중 항상 보이는 HUD, P2 = 창·안내, P3 = 한국어 전용 콘텐츠로 볼 수 있는 대사·튜토리얼.

| 우선 | 파일:줄 | 키(제안 영어 원문) | 비고 |
|---|---|---|---|
| P1 | `Weapon/Gimmick/AmmoGimmick.cs:15` | `Reloading {0}s` / `Ammo {0}/{1}` | 석궁 HUD(`HudText`), 현재 `$"재장전 ..."` |
| P1 | `Weapon/Gimmick/ThrownSpearGimmick.cs:15` | `Spear held` 외 상태 | 투창 HUD |
| P1 | `Weapon/Gimmick/TomePageGimmick.cs:10,14` | `Fire`·`Frost`·`Lightning`, `Page: {0}` | 마도서 HUD |
| P1 ✅ | `Player/PlayerHealthManager.cs:379` | `Survived!` | 치명 피해 버팀 텍스트(LastStand) |
| P1 ✅ | `Enemy/DropManager.cs:173` | `Plunder! x{0}` | 약탈 훈장 골드 배수 텍스트 |
| P2 | `NpcQuest/NpcQuestWindow.cs:204~211,221~223` | `{0} kill`·`Collect`·`Craft`·`Equip` 등 목표 문구 | `NpcQuestWindow` 에 `Loc.` 1회뿐 |
| P2 | `NpcQuest/NpcQuestWindow.cs:236,241,257~259,306,381~383,406~465` | 추적 안내·탭 이름·빈 목록·도움말 | 306행 문구는 AUTO-78 대상 |
| P2 | `NpcQuest/QuestBountyLink.cs:87,97` | `[Bounty] {0}`·`Daily bounty…` | 현상 의뢰 제목·설명 |
| P2 | `Floor1Extra/BountyPanel.cs:157~215`, `BountyRules.cs:164~180`, `BountyKiosk.cs:22` | 의뢰 패널·상태 문구 | 파일 전체 한글 |
| P2 | `Item/RecallCast.cs:43,147,158` | `Recall`·`Hit`·`Moved`·`No item` | 귀환 시전 라벨 |
| P2 | `UI/EventBanner.cs:89~144` | 사망 배너·팁 5줄·퀘스트 수락/완료 | `Loc` 0회 |
| P2 | `Inventory/Model/InventoryFilterRules.cs:27~30`, `Item/ShopRules.cs:80~83` | `Equip`·`Consumable`·`Material`·`All` | 필터 라벨 |
| P2 | `UI/ShopConvenience.cs:279~301`, `UI/InventoryConvenience.cs:170,171`, `UI/ChestConvenience.cs:211`, `UI/SideList/BuybackPanel.cs:117` | 편의 기능 안내 줄 | 키 안내 포함 |
| P2 | `UI/DismantleInput.cs:71,75`, `UI/SetSwapPreview.cs:49,50`, `Map/PortalLabel.cs:84` | 분해 불가 사유·세트 교체 미리보기·`[Sealed]` | 짧은 문구 |
| P3 | `UI/ControlsGuide.cs:59~121` (41행), `UI/ControlsWindow.cs:156,197` | 조작 안내 표 | `Loc` 0회(타이틀만 `Loc.IsKorean` 분기) |
| P3 | `UI/FirstHints.cs:16~67`, `UI/TownServices.cs:30~69` | 튜토리얼·NPC 라벨 | `Loc` 0회 |
| P3 | `NpcQuest/NpcQuestVoice.cs`(97), `NpcQuestCatalog.cs`(10), `Floor1Chatter.cs`, `StoryDialogues.*.cs`, `Floor1Extra/ZoneProgress.cs`(`ZoneKo`/`ZoneEn` 쌍으로 이미 이중화) | 대사·카탈로그 | 한국어 콘텐츠 전용이면 보존 |

참고: `NightRaid.cs:161,243,327`·`HuntingRules.cs:90` 는 `Fail`/예외 메시지(로그성)라 제외 가능. `Loc.IsKorean ? "한글" : "ENGLISH"` 분기 패턴은 제목 몇 곳에서 이미 쓰이므로, 번역 항목이 과한 곳은 이 패턴으로 대체해도 된다.

### 10-3. 후속 큐 (AUTO-90 대상 = P1 5건)
- LP-7 (P1): 위 P1 5건(`AmmoGimmick`·`ThrownSpearGimmick`·`TomePageGimmick` + `PlayerHealthManager`·`DropManager`)을 `LocUi.cs` 항목 추가 + `Loc.T/F` 교체. 호출부 파일이 5개라 AUTO-90 한도(3개)는 기믹 3종부터. `LocUiKeysTests` 통과 확인.
- LP-8 (P2): `NpcQuestWindow`·`EventBanner`·필터 라벨.
- LP-9 (P3): 조작 안내·튜토리얼·NPC 라벨(한국어 전용 방침 결정 후).

### 10-3. 후속 수정 (11-05 AUTO-90) ✅
기믹 HUD 3종 P1(`AmmoGimmick`·`ThrownSpearGimmick`·`TomePageGimmick`)을 `Loc.T/F` 로 교체, `LocUi.cs` 12키 추가 + `sync_i2_terms` 반영. 남은 P1: `PlayerHealthManager.cs:379`(`Survived!`)·`DropManager.cs:173`(`Plunder! x{0}`) — 호출부 파일 3개 한도로 다음 큐. P2·P3 미착수.

11-05 AUTO-91·92: `Survived!`·`Plunder! x{0}` Loc 적용(LocUi 2키, I2 831항). 남은 P1 0건. P2·P3 미착수.
