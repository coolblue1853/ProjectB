var sb = new System.Text.StringBuilder();
foreach (var g in UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[]{"Assets/Prefab/Enemy_BD"})) {
  var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
  var bc = go.GetComponent<BoxCollider2D>();
  if (bc == null) { sb.AppendLine(go.name + " noBox"); continue; }
  var s = go.transform.lossyScale;
  sb.AppendLine(go.name + " world " + (bc.size.x*Mathf.Abs(s.x)).ToString("0.00") + "x" + (bc.size.y*Mathf.Abs(s.y)).ToString("0.00") + " scale " + s.x.ToString("0.00") + " trig " + bc.isTrigger + " layer " + go.layer);
}
var p = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
if (p != null) { var pc = p.GetComponent<Collider2D>(); sb.AppendLine("player " + (pc != null ? pc.bounds.size.ToString() : "nocol")); }
return sb.ToString();
