// 가장 가까운 살아있는 적 1마리를 처치하고, 처치 전후 드롭/시체 수를 기록
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
EnemyHealth target = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude))
{
    if (e.nowHP <= 0) continue;
    var d = Vector2.Distance(e.transform.position, pc.transform.position);
    if (d < best) { best = d; target = e; }
}
if (target == null) return "no enemy";
int drops = UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Exclude).Length;
int bodies = UnityEngine.Object.FindObjectsByType<DeadBody>(FindObjectsInactive.Exclude).Length;
var name = target.name;
target.onlyDamage2Enemy(99999);
return "killed " + name + " dist=" + best.ToString("F1") + " | before drops=" + drops + " bodies=" + bodies + " | targetActive=" + target.gameObject.activeSelf;
