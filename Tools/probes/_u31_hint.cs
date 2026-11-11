// U31: 입력 장치 힌트 전환(가상 패드 버튼 -> UsingGamepad, 키보드 키 -> false) + TalkNpc 프롬프트 문구 -> Logs/playtest/u31.txt
var pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
var kb = UnityEngine.InputSystem.Keyboard.current;
var sb = new System.Text.StringBuilder(); int n = 0;
var npc = UnityEngine.Object.FindFirstObjectByType<TalkNpc>();
System.Func<string> txt = () => { if (npc == null) return "noNpc"; var t = npc.GetComponentInChildren<TMPro.TextMeshPro>(true); return t == null ? "noText" : t.text; };
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
    n++;
    if (n == 3) UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { buttons = (1u << (int)UnityEngine.InputSystem.LowLevel.GamepadButton.South) });
    if (n == 6) { sb.AppendLine("afterPad=" + InputDeviceHint.UsingGamepad); UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()); }
    if (n == 8 && kb != null) UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space));
    if (n == 12) { sb.AppendLine("afterKey=" + InputDeviceHint.UsingGamepad + " npcText=" + txt()); System.IO.File.WriteAllText("Logs/playtest/u31.txt", sb.ToString()); UnityEngine.InputSystem.InputSystem.RemoveDevice(pad); UnityEditor.EditorApplication.update -= upd; }
};
UnityEditor.EditorApplication.update += upd; return "started";
