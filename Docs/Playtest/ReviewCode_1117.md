# 11-17 코드 정적 리뷰 (AUTO-130 최신판, 코드 수정 없음)

- 범위: `git log --since=2026-11-14 -- Assets/Script` 45커밋 중 .cs 변경. 봇 전체(`DebugTools/Bot/`·`PlayObs*`·`PlayRec*`)와 게임 측 위험 부위(PlayerController·EnemyHealth·EnemySpawner·Weapon·SaveManager·ShopConvenience·DotEffectBase·BT 쉐이크). `git show <해시> -- <파일>` 로 바뀐 부분 위주로만 읽음.
- 검증: 코드 읽기와 grep 만. 플레이·EditMode 재실행 안 함. **전부 미검증**(재현 난이도는 코드 읽기 추정).
- 집계: 치명 0 · 권장 6 · 참고 12. 치명 없음 → TechDebt 등록 없음.
- 형식: `파일:줄` · 재현 난이도 · 대안.

## 잘된 점

- 봇 코드는 위치·속도를 직접 쓰지 않음. `Assets/Script/DebugTools` 전체에 `transform.position =`·`velocity =`·`MovePosition`·`Teleport` 대입 0건. 입력 가상화만 한다(`BotInput`·`BotReplay`).
- 게임 코드의 봇 전용 분기 없음. 게임 쪽은 `PlayRecorder` 정적 훅만 호출하고, `On`(= `Enabled && 기록 중`) 이 꺼져 있으면 즉시 반환한다(`PlayRecorder.cs:157`, `:162-166`, `:176-181`).
- ES3 키 이름 변경 없음. 추가만 했다(`SaveManager.cs` 의 `NormalizeItems`·`NormalizeEquip`, `ItemFields = 4`, `EquipRows`·`EquipCols`). 구세이브 배열 패딩으로 호환된다.
- 구독·해제 짝 정상: `PlayBot.cs:146`(Killed += ) ↔ `:367`(Teardown -=), `Run` 실패 경로 `:84` Teardown 호출. `Stop` 후 `_inst` 도 null 로 돌아간다.
- 풀 null 방어: `GetOb` 가 파괴된 항목을 건너뜀(4d3a24f15), BT 노드 `EnemyRoundAttackNode.cs:72-76`·`:117-121` 가 `damage == null` 에서 반환.
- `EnemyHealth` 레지스트리 짝: `Active.Add`(OnEnable) ↔ `Active.Remove`(`:941`), `EnemyDamageObject.Live` 도 OnDisable/OnDestroy 에서 제거. OnDisable 에서 DOTween `Kill` 정리(`:945-953`).
- Loc 키: 충전 UI 문구 전부 `LocUi.cs:177-184` 에 등록 확인(`Total {0}G (owned {1}G)` 포함).
- `DotEffectBase.DestroySelf`(aed570f3e) 는 EditMode 에서 Destroy 를 피함.

## [치명]

없음. 봇 시작 준비 누락은 아래 권장 R2 가 시험 결과를 왜곡하는 수준이라 권장으로 둠(치명이면 TechDebt 등록 필요).

## [권장]

R1. `Assets/Script/DebugTools/PlayObsEnemies.cs:150,155` · `Assets/Script/DebugTools/Bot/HuntBrain.cs:756` — `Prop`(파괴 오브젝트) 판정이 `MonsterTable.Get(name) == null` 단독. 시트에 없는 실제 몹이 있으면 봇이 목표·근접 위협에서 통째로 제외한다. 프리팹 파일명 기준으로 시트 id 와 어긋나는 후보: `Wolf`, `Wolf_Fix`, `StoneRabbit`(시트는 `StoneRabbit_BD`), `RatThief_Summon`, `EnemySampleChase`, `EnemySample_Complte`. 프리팹 파일명과 실제 오브젝트 이름은 다를 수 있어 미확인. 재현: 중간 — 봇 기록에서 `Prop=true` 적 이름을 덤프해 확인. 대안: 파괴 오브젝트에 전용 마커(컴포넌트·태그)를 달고 그걸로 판정. 시트 누락은 경고 로그로 남긴다.

R2. `Tools/probes/bot_prep_level.cs:3` · `Assets/Script/Player/Mastery/SkillTreeService.cs:91,106-109` · `Assets/Script/DebugTools/Bot/BotSkillRules.cs:62,70` — `PREP_LEVEL` 은 `LevelSystem.SetState` 만 호출한다. 공용 스킬 포인트가 `CommonEarnedPoints(Level)` 이라 레벨을 낮추면 `Available = Earned - Spent` 가 음수가 되고, 이미 투자한 노드는 그대로 남는다. 결과: "Lv3 인데 Lv10 스킬 보유" 상태에서 봇이 시작하고, 스킬 투자 학습 시험은 `availCommon > 0` 조건 때문에 돌지 않는다. 재현: 높음(결정적). 대안: 준비 프로브에서 스킬 투자 초기화 API 를 호출하거나(예: `Data.FixOverspent` 류), 최소한 결과 기록에 "스킬 투자 잔존"을 명시한다.

R3. `Tools/bot_run.sh:36-41` — 맵 준비 루프가 20회 실패해도 경고 없이 다음 단계로 간다(`break` 는 `*already*` 일 때만). 봇이 엉뚱한 맵에서 시작해 기록이 오염된다. 재현: 중간(맵 이동 실패 시). 대안: 20회 후 `준비 실패` 출력 후 `exit 1`(restore trap 이 세이브를 원복한다).

R4. `Assets/Script/DebugTools/PlayRecorder.cs:17`(`Enabled = true`), `:78`, `:86-96`(`Boot` 는 `#if UNITY_EDITOR` 밖) — 기록기가 플레이 빌드에서도 기본으로 켜져 매 판 jsonl 을 쓰고 프레임 훅을 돈다. 설계상 의도(사용자 플레이 기록)이지만 배포 빌드 전에 게이트가 필요하다. 재현: 쉬움(빌드 실행 후 `persistentDataPath` 확인). 대안: `#if UNITY_EDITOR || DEVELOPMENT_BUILD` 가드, 또는 빌드 기본 `Enabled=false` + 설정 토글.

R5. `Tools/bot_run.sh`(PREP_TIME 옵션) · 시작 시간대 — `PREP_TIME` 을 안 주면 세이브에 남은 낮/밤 상태 그대로 시작한다(시간 잠금 여부도 실행 간에 이어짐). 실행마다 조건이 달라 밸런스 비교가 안 된다. 재현: 쉬움. 대안: 기본값을 `day` 또는 `night` 로 고정하고 기록 메타에 시간대를 남긴다.

R6. `Tools/probes/bot_prep_noquest.cs:6` · `Assets/Script/DebugTools/Bot/BotQuest.cs:58,68,89` — 준비 프로브는 "진행 중" 퀘스트만 포기한다. 완료 이력·NPC 대사 단계는 그대로라 Lv3 시작인데 Lv10 퀘스트가 이미 완료된 상태가 된다. 카운트는 `_doneAtStart` 로 맞지만 "퀘스트 받기 경로" 시험에서 이미 끝난 체인을 밟는다. 재현: 높음. 대안: 시험용 퀘스트 리셋 API 또는 시작 조건 표(완료 목록)를 기록에 남긴다.

## 봇 시작 조건 점검 (PREP 외 누락)

이미 알려진 장비·가방·숙련도는 `bot_prep_level.cs` 가 건드리지 않음을 재확인했다. 그 외 누락:

- 스킬 투자 잔존 → R2.
- 완료 퀘스트 이력 → R6.
- 시간대 미지정 → R5.
- HP·버프: `LevelSystem.SetState(…, 0)` 는 `ApplyStats(false)`(`LevelSystem.cs:115`)라 HP 전회복을 안 한다. 음식 버프 등 `PlayerBuff` 도 손대지 않아 세이브 상태가 그대로 남는다. 재현: 높음. 대안: 준비 단계에서 `PlayerHealthManager` 전회복 + 버프 해제. (참고)
- 퀵슬롯: `bot_prep_recall.cs:7` 이 3번 슬롯을 `Assign(2, "Recall Scroll")` 로 덮어쓴다. 봇의 퀵슬롯 전제(물약·스킬 칸)와 충돌할 수 있다. (참고)
- 골드: `bot_prep_level.cs:4` 가 `DatabaseManager.money` 를 직접 대입해 UI 갱신 이벤트가 없다. 봇 판정은 money 를 직접 읽어 영향 없음. (참고)

## [참고]

N1. `Tools/probes/bot_prep_time.cs:1-14` — `IsRunning` 가드·`TimeChange.instance` null 가드가 없고, 비공개 필드 `_lockedTimeValue` 를 리플렉션으로 쓰는데 `f` 가 null 이면 NRE. 대안: `if (PlayBot.IsRunning) return …; if (tc == null || f == null) return "…";`.

N2. `Assets/Script/Player/PlayerController.cs:1430-1437` — `OnTriggerStay2D` 가 `isLadder` 를 다시 세운다. 다중 콜라이더 로프에서 Exit 직후 같은 로프 안이면 재잡기가 된다. `ladderJumpCheck`(`:745`, 쓰기 `:1393`)는 읽는 곳이 없는 죽은 필드. 재현: 낮음. 대안: Stay 는 Exit 가 `isUpLadder` 상태로 끝난 뒤에는 무시하거나, 죽은 필드 정리를 따로 한다.

N3. `Assets/Script/Enemy/EnemyHealth.cs:374` — `Awake` 가 `AlwaysElite` 를 무조건 덮어쓴다. `ApplySpawnProfile`(`:191`)이 나중에 다시 쓰는 순서에 기댄 구조. 현재 생성 순서(`CreatePooledItem` → Instantiate 즉시 Awake → 프로필 적용)에서는 맞다. 재현: 낮음. 대안: 순서 의존을 주석으로 명시하거나 `AlwaysElite |=` 로 바꾼다.

N4. `Assets/Script/Enemy/EnemyHealth.cs:291,304`, `OnDisable` `:939-953` — `NightSpawned`·`_nightAtkMul` 을 반환 때 리셋하지 않고, 보스·정예면 `ApplyNightMultipliers` 가 일찍 반환한다. 풀 재사용 때 이전 밤 배율이 남을 수 있다. 재현: 낮음(정예 경로). 대안: `OnDisable` 에서 `NightSpawned = false`.

N5. `Assets/Script/BT/BD_/EnemyCameraShake.cs:25`, `Assets/Script/BT/BD_/EnemyRoundAttackNode.cs:42` — 첫 호출에서 `shakeSequence`·`bt.sequence` 가 null 인데 `.Kill()` 을 바로 부른다. DOTween 확장의 null 가드 여부를 repo 에서 확인하지 못했다(확인 필요). 대안: `?.Kill()`.

N6. `Assets/Script/Weapon/Weapon.cs`(Kind getter) — DB 미로드나 실제 None 무기는 캐시하지 않아 매 호출마다 `FindItem`(DB 선형 탐색). 호출처 `Skill.cs:293`(스킬 사용 때마다), `SkillRuleEvents.cs:148,177`(타격마다). 재현: 낮음. 대안: DB 로드 이후 None 결과도 한 번 캐시.

N7. `Assets/Script/Enemy/MiniBossSite.cs:154` — `body.GetComponent<DeadBody>()` 를 null 가드 없이 호출. 풀 항목에 `DeadBody` 가 없으면 NRE. 재현: 낮음. 대안: `TryGetComponent`.

N8. `Assets/Script/UI/ShopConvenience.cs:205` vs `:256`, `:217` — 확인창에 보여 준 계획과 실제 구매(`RefillQuickSlots`)가 각각 `BuildRefillPlan()` 을 다시 계산한다. 창이 열린 사이 골드·가방이 바뀌면 표시와 다른 금액이 나간다. 재현: 낮음. 대안: 확인창에 넘긴 `RefillPlan` 을 그대로 실행.

N9. `Assets/Script/DebugTools/Bot/BotErrand.cs:382,717` — 판매 탭에서 `HasJunk()`(`ScanModel`+`BulkSaleRules.Build`)이 결정 틱(10Hz)마다 돈다(쿨타임 구간 밖). 재현: 쉬움(프로파일러). 대안: 판매 탭 진입 시 1회 또는 1초 캐시.

N10. 틱당 할당: `Bot/BotQuest.cs:412`(`map` 문자열 연결, `DoTalk` 마다), `Bot/BotErrand.cs:235`(`"errand:" + _phase`), `Bot/BotSkillInvest.cs:128`(`"skill:" + k`). 재현: 쉬움. 대안: 문자열을 캐시하거나 `Note` 를 enum 으로.

N11. `Bot/BotQuest.cs:229-231` — `_mapMobs.Count == 0` 이면 1초마다 `FindObjectsByType<MapInfo>` 와 `GetComponentsInChildren<EnemySpawner>` 를 전수 재스캔. 재현: 쉬움. 대안: 실패 후 재시도 간격을 늘린다.

N12. `Assets/Script/Enemy/MonsterTable.cs:39` — `Get(prefabName)` 가 null 을 가드하지 않는다(`IsAlwaysElite` 만 가드). 현재 호출처(`ExtraDrops.MonsterId`)는 빈 문자열을 돌려주므로 안전. 대안: `Get` 에도 null 가드.

## 보고 메모

- 권장 6건 중 R1(Prop 판정)과 R2(스킬 잔존)가 봇 결과 신뢰도에 가장 큼.
- 전부 코드 읽기 추정이다. 봇 런 1회(`Tools/bot_run.sh`)와 `Prop=true` 덤프로 R1 확인 필요.
