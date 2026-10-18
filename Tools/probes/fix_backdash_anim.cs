// B-116: 너클 RSkill(Sway Back/Hoof Step)의 skillAnim "BackDash" 가 플레이어 AnyPortrait 에 없음 → 같은 무기 LSkill(대시) 애니로 교체
var sb = new System.Text.StringBuilder();
foreach (var n in new[]{ "Leather Knuckles", "TuskKnuckles" })
{
    string path = "Assets/Resources/Weapon/" + n + ".prefab";
    var root = PrefabUtility.LoadPrefabContents(path);
    var skills = root.GetComponentsInChildren<Skill>(true);
    string dashAnim = skills[0].skillAnim;
    foreach (var s in skills) if (s.skillAnim == "BackDash") { s.skillAnim = dashAnim; sb.Append(n + "/" + s.name + " -> " + dashAnim + "; "); }
    PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root);
}
return sb.ToString();
