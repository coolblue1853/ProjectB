// 이름에 KEY가 들어간 활성 맵의 스포너 영역 중심으로 플레이어를 순간이동 (KEY는 sed로 치환)
var key = "__KEY__";
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
foreach (var sp in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude))
{
    if (sp.name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
    var col = sp.GetComponent<Collider2D>();
    var p = col != null ? (Vector3)col.bounds.center : sp.transform.position;
    pc.transform.position = new Vector3(p.x, p.y, 0);
    return "tp to " + sp.name + " path=" + sp.transform.parent.name + " pos=" + p + " bounds=" + (col ? col.bounds.size.ToString() : "-");
}
var names = new System.Text.StringBuilder();
foreach (var sp in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude)) names.Append(sp.name + ", ");
return "not found. active spawners: " + names;
