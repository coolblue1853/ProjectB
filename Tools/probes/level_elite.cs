// 가장 가까운 적을 정예 승격 후 처치 → EXP 4배 로그
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
EnemyHealth target = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude))
{
    if (e.nowHP <= 0 || e.isBossMob) continue;
    var d = Vector2.Distance(e.transform.position, pc.transform.position);
    if (d < best) { best = d; target = e; }
}
if (target == null) return "no enemy";
target.PromoteElite();
int lv = LevelSystem.Level, ex = LevelSystem.Exp;
var n = target.name;
bool el = target.IsElite;
target.onlyDamage2Enemy(999999);
return n + " elite=" + el + " before Lv" + lv + " exp=" + ex;
