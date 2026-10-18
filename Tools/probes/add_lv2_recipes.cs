// Lv2 제작 레시피를 대장장이/자가제작 UI 빈칸에 추가 (편집 모드, 씬 저장). 이미 있으면 재료만 갱신
if (EditorApplication.isPlaying) return "stop play first";
var recipes = new System.Collections.Generic.List<object[]> {
  new object[]{ "armor", "CliffCap", new[]{"T1_Ore","Wood","Glowing Spore","Cliff Fox Pelt"}, new[]{8,10,12,6} },
  new object[]{ "armor", "CliffArmor", new[]{"T1_Ore","Wood","Ancient Bone","Cracked Shield Shard"}, new[]{15,15,12,4} },
  new object[]{ "armor", "CliffLegArmor", new[]{"T1_Ore","Wood","Goblin Fang","Cliff Fox Pelt"}, new[]{8,10,12,6} },
  new object[]{ "armor", "CliffHandArmor", new[]{"T1_Ore","Wood","Membrane Wing","Goblin Fang"}, new[]{8,8,10,6} },
  new object[]{ "armor", "CliffShoes", new[]{"T1_Ore","Wood","Cliff Fox Pelt","Spongy Mushroom Cap"}, new[]{6,8,10,6} },
  new object[]{ "armor", "Eye of the Cliffs", new[]{"Bloodshot Lens","Membrane Wing","T1_Ore"}, new[]{10,10,5} },
  new object[]{ "armor", "Bringer's Sigil Ring", new[]{"Death Sigil","T1_Ore"}, new[]{8,10} },
  new object[]{ "weapon", "CliffBlade", new[]{"T1_Ore","Wood","Goblin Fang","Tarnished Goblin Coin"}, new[]{15,10,15,5} },
  new object[]{ "weapon", "CliffStaff", new[]{"T1_Ore","Wood","Glowing Spore","Bloodshot Lens"}, new[]{12,20,15,4} },
  new object[]{ "weapon", "AbyssStaff", new[]{"Abyssal Core","Death Sigil","T1_Ore"}, new[]{3,5,30} },
  new object[]{ "self", "Roasted Fox Skewer", new[]{"Fox Meat","Wood"}, new[]{2,1} },
};
CraftingManager armor = null, weapon = null, self = null;
foreach (var cm in UnityEngine.Object.FindObjectsByType<CraftingManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
{ if (cm.isSelfCraft) self = cm; else if (cm.isCraftWeapon) weapon = cm; else armor = cm; }
// 이전 실행에서 잘못 들어간 칸 정리: 결과물 이름과 UI 종류가 맞지 않는 Lv2 칸 삭제
foreach (var r in recipes)
{
    var want = (string)r[0] == "armor" ? armor : (string)r[0] == "weapon" ? weapon : self;
    foreach (var cm2 in new[]{ armor, weapon, self })
    {
        if (cm2 == null || cm2 == want) continue;
        foreach (var ni in cm2.LV1craftBox[0].GetComponentsInChildren<NeedItem>(true)) if (ni.name == (string)r[1]) UnityEngine.Object.DestroyImmediate(ni.gameObject);
    }
}
var sb = new System.Text.StringBuilder();
foreach (var r in recipes)
{
  try {
    var cm = (string)r[0] == "armor" ? armor : (string)r[0] == "weapon" ? weapon : self;
    var name = (string)r[1];
    NeedItem existing = null;
    foreach (var ni in cm.LV1craftBox[0].GetComponentsInChildren<NeedItem>(true)) if (ni.name == name) existing = ni;
    if (existing == null)
    {
        // 가장 긴 줄 길이를 한 줄 최대치로 보고, 빈자리가 있는 첫 줄에 복제
        var box = cm.LV1craftBox[0].transform; int maxPer = 0; Transform template = null;
        foreach (Transform page in box) foreach (Transform line in page) { maxPer = Mathf.Max(maxPer, line.childCount); if (template == null && line.childCount > 0) template = line.GetChild(0); }
        Transform target = null;
        foreach (Transform page in box) { foreach (Transform line in page) if (line.childCount < maxPer) { target = line; break; } if (target) break; }
        if (target == null) { sb.Append("[칸 부족 " + name + "] "); continue; }
        var go = (GameObject)UnityEngine.Object.Instantiate(template.gameObject, target);
        go.name = name;
        existing = go.GetComponent<NeedItem>();
        sb.Append("+" + name + "@" + target.parent.name + "/" + target.name + " ");
    }
    existing.needItem = new System.Collections.Generic.List<string>((string[])r[2]);
    existing.needItemAmount = new System.Collections.Generic.List<int>((int[])r[3]);
    EditorUtility.SetDirty(existing);
  } catch (System.Exception e) { sb.Append("[ERR " + r[1] + ": " + e.Message + " armor=" + (armor!=null) + " weapon=" + (weapon!=null) + " self=" + (self!=null) + "] "); }
}
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return sb.ToString();
