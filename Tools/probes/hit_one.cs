// 가장 가까운 몬스터에게 실제 무기 공격 경로(damage2Enemy)로 3회 타격하고 HP 변화를 기록
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
EnemyHealth t = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude))
{ var d = Vector2.Distance(e.transform.position, pc.transform.position); if (e.nowHP > 0 && d < best) { best = d; t = e; } }
if (t == null) return "no enemy";
var w = new int[] { 5, 8, 50, 30, 20, 0, 0, 10, 20, 0 };
var sb = new System.Text.StringBuilder(t.name + " def=" + t.enemyDef + " hp " + t.nowHP);
for (int i = 0; i < 3 && t.nowHP > 0; i++) { t.damage2Enemy(w, 0f, 0f, Vector2.zero, 0f, false, false, 0.05f, 1f); sb.Append(" -> " + t.nowHP); }
return sb.ToString();
