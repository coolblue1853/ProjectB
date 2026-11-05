// PV11 #11: 스포너 구역 안에서 낮/밤 스폰 비교 → Logs/pv11/night2.txt
var pc = PlayerController.instance; var sb = new System.Text.StringBuilder(); var tc = TimeChange.instance;
var all = UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
int nd = 0, nn = 0; EnemySpawner dsp = null, nsp = null;
foreach (var sp in all) { if (sp.isDaytimeSpowner) { nd++; if (dsp == null) dsp = sp; } else { nn++; if (nsp == null) nsp = sp; } }
sb.AppendLine("spawners total=" + all.Length + " dayType=" + nd + " nightType=" + nn + " now day=" + tc.isDaytime);
System.Func<string> snap = () => { var names = new System.Collections.Generic.Dictionary<string,int>(); foreach (var e in EnemyHealth.Active) { string n = e.name.Replace("(Clone)",""); int c; names.TryGetValue(n, out c); names[n] = c + 1; } var s = new System.Text.StringBuilder("mobs=" + EnemyHealth.Active.Count); foreach (var kv in names) s.Append(" " + kv.Key + "x" + kv.Value); return s.ToString(); };
if (nsp == null || dsp == null) { return sb.ToString() + "no spawner of one type"; }
double until = UnityEditor.EditorApplication.timeSinceStartup + 0.1; int st = 0;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { if (UnityEditor.EditorApplication.timeSinceStartup < until) return;
 try {
  if (st == 0) { if (!tc.isDaytime) new SROptions().Day(); pc.transform.position = new Vector3(dsp.transform.position.x, dsp.transform.position.y, pc.transform.position.z); until = UnityEditor.EditorApplication.timeSinceStartup + 10; st = 1; return; }
  if (st == 1) { sb.AppendLine("DAY @" + dsp.name + " day=" + tc.isDaytime + " " + snap()); new SROptions().Night(); pc.transform.position = new Vector3(nsp.transform.position.x, nsp.transform.position.y, pc.transform.position.z); until = UnityEditor.EditorApplication.timeSinceStartup + 12; st = 2; return; }
  if (st == 2) { sb.AppendLine("NIGHT @" + nsp.name + "(" + nsp.MonsterProfile + ") day=" + tc.isDaytime + " " + snap()); System.IO.File.WriteAllText("Logs/pv11/night2.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; }
 } catch (System.Exception e) { sb.AppendLine("ERR " + e); System.IO.File.WriteAllText("Logs/pv11/night2.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; } };
UnityEditor.EditorApplication.update += upd; return "started";
