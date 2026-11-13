var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var sb = new System.Text.StringBuilder();
foreach (var z in UnityEngine.Object.FindObjectsByType<ZoneReportSign>(FindObjectsInactive.Include, FindObjectsSortMode.None)) sb.Append(z.transform.parent.name + "@" + z.transform.position + " act=" + z.gameObject.activeInHierarchy + "; ");
return sb + " player=" + pc.transform.position;
