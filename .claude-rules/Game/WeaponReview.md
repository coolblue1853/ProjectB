# 무기 평타 · 콤보 · 스킬 감사 (Weapon Review)

> 상태: 코드·프리팹 정적 감사(2026-10-18). **코드 수정 없음, 에디터 플레이 미검증.** 줄 번호는 커밋 644eab18 기준이다.
> 범위: `Script/Player/{AttackManager,Weapon,PlayerController,DamageObject}.cs`, `Script/Weapon/Skill.cs`, `Script/UI/SkillCooldown.cs`, `Script/Combat/Feedback/HitFeedback.cs`, `Resources/Weapon/*.prefab`(grep), `ItemSheet.csv` 무기 행.
> 표기: **[중복 B-xxx]** = `Game/TechDebt.md`에 이미 있는 항목, **[관련 C-n]** = `Game/PlayerStateDesign.md` 1.3절 충돌 시나리오. 새 항목 ID는 `W-nn`.

## 0. 요약

- 무기 고유 로직은 없다. **모든 무기가 `Weapon` + `Skill` 컴포넌트 하나씩을 프리팹 Inspector 값만 바꿔 쓴다.** 무기 타입 enum·ScriptableObject가 없다.
- ItemSheet에는 무기 수치가 없다(이름·등급·세트만). 데미지·콤보·스킬 수치는 전부 `Resources/Weapon/<이름>.prefab`에 있다.
- 플레이어 애니는 AnyPortrait(`apPortrait.Play`)로 재생한다. Animator `.controller` 78개는 **이펙트 스프라이트 전용**이고, 콜라이더를 켜고 끄는 애니 커브·애니 이벤트가 없다(grep: `BoxCollider2D`·`functionName` 0건, `GreatDarkAbsorb/BeforeAbsorb.anim` 1건 제외).
- 발견: **P0 0 / P1 9 / P2 14 / P3 12** (새 항목 기준, 중복·관련 항목은 개수에서 제외).

## 1. 무기 목록과 공격 흐름

### 1.1 무기별 데이터 (프리팹 기준)

| 무기(ItemSheet 키) | 계열 | 콤보 | 단계별 attckSpeed(s) | 이동정지 | 평타 판정 프리팹 (maxE / hit / 유지s / isSkill) | 스킬 A/S/D/F (쿨s, 홀드) |
|---|---|---|---|---|---|---|
| RustedSword | 한손검 | 3 | 0.5/0.5/0.7 | O | Steb·VashDown·VashUp (1/1/0.5/0) | Smash Down(4), Slash Attck(3) |
| RustedSwordandShield | 검+방패 | 3 | 0.7/0.7/1.0 | O | 위와 같음 | Fierce Thrust(3), Shield Charge(5, 대시), Hold the Line(1, **홀드 1s**) |
| PoisonSword | 한손검 | 2 | 0.3/0.3 | O | PoisonVeshDown·Side (1/1/0.5/0) | Vile Ambush(0.1), This is Our Turf(0.1, 지면) |
| SacreGreatSword | 대검 | 2 | 1.0/1.0 (+delay 0/0) | O | GreatDownAttack·GreatSideAttack (3/2/0.7/**1**) | Gathering Nightmare(0.1, **홀드 8s**), Approaching Nightmare(0.1) |
| ScareSide | 낫 | 2 | 0.7/0.7 | O | GreatDownAttack 1·GreatUpAttack (3/2/0.7/**1**) | Sinister Slash, Nightmare Eruption, Whirling Malice(**홀드 5s**, 이동 가능), Dark Harvest (전부 0.1) |
| CliffBlade | 낫(ScareSide 복제) | 2 | 0.7/0.7 | O | ScareSide와 동일 | 이름만 다르고 ScareSide와 동일 |
| DoubleDagger | 단검 | 3 | 0.3/0.3/0.4 | O | DaggerSteb·DaggerVashDown (1/1/0.5/0) | Vicious Assault(0.1, 10연타), Double Step(0.1, 버프), Sly Stride(0.1, 버프) |
| RatCheifDagger | 단검 | 3 | 0.3/0.3/0.4 | **X** | CheifStab·VashDown (1/1/0.5/0) | Swift Assault, Ground Sweep, Rally Now!(소환) (전부 0.1) |
| Leather Knuckles | 너클 | 2 | 0.35/0.35 | O | PunchAttack (1/1/0.5/0) | Ducking Dash(1), Sway Back(1), Uppercut(1) — 전부 `isCancleAttack` |
| ScareClow | 클로 | 2 | 0.4/0.4 | O | ClowAttackL·R (3/2/0.7/**1**) | Night Hunt(0.1, 6연타 대시·캔슬), Rising Dread(0.1), Encroaching Darkness(0.1, 애니 없음) |
| Wood Spear | 창 | 1 | 0.55 | O | SpearSteb (1/1/0.5/0) | Double Strike(2), Charge Forward!(0.1, 대시) |
| LeafAdae | 아대(투척) | 1 | 0.7 | X | TrackingLeaf 유도 발사체 (3/1/1/0) | Scattering Leaves(1, 원형), Haste(20), Falling Leaf Step(1) |
| LeafStaff | 지팡이 | 2 | 0.7/0.7(/1) | X | SkillLaunch 발사체 (3/1/1/0) | Leaf Teleport(4), Rising Roots(0.1, 6연속 지면·레이), Nature's Healing(0.1, 힐 장판) |
| ScareStaff | 지팡이 | 1 | 0.6 | X | DarkAttack 발사체 (10/1/2/0) | Manifest Darkness(선딜 0.7), Dream Eater, Creeping Nightmare(선딜 0.3) (전부 0.1) |
| CliffStaff / AbyssStaff | 지팡이(ScareStaff 복제) | 1 | 0.6 | X | ScareStaff와 동일 | 이름만 다르고 ScareStaff와 동일 |
| PickAxe | 곡괭이(레거시) | 3 | 0.7/0.7/1.0 | O | Steb·VashDown·VashUp | 스킬 2개(이름·사운드·skillNum 필드 없음) — **W-01** |
| ToxicPickAxe | 곡괭이(레거시) | 2 | **빈 배열** | O | ToxicPADamge1·2(피벗 null) | **슬롯 전부 null** — **W-01** |

- 활·창술 계열 전용 로직은 없다(창은 찌르기 1타, 원거리는 지팡이·아대 발사체뿐).
- 모든 무기의 `maxComboTime` = **1초**, `isSkillCancel`은 직렬화돼 있지 않다(= 전부 false).

### 1.2 평타 흐름 (공통)

| 단계 | 처리 | 위치 |
|---|---|---|
| 입력 | X `triggered` → `InputBuffer(0.1s).Press`. `CanMeleeNow`(=`isAttackWait && (isSkillAttackWait‖isSkillCancel)`)가 true인 프레임에 Consume | `AttackManager.cs:59-78` |
| 상태 | `MeleeAttack`: 콤보 리셋 시퀀스(maxComboTime) 재시작 → `nowConboCount` 1..max 순환 | `Weapon.cs:139-158` |
| 대기 | `CheckAttackWait`: MoveLock(옵션)·LadderLock 토큰, `isAttackWait=false`, `(attckSpeed[+delay]) / 공속배율` 뒤 해제 | `Weapon.cs:179-197` |
| 애니 | `pC.ActiveAttackAnim(name, attckSpeed/배율)` → `apPortrait.Play` + 애니 속도 = 공속배율, 같은 시간 뒤 `isAttackAnim=false` | `Weapon.cs:169`, `PlayerController.cs:838-879` |
| 판정 | 무기 자식으로 `attackPrefab[n]` Instantiate → **생성 즉시 콜라이더 활성**, `OnTriggerEnter2D`로 적당 1회(다단은 사전 초기화 반복) | `Weapon.cs:170`, `DamageObject.cs:431-536, 372-406` |
| 데미지 | `EnemyHealth.damage2Enemy` → `DamageCalculator` → `HitFeedback.OnEnemyHit`(플래시·히트스톱·숫자) | `EnemyHealth.cs:224-264` |

### 1.3 스킬 흐름 (공통)

| 단계 | 처리 | 위치 |
|---|---|---|
| 입력 | A/S/D/F `triggered`만(버퍼 없음) → `Weapon.ActiveWeaponSkill(i)` | `AttackManager.cs:80-83` |
| 조건 | `isButtonDownSkill` && 쿨 아님 && 스태미나 `>` 소모량 && (평타·스킬 대기 끝 ‖ `isCancleAttack`) | `Skill.cs:283-284` |
| 생성 | Round(부채꼴) / Normal / Ground. 선딜(`skillWaitTime`)은 **저장되지 않는 DOTween 시퀀스**로 지연 | `Skill.cs:286-298, 390-419` |
| 애니·대기 | `SkillEffect`(애니·스태미나·캐스트 VFX) → 일반: 쿨 시작 + `CheckAttackWait`(isSkillAttackWait=false) / 홀드: holdingTime 시퀀스 | `Skill.cs:345-376, 420-450, 476-496` |
| 다단 | `skillprefab.Length>1`이면 코루틴 `SpawnSkills`가 interval마다 생성(레이·지면 보정) | `Skill.cs:186-244` |
| 홀드 해제 | 키 `canceled` → 자기 슬롯(`skillNum`)이면 종료·쿨 시작·오브젝트 삭제 | `Skill.cs:303-326` |

## 2. 콤보 구조 현황

| 항목 | 현재 | 평가 |
|---|---|---|
| 단계 수 | 1~3 (무기 프리팹 `maxComboCount`) | 지팡이·창·아대는 1타 반복이라 콤보 개념이 없다 |
| 선입력(버퍼) | 평타만 0.1초 고정(`AttackManager.cs:10`). 스킬·평타→스킬 전환은 버퍼 없음 | 후딜 0.3~1.0초 대비 짧다(W-09, W-15) |
| 다음 타 허용 시점 | 후딜(`attckSpeed`) **전부** 끝난 뒤. 캔슬 윈도우 개념 없음 | 메이플식 "후딜 일부만 캔슬 불가" 구간 없음 |
| 리셋 조건 | **입력 시점부터** `maxComboTime`(1초) 무입력 → 0. 마지막 단계 뒤엔 1단계로 순환 | 유효 이음 창 = 1 − 후딜 → 무기마다 다름, 대검은 0(W-02, W-03) |
| 캔슬 | 스킬 `isCancleAttack`(평타 후딜 무시) / 평타 `isSkillCancel`(직렬화 안 됨 → 사용 불가) / Z 대시는 막지 않지만 **취소도 아님**(W-14) | |
| 피격 | 공격·스킬 중단 없음, 경직 중에도 공격 입력 가능(W-17) | PlayerStateDesign Q-A 임시 가정과 다름 |
| 피니시 | 마지막 타 특수 처리 없음(데미지·넉백 배율도 프리팹 값만) | 4절 제안 |

## 3. 문제 목록

### 3.1 P1 — 핵심 기능·수치 오류

| ID | 대상 | 위치 | 증상 | 원인 | 수정 제안 |
|---|---|---|---|---|---|
| W-01 | PickAxe, ToxicPickAxe | `Weapon.cs:125, 164, 169-170`, `Skill.cs:395`, 프리팹 | ToxicPickAxe: 장착 시 NRE(스킬 슬롯 전부 null인데 `skill[0].Disactive…`), 평타 시 `attckSpeed`/`attackAnimName` 빈 배열·피벗 null로 예외. PickAxe: `weaponSound`·`skillSound` 필드가 없어 빈 배열 → 사운드 줄에서 예외 → **`CheckAttackWait` 미호출 → 후딜 없이 무한 평타**, 스킬도 `SkillAnimCheck` 전에 예외 → 쿨·스태미나 미소모 무한 스킬 | 구버전 프리팹(필드 추가 이전)이 갱신되지 않음 | ① 두 프리팹 데이터 보정 또는 ItemSheet 획득 경로(`diopvh`)에서 제외 ② `Weapon.OnValidate`/`Awake`에서 배열 길이 ≥ maxComboCount 검사 후 에러 로그 ③ `CheckSkill`의 `skill[0]` 대신 `skillBackGround` 직접 끄기 ④ 사운드는 `TryPlay(arr, i)` 헬퍼로 |
| W-02 | SacreGreatSword | `Weapon.cs:141-144, 191-196` | **2타(GreatSideAttack)가 나오지 않는다**(기본 공속) | 리셋 시퀀스(1.0s)와 후딜 시퀀스(1.0s)가 같은 프레임에 끝나고 리셋이 먼저 생성돼 먼저 실행 → 다음 입력은 다시 1타 | 리셋 타이머를 **후딜 종료 시점부터** `comboGrace`(예: 0.35s) 동안으로 바꾼다(W-03과 같은 수정) |
| W-04 | 스킬·평타 전반 | `Skill.cs:233-235, 271-273, 382-384`, `Weapon.cs:171-172`, 프리팹 `isSkill` | 스킬 중 `isSkill=0` 다수(SummonSacre, DaggerPowerSteb, LeafSkillStraight, UpperCut, PoisonVeshStrike, PoisonGround, CrossAttack, SkillSummon, PowerSteb 등) → **스킬뎀% 미적용·평타 히트스톱**. 평타 중 `isSkill=1` 다수(GreatDown/Up/Side, ClowAttackL/R) → **평타에 스킬뎀% 적용** | 스킬 여부를 코드가 아니라 공유 프리팹 플래그에 맡김(같은 프리팹을 평타·스킬이 같이 씀) | `Skill`이 생성 시 `dmOb.isSkill = true`, `Weapon.CreatAttackPrefab`이 `false`로 덮어쓴다. 프리팹 값은 무시 |
| W-05 | 다단·다수 타격 판정 전부 | `DamageObject.cs:456-461` | 왼쪽을 보고 친 다단 공격(대검·낫·클로 평타 hit 2, maxE 3 등)에서 **넉백 방향이 타격마다 번갈아 뒤집혀** 적이 플레이어 쪽으로 끌려온다 | 직렬화 필드 `knockbackDir.x`를 부호 반전으로 **누적 변경** | 지역 변수 `Vector2 dir = knockbackDir; dir.x = Mathf.Abs(dir.x) * sign` |
| W-06 | 홀드 스킬 4종(Skybreaker, Whirling Malice, Gathering Nightmare, Hold the Line) | `Skill.cs:430-448, 492-495`, `Weapon.cs:137` | 홀드 중 X로 **평타가 나간다** → 평타 애니가 회전·방패 애니를 덮고, 끝나면 Idle/Walk로 돌아간 채 회전 판정만 계속 | 홀드 분기가 `weapon.isSkillAttackWait`를 내리지 않음 | 홀드 시작 시 `isSkillAttackWait=false`, `EndHold`에서 true. (다른 스킬 동시 발동·키 해제 혼선은 **[중복 B-113]**, 정지 플래그 경로는 **[관련 C-4]**) |
| W-07 | 근접 평타 전부 | `Weapon.cs:170`, `DamageObject.cs:431`, 프리팹 콜라이더 기본 활성·holdingTime 0.5~0.7 | **판정이 애니와 무관**: 휘두르기 시작 프레임에 이미 맞고, 휘두른 뒤 0.5~0.7초 동안 걸어 들어온 적도 맞는다. 타격감·회피 판정 모두 어긋남 | 판정 활성 구간(active frames) 개념이 없음, 이펙트 애니에 콜라이더 커브 없음 | `attackActiveStart[]/attackActiveEnd[]`(초, 공속 배율 적용)를 Weapon에 추가하고 그 구간만 콜라이더 ON. 기본값은 현행(0 / holdingTime)으로 호환 |
| W-08 | 이동정지 무기(검·대검·낫·단검(Double)·너클·클로·창) | `PlayerController.cs:252-255` | **공중에서 평타를 연타하면 떨어지지 않는다**(호버). 점프 정점에서 무한 체공 가능 | MoveLock 중 매 프레임 `rb.linearVelocity = Vector2.zero` → y 속도까지 0 | x만 0으로(`new Vector2(0, rb.linearVelocity.y)`). 공중 공격 낙하 감속이 필요하면 별도 `airAttackGravityScale` |
| W-09 | 모든 스킬 | `AttackManager.cs:80-83`, `Skill.cs:283-284` | 평타·스킬 후딜 중에 누른 스킬은 **버려진다**(입력 씹힘). 평타 → 스킬 연계가 박자 맞추기 게임이 됨 | 스킬은 `triggered` 프레임에만 판정, 버퍼 없음 | 슬롯별 `InputBuffer[4]`(0.2s). 매 프레임 `CanSkillNow(i)`이면 Consume. 평타 버퍼와 동시에 차 있으면 **나중 입력 우선** |
| W-10 | 넉백 중·경직 중 | `AttackManager.cs:61`, `PlayerHealthManager.cs:294, 329` | 피격 넉백·경직 중에도 평타·스킬이 나간다(이동은 막히는데 공격은 됨). 슈퍼아머 의미가 약해짐 | AttackManager는 `isOpenUI`(InputLock)만 본다 | 경직 동안 `InputLock` 또는 `AttackLock` 토큰. 슈퍼아머면 예외. **[관련 PlayerStateDesign Q-A]** |

### 3.2 P2 — 조작감·잠재 크래시

| ID | 대상 | 위치 | 증상 | 원인 | 수정 제안 |
|---|---|---|---|---|---|
| W-03 | 콤보 무기 전부 | `Weapon.cs:141-144` | 콤보 이음 창 = 1 − 후딜: 대검 0s, 검·낫 0.3s, 단검 0.7s. 공속 디버프(또는 낮은 공속 세트)면 콤보가 끊긴다 | 리셋 타이머가 **입력 시점** 기준 | 후딜 종료 + `comboGrace`(무기별, 기본 0.35s). 공속 배율은 후딜에만 |
| W-11 | 공격 애니 전부 | `PlayerController.cs:155-179, 850` | 첫 타격의 히트스톱이 끝나면 `SetAnimationSpeed(1f)`로 **공속 애니 배율이 사라진다** → 고공속에서 애니가 1배속으로 느려지다 `attackAnimSequence`에 잘린다. 히트스톱 동안 논리 타이머(후딜·콤보·Invoke)는 계속 흘러 애니가 뒤처진다 | 전역 속도 0→1 복귀, DOTween 시퀀스는 정지 대상 아님 | 히트스톱 해제 시 직전 속도(`_attackAnimSpeed`)로 복귀. 후딜 시퀀스도 히트스톱 시간만큼 `timeScale=0` 또는 지연 |
| W-12 | 공중 공격 | `PlayerController.cs:843-847, 205-207, 923-927` | 공중 공격 후 **공중에서 Idle 애니**가 재생되고, 착지 시 Idle 전환 조건(`isJumpAnim`)도 깨진다 | `ActiveAttackAnim`이 `isJumpAnim=false`로 지움 | 공격 종료 콜백에서 `isGround`가 아니면 Jump/Fall 애니로 복귀(FSM Phase 7 전까지 임시) |
| W-13 | 이동 가능 무기(지팡이 4종, LeafAdae, LeafStaff, RatCheifDagger) | `PlayerController.cs:514, 536`, `Weapon.cs:170`(무기 자식 생성) | 공격 중 방향키로 **뒤돌면 근접 판정·스킬 피벗도 같이 뒤집힌다**. 걷기 애니는 막혀 공격 모션으로 미끄러진다 | 공격 중 방향 고정 없음, 판정이 플레이어 자식 | `turnLockTime`(애니 전반부) 동안 localScale 변경 금지. 근접 판정은 생성 순간 방향을 저장해 부모 분리 또는 scale 고정 |
| W-14 | 모든 무기 | `PlayerController.cs:276, 589` | Z 대시는 공격 중에도 나가지만 **공격을 취소하지 않는다**: 공격 애니 그대로 미끄러지고, 대시가 끝나면 남은 MoveLock으로 다시 정지, 후딜도 그대로 | 대시가 Weapon/Skill 대기·애니를 정리하지 않음 | "대시 캔슬" 정식화: `Weapon.CancelRecovery()`(후딜·락·콤보 유지) + `StopAttackAnim()`. 캔슬 가능 구간은 판정 활성 이후로 제한 |
| W-15 | 이동정지 무기 | `PlayerController.cs:292-297`, `AttackManager.cs:10` | 후딜 중 누른 점프·평타가 0.1초보다 일찍이면 버려진다. 0.3~1.0초 후딜에서 체감상 "씹힘" | 버퍼 0.1초 하드코딩 | 버퍼 시간을 `[SerializeField]`로 빼고 공격 0.2s·점프 0.15s로 상향 |
| W-16 | 선딜 스킬(Staff 3종 Bolt·Quake, 지연 Ground/Round) | `Skill.cs:294-297, 399-401, 414-416` | 선딜 시퀀스를 저장·Kill·SetLink 하지 않음 → 선딜 중 무기 해제 시 파괴된 피벗 접근(MissingReference), 피격·대시로 취소할 방법 없음, 사운드는 선딜 전에 재생 | 익명 시퀀스 | `castSequence` 필드 + `SetLink(gameObject)`, `CancelCast()`. 사운드는 생성 콜백 안으로 **[관련 B-205]** |
| W-17 | 홀드+선딜 조합(현재 데이터엔 없음) | `Skill.cs:371 vs 442` | `skillSequence`를 선딜(SetSkillDmg)과 홀드 타이머가 같이 써서 선딜 시퀀스를 잃는다 → 키를 떼도 늦게 생성된 오브젝트가 남는다 | 필드 하나 재사용 | 시퀀스 분리(`_castSeq`, `_holdSeq`) |
| W-18 | 홀드 스킬 시간 만료 | `Skill.cs:447` | 시간 만료 경로의 `dmOb.DestroyObject()`에 null 가드가 없다(해제 경로 317만 가드) | B-113 1차 수정 범위 밖 | `EndHold(bool released)` 하나로 합치기 **[관련 B-113]** |
| W-19 | Rising Roots(LeafStaff), 지면 보정 다단 스킬 | `Skill.cs:201-204, 215, 222, 226-236`, `171-185` | 레이가 지면을 못 찾으면 `hit.point=(0,0)` 기준 y로 생성, `GroundCheck` 실패 시 `damageObject`가 **이전 오브젝트**를 가리켜 데미지 재설정, `isNullParent`면 null에서 NRE. `groundCollisionLayer`는 선언만 되고 안 쓰임 | 실패 분기 없음 | 매 반복 `damageObject = null`로 시작, 실패면 `continue`. 지면 레이는 `groundCollisionLayer` 사용 |
| W-20 | 스킬 쿨다운 데이터 | 프리팹 `SkillCoolTime` | 12종 무기 중 다수 스킬 쿨 **0.1초** → 쿨다운이 사실상 없고 스태미나만 제한. 핵앤슬 리소스 루프가 스태미나·포만감에 몰림 | 테스트 값 방치 추정 | 무기 밸런스 표(4절 WeaponData)로 이관 후 일괄 조정. 기획 확인 필요(OpenQuestions 후보) |
| W-21 | 너클 스킬 3종 | 프리팹 `isCancleAttack=1`, `Skill.cs:284`, `PlayerController.SkillDash` | 스킬 후딜 중에도 다른 캔슬 스킬이 나가 대시·백대시·어퍼가 서로를 끊는다 → SkillDash 시퀀스 Kill로 무적·중력 복구 누락 가능 | `isCancleAttack`이 평타뿐 아니라 **스킬 후딜도** 무시 | `isCancleAttack`은 `isAttackWait`만 무시하도록, 스킬→스킬 캔슬은 별도 플래그 **[관련 B-114 C2]** |
| W-22 | 판정 오브젝트 전반 | `DamageObject.cs:236, 340-347, 376-397` | 다단 초기화 시퀀스·파괴 시퀀스를 저장하지 않아 파괴 후 콜백이 `this.gameObject` 접근 → MissingReference | 시퀀스 미보관 | `SetLink(gameObject)` 일괄 **[중복 B-205]** |
| W-23 | 지연 평타(SacreGreatSword delay [0,0]) | `Weapon.cs:161-162` | `isHasDelayTime=false`인데 배열 길이로 판단해 `Invoke` 경로를 탄다(1프레임 지연). 지연 중 콤보 리셋이 먼저 오면 `nowConboCount-1 = -1` 인덱스 예외 가능 | 플래그 무시, 문자열 Invoke | `isHasDelayTime`로 분기, 생성 시점의 콤보 단계를 인자로 캡처 |
| W-24 | 원형 스킬 | `Skill.cs:252` | 탄 수가 1이면 `(end-start)/0` → NaN 각도 | 분모 검사 없음 | `allBullet<=1`이면 startAngle 하나로 생성 |

### 3.3 P3 — 정리·하드코딩

| ID | 위치 | 내용 | 제안 |
|---|---|---|---|
| W-25 | `Weapon.cs:44-46, 134` | `delayAnimName`(대검 `GreatVashDownDelay` 등)·`isSkillCancel` 미사용/미직렬화 → 선딜 애니가 재생되지 않는다 | 선딜 애니 재생 구현 또는 필드 제거 |
| W-26 | `PlayerController.cs:857-869, 885-890` | `spriteReverse`(현재 전 무기 0) 복원이 시퀀스 콜백에만 있어 Kill되면 뒤집힌 채 남음 | `StopAttackAnim`에서도 복원 |
| W-27 | `Weapon.cs:78`, `PlayerHealthManager.cs:16` | `armorAtp`를 쓰는 곳이 없어 `damgeArray[9]`가 항상 0 | 필드 용도 결정 또는 제거 |
| W-28 | `Weapon.cs:170`, `DamageObject.cs:153, 255`, `Skill.cs:119, 135` | 타격마다 Instantiate + `FindWithTag` 2회, 스킬마다 `FindWithTag` | 판정 오브젝트 풀링, 플레이어 참조 주입 |
| W-29 | `Weapon.cs:86` | `transform.parent.GetChild(0)`로 PHM 탐색(계층 의존) | `PlayerHealthManager.Instance` |
| W-30 | `Skill.cs:283` | 스태미나 `>` 비교 → 정확히 같은 양이면 사용 불가(대시도 같음) | `>=` |
| W-31 | `Skill.cs:148` | 무기 장착 시 모든 스킬 쿨다운 시작 | 무기 교체 쿨 악용 방지 의도인지 기획 확인 |
| W-32 | 프리팹 | CliffBlade = ScareSide, CliffStaff/AbyssStaff = ScareStaff (프리팹·애니·쿨 동일) | Lv2 고유 스킬 작업 시 분리(`Game/Lv2ContentPlan.md`) |
| W-33 | `PlayerController.cs:853-856, 970-976` | 공격마다 `Debug.Log("공격시작")`(로프) | 제거 또는 GameLog |
| W-34 | 하드코딩 수치 | 버퍼 0.1(`AttackManager:10`), 히트스톱 0.08 쓰로틀·0.03/0.04/0.05(`PC:157`, `HitFeedback:57`), 다단 `+0.01`·콜라이더 0.01s 토글(`DO:383, 391-393`), 소켓 z 25/35(`Weapon:102-117`), 벽 여유 0.2(`Skill:169`), 지면 FX +0.15(`Skill:357`) | `CombatTuning` ScriptableObject 하나로 모음 |
| W-35 | `SkillCooldown.cs:73` | 쿨감 ≥100%면 쿨 0/음수 | `Mathf.Max(최소쿨, …)` |
| W-36 | `AttackManager.cs:16`, `Weapon.cs:30` | `states`, `time` 미사용 public 필드 | 정리(직렬화 필드는 `FormerlySerializedAs` 규칙 준수) |

### 3.4 기존 등록 항목 (중복 — 개수 제외)

- **B-113** 홀드 스킬 해제·동시 발동·슬롯 혼선 — W-06/W-18과 같은 코드 영역. 프리팹 확인 결과 현재 모든 무기의 `skillNum`은 실제 슬롯 번호와 일치한다.
- **B-114** C1/C2 스킬 대시 중첩, C4/C7 평타·스킬 잠금 경로 — W-21과 연관.
- **B-205** DOTween 시퀀스 미정리 — W-16, W-22.
- PlayerStateDesign 2.3 입력 버퍼(평타 0.1s)는 구현됨. 스킬 버퍼는 설계에 없음 → W-09.

## 4. 메이플 + 디아블로식 핵앤슬 기준 추가 제안

| 제안 | 내용 | 선행 |
|---|---|---|
| **WeaponData(ScriptableObject)** | 무기 타입 enum(검/대검/낫/단검/너클/클로/창/아대/지팡이), 단계별 {애니, 판정 프리팹, 후딜, 판정 시작·끝, 캔슬 가능 시점, 데미지 배율, 넉백, 피니시 여부}. 프리팹은 참조만 | W-01, W-04, W-20 |
| 판정 활성 구간 | 애니 정규화 시간 기준 `active[start,end]`. 히트스톱·공속에 연동 | W-07, W-11 |
| 캔슬 윈도우 | 단계별 `cancelFrom`(정규화 0~1): 이후 평타·스킬·대시·점프로 캔슬. 메이플식 "스킬은 평타 후딜 캔슬", 디아식 "이동 입력으로 후딜 캔슬" 옵션 | W-14 |
| 선입력 통합 | 평타·스킬·점프·대시를 한 `ActionBuffer`(마지막 입력 1개, 0.2s)로. 홀드 중 무시 규칙(Q-D) 포함 | W-09, W-15 |
| 콤보 유예 | 후딜 종료 후 `comboGrace` + 마지막 타 뒤 `finisherRecovery`(긴 후딜)로 리듬 차별화 | W-02, W-03 |
| 무기별 피니시 | 마지막 타: 검 = 넓은 베기+넉백, 대검 = 지면 충격파(카메라 흔들림 tier↑), 단검 = 다단 찌르기 3hit, 너클 = 어퍼 띄우기, 창 = 관통 찌르기(maxE↑), 지팡이 = 3번째 발사체 폭발, 아대 = 부채꼴 3발 | WeaponData |
| 공중 공격 | 공중 전용 단계(하단 베기·내려찍기), 공중 공격 1회 체공 보너스 후 낙하. 무한 호버 금지 | W-08, W-12 |
| 방향 전환 규칙 | 판정 전 구간만 방향 전환 허용(메이플식 "뒤돌아 치기"), 판정 이후 고정 | W-13 |
| 히트스톱 연동 | 히트스톱 시간만큼 후딜·콤보 타이머 정지, 피니시·치명타에 별도 tier | W-11, `GameFeelPlan.md` |
| 세트 연동 훅 | 피니시 적중·N콤보 달성 이벤트(`OnComboStep(step, isFinisher)`)를 세트 효과(`SetEffectApplier`)가 구독 → "3타째 출혈", "피니시 시 쿨감" 같은 디아식 세트 옵션 | QualityRoadmap(무기 스킬 + 세트 강화) |
| 스킬 차지/홀드 정리 | 홀드는 `EndHold` 단일 경로, 홀드 중 평타·타 스킬 금지, 차지 단계별 배율 | W-06, B-113 |
| 연타 피로 방지 | 평타 자동 반복(키 누르고 있기) 옵션 — 핵앤슬 손목 부담 완화 | 선입력 통합 |
| 샌드박스 | 허수아비 + DPS/콤보 단계 로그 프로브로 무기별 검증 | CombatSpec 8절 |

## 5. 권장 수정 순서 (작은 커밋 단위)

각 커밋은 컴파일·플레이 가능 상태를 유지하고, 검증 방법(프로브/수동)을 본문에 적는다.

1. **(수정) 넉백 방향 누적 반전** — W-05. `DamageObject` 지역 변수화. 검증: 허수아비 2마리 왼쪽 다단 평타 → 둘 다 왼쪽으로 밀림.
2. **(수정) 공중 공격 호버** — W-08. x만 0. 검증: 점프 정점 연타 시 낙하.
3. **(수정) 스킬·평타 isSkill 코드 강제** — W-04. 검증: 프로브로 평타/스킬 `GameLog` `[스킬]` 태그 확인.
4. **(수정) 레거시 곡괭이 데이터·배열 검증** — W-01. `OnValidate` 경고 + 두 프리팹 보정(또는 획득 경로 제외). 검증: 프로브로 18종 무기 순차 장착 → `MeleeAttack` 3회 → 콘솔 에러 0.
5. **(수정) 콤보 리셋을 후딜 종료 기준으로** — W-02, W-03(+W-23 단계 캡처). 검증: 대검 2타 출현, 단검 3타 순환.
6. **(수정) 홀드 중 평타 금지 + EndHold 단일화** — W-06, W-18(B-113 Phase 0과 묶어도 됨).
7. **(수정) 경직 중 공격 금지** — W-10. 슈퍼아머 예외.
8. **(추가) 스킬 입력 버퍼 + 버퍼 시간 직렬화** — W-09, W-15.
9. **(수정) 히트스톱 후 공속 애니 속도 복원** — W-11.
10. **(수정) 공중 공격 후 Jump 애니 복귀 / 공격 중 방향 고정 옵션** — W-12, W-13.
11. **(리팩터) 스킬 선딜·홀드 시퀀스 보관·SetLink** — W-16, W-17, W-22(B-205).
12. **(수정) Rising Roots 지면 보정 실패 분기, 원형 탄 1발** — W-19, W-24.
13. **(추가) 판정 활성 구간 필드** — W-07(기본값 현행 호환).
14. **(추가) 대시 캔슬 정식화 + 캔슬 윈도우** — W-14, W-21.
15. **(리팩터) WeaponData SO 도입 → 피니시·공중 단계·세트 훅** — 4절. 쿨다운 일괄 조정(W-20)은 기획 확인 후.
16. P3 정리(W-25~W-36)는 해당 파일을 고칠 때 함께.
