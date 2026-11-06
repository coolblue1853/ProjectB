CombatHud.NotifyLocked(1, 3);
var sb = new System.Text.StringBuilder("visible=" + CombatHud.NoticeVisible);
foreach (var t in UnityEngine.Object.FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (t.text != null && t.text.Contains("3")&& t.text.Contains("[S]")) { sb.Append(" | " + t.name + " active=" + t.gameObject.activeInHierarchy + " a=" + t.alpha + " pos=" + t.rectTransform.position + " '" + t.text + "'"); } }
return sb.ToString();
