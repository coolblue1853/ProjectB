var p = PlayerController.instance;
if (p == null) return "no player";
var sb = new System.Text.StringBuilder();
foreach (var n in new[]{"Rabbit_BD","WildBoar_BD","SporeShroom_BD","Scarecrow_BD"}) {
  var pf = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/" + n + ".prefab");
  var go = UnityEngine.Object.Instantiate(pf, p.transform.position + new Vector3(4f, 0.5f, 0f), Quaternion.identity);
  go.name = n;
  var eh = go.GetComponent<EnemyHealth>();
  var bc = go.GetComponent<BoxCollider2D>();
  var s = go.transform.lossyScale;
  sb.AppendLine(n + " beh=" + (eh.Behavior == null ? "null" : eh.Behavior.Move + "/" + eh.Behavior.Aggro + "/chase" + eh.Behavior.ChasesPlayer) + " world " + (bc.size.x*Mathf.Abs(s.x)).ToString("0.00") + "x" + (bc.size.y*Mathf.Abs(s.y)).ToString("0.00"));
}
return sb.ToString();
