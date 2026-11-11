var sb = new System.Text.StringBuilder();
var root = GameObject.Find("Map");
if (root == null) { return "no Map root (not play?)"; }
foreach (Transform m in root.transform) {
  var ports = m.GetComponentsInChildren<Portal>(true);
  sb.Append(m.name + " active=" + m.gameObject.activeSelf + " portals:");
  foreach (var p in ports) { sb.Append(" [" + p.name + " " + p.transform.position.x.ToString("F1") + "," + p.transform.position.y.ToString("F1") + "]"); }
  sb.AppendLine();
}
return sb.ToString();
