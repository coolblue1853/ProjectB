// PV9: 실 몹 공격 경로 방어 중 피격 → Logs/pv9/mob.txt (A 미방어 6초, B G홀드 6초)
var pc = PlayerController.instance; var kb = UnityEngine.InputSystem.Keyboard.current; var phm = PlayerHealthManager.Instance; var sb = new System.Text.StringBuilder();
var sg = Weapon.Equipped.GetComponentInChildren<ShieldGuard>(true);
Action<bool> g = on => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, on ? new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.G) : new UnityEngine.InputSystem.LowLevel.KeyboardState());
int st = 0; double until = UnityEditor.EditorApplication.timeSinceStartup + 0.1; EnemyHealth foe = null; int lastHp = 0, loss = 0, drops = 0; GuardOutcome lo = new GuardOutcome(); int blockedN = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup;
 if (st == 3 || st == 4) { if (foe) foe.transform.position = pc.transform.position + new Vector3(1.2f, 0, 0); if (phm.nowHp < lastHp) { sb.AppendLine("  hit st=" + st + " -" + (lastHp - phm.nowHp) + " holding=" + sg.Holding + " blocked=" + sg.LastOutcome.Blocked + " parried=" + sg.LastOutcome.Parried); loss += lastHp - phm.nowHp; drops++; } lastHp = phm.nowHp; if (sg.LastOutcome.Blocked && !lo.Blocked) blockedN++; lo = sg.LastOutcome; }
 if (now < until) return;
 try {
 if (st == 0) { pc.transform.position = new Vector3(-384, 9, pc.transform.position.z); until = now + 8; st = 1; return; }
 if (st == 1) { foe = EnemyHealth.Active.FirstOrDefault(e => e.isActiveAndEnabled && !e.IsElite && !e.isBossMob && e.IsMonster);
   sb.AppendLine("foe=" + (foe ? foe.name : "null") + " dist=" + (foe ? Vector2.Distance(foe.transform.position, pc.transform.position) : -1)); if (foe == null) { st = 9; return; }
   foe.nowHP = 99999; phm.fullHP = 9999; phm.nowHp = 9999; foe.transform.position = pc.transform.position + new Vector3(1.2f, 0, 0); until = now + 1; st = 2; return; }
 if (st == 2) { phm.nowHp = 9999; lastHp = 9999; loss = 0; drops = 0; lo = sg.LastOutcome; until = now + 6; st = 3; return; }
 if (st == 3) { sb.AppendLine("A 미방어 6초 손실=" + loss + " 피격횟수=" + drops + " foeDist=" + Vector2.Distance(foe.transform.position, pc.transform.position)); foe.transform.position = pc.transform.position + new Vector3(1.2f, 0, 0); phm.nowHp = 9999; lastHp = 9999; loss = 0; drops = 0; blockedN = 0; g(true); until = now + 6; st = 4; return; }
 if (st == 4) { sb.AppendLine("B G홀드 6초 손실=" + loss + " 피격횟수=" + drops + " blocked횟수=" + blockedN + " holding=" + sg.Holding + " last: taken=" + sg.LastOutcome.Taken + " blocked=" + sg.LastOutcome.Blocked + " parried=" + sg.LastOutcome.Parried); g(false); st = 9; }
 } catch (Exception e) { sb.AppendLine("ERR " + e); st = 9; }
 if (st == 9) { System.IO.File.WriteAllText("Logs/pv9/mob.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; st = 10; } };
UnityEditor.EditorApplication.update += upd; return "started";
