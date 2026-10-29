// B-113 검증: 홀드 스킬 키를 누른 채 다른 스킬 키를 눌렀다 떼도 홀드 유지 + 다른 스킬 미발동, 자기 키를 떼면 종료·자기 슬롯 쿨.
// 먼저 equip_weapon(CliffBlade 등 홀드 스킬 무기) + input_bg_on. 결과는 콘솔 "[probe-hold]" 로그. 끝나면 input_bg_off
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var w = am != null ? am.equipWeapon : null; // 입력을 처리하는 쪽의 무기 (FindFirstObjectByType<Weapon> 은 이전 무기를 잡을 수 있음)
if (w == null) return "no weapon";
int hi = -1, oj = -1;
for (int i = 0; i < w.skill.Length; i++)
{
    if (w.skill[i] == null) continue;
    if (hi < 0 && w.skill[i].isHoldSkill) hi = i; else if (oj < 0 && !w.skill[i].isHoldSkill) oj = i;
}
if (hi < 0 || oj < 0) return "need hold + normal skill: hold=" + hi + " other=" + oj;
var keys = new UnityEngine.InputSystem.Key[] { UnityEngine.InputSystem.Key.A, UnityEngine.InputSystem.Key.S, UnityEngine.InputSystem.Key.D, UnityEngine.InputSystem.Key.F };
var kb = UnityEngine.InputSystem.Keyboard.current;
var hold = w.skill[hi]; var other = w.skill[oj];
other.isCancleAttack = true; // 동시 발동 재현: 홀드 중 평타 대기가 내려가도 발동 가능한 스킬
var cd = hold.skillCooldown;
float cdH = cd.cooldownTime[hi], cdO = cd.cooldownTime[oj];
cd.cooldownTime[hi] = 30f; cd.cooldownTime[oj] = 30f; // 쿨이 걸렸는지 보이도록 잠시 늘림(프로브 끝에 복구)
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(keys[hi]));
DG.Tweening.DOVirtual.DelayedCall(0.4f, () => {
    Debug.Log("[probe-hold] 1 pressed own: holding=" + hold.IsHolding);
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(keys[hi], keys[oj]));
}, true);
DG.Tweening.DOVirtual.DelayedCall(0.6f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(keys[hi])), true);
DG.Tweening.DOVirtual.DelayedCall(0.9f, () => {
    Debug.Log("[probe-hold] 2 other pressed+released: holding=" + hold.IsHolding + " otherCooldown=" + cd.isCooldown[oj]);
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState());
}, true);
DG.Tweening.DOVirtual.DelayedCall(1.3f, () => {
    Debug.Log("[probe-hold] 3 own released: holding=" + hold.IsHolding + " holdSlotCooldown=" + cd.isCooldown[hi] + " inspectorSkillNum=" + hold.skillNum + " moveLock=" + PlayerStatus.Has(StatusFlag.MoveLock));
    other.isCancleAttack = false;
    cd.cooldownTime[hi] = cdH; cd.cooldownTime[oj] = cdO;
}, true);
return "hold slot=" + hi + "(" + hold.skillName + ") other slot=" + oj + "(" + other.skillName + ")";
