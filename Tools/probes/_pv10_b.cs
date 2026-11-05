// PV10 #2: 보스(Gatekeeper) 실제 EnemyRoundAttackNode.OnStart 로 분리 생성 탄막, 일반 vs 정예 피격 비교 → Logs/pv10/b.txt
var pc = PlayerController.instance; var phm = PlayerHealthManager.Instance; var sb = new System.Text.StringBuilder();
EnemyHealth foe = null; foreach (var e in EnemyHealth.Active) if (e.isBossMob && e.name.StartsWith("Gatekeeper")) foe = e;
var bt = foe.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>(); var node = bt.FindTask<EnemyRoundAttackNode>();
bt.DisableBehavior(); foe.nowHP = foe.maxHP;
sb.AppendLine("foe=" + foe.name + " node=" + node.attackName + " r=" + node.radius + " n=" + node.bulletCount + " pivot=" + (node.attackPivot ? node.attackPivot.transform.position.ToString() : "null") + " parent=" + node.isSetParent);
int st = 0; double until = 0; int lastHp = 0; int maxHit = 0; var hits = new System.Collections.Generic.List<int>();
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => { double now = UnityEditor.EditorApplication.timeSinceStartup;
 if (st == 2 || st == 5) { var pv = node.attackPivot.transform.position; pc.transform.position = new Vector3(pv.x + 0.3f, pv.y, pc.transform.position.z); if (phm.nowHp < lastHp) hits.Add(lastHp - phm.nowHp); lastHp = phm.nowHp; }
 if (now < until) return;
 try {
  if (st == 0 || st == 3) { if (st == 3) { sb.AppendLine("일반 hits=" + string.Join(",", hits.ConvertAll(x => x.ToString()).ToArray())); foe.isBossMob = false; foe.PromoteElite(); foe.isBossMob = true; sb.AppendLine("elite=" + foe.IsElite); hits.Clear(); }
    phm.fullHP = 99999; phm.nowHp = 99999; lastHp = 99999; node.OnStart(); until = now + 0.5; st++; return; }
  if (st == 1 || st == 4) { until = now + 4; st++; return; }
  if (st == 2) { st = 3; until = now; return; }
  if (st == 5) { sb.AppendLine("정예 hits=" + string.Join(",", hits.ConvertAll(x => x.ToString()).ToArray())); st = 9; }
 } catch (Exception e) { sb.AppendLine("ERR " + e); st = 9; }
 if (st == 9) { System.IO.File.WriteAllText("Logs/pv10/b.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; st = 10; } };
UnityEditor.EditorApplication.update += upd; return "started";
