var sb = new System.Text.StringBuilder();
foreach (var m in UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  var tms = m.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true);
  var tiles = new System.Collections.Generic.Dictionary<string,int>();
  int cells = 0;
  foreach (var tm in tms) { foreach (var p in tm.cellBounds.allPositionsWithin) { var t = tm.GetTile(p); if (t != null) { cells++; string k = t.name; tiles[k] = tiles.ContainsKey(k) ? tiles[k] + 1 : 1; } } }
  var srs = m.GetComponentsInChildren<SpriteRenderer>(true);
  var spr = new System.Collections.Generic.Dictionary<string,int>();
  foreach (var s in srs) { if (s.sprite == null) continue; string k = s.sprite.texture.name; spr[k] = spr.ContainsKey(k) ? spr[k] + 1 : 1; }
  sb.Append(m.name + " | tilemaps " + tms.Length + " cells " + cells + " | tiles ");
  int n = 0; foreach (var kv in tiles) { if (n++ < 6) sb.Append(kv.Key + "x" + kv.Value + " "); }
  sb.Append("| sprites " + srs.Length + " tex: ");
  n = 0; foreach (var kv in spr) { if (n++ < 8) sb.Append(kv.Key + "x" + kv.Value + " "); }
  sb.AppendLine();
}
return sb.ToString();
