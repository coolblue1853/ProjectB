string s = "open=" + NpcQuestWindow.IsOpen;
foreach (var t in UnityEngine.Object.FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None)) if (t.gameObject.activeInHierarchy && t.text != null && (t.text.Contains("[-]") || t.text.Contains("[+]"))) s += "\n" + t.text;
return s;
