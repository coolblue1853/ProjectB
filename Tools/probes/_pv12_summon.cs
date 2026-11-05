// PV12: 장착 FangDagger 의 F 키(스킬 3 Howl of the Pack) → SummonRunner 생성·몹 피격·소멸 → Logs/pv12/summon.txt
var pc = PlayerController.instance; var phm = PlayerHealthManager.Instance; var sb = new System.Text.StringBuilder(); var tc = TimeChange.instance;
phm.fullHP = 99999; phm.nowHp = 99999; phm.nowStemina = phm.fullStemina;
if (!tc.isDaytime) new SROptions().Day();
bool ok = MapInfo.GoToMap("Lv1_NearForest", pc.gameObject);
EnemySpawner dsp = null; foreach (var sp in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) if (sp.isDaytimeSpowner && dsp == null) dsp = sp;
var kb = UnityEngine.InputSystem.Keyboard.current;
double until = UnityEditor.EditorApplication.timeSinceStartup + 8; int st = 0; double tKey = 0; int maxRunners = 0; double firstSeen = -1, goneAt = -1;
System.Func<float> hpSum = () => { float s = 0; foreach (var e in EnemyHealth.Active) if (e != null && !e.isBossMob && ((Vector2)e.transform.position - (Vector2)pc.transform.position).sqrMagnitude < 12f * 12f) s += e.nowHP; return s; };
System.Func<int> near = () => { int n = 0; foreach (var e in EnemyHealth.Active) if (e != null && !e.isBossMob && ((Vector2)e.transform.position - (Vector2)pc.transform.position).sqrMagnitude < 12f * 12f) n++; return n; };
EnemyHealth trk = null; double lastLog = 0; float hp0 = 0; int n0 = 0; var hpMap = new System.Collections.Generic.Dictionary<EnemyHealth,float>(); var dropped = new System.Collections.Generic.HashSet<EnemyHealth>(); double dmgTot = 0; bool snap = false;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup;
 try {
  if (st == 1) { foreach (var e in EnemyHealth.Active) { if (e == null) continue; float h0; if (hpMap.TryGetValue(e, out h0)) { if (e.nowHP < h0) { dropped.Add(e); dmgTot += h0 - e.nowHP; } } hpMap[e] = e.nowHP; }
   if (!snap && now - tKey > 0.5) { snap = true; var rs = UnityEngine.Object.FindObjectsByType<SummonRunner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None); string rn = ""; foreach (var r in rs) rn += r.name + "@" + r.transform.position.x.ToString("0.0") + " "; sb.AppendLine("t+0.5: cd3=" + SkillCooldown.instance.Remaining(3).ToString("0.0") + " sta=" + phm.nowStemina + " runners=" + rn + " player=" + pc.transform.position.x.ToString("0.0")); }
   if (trk != null && now - lastLog > 0.2) { lastLog = now; var rr = UnityEngine.Object.FindObjectsByType<SummonRunner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None); sb.AppendLine("  t=" + (now - tKey).ToString("0.0") + " mob=" + trk.name + " hp=" + trk.nowHP + " x=" + trk.transform.position.x.ToString("0.0") + " wolf=" + (rr.Length > 0 ? rr[0].transform.position.x.ToString("0.0") : "-") + " lscale=" + pc.transform.localScale.x); }
   int c = UnityEngine.Object.FindObjectsByType<SummonRunner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length; if (c > maxRunners) maxRunners = c; if (c > 0 && firstSeen < 0) firstSeen = now - tKey; if (c == 0 && firstSeen >= 0 && goneAt < 0) goneAt = now - tKey; }
  if (now < until) return;
  if (st == 0) {
   pc.transform.position = new Vector3(dsp.transform.position.x, dsp.transform.position.y, pc.transform.position.z); until = now + 6; st = 5; return; }
  if (st == 5) {
   EnemyHealth tgt = null; float bd = 1e9f; foreach (var e in EnemyHealth.Active) { if (e == null || e.isBossMob || e.nowHP <= 0) continue; float dd = ((Vector2)e.transform.position - (Vector2)pc.transform.position).sqrMagnitude; if (dd < bd) { bd = dd; tgt = e; } }
   if (tgt != null) { sb.AppendLine("target " + tgt.name + " hp=" + tgt.nowHP + " dist0=" + Mathf.Sqrt(bd).ToString("0.0")); tgt.transform.position = new Vector3(pc.transform.position.x + 2.5f, pc.transform.position.y, tgt.transform.position.z); }
   trk = tgt;
   Weapon w = Weapon.Equipped; hp0 = hpSum(); n0 = near();
   sb.AppendLine("goto=" + ok + " weapon=" + (w ? w.name : "none") + " nearMobs=" + n0 + " hpSum=" + hp0 + " runners=" + UnityEngine.Object.FindObjectsByType<SummonRunner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length + " cd3=" + SkillCooldown.instance.Remaining(3).ToString("0.0") + " sta=" + phm.nowStemina);
   UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.F));
   DG.Tweening.DOVirtual.DelayedCall(0.1f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()), true);
   foreach (var e in EnemyHealth.Active) if (e != null) hpMap[e] = e.nowHP;
   tKey = now; st = 1; until = now + 12; return; }
  if (st == 1) {
   sb.AppendLine("after F: maxRunners=" + maxRunners + " firstSeen=" + firstSeen.ToString("0.0") + "s goneAt=" + goneAt.ToString("0.0") + "s cd3=" + SkillCooldown.instance.Remaining(3).ToString("0.0") + " sta=" + phm.nowStemina + " nearMobs=" + near() + " hpSum=" + hpSum() + " (before " + hp0 + ", n " + n0 + ") droppedMobs=" + dropped.Count + " dmgTotal=" + dmgTot + " runnersNow=" + UnityEngine.Object.FindObjectsByType<SummonRunner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length);
   System.IO.Directory.CreateDirectory("Logs/pv12"); System.IO.File.WriteAllText("Logs/pv12/summon.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.Directory.CreateDirectory("Logs/pv12"); System.IO.File.WriteAllText("Logs/pv12/summon.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
