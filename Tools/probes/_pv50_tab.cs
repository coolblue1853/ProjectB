// AUTO-50: 도감 열기(B 대신 LT+SELECT) → RB/LB 탭 전환·순환. Logs/pv50/tab.txt
var sb = new System.Text.StringBuilder(); var bw = UnityEngine.Object.FindFirstObjectByType<BestiaryWindow>(FindObjectsInactive.Include);
var pad = UnityEngine.InputSystem.Gamepad.current; if (pad == null) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); pad.MakeCurrent();
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
System.Func<string> snap = () => "open=" + bw.GetType().GetField("open", flags).GetValue(bw) + " zoneTab=" + bw.GetType().GetField("zoneTab", flags).GetValue(bw);
var B = UnityEngine.InputSystem.LowLevel.GamepadButton.LeftShoulder; var R = UnityEngine.InputSystem.LowLevel.GamepadButton.RightShoulder;
var seq = new object[][] { new object[]{"LT+SELECT 열기", 1}, new object[]{"RB", 2}, new object[]{"RB(순환)", 2}, new object[]{"LB(역순환)", 3}, new object[]{"LB", 3}, new object[]{"닫기 B", 4} };
int i = -1, ph = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 0.5;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
 if (ph == 0) { i++; if (i >= seq.Length) { System.IO.Directory.CreateDirectory("Logs/pv50"); System.IO.File.WriteAllText("Logs/pv50/tab.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
   var s = new UnityEngine.InputSystem.LowLevel.GamepadState(); int k = (int)seq[i][1];
   if (k == 1) { s = s.WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.Select); s.leftTrigger = 1f; } else if (k == 2) s = s.WithButton(R); else if (k == 3) s = s.WithButton(B); else s = s.WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.East);
   UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, s); until = now + 0.3; ph = 1; return; }
 if (ph == 1) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()); until = now + 1.0; ph = 2; return; }
 if (ph == 2) { sb.AppendLine(seq[i][0] + " => " + snap()); until = now + 0.3; ph = 0; } };
UnityEditor.EditorApplication.update += upd; return "started";
