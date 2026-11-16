var pl = UnityEngine.Object.FindObjectsByType<PickupLog>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
var sb = new System.Text.StringBuilder();
foreach (var p in pl) { var f = typeof(PickupLog).GetField("fullText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic); var t = (TMPro.TMP_Text)f.GetValue(p); sb.Append("text='" + t.text + "' active=" + t.gameObject.activeSelf + " alpha=" + t.alpha); }
return sb.ToString();
