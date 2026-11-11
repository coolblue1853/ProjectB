var sb = new System.Text.StringBuilder();
foreach (var n in new[]{"WildBoar_BD","SporeShroom_BD","Scarecrow_BD","Rabbit_BD"}) {
  var go = GameObject.Find(n); if (go == null) { sb.AppendLine(n + " gone"); continue; }
  var eh = go.GetComponent<EnemyHealth>();
  sb.AppendLine(n + " latched=" + eh.ChaseLatched + " provoked=" + eh.IsProvoked + " dx=" + (PlayerController.instance.transform.position.x - go.transform.position.x).ToString("0.0") + " x=" + go.transform.position.x.ToString("0.0"));
}
return sb.ToString();
