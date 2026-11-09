// 모든 프리팹의 Skill.skillName/skillDetail 덤프 (읽기 전용)
var sb = new System.Text.StringBuilder();
var seen = new System.Collections.Generic.HashSet<string>();
foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
{
    string p = AssetDatabase.GUIDToAssetPath(g);
    if (p.Contains("/ThirdParty/") || p.Contains("/Plugins/")) continue;
    var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
    if (go == null) continue;
    foreach (var s in go.GetComponentsInChildren<Skill>(true))
    {
        string key = s.skillName + "|" + s.skillDetail;
        if (seen.Add(key)) sb.AppendLine(System.IO.Path.GetFileName(p) + "\t" + s.skillName + "\t" + (s.skillDetail ?? "").Replace("\n", "\n"));
    }
}
System.IO.File.WriteAllText("Logs/skill_texts.tsv", sb.ToString());
return "ok " + seen.Count;
