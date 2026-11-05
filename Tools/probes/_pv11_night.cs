// PV11 #11: 낮→밤 전환 후 스폰·상태 → Logs/pv11/night.txt
var pc = PlayerController.instance; var sb = new System.Text.StringBuilder(); var tc = TimeChange.instance;
if (!tc.isDaytime) new SROptions().Day();
bool ok = MapInfo.GoToMap("Lv1_NearForest", pc.gameObject);
double until = UnityEditor.EditorApplication.timeSinceStartup + 6; int st = 0;
System.Func<string> snap = () => { var s = new System.Text.StringBuilder("day=" + tc.isDaytime + " mobs=" + EnemyHealth.Active.Count + " spawners:"); foreach (var sp in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) s.Append(" " + sp.name + "=" + sp.MonsterProfile); var names = new System.Collections.Generic.Dictionary<string,int>(); foreach (var e in EnemyHealth.Active) { string n = e.name.Replace("(Clone)",""); int c; names.TryGetValue(n, out c); names[n] = c + 1; } s.Append(" | mobs:"); foreach (var kv in names) s.Append(" " + kv.Key + "x" + kv.Value); return s.ToString(); };
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { if (UnityEditor.EditorApplication.timeSinceStartup < until) return;
 try {
  if (st == 0) { sb.AppendLine("goto=" + ok + " DAY: " + snap()); new SROptions().Night(); until = UnityEditor.EditorApplication.timeSinceStartup + 10; st = 1; return; }
  if (st == 1) { sb.AppendLine("NIGHT: " + snap() + " bgm=" + "?"); new SROptions().Day(); until = UnityEditor.EditorApplication.timeSinceStartup + 5; st = 2; return; }
  if (st == 2) { sb.AppendLine("DAY again: " + snap()); System.IO.File.WriteAllText("Logs/pv11/night.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.File.WriteAllText("Logs/pv11/night.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
