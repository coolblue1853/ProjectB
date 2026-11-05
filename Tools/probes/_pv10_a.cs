// PV10 #2: 보스 맵 이동 후 활성 몹 목록 → Logs/pv10/a.txt
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var phm0 = PlayerHealthManager.Instance; phm0.fullHP = 99999; phm0.nowHp = 99999;
bool ok = MapInfo.GoToMap("Lv1_TowerOutskirts", pc.gameObject);
var tc = TimeChange.instance;
if (tc != null && !tc.isDaytime) { float target = (tc.endNighttime + 0.05f) * tc.totalTime; float add = target - tc.value * tc.totalTime; if (add < 0) add += tc.totalTime; tc.AddTime(add); }
double until = UnityEditor.EditorApplication.timeSinceStartup + 6; 
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { if (UnityEditor.EditorApplication.timeSinceStartup < until) return; UnityEditor.EditorApplication.update -= upd;
 var sb = new System.Text.StringBuilder("goto=" + ok + " pos=" + pc.transform.position + "\n");
 foreach (var e in EnemyHealth.Active) sb.AppendLine(e.name + " boss=" + e.isBossMob + " elite=" + e.IsElite + " pos=" + e.transform.position + " hasRound=" + (e.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>() != null && e.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>().FindTask<EnemyRoundAttackNode>() != null));
 System.IO.Directory.CreateDirectory("Logs/pv10"); System.IO.File.WriteAllText("Logs/pv10/a.txt", sb.ToString()); };
UnityEditor.EditorApplication.update += upd; return "started";
