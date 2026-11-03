var sb = new System.Text.StringBuilder();
foreach (var m in UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  if (m.name != "Lv1_Swamp" && m.name != "Lv1_BanditFortress" && m.name != "Lv1_TowerOutskirts") continue;
  sb.AppendLine("== " + m.name + " z=" + m.transform.position.z);
  foreach (var p in m.GetComponentsInChildren<Portal>(true)) sb.AppendLine(" portal " + p.name + " " + p.transform.position.ToString("F1"));
  foreach (var s in m.GetComponentsInChildren<EnemySpawner>(true)) { var b = s.GetComponent<BoxCollider2D>(); sb.AppendLine(" sp " + s.name + " day=" + s.isDaytimeSpowner + " " + (b != null ? b.bounds.ToString("F0") : "nobox") + " prefab=" + (s.EnemyPrefab != null ? s.EnemyPrefab.name : "-")); }
  foreach (var t in m.GetComponentsInChildren<TrackingSpot>(true)) sb.AppendLine(" track " + t.name + " " + t.transform.position.ToString("F1"));
  foreach (var c in m.GetComponentsInChildren<Collider2D>(true)) { if (c.isTrigger) continue; if (c.bounds.size.x > 25f) sb.AppendLine(" ground " + c.name + " " + c.bounds.ToString("F0")); }
}
return sb.ToString();
