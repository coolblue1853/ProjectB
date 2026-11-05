// PV9: 방패 장착 중 패드 LT → 방어 O, SkillS 무시 / 키보드 S → SkillS 발동 → Logs/pv9/lt.txt
var kb = UnityEngine.InputSystem.Keyboard.current; var sb = new System.Text.StringBuilder();
var pad = UnityEngine.InputSystem.Gamepad.current; if (pad == null) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); pad.MakeCurrent();
var sg = Weapon.Equipped.GetComponentInChildren<ShieldGuard>(true); var sc = SkillCooldown.instance;
var w = Weapon.Equipped; sb.AppendLine("skill[1]=" + (w.skill.Length > 1 && w.skill[1] != null ? w.skill[1].GetType().Name : "null") + " rem0=" + sc.Remaining(1));
Action<float> lt = v => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftTrigger = v });
Action<bool> s = on => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, on ? new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.S) : new UnityEngine.InputSystem.LowLevel.KeyboardState());
int st = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 0.2;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
 try {
 if (st == 0) { lt(1f); until = now + 0.5; st = 1; return; }
 if (st == 1) { sb.AppendLine("LT 홀드 holding=" + sg.Holding + " rem(S)=" + sc.Remaining(1)); lt(0f); until = now + 0.3; st = 2; return; }
 if (st == 2) { sb.AppendLine("LT 뗌 holding=" + sg.Holding + " rem(S)=" + sc.Remaining(1)); until = now + 0.5; st = 3; return; }
 if (st == 3) { s(true); until = now + 0.3; st = 4; return; }
 if (st == 4) { sb.AppendLine("키보드 S rem(S)=" + sc.Remaining(1) + " holding=" + sg.Holding); s(false); st = 9; }
 } catch (Exception e) { sb.AppendLine("ERR " + e); st = 9; }
 if (st == 9) { System.IO.File.WriteAllText("Logs/pv9/lt.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; st = 10; } };
UnityEditor.EditorApplication.update += upd; return "started";
