// 주인공 스트레스(플레이 중, 먼저 input_bg_on·장착): 6초 동안 달리기·점프·대시·연타·피격·제동·공중 공격을 섞어 실행 → 콘솔 오류는 셸에서 read_console 로 센다.
// 그림 교체·먼지·번쩍임·잔상·궤적이 서로 부딪혀 예외가 나지 않는지 넓게 본다(PixelRigDesign 10-26)
var pc = PlayerController.instance;
var kbd = UnityEngine.InputSystem.Keyboard.current;
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var jump = typeof(PlayerController).GetMethod("Jump", flags);
var dash = typeof(PlayerController).GetMethod("Dash", flags);
var phm = UnityEngine.Object.FindFirstObjectByType<PlayerHealthManager>();
System.Action<UnityEngine.InputSystem.LowLevel.KeyboardState> key = st => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kbd, st);
var R = UnityEngine.InputSystem.Key.RightArrow; var L = UnityEngine.InputSystem.Key.LeftArrow; var S = UnityEngine.InputSystem.Key.LeftShift;
// 프로브 컴파일러(codedom)는 튜플을 못 읽어 시간·동작 배열 두 개로
float[] times = { 0.0f, 0.15f, 0.25f, 0.8f, 1.0f, 1.2f, 1.8f, 2.3f, 2.6f, 2.9f, 3.4f, 3.6f, 4.0f, 4.4f, 5.0f, 5.2f };
System.Action[] acts = {
  () => key(new UnityEngine.InputSystem.LowLevel.KeyboardState(R)),
  () => key(new UnityEngine.InputSystem.LowLevel.KeyboardState(R, S)),
  () => key(new UnityEngine.InputSystem.LowLevel.KeyboardState(R)),
  () => key(new UnityEngine.InputSystem.LowLevel.KeyboardState()),
  () => jump.Invoke(pc, null),
  () => Weapon.Equipped.MeleeAttack(),
  () => dash.Invoke(pc, null),
  () => Weapon.Equipped.MeleeAttack(),
  () => Weapon.Equipped.MeleeAttack(),
  () => Weapon.Equipped.MeleeAttack(),
  () => phm.damage2Player(1, 0.2f, 0f, Vector2.left, 0f, false),
  () => key(new UnityEngine.InputSystem.LowLevel.KeyboardState(L)),
  () => jump.Invoke(pc, null),
  () => dash.Invoke(pc, null),
  () => key(new UnityEngine.InputSystem.LowLevel.KeyboardState()),
  () => Weapon.Equipped.MeleeAttack(),
};
for (int i = 0; i < times.Length; i++) { var act = acts[i]; DG.Tweening.DOVirtual.DelayedCall(times[i], () => act(), false); }
return "scheduled " + times.Length;
