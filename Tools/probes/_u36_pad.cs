// U36: 가상 패드 LT+R3 -> 미니맵 토글, 아티팩트 비발동 확인 -> Logs/playtest/u36.txt
var hud = UnityEngine.Object.FindFirstObjectByType<MinimapHud>(); if (hud == null) return "no minimap";
var pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
var sb = new System.Text.StringBuilder(); int n = 0;
bool v0 = (bool)typeof(MinimapHud).GetField("_visible",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(hud);
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
    n++;
    if (n == 3) UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftTrigger = 1f, buttons = (1u << (int)UnityEngine.InputSystem.LowLevel.GamepadButton.RightStick) });
    if (n == 6) UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState());
    if (n == 10) { sb.AppendLine("before=" + v0 + " after=" + (bool)typeof(MinimapHud).GetField("_visible",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(hud)); System.IO.File.WriteAllText("Logs/playtest/u36.txt", sb.ToString()); UnityEngine.InputSystem.InputSystem.RemoveDevice(pad); UnityEditor.EditorApplication.update -= upd; }
};
UnityEditor.EditorApplication.update += upd; return "started";
