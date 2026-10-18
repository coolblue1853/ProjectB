// 가상 키 입력 (__KEY__ = UnityEngine.InputSystem.Key 이름). 먼저 input_bg_on 프로브로 백그라운드 입력을 켜고, 끝나면 input_bg_off 로 복구할 것
var kb = UnityEngine.InputSystem.Keyboard.current;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.__KEY__));
DG.Tweening.DOVirtual.DelayedCall(0.1f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()), true);
return "pressed __KEY__";
