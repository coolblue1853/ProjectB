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
| 4 | `Combat/ContactDamage.cs:107` | 접촉 적마다 `GetComponent<Collider2D>()` | 적 수에 비례 |
| 5 | `Weapon/SummonRunner.cs:65` | 타격마다 `GetComponent<DamageObject>()` | 소환수 있을 때만 |
| 6 | `Weapon/OrbitWisps.cs:72` | 타격마다 `GetComponent<DamageObject>()` | 낮음 |
| 7 | `Weapon/HomingOrb.cs:57` | 대상 `GetComponent<Collider2D>()` | 낮음(대상 갱신 시) |
| 8 | `Enemy/EnemyHealth.cs:598` | 몹마다 `Camera.main` 조회 | 적 수 비례, 캐시 권장 |
| 9 | `Enemy/StatusTint.cs:43,44` | `GetComponentInChildren` 2회 | `CheckInterval` 로 제한돼 낮음 |
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
| 18 | TrapNormal | 삭제 후보 | 코드·CSV·씬 어디에도 없음 |
| 19 | Crystal refund on 9 | 삭제 후보 | 사용처 없음 |
| 20 | HAMMER! | 삭제 후보 | 사용처 없음 |
| 21 | U / Pad RT: buy back top | 삭제 후보 | `Buyback.cs` 는 `Nothing to buy back` 만 사용, 안내 문구 호출 없음 |
| 22 | U: buy back last sold item | 삭제 후보 | 위와 동일 |

요약: 보존 17(오탐 1 포함) · 삭제 후보 5 · **번역 누락 0**(반대 방향 누락은 §1-1 LP-1 로 처리 완료). 삭제 후보 5종은 위험이 낮지만 수혜도 작아(문자열 5개) 우선순위 P4, 필요 시 `LocUi.cs` 5행 제거 + `sync_i2_terms` 재실행 + `LocUiKeysTests` 확인.
