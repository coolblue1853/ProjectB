// soak 한 스텝: 활성 스포너 중 무작위로 이동 → 근처 몬스터 타격·처치 → 무기 평타·스킬 → 드롭 줍기(플레이어를 드롭 위치로)
var rnd = new System.Random();
var pc = PlayerController.instance;
var sb = new System.Text.StringBuilder();
var sps = UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
if (sps.Length > 0 && rnd.NextDouble() < 0.4)
{
    var sp = sps[rnd.Next(sps.Length)];
    var col = sp.GetComponent<Collider2D>();
    var p = col != null ? (Vector3)col.bounds.center : sp.transform.position;
    pc.transform.position = new Vector3(p.x, p.y, 0);
    sb.Append("tp " + sp.name + "; ");
}
var w = UnityEngine.Object.FindFirstObjectByType<Weapon>();
if (w != null)
{
    if (w.CanMeleeNow) { w.MeleeAttack(); sb.Append("melee; "); }
    int slot = rnd.Next(4);
    try { w.ActiveWeaponSkill(slot); sb.Append("skill" + slot + "; "); } catch (System.Exception e) { sb.Append("skillErr " + e.GetType().Name + "; "); Debug.LogError("[PB][Soak] ActiveWeaponSkill(" + slot + ") " + e); }
}
int hits = 0;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
{
    if (e.nowHP <= 0 || Vector2.Distance(e.transform.position, pc.transform.position) > 12f) continue;
    var arr = new int[] { 5, 12, 30, 20, 10, 20, 10, 5, 10, 0 };
    e.damage2Enemy(arr, 0.2f, 3f, new Vector2(1, 1), 1f, false, rnd.NextDouble() < 0.3, 0.05f, 1f);
    if (rnd.NextDouble() < 0.2) e.CreatPoisonPrefab(2, 0.3f, 3);
    hits++;
    if (hits >= 4) break;
}
sb.Append("hits=" + hits + "; ");
var drops = UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
if (drops.Length > 0 && rnd.NextDouble() < 0.5) { pc.transform.position = drops[0].transform.position; sb.Append("goto drop " + drops[0].itemData.name + "; "); }
sb.Append("inv Wood=" + InventoryManager.instance.CountItem("Wood") + " status=" + PlayerStatus.Dump().Replace("\n", " ").Trim());
return sb.ToString();
