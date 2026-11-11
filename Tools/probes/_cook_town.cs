var sb = new System.Text.StringBuilder();
foreach (var m in UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (m.name == "Town") {
 sb.AppendLine("Town pos " + m.transform.position);
 foreach (UnityEngine.Transform t in m.transform) sb.AppendLine(t.name + " " + t.position + " " + t.gameObject.activeSelf);
}
return sb.ToString();
