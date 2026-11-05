# 11-05 8차 큐 코드 커밋 정적 리뷰 (AUTO-99, 코드 수정 없음)

- 범위: 329c26ea(AUTO-76)·4ebf7459(77)·03f48480(78)·dca8628a(79)·b277dee0(81)·8bf1db16(83)·54ec8600(84)·a1396b54(86)·2dd1e9c7(88)·05e3d237(90). 형식은 `ReviewCode_1105b.md`.
- 검증: `git show` 와 현재 파일 읽기만. 플레이·EditMode 재실행 안 함. **전부 미검증**(재현 가능성은 코드 읽기 추정).

### [필수]

(없음. 아래 [권장] 중 1·2번이 10차 후보 우선)

### [권장]

1. `Assets/Script/Floor1Extra/NightRaid.cs:325` — `_deadline` 이 `Cooldown` 기본값(시각 0)이라 `Arm` 전에 `Tick()` 이 불리면 `Ready` 가 즉시 true 가 되어 "시간이 지나 습격을 놓쳤다" 로 Fail 한다. 기존 float 도 기본 0 이라 `Time.time >= 0` 으로 동일 동작이라 회귀는 아님. `Tick` 은 286행 Arm 이후 상태에서만 불린다고 읽히나 상태 가드는 미확인. 재현: 낮음. 대안: Tick 진입을 raid-active 상태로 가드하거나 `Cooldown` 에 `Armed` 플래그 추가. **해소(AUTO-106 NightRaidRules.TimedOut 활성 가드, 실기 미검증)**
2. `Assets/Script/Enemy/TempMobDash.cs:58,112` — `_stateUntil` 기본 준비 상태. 풀링 재사용 시 이전 개체의 `_readyAt`·`_stateUntil` 이 남는다(초기화는 `_readyAt.Arm` 0.3초뿐, `_stateUntil` 은 리셋 안 함). 상태가 Crouch/Dash 가 아닐 때는 안 쓰여서 실영향은 낮다. 재현: 낮음. 대안: 활성화 시 `_stateUntil = default`. **해소(AUTO-108 OnEnable 에서 _stateUntil 리셋, 플레이 미검증)**
3. `Assets/Script/Combat/CombatRules.cs:72` — `WeaponCancelFraction` 이 `weaponFraction / MoveCancelFraction` 으로 나눈다. `MoveCancelFraction` 은 const 0.4 라 0 나눔은 없고, `Weapon._moveCancelFraction` 은 Range(0.2~0.8) 라 클램프 0.05~0.95 와 함께 이중 가드다. 인스펙터 Range 는 스크립트/프리팹 YAML 직접 편집 시 우회 가능(재현: 낮음). 대안: 없음(Clamp 가 최종 가드로 충분).
4. (해소 AUTO-110: Awake 에서 레이어 없으면 경고 1회) `Assets/Script/Weapon/Gimmick/ThrownSpearGimmick.cs:ClearDistance` — `Physics2D.Raycast` 가 `Ground`/`Wall` 레이어명에 의존한다. 레이어 이름이 없으면 `GetMask` 가 0 을 반환해 항상 사거리 전체로 날아가 벽을 뚫는다(재현: 레이어 이름 변경 시에만). 또 플레이어가 벽에 붙어 있으면 `hit.distance` 0 → `FlightTime` 0 → 다음 Update 즉시 착지(의도 동작, `FlightTime` 은 0 거리 허용). 대안: `LayerMask.NameToLayer` 로 -1 검사 로그.
5. `Assets/Script/Combat/Rules/Cooldown.cs` + `Time.time` 사용처(ContactDamage·NightRaid·TempMobDash) — 모두 `Time.time`(timeScale 영향)으로 통일되어 일관됨. 히트스톱·일시정지(timeScale 0) 중에는 쿨다운이 같이 멈춘다. `unscaledTime` 을 쓰는 `PadFieldInput`(`Time.unscaledTime`)과 혼용되나 서로 다른 값을 비교하지 않아 문제 없음. 재현: 해당 없음. 대안: 없음, `Cooldown` 주석에 "now 는 한 객체 안에서 같은 시계로 주입" 한 줄 권장.

### [참고]

6. `Assets/Script/UI/KeyBind/PadFieldInput.cs:33-40` — `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` 구동기는 `DontDestroyOnLoad` 로 한 번 생성. 에디터에서 도메인 리로드 끄기 옵션(Enter Play Mode Options)이면 정적 `_frame`·`_select` 가 남고 구동기가 중복 생성될 수 있다. `Refresh` 는 `Time.frameCount` 가드라 중복 호출은 무해. 재현: 옵션 켠 경우만. 대안: 생성 전 `FindFirstObjectByType` 확인.
7. `PadFieldInput.cs:74-78` — LT 눌림 프레임에 `_selectInUi=false` 로 초기화하나 `_l3InUi` 는 같은 취소(`_l3.Cancel()`)에서 초기화하지 않는다. 비대칭. 재현: 낮음(L3 소비자 한정). 대안: `_l3InUi=false` 한 줄 추가. **해소(AUTO-109 _l3InUi=false)**
8. `Assets/Script/Player/AttackManager.cs` (4ebf7459) — `c == Gamepad.current?.leftTrigger` 참조 비교. `Unity` 오브젝트가 아닌 `InputControl` 이라 `?.` 안전, 패드 해제 시 null 비교 false. 문제 없음.
9. `Assets/Script/NpcQuest/NpcQuestWindow.cs` (03f48480) — 문구 1줄 변경. 해당 문구가 Loc 키(영어 문장)라면 I2 번역 키 불일치 가능. 미확인. 대안: `I2Languages.asset` 에서 이전 문구 grep.
10. `Assets/Script/Combat/Rules/CountdownTimer.cs` (b277dee0) — `Start` 음수→0 가드. 테스트 12줄 추가. 이상 없음.
11. `Assets/Script/Weapon/Gimmick/TomePageGimmick.cs` (05e3d237) — `PageNames` 가 영어 키로 바뀌어 `Loc.T(PageNames[_tome.Page])` 로 조회. `_tome.Page` 가 0~2 밖이면 IndexOutOfRange 이나 `TomePage` 규칙이 순환 범위를 보장하는 걸로 읽힘(미확인). 신규 Loc 키 8종(`Fire`·`Frost`·`Lightning` 등)은 `LocUi.cs` 에 한국어 쌍으로 추가됨. 동적 조합 키는 `Loc.T(PageNames[...])` 뿐이고 키 3종이 모두 정의돼 있어 누락 없음. 대안: 없음.
12. `Assets/Resources/ItemSheet.csv`·`MonsterExtraDrops.csv` (a1396b54) — 신규 313~315 행. 데이터 정합은 AUTO-98 EditMode 로 고정 예정. 이번 점검에서는 읽지 않음.

### 확인 결과(문제 없음)

- `ThrownSpear.FlightTime`: speed<=0 → 0, 음수 거리 → `Max(0)`. 0 나눔 없음. 레이캐스트 레이어는 권장 4 참고.
- `Cooldown.Ready` 경계 포함(`>=`)이라 기존 `Time.time >= x` 와 동치, `Time.time < x` 는 `!Ready` 와 동치. 8bf1db16·54ec8600 치환의 비교 방향 전부 diff 상 일치.
- 삭제한 Loc 키 동적 조회: 이번 범위 커밋(05e3d237)은 삭제가 아니라 추가이며, 기존 한국어 리터럴을 영어 키로 교체. 이전 한국어 문자열을 키로 쓰던 곳은 grep 상 없음(.cs 한정).
