var sb = new System.Text.StringBuilder();
foreach (var p in UnityEngine.Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
 if (p.closeMap != null && p.openMap != null && p.closeMap.name == "Town") sb.AppendLine(p.name + " -> " + p.openMap.name + " @" + p.destination.position.ToString("F1"));
var tc = GameObject.Find("Town"); sb.AppendLine("town active=" + (tc!=null && tc.activeInHierarchy) + " player=" + PlayerController.instance.transform.position.ToString("F1"));
return sb.ToString();
