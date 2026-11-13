# 1층계 주말 확인 전 준비도 점검 (11-11, 읽기 전용 감사)

- 범위: `Game/Floor1Completion.md` §4(4-1~4-5)·§5·§6(L1~L16)·§7·§9·§14 를 코드·CSV·기존 검증 기록과 대조.
- 방법: `grep -n`·CSV 파싱으로 실제 데이터 확인. 기존 `Docs/Playtest/Floor1PlayVerify.md`(PV1·PV2 등)의 에디터 실측 결과를 근거로 인용. **이번 감사에서 Unity 실행·플레이·코드 변경은 하지 않았다.** UnityMCP 는 ECONNREFUSED 로 연결 불가 상태였다.
- 상태 표기: **완료** = 데이터·코드 존재 + 실측 기록 있음 / **부분** = 일부만 있거나 문서와 다름 / **없음** = 존재하지 않음 / **미검증** = 구현은 있으나 플레이 확인 기록 없음.

---

## 0. 주말 확인 전 더 해야 할 것 (우선순위)

### A. 사용자 결정 필요 (코드 전에 답이 있어야 함)

| 순서 | 결정 항목 | 왜 필요한가 | 근거 |
|---|---|---|---|
| A1 | **신규 맵 2장(`Lv1_DeepForestCore`·`Lv1_MistSwamp`) 씬 저장 방침** | 두 맵은 `SampleScene.unity` 안에만 있고, 그 씬은 `38ba36c6f` 로 커밋됨(사용자 미커밋 변경 포함 가능). 백업은 `Assets/Backup/Floor1Maps/*.prefab` 뿐이며 포털 씬 참조는 끊긴 상태. 저장 안 하면 다음 세션에서 소실 위험 | `Docs/Handoff/queue.md` 맵 2장 항목, `Docs/Handoff/scene_backup/README.md` |
| A2 | **퀘스트 EXP 기준** (문서 43,900 vs CSV 90,700) | 문서 §4-1·§14-3(51,100)·§14-9(196분)은 옛 합계. 실제 CSV 신규 29건 합 90,700 이고 `QuestsFloor1DataTests.cs:40` 이 90,700 을 고정값으로 검사한다(R-1 재조정 흔적). 최단 시간 모델(Floor1Fastest 176/168분)은 CSV 기준 | `Assets/Resources/QuestsFloor1.csv`, `Assets/Editor/Tests/QuestsFloor1DataTests.cs:29-40` |
| A3 | **정예 인장 지급 범위** | 문서 §4-5 는 "정예 둥지 5곳 전부 인장 지급"이나, 구현은 늪·성채·탑 3곳만(`NestHuntPlacer`, `ZoneSeal_*` 3종). 일반·깊은 숲 둥지는 인장 없음. Q308 보상의 `ZoneSeal_Deep` 은 아이템 행 없음(CSV 주석 "인장 보상은 아이템 추가 후 교체") | `QuestsFloor1.csv:46`, `ItemSheet.csv:258-260`, `NestHuntPlacer.cs` |
| A4 | **Q701 목표** | 문서: "인장 3개 + 정예 2". CSV: `killElite * 5`(정예 아무거나 5). 졸업 시험 달성도(인장 40%)와 실제 퀘스트가 따로 논다 | `QuestsFloor1.csv:63` |
| A5 | **잡템 일괄 판매 UI** | 문서 §14-5 는 상점 창 버튼. 구현은 기존 J 키 / 패드 LT+L3 방식(커밋 40c0de72 에 명시). 버튼을 추가할지, 키 방식으로 문서를 고칠지 | `InventoryConvenience.cs:180`, `ControlsGuide.cs:126` |
| A6 | **Q704 처리** (소프트 게이트 확정) | §14-1: "Q704 삭제 또는 대화 1회로 축소". CSV 에는 Q704(`talk elder`, 선행 Q701)가 그대로 있음 | `QuestsFloor1.csv:66` 부근 |
| A7 | **문지기 a=1 보너스 강화석 +2 도입 여부** | §14-1 수치. 코드 미구현(아래 B1). 넣을지와 수치 확정 필요 | `GatekeeperTrial.cs`, `Gatekeeper_BD.asset` |

### B. 코드·데이터로 가능 (그래픽 불필요, 에이전트가 처리 가능)

| 순서 | 작업 | 규모 | 근거 |
|---|---|---|---|
| B1 | 문지기 a=1 보너스(강화석 +2) + 입장 알림 문구 "문지기가 강해졌다"(현재는 "시험 준비도 N%" 로그만) | 소 | `EnemyHealth.cs:71-75`, `LocUi.cs:168` |
| B2 | 퀘스트 목표 정렬(문서 표 ↔ CSV): Q107 `armor` 1→2부위, Q406 낚시 3종 각 1, Q509 희귀 군락 3회(현재 Bog Copper Ore), Q607 발굴 기록 8편(현재 Ancient Relic Shard), Q608 Ruin Mote 8 추가(`objective2`/`target2`/`count2` 열이 이미 있어 데이터만으로 가능), Q310 심부 심재 5 수집(현재 DeepSpore kill 8) | 소(데이터) | §4-1 표 대비 편차. 표 참조 |
| B3 | 도감 구역 전종 처치 보상 B08~B13 (현재 B01~B07 만) | 소~중 | `Floor1BestiaryRewards.csv`, §4-5 |
| B4 | 사냥 의뢰 신규 6건(미니보스·정예 대상, 성채·탑 위주) 추가 | 소(데이터) | `Floor1Bounties.csv` 18행(D01~D17), 미니보스 id 0건 |
| B5 | 잡템 후보 재확인: `Tough Rat Hide` 가 Junk 미분류(후보 14종 중 13종만 분류). 레시피·퀘스트 참조 여부 확인 후 분류 | 소 | `ItemSheet.csv:74` |
| B6 | 문서 정정: `Floor1Completion.md` §4-1 EXP 합·§14-3 합·§14-9 시간·§4-5 인장 5곳 문구를 실제 값으로 갱신 | 소(문서) | 위 A2·A3 결정 후 |
| B7 | 몹 6종 원거리·돌진 행동(문서 §7 역할: 숲 사냥꾼 원거리, 나무꾼 돌진 등). 현재 스폰·드롭만 되고 행동은 호스트 BT 재사용 | 중 | PV1 "못 한 것" 목록 |
| B8 | 신규 무기 3종(석궁·투창·마도서) 행 313~315 는 있음. 프리팹·기믹 부착 여부 확인, 기믹 구현은 후순위 | 확인(소)·기믹 대 | `ItemSheet.csv:271-273`, §6 L8 |
| B9 | 플레이 검증 PV4(주말 전 Unity 가능 시): 문지기 실처치(a=0 / a=1 TTK), 포털 FallRescue 전환, 수호자 배너 실제 표시, 도감 구역 완료율 탭, J 키 일괄 판매, 세트 HUD 한 줄 | 중(에디터 필요) | 아래 §2 "미검증" 항목 |

### C. 그래픽 필요 (사용자 검토 후 교체 원칙)

| 순서 | 작업 | 현재 상태 | 근거 |
|---|---|---|---|
| C1 | **구역 인장 3종 아이콘** (`Item/ZoneSeal_Swamp·Fortress·Tower`) | 파일 0개. 콘솔 Error "이미지 파일을 로드하지 못했습니다" | PV1 (2) "주의 실패(버그)", `Assets/Resources/Item/` 에 ZoneSeal 없음 |
| C2 | 신규 몹 6종 전용 스프라이트 | 기존 프리팹 틴트 재사용 | §8 |
| C3 | 미니보스 5종 연출(금색 외곽선·확대 임시) | 임시. 보스급 인상 판단 필요 | §8 "높음" |
| C4 | 신규 맵 2장 타일·안개 | 기존 타일 색조 + 흰 안개 스프라이트 임시 | §14-3 |
| C5 | 신규 무기 3종 아이콘, 신규 제작 방어구 30점 아이콘 | 행은 있음, 아이콘 실존 미확인 | §8 |
| C6 | 구역 수호자 배너·HUD 디자인 | 텍스트만 | §14-7, §8 |

---

## 1. 항목별 점검표

### 1-1. §4 "할 것" 설계

| 항목 | 상태 | 근거(파일:줄 또는 CSV id) |
|---|---|---|
| §4-1 신규 퀘스트 29건 존재 | **부분** | `QuestsFloor1.csv` 에 Q106~Q704 29행 + §14-3 신규 4행(Q310·Q311·Q410·Q411) 모두 존재(33/33). 목표·보상이 문서와 다른 행 있음 — 아래 1-2 |
| §4-1 EXP 합 43,900 | **부분(문서 stale)** | CSV 신규 29건 합 90,700. 테스트 `QuestsFloor1DataTests.cs:40` 고정. 문서 값과 불일치 (A2) |
| §4-2 졸업 시험 소프트 게이트 | **완료(코드)** | `Q604` 선행 = Q601(`QuestsFloor1.csv`). `GatekeeperTrial.cs` 달성도 식 = §14-1 과 일치. 실처치 미검증 (`queue.md` "탑 문지기 미검증") |
| §4-2 입장 알림 "문지기가 강해졌다" | **부분** | `EnemyHealth.cs:74` "시험 준비도 N%" 로그만. 문구 없음 (B1) |
| §4-2 a=1 보너스 강화석 +2 | **없음** | `GatekeeperTrial.cs` 에 없음. `Gatekeeper_BD.asset` 의 Enhance 0건 (B1) |
| §4-3 계보 세트 제작 +30행 | **완료** | `ArmorCraftRecipes.csv` 46행(Wisp·Fae·Gloom·Outlaw·Raider·Stone 각 5부위 + 기존 3세트). PV1 (5) 12페이지 46칸 UI 통과 |
| §4-4 정예 둥지 확정 드롭(★3) | **부분** | `NestHuntPlacer.cs` 인장 3종. PV1 (2) 늪·성채·탑 소환·드롭 통과. 기존 둥지 인장은 없음 (A3) |
| §4-4 미니보스 M1~M5 HP·드롭 | **완료** | `MiniBosses.csv` 5행(AlphaWolf·OldBear·OldCroc·BanditLieutenant·ElderGolem). PV1 (2) HP 460/1150/940/880/1050, 확정 드롭 통과 |
| §4-4 보스 재처치 감소 적용 | **완료** | PV1 (2) 늑대 재처치 배율 0.5/0.25 실측 |
| §4-5 도감 구역 전종 처치 B08~B13 | **없음** | `Floor1BestiaryRewards.csv` 는 B01~B07 만 (B3) |
| §4-5 도감 구역 완료율 탭 | **미검증** | `ZoneProgress.cs:139` `CompletionPercent`, `BestiaryWindow.cs` 사용. 커밋 6be8292a "미검증" |
| §4-5 사냥 의뢰 신규 6건 | **없음** | `Floor1Bounties.csv` 18행, 미니보스·신규 몹 대상 0건 (B4) |
| §4-5 채집 연결(Q108·Q509) | **부분** | Q108 Forest Mushroom 5 (문서 "약초" 표현과 근사). Q509 는 Bog Copper Ore 3 (문서 "희귀 군락 3회" 와 다름) (B2) |
| §4-5 추적 정예 5곳 인장 | **부분** | 인장 3종만 지급 (A3) |
| §4-5 상점·강화 목표 | **완료** | `enhance` 행 Q409(+3)·Q507(+5)·Q609(방어구 +5) |

### 1-2. §4-1 신규 퀘스트 편차 (CSV 기준)

| ID | 문서 목표 | CSV 목표 | 상태 |
|---|---|---|---|
| Q107 | 기본 방어구 2부위 착용 | `equip armor` 1 | 편차 (B2) |
| Q206 | WolfCap 제작 | `craft WolfLegArmor` 1 | 편차(경미) |
| Q308 | 깊은 숲 둥지 정예 → `ZoneSeal_Deep` | `killElite *` 1, 보상 `Enhance Stone` 2 | 인장 보상 없음 (A3) |
| Q310 | 심부 심재 5 수집 | `kill DeepSpore_BD` 8 | 편차 (B2) |
| Q406 | 낚시 3종 각 1 | `collect Amber Trout` 2 | 편차 (B2) |
| Q509 | 희귀 군락 3회 | `collect Bog Copper Ore` 3 | 편차 (B2) |
| Q607 | 발굴 기록 8편 | `collect Ancient Relic Shard` 8 | 편차 (B2) |
| Q608 | Monolith Shard 6 · Ruin Mote 8 | Monolith Shard 6 만 | Ruin Mote 누락 (B2, `objective2` 열로 추가 가능) |
| Q701 | 인장 3 + 정예 2 | `killElite *` 5 | 편차 (A4) |
| Q704 | 선언(Q701·702·703 선행) | `talk elder`, 선행 Q701 | §14-1 처리 대기 (A6) |
| Q410·Q411 | 낚시 희귀 어종 1 · Slime Gel 8 | Spirit Koi 1 · Slime Gel 8 | 일치 |

처리 메모: 큐 `Docs/Handoff/queue.md` L2·L10 항목에 "임시: 목표 1개 제한으로 표와 다른 Q108·406·509·607·608" 이라고 이미 적혀 있다. 즉 편차는 알려진 임시 조치다.

### 1-3. §5 무기 타입·세트

| 항목 | 상태 | 근거 |
|---|---|---|
| §5-1 신규 5종 무기 세트 키 편입(TwinFang→Wolf 등) | **완료** | `ItemSheet.csv:241-245` 의 키 열(tfName 열)에 WolfArmor·GroveArmor·OutlawArmor·FaeArmor 입력. 문서의 "setName 공란" 문제는 이 열 기준으로 해소. 기존 FangDagger(`:106`)와 같은 규약 |
| §5-1 석궁·투창·마도서 행 | **부분** | 행 존재(`ItemSheet.csv:271-273` SentryCrossbow·HuntersJavelin·FairysGrimoire). 문서 "행 없음"은 낡은 기술. 프리팹·기믹 부착은 미확인 (B8) |
| §5-1 타입 14종 체험 | **부분** | 문서 표 기준 기믹 ○ 5종. 신규 몹 드롭 연결 PV1 (3) 확인 |
| §5-2 세트 효과 Wisp·Fae·Gloom·Outlaw·Raider·Stone | **완료** | `SetItem.csv` 각 `_2/_4/_5` 행 존재 (`:17-40`) |

---

## 2. §6 로직(L1~L16)·§7·§9·§14 점검

### 2-1. §6 로직 항목

| 항목 | 상태 | 근거 |
|---|---|---|
| L1 목표 타입 killElite·equip·enhance·bestiary | **완료** | `NpcQuestCatalog.cs:221-223`(equip·enhance·bestiary), killElite 는 `NpcQuestObjective`. PV1 (4) 4종 모두 실측 통과 |
| L2 졸업 게이트 + 인장 키 아이템 | **부분** | 인장 `ItemSheet.csv:258-260`(acqPath quest, price 0). 판매 금지 `ItemTradeRules.cs:6`. **아이콘 없음**(C1). 게이트 자체는 소프트(§14-1) |
| L3 미니보스 5종 | **완료** | `MiniBosses.csv` 5행, PV1 (2) |
| L4 정예 둥지 3곳 | **완료(3곳)** | PV1 (2) 늪·성채·탑 통과. 5곳 요구와 차이는 A3 |
| L5 세트 제작 레시피 +30행 | **완료** | `ArmorCraftRecipes.csv` 46행, PV1 (5) |
| L6 구역 배너·세트 HUD·도감 완료율 | **부분(미검증)** | 배너 `ZoneBanner.cs:94,140` + 수호자 한 줄 `ZoneProgress.cs:84-94`. 완료율 `ZoneProgress.cs:139`. 세트 HUD 는 `NpcQuestTrack` 쪽, 확인 미흡. 커밋 6be8292a "미검증" |
| L7 신규 몹 6종 | **부분** | `MonsterSheet.csv` 6행, PV1 (3) 스폰 비율 통과. **원거리·돌진 행동 미구현**(B7) |
| L8 석궁·투창·마도서 기믹 | **없음(후순위)** | 문서상 후순위. 행만 존재 (B8) |
| L9 최단 시간 재계산 | **완료(모델)** | `Docs/Playtest/Floor1Fastest.md`. k=2.0 168분(≥130 통과), k=1.5 176분(160~200 통과), 그라인드 ≤3%. 실플레이 미검증 |
| L10 성채 분기 필수화 | **취소됨** | §14-2 취소. `Q601` 선행은 `Q401`(성채 무관) 로 복귀 확인 |
| L11 문지기 달성도 강화 | **완료(코드)** | `GatekeeperTrial.cs`(HP ×, 피해 ×). 테스트 `Assets/Editor/Tests/GatekeeperTrialTests.cs`(테스트 5개). 실전투 미검증 |
| L12 하위 맵 2장 | **부분** | 구역 상자 `Floor1Treasures.csv:8-9`(T07·T08), 몹 `HuntingSpawnsFloor1.csv:94-95`. 맵 오브젝트는 `SampleScene.unity` 안(2건). 씬 미커밋·백업 프리팹만 (A1) |
| L13 제작 골드 열 | **완료(데이터)** | `ArmorCraftRecipes.csv` 헤더 `gold` 열, Wolf 210·Grove 320 등. 제작 창 비용 표시는 커밋 기록상 완료, UI 미검증 |
| L14 잡템 분류·일괄 판매 | **부분** | `ItemSheet.csv` 19번째 열 `category`(Junk 13행, Tough Rat Hide 미분류 B5). `BulkSaleRules.cs`. 방식은 J 키/LT+L3 (A5). UI 미검증 |
| L15 포털 전환 점검 | **부분(미검증)** | 커밋 b4ff55cc 완료 기록. FallRescue(낙사 복귀) 미검증 |
| L16 샛길 경제 시뮬 | **완료** | `Docs/Playtest/SideRoadEconomy.md`. 결론 "조정 불필요(밤 19%)". 문서 목표(물약비 20~30%)는 충족 |

### 2-2. §7 몹·맵

| 항목 | 상태 | 근거 |
|---|---|---|
| 신규 몹 6종 스폰 | **완료** | `MonsterSheet.csv` 6행, PV1 (3) |
| 신규 몹 6종 행동 | **없음** | 원거리·돌진 미구현 (B7) |
| 구역 앵커 3지점 데이터 | **완료** | `SpawnPointsFloor1.csv`, `Floor1UndergroundAnchors.csv`, `HuntingSpawnsFloor1.csv` (`RoadLink` 는 기존) |
| 서브맵 3장(심부·늪 안개·석실 맵) | **해당 없음(후순위)** | §7 명시: 씬 작업 별도 승인. 2장은 §14-3 로 진행, 석실은 앵커 좌표만 (`Floor1UndergroundAnchors.csv`) |

### 2-3. §9 검증 계획

| 항목 | 상태 | 근거 |
|---|---|---|
| EditMode 선행 그래프·보상 실존·EXP 합 | **완료(기준 변경)** | `QuestsFloor1DataTests.cs`. EXP 합 기준이 90,700 으로 바뀜 (A2) |
| 최단 시간 모델 합격 기준 | **완료(모델)** | Floor1Fastest.md. §14-9 의 196분과 불일치 (B6) |
| 실플레이 통주(Q101→Q704) | **없음** | 주말 확인 대상. 이 단계 전까지 모든 시간 수치 미검증 |

### 2-4. §14 결정 반영

| 항목 | 상태 | 근거 |
|---|---|---|
| §14-1 졸업 시험 소프트·달성도 | **완료(코드)** | `GatekeeperTrial.cs:5,14-17`, `EnemyHealth.cs:63-75`. 문지기 a=0 처치 가능 여부는 미검증(사망 ≤3 기준) |
| §14-1 a=1 보너스 강화석 +2 | **없음** | B1 |
| §14-1 입장 알림 문구 | **부분** | A7/B1 |
| §14-2 성채 선택 + 상향 6안 | **미확인** | 드롭률 35→45%, 산적 대장 강화석 12 등 데이터 대조 미실시(감사 범위 밖, 다음 점검 필요) |
| §14-3 신규 맵 2장 + 퀘스트 4건 | **부분** | 퀘스트 4건 완료(Q310 편차 B2). 맵 저장 A1 |
| §14-4 세트 제작 골드 | **완료(데이터)** | `ArmorCraftRecipes.csv` gold 열 |
| §14-5 잡템·일괄 판매 | **부분** | L14 참조 |
| §14-6 포털 전환 점검 | **부분(미검증)** | L15 참조 |
| §14-7 수호자 메시지 | **부분(미검증)** | `ZoneProgress.cs:7-9`. GUARD-BANNER 버그 수정 기록(queue). PV2 1번에서는 배너 텍스트 없음으로 기록됨. 현재 표시는 미확인 |
| §14-8 샛길 경제 시뮬 | **완료** | SideRoadEconomy.md (L16) |
| §14-9 시간 모델 196분 | **불일치** | Floor1Fastest 176/168분. 문서 수치 갱신 필요 (B6) |

---

## 3. 주말 플레이 확인 체크리스트(미검증 목록)

1. 문지기 실처치: a=0 과 a=1 의 TTK 차이 1.3~1.6배, a=0 권장 장비 사망 ≤3회 (§14-1 합격 기준).
2. 포털 전환: 암전 0.2초 → 카메라 스냅 → 페이드인, 낙사 복귀(FallRescue) 포함. 도착 직후 연속 캡처.
3. 수호자 배너: 구역 입장 시 한 줄 표시 여부(GUARD-BANNER 수정 후 재확인).
4. 도감 구역 완료율 탭, 세트 HUD 한 줄 표시.
5. 잡템 일괄 판매: J 키(패드 LT+L3) 동작, 장비·재료·키 아이템 제외 확인.
6. 인장 3종 아이콘 깨짐(C1), 인장 판매 불가.
7. 신규 맵 2장 진입·포털 연결(씬 저장 여부 A1 결정 후).
8. 통주 실측으로 Floor1Fastest 모델 보정(비사냥 시간 ±20% 가정).

---

## 4. 판단 필요 요약

- 판단 필요: A1 맵 씬 저장, A2 EXP 기준(90,700 vs 43,900), A3 인장 지급 범위(3곳 vs 5곳), A4 Q701 목표, A5 일괄 판매 방식, A6 Q704 처리, A7 문지기 보너스 도입.
- 이번 감사는 정적 점검이다. 코드 변경·커밋 없음. 문서만 신규 작성.
