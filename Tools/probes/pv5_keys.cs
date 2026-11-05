// PV5: 키 입력 경로(Backspace 튜토리얼 건너뛰기, J 퀘스트 창) → Logs/pv5/keys.txt
var kb = UnityEngine.InputSystem.Keyboard.current; var sb = new System.Text.StringBuilder();
var fh = UnityEngine.Object.FindFirstObjectByType<FirstHints>(); var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var tFld = typeof(FirstHints).GetField("_showingTutorial", bf); var seenFld = typeof(FirstHints).GetField("_seen", bf);
((System.Collections.Generic.HashSet<string>)seenFld.GetValue(fh)).Clear();
typeof(FirstHints).GetMethod("TryStartTutorial", bf).Invoke(fh, null);
System.Action<UnityEngine.InputSystem.Key, bool> key = (k, on) => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, on ? new UnityEngine.InputSystem.LowLevel.KeyboardState(k) : new UnityEngine.InputSystem.LowLevel.KeyboardState());
int st = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 2.5; UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
  if (st == 0) { sb.AppendLine("tutorial showing before=" + tFld.GetValue(fh)); key(UnityEngine.InputSystem.Key.Backspace, true); until = now + 0.3; st = 1; return; }
  if (st == 1) { key(UnityEngine.InputSystem.Key.Backspace, false); until = now + 0.5; st = 2; return; }
  if (st == 2) { sb.AppendLine("Backspace 후 showing=" + tFld.GetValue(fh)); sb.AppendLine("J 전 open=" + NpcQuestWindow.IsOpen); key(UnityEngine.InputSystem.Key.J, true); until = now + 0.3; st = 3; return; }
  if (st == 3) { key(UnityEngine.InputSystem.Key.J, false); until = now + 0.8; st = 4; return; }
  if (st == 4) { sb.AppendLine("J 후 open=" + NpcQuestWindow.IsOpen); key(UnityEngine.InputSystem.Key.J, true); until = now + 0.3; st = 5; return; }
  if (st == 5) { key(UnityEngine.InputSystem.Key.J, false); until = now + 0.8; st = 6; return; }
  if (st == 6) { sb.AppendLine("J 두 번째 후 open=" + NpcQuestWindow.IsOpen); System.IO.File.WriteAllText("Logs/pv5/keys.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
