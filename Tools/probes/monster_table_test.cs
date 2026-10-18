// 플레이 중 가장 가까운 몬스터의 maxHP/Def 가 MonsterSheet 행과 같은지
var pc = PlayerController.instance; EnemyHealth t = null; float bd = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) { var d = Vector2.Distance(e.transform.position, pc.transform.position); if (d < bd) { bd = d; t = e; } }
if (t == null) return "no enemy";
var r = MonsterTable.Get(t.name);
return t.name + " hp=" + t.maxHP + " def=" + t.enemyDef + " | row=" + (r != null ? r.maxHP + "/" + r.enemyDef + " floor" + r.floor : "none") + " rows=" + System.Linq.Enumerable.Count(MonsterTable.All());
