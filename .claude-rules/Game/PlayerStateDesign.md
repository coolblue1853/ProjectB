# 플레이어 상태 · 상태 토큰 · 스탯 재설계

> 상태: 설계안(2026-10-18, 미구현). QualityRoadmap B1(FSM)·B2(토큰)·B3(입력 버퍼)·B5(스탯), TechDebt B-113·B-114를 다룬다.
> 원칙: **단계마다 컴파일·플레이가 가능해야 한다.** 씬·프리팹 직렬화 필드는 지우거나 이름을 바꾸지 않는다. 정적(static) 값은 직렬화되지 않으므로 프로퍼티(facade)로 바꿔도 안전하다.
> 줄 번호는 커밋 928178f1 기준이다.

## 1. 현재 상태 분석

### 1.1 전역 플래그 — `DatabaseManager`(static)

| 플래그 | 켜는 곳 | 끄는 곳 | 읽는 곳 |
|---|---|---|---|
| `isInvincibility` (DB:28) | PC:533 `Dash`, PC:562 `SkillDash`(isDashInvins) | PC:604 `EndInvincible`(Invoke, PC:534), PC:595 `EndInvincibility`(dashSequence, PC:589) | PHM:205 |
| `isSuperArmor` (DB:15) | PC:565 `SkillDash`, DO:173 `BuffOn` | PC:600 `EndSuperArmor`(Invoke, PC:566), DO:189 `BuffOff` | PHM:221, 233 |
| `weaponStopMove` (DB:29) | Weapon:146, Skill:411(홀드), Skill:433 | Weapon:156(시퀀스), Skill:442(시퀀스), Skill:448, PC:218(대시), PC:557(SkillDash), DO:185(BuffOff) | PC:192, 197, 202, 208, 228 |
| `checkAttackLadder` (DB:30) | Weapon:147, Skill:436 | Weapon:157, Skill:443, Skill:449 | PC:259, PC:841 |
| `isOpenUI` (DB:31) | Chest:281, CraftingManager:445/483/497, ShopManager:364, InventoryManager:1239 | CraftingManager:455, ShopManager:381, InventoryManager:589/1230 | AttackManager:60, PC:166/184/189/216/259/307, 각 UI의 열기 조건 |
| `isUsePortal` (DB:32) | Portal:38, 50 | Portal:100, 106 | Portal:34, 47 (포털 재진입 가드 — 플레이어 상태 아님, 이번 범위 밖) |

PC = `Script/Player/PlayerController.cs`, PHM = `PlayerHealthManager.cs`, DO = `DamageObject.cs`, Skill = `Script/Weapon/Skill.cs`.

### 1.2 `PlayerController` 내부 상태

| 상태 | 쓰는 곳 | 비고 |
|---|---|---|
| `states` 문자열 (PC:48, public 직렬화) | "moveLeft" 450, "moveRight" 472, "move" 499/503/513/552/590, "moveUp" 321, "moveDown" 313, "dash" 537/558, "jump" 652, "wallJump" 610, "s" 625 | 15곳에서 `!= "dash"` 등으로 비교. "move"는 사실상 "0.3초 입력 없음" 디바운스 값 |
| `isGround` (PC:28) | FixedUpdate 698/706, Jump 655 | 레이캐스트 1개(hit1)만 사용 |
| `isOnGround` (PC:667) | 착지 시 false 681, `ReturnIsGround` Invoke 0.2s 후 true 662 | 이름과 달리 "착지 재판정 허용" 래치. 점프 0.2초 안 착지는 무시됨 |
| `jumpsRemaining` (PC:25) | 리셋 682/264/858, 감소 654 | 낙하만 해도 2회 남음 → 공중 2단 점프 |
| `isJumpAnim` (PC:628) | true 381/645, false 163/685/775/852 | 애니 플래그가 중력·Idle 판정까지 겸함(PC:156) |
| `isAttackAnim` (PC:763) | true 778, false 795(시퀀스)/815 | 공격 애니 중 이동 애니 억제 |
| `isLadder` / `isUpLadder` / `isRopeAttack` (PC:759/760/876) | 844·884 / 267·220·243·893 / 782·822·902 | 사다리 범위 / 매달림 / 로프 공격 |
| `isRun` (PC:29) | 199/205/410/500/504/514 | 토글 + 스태미나 시퀀스 |
| `isAttacked` / `isAttackedUp` (PC:102-103) | PHM:225-226 true, PHM:260 false, PC:168 | 경직. 외부(PHM)에서 public 필드를 직접 씀 |
| `boxColliderTrue` / `platformTrue` (PC:98-99) | 280 / 535·567·650·891, 862 / 909 | 합쳐서 `bc.isTrigger` 결정(PC:175-182) |
| `uiDashWait` (PC:115) | CraftingManager:476/490 true, `DashWait2F` 0.1s 후 false | UI 닫은 직후 대시 방지 |
| `rb.gravityScale` | 3: 158/219/242/550/587/892, 0: 266/540/570 | 매 프레임 Update에서 3으로 덮어씀(PC:156-160) |

기타: `Weapon.isAttackWait/isSkillAttackWait`(Weapon:27-28, Skill:437/444/450), `Skill.isActiveHold`(Skill:38), `PHM.isNoDamge`(PHM:39, 피격 무적 — 지역 bool이라 충돌 없음), `AttackManager.states`(미사용).

### 1.3 충돌 버그 (구체 시나리오)

| # | 시나리오 | 원인 | 결과 |
|---|---|---|---|
| C1 | 무적 대시 스킬 도중 Z 대시 | `Dash`가 `dashSequence.Kill()`(PC:547) → SkillDash 종료 콜백(중력 복구·무적 해제) 소실. 대시의 `Invoke("EndInvincible")`가 0.2s 뒤 무적을 끔 | 스킬 무적이 중간에 꺼짐 (B-114) |
| C2 | SkillDash(무적) 직후 SkillDash(비무적) — 연속 스킬/다단 대시 | 두 번째 호출이 첫 시퀀스를 Kill → `EndInvincibility(true)` 미실행. 두 번째는 isDashInvins=false라 해제 안 함 | **무적이 영구 유지** (다음 일반 대시가 우연히 끌 때까지) |
| C3 | 슈퍼아머 홀드 버프(DO:173) 중 대시 스킬 | `EndSuperArmor` Invoke(PC:566)가 버프의 슈퍼아머를 끔. 반대로 BuffOff(DO:189)가 대시의 슈퍼아머를 끔 | 슈퍼아머 조기 해제 |
| C4 | 평타 대기 중 `isCancleAttack` 스킬 / 홀드 스킬 중 평타 | Weapon:154 시퀀스를 저장·Kill하지 않음 → 평타 대기 끝에 `weaponStopMove=false`(Weapon:156) | 정지형 스킬 도중 이동 가능 |
| C5 | 정지형 홀드 스킬 중 대시 | PC:218이 `weaponStopMove=false` 강제. 홀드 종료 시점과 무관 | 홀드가 끝나기 전에 정지가 영구 해제 |
| C6 | 아무 홀드 버프 오브젝트 소멸 | DO:183-186이 출처와 무관하게 `weaponStopMove=false` | 다른 스킬의 정지 해제 |
| C7 | C4와 같은 경로로 `checkAttackLadder` | Weapon:157 / Skill:443 | 공격 중 사다리 잡기 가능/불가가 뒤섞임 |
| C8 | 인벤토리 + 창고 동시 열림 후 하나 닫기 | `isOpenUI`가 bool 하나 | 다른 UI가 열린 채로 조작 가능 |
| C9 | 무기 교체·파괴 직후 | Weapon/Skill 시퀀스가 파괴된 오브젝트의 람다를 실행하거나, Kill되면 해제 콜백이 안 옴 | 플래그가 켜진 채 남을 수 있음 |
| C10 | 공격 애니 중 착지/대시 | `isJumpAnim`, `isAttackAnim`, `states`가 각자 Idle 재생 조건을 가짐(PC:146, 478, 683, 850) | 애니가 매 프레임 `Play` 재호출, 잘못된 애니 유지 |

### 1.4 홀드 스킬 해제 버그 (B-113)

- `Skill.OnEnable`(Skill:94-101)이 **A/S/D/F 4개 액션 전부**의 `canceled`에 `OnSkillReleased`를 건다. 홀드 중 **다른 스킬 키를 떼도** 홀드가 끝난다.
- 홀드 분기(Skill:407-425)는 `weapon.isSkillAttackWait`를 내리지 않는다 → 홀드 도중 **다른 스킬이 동시에 발동**되고, 그 키를 떼는 순간 첫 홀드가 끝난다.
- `OnSkillReleased`(Skill:306)는 Inspector 필드 `skillNum`으로 쿨다운 슬롯을 고른다. 실제 슬롯은 `ActiveSkill(num)`의 인자다 → 둘이 다르면 엉뚱한 슬롯에 쿨이 걸린다.
- 선딜이 있는 홀드 스킬은 `damageObject`가 시퀀스(Skill:376-378)로 늦게 생성된다. 선딜 중에 떼면 Skill:303에서 NRE.

## 2. 목표 설계

새 코드는 모두 `Assets/Script/Player/State/`, `Assets/Script/Combat/Stats/`에 둔다. **순수 C# 클래스**로 만들어(MonoBehaviour 아님) 프리팹 GUID·Missing Script 문제를 피하고 EditMode 테스트가 가능하게 한다. 시간은 `Func<float> Clock`(기본 `Time.time`)으로 주입한다.

### 2.1 계층형 FSM — `PlayerStateMachine.cs`

```csharp
public enum PlayerState { Grounded, Air, Ladder, Dash, Attack, Skill, Hit, Dead }
```

| 그룹 | 상태 | 의미 |
|---|---|---|
| Locomotion | Grounded, Air, Ladder, Dash | 물리·중력·이동 입력을 결정 |
| Action (오버레이) | Attack, Skill | 진입해도 **하위 Locomotion을 기억**한다(공중 공격, 로프 공격). 물리는 하위 상태가 계속 처리 |
| Override | Hit, Dead | 모든 입력을 막는다 |

- `Current`(최상위)와 `Locomotion`(하위)을 따로 가진다. `Current`가 Attack/Skill이면 `Locomotion`은 Grounded/Air/Ladder 중 하나다.
- `bool TryChange(PlayerState to, string reason)`: 허용표에 없으면 false. `event Action<PlayerState, PlayerState> Changed`. 마지막 8회 전이를 링 버퍼로 남겨 프로브에서 본다.
- 판정(가드)은 `PlayerController`가 하고, FSM은 **허용표 + 기록**만 한다(테스트하기 쉽게).

**전이표** (행 = 현재, 열 = 다음, ● 허용 / 빈칸 금지)

| from \ to | Grounded | Air | Ladder | Dash | Attack | Skill | Hit | Dead |
|---|---|---|---|---|---|---|---|---|
| Grounded | | ● 점프·낙하(코요테 후) | ● ↑↓ + 사다리 범위 + !LadderLock | ● | ● | ● | ● | ● |
| Air | ● 착지(vy≤0 + 레이) | | ● 공중에서 사다리 잡기(현행) | ● | ● | ● | ● | ● |
| Ladder | ● 바닥 도달(InGroundPlayer) | ● 점프·범위 이탈 | | ● | ● 로프 공격 | ● | ● | ● |
| Dash | ● 종료+지면 | ● 종료+공중 | | | | | | ● |
| Attack | ● 종료(하위 복귀) | ● 종료(하위 복귀) | ● 종료(하위 복귀) | ● 대시 캔슬(현행 PC:218) | ● 다음 콤보 | ● `isCancleAttack` | ● | ● |
| Skill | ● 종료 | ● 종료 | ● 종료 | ● 대시 캔슬(홀드는 종료 처리) | ● `isSkillCancel` | | ● | ● |
| Hit | ● 경직 끝 | ● 경직 끝 | | | | | ● 재피격(경직 갱신) | ● |
| Dead | ● 부활(미구현) | | | | | | | |

- Dash는 Hit을 받지 않는다(대시 무적). 무적이 아닌 대시 스킬은 Skill 상태다(Dash는 Z 대시 전용).
- Hit 진입 조건: `!Has(Invincible) && !Has(SuperArmor) && stiffTime > 0`. 슈퍼아머 피격은 상태 전이 없이 넉백 10%만(현행 유지).
- **[임시 가정]** Hit은 Attack/Skill을 끊는다: 애니만 중단하고 이미 생성된 데미지 오브젝트는 유지, 홀드 스킬은 정상 종료 처리(쿨 시작). 현행은 끊지 않는다 → 변경점이므로 PR에 명시.
- Dead: `nowHp == 0`(PHM:176)에서 진입. 진입 시 `PlayerStatus.ForceClear("Dead")`.
- `public string states`(직렬화)는 지우지 않는다. 마이그레이션 동안 FSM 전이 때 `states = Current.ToString()`으로 미러링하고, 비교 코드는 단계적으로 FSM 질의(`IsIn(PlayerState.Dash)`)로 바꾼다.
- 애니메이션은 `PlayerAnimDriver`(순수 C#, `apPortrait` 래핑) 한 곳에서 우선순위로 고른다: Dead > Hit > Attack/Skill(요청 애니) > Dash > Ladder(RopeUp/Down/Stay) > Air(Jump) > Grounded(Idle/Walk/Run). **마지막으로 재생한 이름을 캐시**해 같은 애니를 매 프레임 `Play`하지 않는다. `ActiveAttackAnim`/`StopAttackAnim`도 드라이버를 거친다.

### 2.2 상태 토큰(ref-count) — `PlayerStatus.cs`

```csharp
public enum StatusFlag { Invincible, SuperArmor, MoveLock, LadderLock, InputLock, DashLock }

public static class PlayerStatus
{
    public static Func<float> Clock = () => Time.time;
    public static event Action<StatusFlag, bool> Changed;          // 0↔1 경계에서만 발생
    public static bool Has(StatusFlag flag);
    public static IDisposable Acquire(StatusFlag flag, string owner,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
    public static IDisposable AcquireFor(StatusFlag flag, float seconds, string owner, ...); // 만료 시 자동 해제
    public static void Tick();                                     // PlayerController.Update 맨 앞에서 1회
    public static void SetLegacy(StatusFlag flag, bool on);        // facade 전용
    public static void ForceClear(string reason);                  // 사망·씬 전환·디버그
    public static string Dump();                                   // "Invincible: Dash(PlayerController.cs:533) 0.12s left ..."
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetStatics();
}
```

- 핸들은 클래스(`StatusHandle : IDisposable`)이며 `Dispose`는 **멱등**이다(두 번 불러도 한 번만 감소). 해제된 핸들을 다시 쓰지 않는다.
- 플래그별로 살아 있는 핸들 목록을 갖고, `Count > 0`이 곧 `Has`다. owner 문자열 + 호출 위치로 누가 잡고 있는지 `Dump`로 본다.
- `AcquireFor`는 DOTween·Invoke를 쓰지 않는다. `Tick`에서 만료를 검사한다 → 오브젝트 파괴와 무관하게 반드시 풀린다.
- 플래그 매핑: `isInvincibility`→Invincible, `isSuperArmor`→SuperArmor, `weaponStopMove`→MoveLock, `checkAttackLadder`→LadderLock, `isOpenUI`→InputLock, `uiDashWait`→DashLock.
- **Facade:** `DatabaseManager`의 정적 bool 필드를 같은 이름의 정적 프로퍼티로 바꾼다.
  ```csharp
  public static bool isInvincibility { get => PlayerStatus.Has(StatusFlag.Invincible); set => PlayerStatus.SetLegacy(StatusFlag.Invincible, value); }
  ```
  `SetLegacy`는 플래그마다 **레거시 핸들 1개**만 켜고 끈다. 기존 코드는 그대로 컴파일·동작하고, 토큰으로 옮긴 호출자의 핸들은 레거시 `= false`가 끄지 못한다. 모든 쓰기가 옮겨지면 setter에 `[Obsolete]`를 붙이고 최종적으로 제거한다.
- 의미 규칙: MoveLock 중에도 Dash 상태는 이동한다(대시가 MoveLock을 **끄지 않고 무시**한다). C5 해결.

### 2.3 입력 버퍼 · 코요테 타임 — `InputBuffer.cs`

```csharp
public sealed class InputBuffer   { public InputBuffer(float window); void Press(float now); bool Peek(float now); bool Consume(float now); void Clear(); }
public sealed class CoyoteTimer   { public CoyoteTimer(float window); void SetGrounded(bool g, float now); bool InCoyote(float now); void ConsumeJump(); }
```

| 항목 | 값 | 동작 |
|---|---|---|
| 점프 버퍼 | 0.1s (`[SerializeField] private float _jumpBufferTime = 0.1f`) | 점프 키 → `Press`. 매 프레임 `CanJump()`이면 `Consume` 후 점프. MoveLock·공격 후딜이 0.1초 안에 끝나거나, 점프를 다 쓴 상태에서 착지 직전에 눌러도 착지 순간에 점프한다 |
| 공격 버퍼 | 0.1s (AttackManager) | X → `Press`. `Weapon.CanMeleeNow`(=`isAttackWait && (isSkillAttackWait || isSkillCancel)`)가 true가 되는 프레임에 `Consume` 후 `MeleeAttack`. 콤보 리셋 타이머는 실제 발동 때만 갱신한다 |
| 코요테 | 0.08s (`_coyoteTime = 0.08f`) | 발판에서 걸어 내려간 뒤 0.08초 안의 점프는 **지상 점프**로 친다 |

- 착지 판정: `isOnGround` 래치(Invoke 0.2s)를 없애고 `vy <= 0.01 && 레이 hit`이면 착지로 바꾼다. 두 레이(hit1·hit2) 중 하나만 맞아도 지면.
- **[임시 가정]** 낙하 후 코요테가 지나면 지상 점프 1회를 소모한다(`jumpsRemaining = maxJumps - 1`). 현행은 낙하 후에도 2회 점프가 가능하다. 토글 `[SerializeField] private bool _fallConsumesGroundJump = true`로 되돌릴 수 있게 한다.
- 버퍼는 Hit/Dead 진입 시 `Clear()`한다(경직 풀리자마자 의도치 않은 점프 방지).

### 2.4 홀드 스킬 해제 수정 (B-113)

1. **해제 라우팅을 AttackManager로 옮긴다.** AttackManager가 이미 slot별 `skillAction[4]`를 갖고 있다. `OnEnable`에서 slot마다 `canceled` 핸들러(인덱스를 캡처한 델리게이트를 배열에 저장)를 걸고, `OnDisable`에서 같은 델리게이트로 해제한다 → `equipWeapon.ReleaseWeaponSkill(i)` → `skill[i].OnHoldRelease(i)`.
2. `Skill`은 자체 `KeyAction`·`canceled` 구독(Skill:10-11, 94-109, 120-124)을 제거한다(필드는 private이라 직렬화 영향 없음).
3. `Skill`에 `private int _activeSlot = -1`을 두고 `ActiveSkill(num)`에서 저장한다. `OnHoldRelease(slot)`는 `slot != _activeSlot`이면 무시. 쿨다운도 `_activeSlot`으로 건다(Inspector `skillNum`은 표시용으로만 남긴다).
4. 홀드 중 다른 스킬 입력은 무시한다: `Weapon`에 `public Skill ActiveHoldSkill { get; private set; }`. `ActiveWeaponSkill(num)`은 홀드 중이고 다른 슬롯이면 return.
5. 선딜 중 해제: 생성 시퀀스를 필드에 저장하고, 해제 시 Kill + `damageObject`/`dmOb` null 가드. 데미지 오브젝트가 없으면 쿨다운·대기 해제만 한다.
6. 홀드 상태 정리(쿨 시작 + 대기 해제 + 애니 정지)를 `EndHold(bool released)` 하나로 모아 키 해제·시간 만료·Hit·대시 캔슬·무기 해제(OnDisable) 모두 여기로 온다.

### 2.5 Modifier 기반 스탯 — `Combat/Stats/`

```csharp
public enum StatId { AttackSpeed, MoveSpeed, Def, CritRate, CritDmg, BasicDmg, PoisonDmg, IncDmg,
                     IncomingDmg, BleedIncDmg, DropRate, HitCount, MaxHp,
                     SkillCooldown, SkillHitCount, SkillBulletCount }   // 마지막 3개는 스킬 이름 키
public enum ModType { Add = 100, Percent = 200, Multiply = 300 }      // 정렬 순서 = 적용 순서

public sealed class StatModifier {
    public readonly StatId Stat; public readonly string SkillKey;     // null = 전역
    public readonly ModType Type; public readonly float Value;
    public object Source;  // 장비 인스턴스
    public string Group;   // 세트 효과 "Set:Rabbit_3"
    public string Id;      // 버프 "Buff:AttSpeed:12345"
    public string Owner;   // 디버그 표기
}
```

- **계산식:** `Value = (Base + ΣAdd) × (1 + ΣPercent/100) × Π(1 + Multiply/100)`. Stat은 dirty 플래그로 지연 계산하고, 값이 바뀌면 `PlayerStats.Changed(StatId)` 이벤트를 낸다.
- **퍼센트 포인트 스탯**(AttackSpeed, MoveSpeed, CritRate, CritDmg, IncDmg, IncomingDmg, BleedIncDmg, DropRate, SkillCooldown)은 Base 0 + **Add만** 쓴다. 값 자체가 %이므로 `StatMath`(`1 + x/100f`)는 그대로 둔다. Percent/Multiply는 실제 기준값이 있는 스탯(MaxHp, Def)에만 쓴다.
- 정수가 필요한 곳은 `PlayerStats.GetInt(id)` = `Mathf.RoundToInt`(0.9999 내림 방지).
- **API:** `PlayerStats.Add(mod)`, `RemoveBySource(object)`, `RemoveByGroup(string)`, `RemoveById(string)`, `Get(id)`, `GetInt(id)`, `GetSkillInt(id, skillName)`, `SetLegacy(id, int)`, `Dump()`, `ResetAll()`(테스트·SubsystemRegistration).
- 같은 `Id`로 다시 Add하면 **교체**한다(갱신형 버프용). 현행처럼 중첩하려면 Id에 인스턴스 ID를 붙인다 → **[임시 가정]** 현행 유지(중첩).

| 출처 | 현재 코드 | 새 키 | 해제 |
|---|---|---|---|
| 장비 | `EquipBoxCheck.SetEquipment/DisableEquipment`(EquipBoxCheck:150-191), 쿨감 125-147 | `Source = Equipment 인스턴스` | `RemoveBySource(equip)` — 더하기/빼기 비대칭이 구조적으로 사라진다 |
| 세트 | `SetItem.ActiveSetEffect`(SetItem:50-160) | `Group = "Set:{name}_{level}"` | `RemoveByGroup` |
| 버프 | `PlayerBuff`(Start/ActiveBuff/DestoryBuff), DO `BuffOn/BuffOff`(DO:169-196) | `Id = "Buff:{type}:{instanceId}"` | `RemoveById`. "def"/"Def" 표기 차이는 `StatId` 파싱(대소문자 무시)으로 흡수 |

- **Facade:** 정적 int 필드를 프로퍼티로 바꾼다.
  ```csharp
  public static int playerDef { get => PlayerStats.GetInt(StatId.Def); set => PlayerStats.SetLegacy(StatId.Def, value); }
  ```
  `SetLegacy`는 `value - 현재값`만큼 **레거시 Add 모디파이어**(Source = `LegacySource`)를 조정한다 → `+=`/`-=` 쓰는 기존 코드가 그대로 동작한다. 단, 레거시 쓰기가 남은 스탯에 Percent/Multiply를 넣으면 역산이 틀리므로 `PlayerStats.Add`에서 경고 로그를 낸다.
- 스킬별 Dictionary(`skillCoolDown`, `skillHitCount`, `skillBulletCount`)는 프로퍼티로 감쌀 수 없다. **읽는 쪽**(Skill:184/242, SkillCooldown:69)을 먼저 `GetSkillInt`로 바꾸고, 다음 단계에서 쓰는 쪽을 옮긴 뒤 Dictionary에 `[Obsolete]`를 붙인다. `bleedingEquipment*`는 스탯이 아니라 발동 데이터이므로 유지한다.
- `money`, `inventoryItemStack`, `setsEffectStack`은 이번 범위 밖이다.

## 3. 마이그레이션 단계

각 단계 = 커밋 1개. 단계마다 **컴파일 에러 0 → EditMode 테스트 → 플레이 프로브** 순으로 검증한다(`Framework/EditorAutomation.md`). 테스트는 `Assets/Editor/Tests/`(asmdef 없음 → Assembly-CSharp 참조 가능), 프로브는 `Tools/probes/`(C# 6 문법, `UnityEngine.Object` 명시, `return 문자열;`).

### Phase 0 — B-113 핫픽스 (Skill.cs만)

> **진행(2026-10-30):** ✅ `_activeSlot`(해제 판정·쿨다운), `Skill.IsHolding` + `Weapon.ActiveWeaponSkill` 홀드 중 다른 슬롯·같은 슬롯 발동 차단(설계 2.4-4 선반영), 선딜 시퀀스를 `_spawnDelaySequence`/`_dmgDelaySequence` 필드로 저장해 해제 시 Kill(2.4-5). 리뷰 반영: `OnDisable` 에서 홀드·선딜 시퀀스 Kill + `isActiveHold`/`_activeSlot` 초기화 + 스킬 대기 복구(프로브: 홀드 중 무기 끄고 켜면 IsHolding=False·isSkillAttackWait=True·MoveLock 없음). 2차(10-30) ✅ `EndHold()` 하나로 키 해제·시간 만료 정리(시퀀스 3개 Kill·`SpawnSkills` 코루틴 `_spawnRoutine` 중단·`_activeSlot` 쿨·`CheckAttackWait`·`StopAttackAnim`·데미지 오브젝트 즉시 파괴 — 만료도 이제 즉시 파괴, 예전 만료는 `DestroyObject` 지연 파괴였다). `OnDisable` 은 쿨·애니 없이 상태만 정리(별도 경로 유지 — 2.4-6의 `EndHold(bool)` 로 합치는 것은 2.4-1~2 때). 순차 생성으로 먼저 나온 오브젝트는 각자 수명에 맡긴다(마지막 하나만 파괴, 예전과 같음). 일반 스킬의 앞 시전 코루틴은 멈추지 않는다. `ActiveSkill` 시작에 `damageObject`/`dmOb` 초기화 + `SetSkillDmg` null 가드(바닥 못 찾은 GroundAttack이 이전 시전 오브젝트를 다시 잡거나 NRE 나던 것). `equip_weapon.cs` 는 이전 무기를 해제·파괴(세트 수 복구)하고 `ActivePrefab` 까지 호출. 검증: `hold_skill_expire.cs`(holdingTime 0.6, 키 유지 → 홀드 종료·오브젝트 0·쿨·스킬 대기 복구), `hold_skill_multi.cs`(프리팹 4개·간격 1초, 0.4초에 해제 → 6초 뒤 오브젝트 0), `hold_skill.cs` 재통과, EditMode 135/135. GroundAttack 실패 경로는 코드 검토만(미검증).
>
> **3차(10-30) ✅ 2.4-1~3·6 완료:** Skill 의 `KeyAction`·`canceled` 구독 제거. AttackManager 가 Awake 에서 슬롯별 해제 델리게이트 4개를 만들어 OnEnable 구독/OnDisable 해제 → `Weapon.ReleaseWeaponSkill(i)` → `Skill.OnHoldRelease(i)`(`slot == _activeSlot`) → `EndHold(true)`. `Weapon.CancelHold` ← `AttackManager.CancelHold` ← `PlayerController.CancelSkillHold()`: 대시 시작(`Dash()` 첫 줄)·피격 경직 진입(`PlayerHealthManager.damage2Player`, 슈퍼아머·경직 0 이면 유지). `OnDisable` 은 홀드 중이면 `EndHold(false)`(쿨·애니 없이 잠금·스킬 대기 복구 + 데미지 오브젝트 즉시 파괴 — 예전엔 남겨 뒀다, 버프는 `DamageObject.OnDestroy` 가 해제), 아니면 `KillPendingSpawns`. 검증: `hold_skill.cs`(다른 슬롯은 `ActiveWeaponSkill`/`ReleaseWeaponSkill` 직접 호출 — 콜백 안에서 두 키 상태를 큐잉하면 두 번째 키가 반영되지 않는 경우가 있었다), `hold_skill_cancel.cs`(가상 Z 대시·`damage2Player` 경직 → 홀드 종료), expire/multi/비활성화 재통과, EditMode 135/135, 콘솔 에러 0. 남은 것: AttackManager 가 홀드 중 꺼지면 해제를 놓친다(지금은 끄는 코드 없음), 한 프레임 안의 누름·뗌은 홀드가 만료까지 간다(시작 시 `IsPressed()` 확인으로 막을 수 있음).

- 수정: `Skill.cs` — `_activeSlot` 저장, `OnSkillReleased`에서 `context.action != skillAction[_activeSlot]`이면 return, 쿨다운을 `_activeSlot`으로, `damageObject`/`dmOb` null 가드.
- 검증
  - [ ] 컴파일 0 에러
  - [ ] 프로브 `hold_skill.cs`: 홀드 스킬 슬롯에서 `equipWeapon.ActiveWeaponSkill(i)` 호출 후 `Skill.isActiveHold`(리플렉션) 출력. 키 입력 주입은 금지이므로 해제 검증은 수동 플레이로 하고, 미확인이면 커밋에 "미검증"

### Phase 1 — PlayerStatus 도입 + bool facade

- 추가: `Script/Player/State/PlayerStatus.cs`, `StatusHandle`(같은 파일), `Assets/Editor/Tests/PlayerStatusTests.cs`, `Tools/probes/status_dump.cs`
- 수정: `DatabaseManager.cs` — bool 5개(`isSuperArmor` 15, `isInvincibility` 28, `weaponStopMove` 29, `checkAttackLadder` 30, `isOpenUI` 31)를 프로퍼티로. `isUsePortal`은 그대로. `PlayerController.Update` 첫 줄에 `PlayerStatus.Tick()`.
- 동작 변화 없음(레거시 핸들 1개 = 기존 bool).
- 검증
  - [ ] 테스트: 두 핸들 중 하나 Dispose → 여전히 Has / 이중 Dispose → 카운트 1회만 감소 / `SetLegacy(false)`가 다른 핸들을 못 끔 / `AcquireFor` 만료(Clock 주입) / `ForceClear` / `Changed`는 0↔1에서만
  - [ ] 플레이: 대시·스킬·UI 열기 동작 기존과 동일, `status_dump` 출력 확인

### Phase 2 — 무적·슈퍼아머 토큰화 (B-114 핵심, C1~C3)

- 수정: `PlayerController.cs` — `Dash`: `AcquireFor(Invincible, dashDuration, "Dash")`, `Invoke("EndInvincible")` 삭제. `SkillDash`: 핸들 2개(Invincible 조건부, SuperArmor)를 `AcquireFor(dashDu)`로. `EndInvincible/EndSuperArmor/EndInvincibility` 삭제(문자열 Invoke 잔존 검색: `grep -n "Invoke(\"End"`). dashSequence에는 중력·속도·states 복구만 남긴다.
- 수정: `DamageObject.cs` — `BuffOn`에서 `_superArmorHandle = Acquire(SuperArmor, name)`, `BuffOff`와 `OnDestroy`에서 `Dispose` (+null). DO:183-186(MoveLock 강제 해제)은 Phase 3에서 처리.
- 수정: `PlayerHealthManager.cs` — PHM:205/221/233 읽기를 `PlayerStatus.Has`로(선택; facade로도 동작).
- 검증
  - [ ] 프로브 `dash_overlap.cs`: `SkillDash(0.5f, …, isDashInvins:true)` 직후 리플렉션으로 `Dash()` 호출 → 0.3s 후 `status_dump`에서 Invincible 유지(SkillDash 핸들 생존) → 0.6s 후 해제
  - [ ] 프로브: `SkillDash(무적)` → 즉시 `SkillDash(비무적)` → 1s 후 Invincible 없음(C2)
  - [ ] 플레이: 대시 중 피격 무시, 대시 후 피격 정상

### Phase 3 — MoveLock·LadderLock 토큰화 (C4~C7, C9)

- 수정: `Weapon.cs` — `CheckAttackWait`에서 `_moveLock = isWeaponStopMove ? Acquire(MoveLock) : null`, `_ladderLock = Acquire(LadderLock)`. 시퀀스를 필드에 저장하고 재공격 시 Kill + 이전 핸들 Dispose. `OnDisable`/`OnDestroy`에서 둘 다 Dispose.
- 수정: `Skill.cs` — 홀드/일반 모두 같은 패턴. 홀드는 `EndHold`에서 Dispose.
- 수정: `PlayerController.cs` — PC:218, PC:557의 `weaponStopMove = false` 삭제. PC:192(속도 0)·208(Move)·228(점프) 조건을 `Has(MoveLock) && states != "dash"`로. 
- 수정: `DamageObject.cs` — DO:183-186 삭제(해당 스킬의 MoveLock은 Skill이 소유).
- 검증
  - [ ] 프로브 `lock_overlap.cs`: 평타 시작 → `isCancleAttack` 스킬 발동 → 평타 대기 시간 후 `Has(MoveLock)` 유지 확인
  - [ ] 프로브: 무기 해제(`AttackManager.UnEquipMainWeaopon`) 직후 `status_dump`에 Weapon/Skill 소유 핸들 0개
  - [ ] 플레이: 정지형 스킬 중 대시 → 대시 끝나면 스킬 종료까지 정지 유지

### Phase 4 — InputLock·DashLock (UI)

> **진행(2026-10-18):** 🔶 `UiLock.Set(owner, bool)` — 창마다 이름 붙은 InputLock 토큰. 새 창(강화·도감·퀘스트 대화)은 이전 완료(`UiLockTests` 3개). 레거시 UI도 이행 완료 ✅ — 인벤토리·창고·상점은 함께 열리고 닫히는 묶음이라 `InvGroup` 하나, 제작은 `Craft`. 가상 키 검증: I(열기)→K(차단)→I(닫기)→K(제작 열기)→Esc(닫기) 전부 기대대로 (`Tools/probes/press_key.cs` + `input_bg_on/off`). ✅ DashLock(2026-10-19): `DashWait2F` = `AcquireFor(DashLock, 0.1f, "UiClose")`, 대시 조건 `!Has(DashLock)`, `uiDashWait` 는 직렬화 호환용으로 남기고 미사용. 프로브: 획득 직후 True·1초 뒤 False.

- 수정: `InventoryManager`, `Chest`, `CraftingManager`, `ShopManager` — 열 때 `_uiLock = Acquire(InputLock, "Inventory")`, 닫을 때 Dispose. 각 UI의 "열기 조건 `isOpenUI == false`"는 `!Has(InputLock)`로(현행: 다른 UI가 열려 있으면 안 열림). Chest의 닫기 경로가 InventoryManager:589/1230에 있으므로 **핸들 소유자를 닫는 쪽에서 찾을 수 있게** `InventoryManager.CloseAll()`이 열린 UI 핸들을 모두 Dispose.
- 수정: `CraftingManager:476/490` + `PlayerController.DashWait2F` → `AcquireFor(DashLock, 0.1f, "CraftClose")`. `uiDashWait` 필드는 직렬화되어 있으므로 남기고 `[HideInInspector]` + 미사용.
- 검증: [ ] 인벤토리+창고 동시 → 하나 닫아도 `Has(InputLock)` 유지 [ ] 둘 다 닫으면 이동 가능
- **완료(2026-10-30):** 레거시 쓰기 0 → `isSuperArmor`·`isInvincibility`·`weaponStopMove`·`checkAttackLadder`·`isOpenUI` 를 **읽기 전용 프로퍼티**로(setter 제거, `SetLegacy` 는 테스트·하위 호환용으로만 남음). `UiLockTests.InvGroupAndCraft_Independent`, 플레이 프로브: InvGroup+Craft → InvGroup 닫아도 isOpenUI=True, 둘 다 닫으면 False. EditMode 135/135.

### Phase 5 — 입력 버퍼 · 코요테

- 추가: `Script/Player/State/InputBuffer.cs`(InputBuffer, CoyoteTimer), `Assets/Editor/Tests/InputBufferTests.cs`
- 수정: `PlayerController.cs` — 점프 블록(PC:228-250)을 `_jumpBuffer` 경유로, 착지 판정(PC:670-707)에서 `CoyoteTimer.SetGrounded`, `isOnGround`/`ReturnIsGround` 래치 제거(필드는 public 직렬화 → 남기고 미사용). 새 필드 `[SerializeField] private float _jumpBufferTime = 0.1f, _coyoteTime = 0.08f; private bool _fallConsumesGroundJump = true;` (기존 프리팹에는 초기화 값이 들어간다)
- 수정: `AttackManager.cs`(Update 58-65), `Weapon.cs`(`CanMeleeNow` 추가, `MeleeAttack`의 콤보 타이머 갱신을 발동 성공 시로 이동)
- 검증
  - [ ] 테스트: 0.09s 전 입력 Consume 성공 / 0.11s 실패 / Consume 후 재Consume 실패 / 코요테 0.07s 지상 점프, 0.09s 공중 점프
  - [ ] 플레이: 평타 연타 시 콤보 끊김 없음, 발판 끝 점프 체감

### Phase 6 — FSM 그림자 모드

- 추가: `Script/Player/State/PlayerStateMachine.cs`, `Assets/Editor/Tests/PlayerStateMachineTests.cs`, `Tools/probes/fsm_dump.cs`
- 수정: `PlayerController.cs` — `LateUpdate`에서 기존 플래그로 상태를 **추론**해 `TryChange`(추론 규칙: Dead > `isAttacked` → Hit > `states=="dash"` → Dash > `isAttackAnim` → Attack/Skill > `isUpLadder` → Ladder > `isGround` → Grounded > Air). 거부된 전이는 에디터에서만 `Debug.LogWarning`(문자열 할당은 거부 시에만).
- 동작 변화 없음. 경고가 0이 될 때까지 전이표 또는 기존 코드의 이상 경로를 고친다.
- 검증: [ ] 테스트: 전이표 전 칸(64칸) 허용/거부, Attack 진입 후 Locomotion 보존 [ ] 3분 플레이 후 경고 0, `fsm_dump`로 최근 8개 전이

### Phase 7 — FSM 주도 전환 (상태별 1커밋 권장: Dash → Ladder → Air/Grounded → Attack/Skill → Hit/Dead)

- 수정: `PlayerController.cs` — `states` 비교를 `_fsm.IsIn(...)`으로 교체, 진입/종료 처리(`EnterDash/ExitDash` 등)에서 중력·콜라이더 설정. Update의 중력 강제(PC:156-160)는 상태 진입 시 1회로.
- 수정: `PlayerHealthManager.cs` — `playerController.isAttacked = …`(PHM:225-226/260) → `playerController.EnterHit(stiffTime)` / `ExitHit()`. `isAttacked` 필드는 getter로 미러링(직렬화 필드는 유지).
- 수정: `Skill.cs`/`Weapon.cs` — 발동 시 `TryChange(Skill/Attack)`가 false면 발동하지 않음.
- 검증: [ ] 수동 플레이 체크리스트 — 지상/공중/로프 공격, 대시 캔슬, 하향 점프, 사다리 위·아래 이탈, 피격 경직, 2단 점프 [ ] `fsm_dump` [ ] Phase 6 경고 0 유지

### Phase 8 — 애니메이션 드라이버

- 추가: `Script/Player/State/PlayerAnimDriver.cs`
- 수정: `PlayerController.cs`의 `mainCharacter.Play` 18곳 → 드라이버. Idle 디바운스용 `check`/`RunChecker`/`VerticalRunChecker` 시퀀스 제거.
- 검증: [ ] 프로브에서 드라이버 `LastPlayed`와 `mainCharacter` 현재 애니 비교 [ ] 스크린샷(대기·이동·점프·로프)

### Phase 9 — 스탯 코어 + int facade

> **진행(2026-10-19):** ✅ `Combat/Stats/PlayerStats.cs`(StatId 13종·ModType·StatModifier·Source/Group/Id 해제·Legacy 덧셈 항·Changed·Dump·SubsystemRegistration 리셋) + `DatabaseManager` int 12개 → 프로퍼티 facade. 스킬별 Dictionary 3종과 MaxHp 출처 이관은 Phase 10. `PlayerStatsTests` 6개 포함 EditMode 109/109 통과, 플레이: `equip_set_test`(Cliff 6부위) → `stats_dump.cs` Def 15·Crit 5·CritDmg 20·AtkSpd 10·IncDmg 10(전부 legacy 경로), 콘솔 에러 0.

- 추가: `Script/Combat/Stats/StatId.cs`, `StatModifier.cs`, `Stat.cs`, `PlayerStats.cs`, `Assets/Editor/Tests/PlayerStatsTests.cs`, `Tools/probes/stats_dump.cs`
- 수정: `DatabaseManager.cs` — 12개 int 필드(16-27) → 프로퍼티(`SetLegacy`). `StatMath`는 그대로(facade 경유).
- 검증
  - [ ] 테스트: Add→Percent→Multiply 순서(입력 순서와 무관) / RemoveBySource·Group·Id / 같은 Id 교체 / legacy `+=` 왕복 후 0 / RoundToInt / Changed 이벤트
  - [ ] 기존 `DamageCalculatorTests` 통과 유지
  - [ ] 플레이: 장비 착용·해제 전후 `stats_dump` == 착용 전 값

### Phase 10 — 스탯 출처 이관

> **진행(2026-10-19):** ✅ 10a 장비 — `EquipBoxCheck.SetEquipment` 가 정수 스탯 11종을 `Source = Equipment 인스턴스` 모디파이어로 넣고(재적용 시 먼저 RemoveBySource → 중복 없음), `DisableEquipment` 는 `RemoveBySource` 한 줄. HP·쿨감 Dictionary·출혈 발동 데이터는 기존 경로. 프로브 `equip_cycle.cs`: Cliff 3부위 +3 착용(같은 장비 재적용 포함) → Def 33·모디파이어 3 → 해제 후 전부 0. ✅ 10b 세트 — `SetEffectApplier.Apply(effect, apply, group)`, `SetItem` 이 `"Set:{세트}_{단계}"` 그룹을 넘긴다. 정수 스탯은 그룹 모디파이어(적용 전 RemoveByGroup → 중복 없음), HP·스킬 키는 기존 경로. 테스트 `GroupApply_NoDuplicate_RemoveByGroupReturnsToBaseline` + 플레이 `equip_set_test` → `stats_dump` 전 스탯 legacy 0·mods 1. ✅ 10c 버프 — `PlayerBuff` 를 인스턴스 Id 모디파이어로(`OnDestroy` 에서도 RemoveById → 만료 전 파괴·중복 활성 시 수치 잔류 버그 해결), `DamageObject` 홀드 방어 버프도 Id. 프로브 `buff_cycle.cs`: 속도 버프 2개 중첩 120 → 하나 조기 파괴 60 → 만료 0, 음식 방어 +5 → 해제 0. ⏳ 10d 스킬 키·10e MaxHp.

- 10a 장비: `EquipBoxCheck.SetEquipment/DisableEquipment`, 쿨감 125-147 → `Source = equip`. 체크: **착용과 해제에 같은 Equipment 인스턴스가 전달되는지** 먼저 프로브로 확인(로드 시 재생성되면 Source를 `"Equip:{equipArea}"` 문자열로 대체).
- 10b 세트: `SetItem.ActiveSetEffect` → `Group`. `CheckSetDisEffect`는 `RemoveByGroup` 한 줄로.
- 10c 버프: `PlayerBuff`, DO `BuffOn/BuffOff` → `Id`. `Invoke("DestoryBuff")`는 유지하되 `OnDestroy`에서도 `RemoveById`(파괴 시 누수 방지).
- 10d 스킬 키 스탯: 읽기(Skill:184/242, SkillCooldown:69) → `GetSkillInt` 후 쓰기 이관, Dictionary `[Obsolete]`.
- 10e MaxHp: `PHM.EquipmentActiveTrue/False` → `PlayerStats.Changed(MaxHp)` 구독, `fullHP` 갱신·초과분 클램프(B-106 동작 유지).
- 검증(각 하위 단계): [ ] 프로브 `equip_cycle.cs`: 장비 5개 착용 → 세트 효과 발동 → 전부 해제 → `stats_dump` 모든 값 0(레거시 포함) [ ] 버프 만료·버프 중 장비 교체 [ ] 저장→재시작→로드 후 값 동일

## 4. 위험 요소 · 함정

| 위험 | 내용 | 대응 |
|---|---|---|
| 직렬화 필드 | `PlayerController`의 public 필드(`states`, `isGround`, `isOnGround`, `isLadder`, `isUpLadder`, `jumpsRemaining`, `isAttacked`, `boxColliderTrue`, `platformTrue`, `uiDashWait` 등)는 씬·프리팹 YAML에 값이 있다 | 지우거나 이름을 바꾸지 않는다. 안 쓰게 되면 `[HideInInspector]` + 주석. 이름 변경은 `[FormerlySerializedAs]` 필수. 타입 변경 금지(`states`를 enum으로 바꾸면 값 유실) |
| 새 직렬화 필드 | 기존 프리팹에 없는 필드는 C# 초기화 값이 들어간다 | 반드시 초기화 값을 둔다(0.1f, 0.08f 등) |
| 새 컴포넌트 | 프리팹에 붙여야 하는 MonoBehaviour는 GUID·Missing Script 위험 | 전부 순수 C#. Tick은 `PlayerController.Update`에서 호출 |
| 정적 상태 수명 | `PlayerStatus`/`PlayerStats`는 씬 전환에도 남는다. Enter Play Mode에서 도메인 리로드를 끄면 이전 플레이 값이 남는다 | `SubsystemRegistration` 리셋, 사망·타이틀 복귀 시 `ForceClear` |
| DOTween 콜백 | Weapon:154, Skill:288/348/376/391/419/440, PC:118/329/415/481/548/585/622, PHM 시퀀스 대부분이 저장·Kill·SetLink 없음. 파괴 후 람다 실행(MissingReference) 또는 Kill 시 해제 콜백 누락 | **토큰 해제를 시퀀스 콜백에만 의존하지 않는다.** `AcquireFor`(Tick 만료) 우선, 수동 핸들은 `OnDisable`/`OnDestroy`에서 Dispose. 시퀀스는 필드에 저장하고 재사용 전 Kill(CodeStyle) |
| `Invoke` 문자열 | `"EndInvincible"`, `"EndSuperArmor"`, `"ReturnIsGround"`, `"CreatAttackPrefab"`, `"DestoryBuff"`는 이름 변경·삭제 시 컴파일러가 못 잡는다 | 메서드를 지우기 전 `grep -rn "Invoke(\"이름"`. 대체 시 `CancelInvoke` |
| AnyPortrait | `mainCharacter.Play`를 매 프레임 호출(PC:141/148/152), 초기화 전 호출, 파괴 후 `SetControlParamInt`/`SetMeshAlphaAll`(PHM:211-216, 195-197) | 드라이버에서 이름 캐시 + null 가드. PHM 시퀀스는 `SetLink(gameObject)`. AnyPortrait 소스는 수정하지 않는다 |
| 입력 액션 인스턴스 | `KeyAction`을 PlayerController·AttackManager·Skill(무기마다 4개)이 각자 `new` 한다. 같은 키에 여러 인스턴스 | ✅(10-30) Skill의 인스턴스 제거(해제는 AttackManager). 장기적으로 `PlayerInput` 1곳에서 공급 |
| 키 입력 주입 금지 | EditorAutomation 원칙상 OS 입력 주입 불가 → 키 해제 같은 입력 검증을 프로브로 못 한다 | 입력 경로를 메서드(`ReleaseWeaponSkill(i)`)로 분리해 프로브가 직접 호출. 실제 키 동작은 수동 확인 후 기록 |
| 동작 변화 | Hit이 공격을 끊음, 낙하 후 점프 1회 소모, 착지 래치 제거 | 커밋 본문·CombatSpec에 명시. 토글 필드로 되돌릴 수 있게 |
| 레거시 역산 | facade setter의 delta 방식은 Add 전용 스탯에서만 정확 | Phase 10에서 레거시 쓰기가 0이 된 스탯만 Percent/Multiply 허용 |

## 5. 열린 질문 ([임시 가정]으로 진행)

- Q-A 피격이 공격·스킬을 끊는가? → [임시 가정] 끊는다(애니·홀드 종료, 생성된 오브젝트 유지). 슈퍼아머면 안 끊는다.
- Q-B 발판에서 떨어진 뒤 2단 점프가 가능한가(메이플은 불가)? → [임시 가정] 코요테(0.08s) 후 지상 점프 1회 소모 → 공중 점프 1회만.
- Q-C 같은 버프를 다시 쓰면 중첩인가 갱신인가? → [임시 가정] 현행대로 중첩.
- Q-D 홀드 중 다른 스킬 키 → [임시 가정] 무시(버퍼링도 안 함).
