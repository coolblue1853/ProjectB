// 던파식 공격 방향 잠금 검증(10-30): 오른쪽을 보고 평타 시작 → 곧바로 왼쪽 키 유지 → 모션 동안 localScale.x 부호 유지·위치는 왼쪽 이동 → 모션 끝나면 왼쪽으로 돌아섬.
// 선행: input_bg_on + 오른쪽 키를 잠깐 눌러 오른쪽을 보게 한 뒤 실행. 결과는 AppDomain pb_turnlock (turn_lock_read.cs 로 읽음)
var kbd = UnityEngine.InputSystem.Keyboard.current;
var w = Weapon.Equipped;
if (kbd == null || w == null) return "no keyboard or weapon";
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var animField = typeof(PlayerController).GetField("isAttackAnim", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
w.isWeaponStopMove = false; // 이동 확인용(플레이 모드 인스턴스만, 정지 시 원복)
var log = new System.Text.StringBuilder();
log.Append("weapon=" + w.name + " startSx=" + pc.transform.localScale.x.ToString("F2") + " x=" + pc.transform.position.x.ToString("F3") + "\n");
System.AppDomain.CurrentDomain.SetData("pb_turnlock", "running");
w.MeleeAttack(); // 아직 왼쪽 입력 전 → 오른쪽을 보고 시작
log.Append("attack sx=" + pc.transform.localScale.x.ToString("F2") + " anim=" + animField.GetValue(pc) + "\n");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.LeftArrow));
for (int i = 1; i <= 24; i++)
{
    DG.Tweening.DOVirtual.DelayedCall(i * 0.06f, () => log.Append("t=" + UnityEngine.Time.time.ToString("F2") + " left=" + kbd.leftArrowKey.isPressed + " anim=" + animField.GetValue(pc) + " sx=" + pc.transform.localScale.x.ToString("F2") + " x=" + pc.transform.position.x.ToString("F3") + "\n"), false);
}
DG.Tweening.DOVirtual.DelayedCall(1.5f, () => {
    // 왼쪽 키를 누른 채 다음 공격 → 시작 순간 입력 방향(왼쪽)
    w.MeleeAttack();
    log.Append("attack2 wait=" + w.isAttackWait + " sx=" + pc.transform.localScale.x.ToString("F2") + "\n");
}, false);
DG.Tweening.DOVirtual.DelayedCall(1.7f, () => {
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, new UnityEngine.InputSystem.LowLevel.KeyboardState());
    System.AppDomain.CurrentDomain.SetData("pb_turnlock", log.ToString());
}, false);
return "started weapon=" + w.name;
