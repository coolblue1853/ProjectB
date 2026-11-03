var sb = new System.Text.StringBuilder();
foreach (var m in UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  if (m.name != "Lv1_NormalForest" && m.name != "Lv1_DeepForest") continue;
  sb.AppendLine("== " + m.name);
  var xs = new System.Collections.Generic.List<string>();
  foreach (var s in m.GetComponentsInChildren<EnemySpawner>(true)) {
    if (s.enemyPositon == null) continue;
    var line = s.name + ": ";
    foreach (Transform c in s.enemyPositon.transform) line += c.position.x.ToString("F0") + "," + c.position.y.ToString("F1") + " ";
    sb.AppendLine(line);
  }
}
return sb.ToString();
