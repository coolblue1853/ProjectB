var sb = new System.Text.StringBuilder();
foreach (var s in UnityEngine.Object.FindObjectsByType<MiniBossSite>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None)) {
  sb.AppendLine(s.name + " alive=" + s.Alive);
  if (s.Alive) { var e = s.Enemy; sb.AppendLine(" name=" + e.name + " id=" + e.MonsterId + " HP " + e.nowHP + "/" + e.maxHP + " def " + e.enemyDef + " contact " + e.ContactDamage + " elite=" + e.IsElite + " mini=" + e.IsMiniBoss + " scale " + e.transform.localScale + " pos " + e.transform.position + " lvl " + e.Level); }
}
return sb.ToString();
