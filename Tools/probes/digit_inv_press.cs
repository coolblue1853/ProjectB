// 키 DIGIT(기본 Digit2)를 0.1초 누른다. sed 로 __KEY__ 치환
var kb = UnityEngine.InputSystem.Keyboard.current;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.__KEY__));
DG.Tweening.DOVirtual.DelayedCall(0.1f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()), true);
return "pressed __KEY__";
