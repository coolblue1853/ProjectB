var sb = new System.Text.StringBuilder();
foreach (var sr in UnityEngine.Object.FindObjectsByType<UnityEngine.SpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
  if (sr.sprite == null) continue;
  var root = sr.transform.root.name;
  if (root.IndexOf("Player", System.StringComparison.OrdinalIgnoreCase) < 0 && root.IndexOf("Adventurer", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
  sb.AppendLine(sr.name + " sprite " + sr.sprite.name + " px " + sr.sprite.rect.size + " ppu " + sr.sprite.pixelsPerUnit + " worldBounds " + sr.bounds.size + " scale " + sr.transform.lossyScale);
  if (sb.Length > 1200) break;
}
return sb.ToString();
