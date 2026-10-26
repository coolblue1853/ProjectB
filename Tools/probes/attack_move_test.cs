// 평타 중 이동 검증: 오른쪽 키를 누른 채 0.6초 걷기(평소 속도) → 평타 → 후딜 동안 x·속도 샘플. 결과는 AppDomain pb_atkmove (attack_move_read.cs 로 읽음)
var kbd = UnityEngine.InputSystem.Keyboard.current;
var w = Weapon.Equipped;
if (kbd == null || w == null) return "no keyboard or weapon";
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var rb = pc.GetComponent<Rigidbody2D>();
var log = new System.Text.StringBuilder();
log.Append("weapon=" + w.name + " stopMove=" + w.isWeaponStopMove + "\n");
System.AppDomain.CurrentDomain.SetData("pb_atkmove", "running");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.RightArrow));
DG.Tweening.DOVirtual.DelayedCall(0.6f, () => log.Append("walk vx=" + rb.linearVelocity.x.ToString("F2") + " x=" + pc.transform.position.x.ToString("F3") + "\n"), false);
DG.Tweening.DOVirtual.DelayedCall(0.65f, () => { w.MeleeAttack(); log.Append("attack x=" + pc.transform.position.x.ToString("F3") + "\n"); }, false);
for (int i = 1; i <= 8; i++)
{
    float t = 0.65f + i * 0.06f;
    DG.Tweening.DOVirtual.DelayedCall(t, () => log.Append("t+" + (UnityEngine.Time.time).ToString("F2") + " wait=" + w.isAttackWait + " lock=" + DatabaseManager.weaponStopMove + " vx=" + rb.linearVelocity.x.ToString("F2") + " x=" + pc.transform.position.x.ToString("F3") + " sx=" + pc.transform.localScale.x.ToString("F1") + "\n"), false);
}
DG.Tweening.DOVirtual.DelayedCall(1.3f, () => {
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, new UnityEngine.InputSystem.LowLevel.KeyboardState());
    System.AppDomain.CurrentDomain.SetData("pb_atkmove", log.ToString());
}, false);
return "started weapon=" + w.name;
