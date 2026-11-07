var sb = new System.Text.StringBuilder();
var m = UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
foreach (var mi in m) {
  if (mi.name != "Lv1_NearForest_SideRoad" && mi.name != "Lv1_Swamp") continue;
  foreach (var tm in mi.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true)) {
    UnityEngine.Sprite sp = null;
    foreach (var p in tm.cellBounds.allPositionsWithin) { var t = tm.GetTile<UnityEngine.Tilemaps.Tile>(p); if (t != null && t.sprite != null) { sp = t.sprite; break; } }
    sb.AppendLine(mi.name + "/" + tm.name + " cellSize " + tm.cellSize + " sprite " + (sp != null ? sp.name + " " + sp.rect.size + " ppu " + sp.pixelsPerUnit : "none"));
  }
}
var cam = UnityEngine.Camera.main;
sb.AppendLine("cam ortho " + (cam != null ? cam.orthographicSize.ToString() : "null") + " screenH " + UnityEngine.Screen.height);
return sb.ToString();
