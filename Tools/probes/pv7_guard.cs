// PV7: 방패 G 키 방어·패링·반격 → Logs/pv7/guard.txt
var kb = UnityEngine.InputSystem.Keyboard.current; var sb = new System.Text.StringBuilder();
var phm = PlayerHealthManager.Instance; var sg = Weapon.Equipped.GetComponentInChildren<ShieldGuard>(true); var cb = Weapon.Equipped.GetComponentInChildren<CounterBuff>(true);
Action<bool> g = on => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, on ? new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.G) : new UnityEngine.InputSystem.LowLevel.KeyboardState());
int st = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 0.5; int hp0 = 0;
Action hit = () => { hp0 = phm.nowHp; phm.damage2Player(100, 0.3f, 3f, Vector2.right, phm.transform.position.x - 1, false); };
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < until) return;
 try {
 if (st == 0) { phm.fullHP = 9999; phm.nowHp = 9999; hit(); until = now + 0.3; st = 1; return; }
 if (st == 1) { sb.AppendLine("A 미방어 손실=" + (hp0 - phm.nowHp) + " holding=" + sg.Holding + " cb=" + cb.Active); until = now + 2.5; st = 2; return; }
 if (st == 2) { g(true); until = now + 0.5; st = 3; return; }
 if (st == 3) { sb.AppendLine("B G홀드 0.5초 holding=" + sg.Holding); hit(); until = now + 0.3; st = 4; return; }
 if (st == 4) { var o = sg.LastOutcome; sb.AppendLine("B 홀드방어 손실=" + (hp0 - phm.nowHp) + " taken=" + o.Taken + " blocked=" + o.Blocked + " parried=" + o.Parried + " cbActive=" + cb.Active + " mul=" + cb.HitMultiplier(null,null,null)); g(false); until = now + 3.5; st = 5; return; }
 if (st == 5) { sb.AppendLine("B 3.5초 뒤 cbActive=" + cb.Active + " holding=" + sg.Holding); g(true); until = now + 0.08; st = 6; return; }
 if (st == 6) { sb.AppendLine("C 누른 직후 holding=" + sg.Holding); hit(); until = now + 0.3; st = 7; return; }
 if (st == 7) { var o = sg.LastOutcome; sb.AppendLine("C 패링 손실=" + (hp0 - phm.nowHp) + " parried=" + o.Parried + " reflected=" + o.Reflected + " cb=" + cb.Active); g(false); until = now + 0.1; st = 8; return; }
 if (st == 8) { sb.AppendLine("D 해제 후 holding=" + sg.Holding); st = 9; }
 } catch (Exception e) { sb.AppendLine("ERR " + e); st = 9; }
 if (st == 9) { System.IO.Directory.CreateDirectory("Logs/pv7"); System.IO.File.WriteAllText("Logs/pv7/guard.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; st = 10; } };
UnityEditor.EditorApplication.update += upd; return "started";
