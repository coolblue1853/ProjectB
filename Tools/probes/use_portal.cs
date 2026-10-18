// openMap 이름에 KEY가 들어간 포털을 코드로 작동 (입력 없이 Portal.Update와 같은 처리)
var key = "__KEY__";
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Exclude))
{
    if (p.openMap == null || p.destination == null || p.openMap.name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
    pc.transform.position = p.destination.position;
    typeof(Portal).GetMethod("AbleOpenMap", flags).Invoke(p, null);
    typeof(Portal).GetMethod("AbleCloseMap", flags).Invoke(p, null);
    if (p.proCamera != null) p.proCamera.CenterOnTargets();
    return "portal " + p.name + " -> " + p.openMap.name + " (closed " + (p.closeMap ? p.closeMap.name : "-") + ") player=" + pc.transform.position;
}
var sb = new System.Text.StringBuilder("not found. portals: ");
foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Include)) sb.Append(p.name + "[" + (p.openMap ? p.openMap.name : "null") + "] ");
return sb.ToString();
