var sb = new System.Text.StringBuilder();
foreach (var t in UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) if (t.text != null && t.text.Contains("수호자")) sb.Append("[" + t.name + ":" + t.text + "] ");
var m = new System.Text.StringBuilder(); foreach (var mi in UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) m.Append(mi.name+" ");
return "banner=" + (sb.Length==0?"none":sb.ToString()) + " maps=" + m + " pos=" + PlayerController.instance.transform.position.ToString("F1");
