var sb = new System.Text.StringBuilder();
Portal pick=null;
foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
 if (p.openMap != null && p.openMap.name == "Town" && p.closeMap != null) { sb.AppendLine(p.name + " from " + p.closeMap.name + " active=" + p.closeMap.activeInHierarchy); if (p.closeMap.activeInHierarchy) pick = p; }
if (pick != null) { bool ok = pick.Use(PlayerController.instance.gameObject); sb.AppendLine("use " + pick.name + "=" + ok); }
return sb.ToString();
