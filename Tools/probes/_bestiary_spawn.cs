var sb = new System.Text.StringBuilder();
var set = new System.Collections.Generic.SortedDictionary<string, int>();
foreach (var s in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  var map = s.GetComponentInParent<MapInfo>(true);
  string mn = map != null ? map.name : "-";
  string key = mn + "/" + s.name + " day=" + s.isDaytimeSpowner + " self=" + s.gameObject.activeSelf + " ready=" + s.isReady + " prefab=" + (s.EnemyPrefab != null ? s.EnemyPrefab.name : "null") + " profile=" + s.MonsterProfile;
  if (mn.StartsWith("Lv1_NearForest") || s.MonsterProfile.Contains("Stone") || s.MonsterProfile.Contains("Goblin")) sb.AppendLine(key);
}
return sb.ToString();
