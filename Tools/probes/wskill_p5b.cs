var sb = new System.Text.StringBuilder();
for (int i = 0; i < 5; i++) { SkillTreeService.Invest("Sword", "P1"); }
SkillTreeService.Invest("Sword", "P2"); SkillTreeService.Invest("Sword", "P2");
foreach (var tab in new[] { SkillTab.Life, SkillTab.Convenience, SkillTab.Type })
{
    SkillTreeWindow.OpenAt(tab);
    sb.AppendLine("== " + tab);
    foreach (var t in UnityEngine.Object.FindObjectsByType<TMPro.TextMeshProUGUI>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None))
        if (t.transform.root.name == "[SkillTreeWindow]" && (t.name == "Effect" || t.name == "Hint" || t.name == "Header") && !string.IsNullOrEmpty(t.text)) sb.AppendLine("  " + t.name + ": " + t.text);
}
return sb.ToString();
