// 씬의 모든 포털 연결 상태 (편집 모드에서도 동작)
var sb = new System.Text.StringBuilder();
foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Include))
{
    Transform t = p.transform; string path = t.name; while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
    sb.AppendLine(path + " active=" + p.gameObject.activeInHierarchy + "/" + p.gameObject.activeSelf
        + " pos=" + p.transform.position
        + " dest=" + (p.destination ? p.destination.name + p.destination.position : "NULL")
        + " open=" + (p.openMap ? p.openMap.name : "NULL") + " close=" + (p.closeMap ? p.closeMap.name : "NULL")
        + " cam=" + (p.proCamera ? "ok" : "NULL"));
}
foreach (var pc in UnityEngine.Object.FindObjectsByType<PoolCheck>(FindObjectsInactive.Include)) sb.AppendLine("PoolCheck on " + pc.name);
var map = GameObject.Find("Map");
if (map) foreach (Transform c in map.transform) sb.AppendLine("map child: " + c.name + " active=" + c.gameObject.activeSelf + " pos=" + c.position);
return sb.ToString();
