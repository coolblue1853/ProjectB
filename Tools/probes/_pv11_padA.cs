var pad = UnityEngine.InputSystem.Gamepad.current; if (pad == null) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); pad.MakeCurrent();
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.South));
DG.Tweening.DOVirtual.DelayedCall(0.15f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()), true);
return "pad A";
