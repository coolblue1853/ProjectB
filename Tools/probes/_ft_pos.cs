var sb = new System.Text.StringBuilder();
foreach (var f in UnityEngine.Object.FindObjectsByType<FastTravelNpc>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  if (f.gameObject.activeInHierarchy) sb.AppendLine(f.name + " " + f.transform.position + " player " + PlayerController.instance.transform.position);
}
return sb.ToString();
