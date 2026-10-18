// 계보 무기 4호 Shell Shield (TowerWorld 12.1): 숲달팽이 → 검+방패. RustedSwordandShield 복제.
// Shell Pierce / Shell Bash(넉백 1.5배) / Shell Guard(홀드 방어 버프 100→180, 쿨 6s). 달팽이 드롭 2%. 에디트 모드, 재실행 안전
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/", F = DO + "ShellShield/";
System.IO.Directory.CreateDirectory(F);
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
AssetDatabase.ImportAsset("Assets/Resources/Item/ShellShield.png");
var imp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/ShellShield.png");
imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode; imp.SaveAndReimport();
string path = W + "ShellShield.prefab";
if (!System.IO.File.Exists(path)) AssetDatabase.CopyAsset(W + "RustedSwordandShield.prefab", path);
var root = PrefabUtility.LoadPrefabContents(path);
root.name = "ShellShield";
var w = root.GetComponent<Weapon>();
w.minDmg = 10; w.maxDmg = 16; w.energyColor = new Color(0.75f, 0.55f, 0.85f);
var names = new[]{ "Shell Pierce", "Shell Bash", "Shell Guard" };
var skills = root.GetComponentsInChildren<Skill>(true);
for (int i = 0; i < skills.Length && i < names.Length; i++)
{
    skills[i].skillName = names[i];
    if (i == 0) continue;
    var src = skills[i].skillprefab[0];
    string dst = F + "Shell" + src.name + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    var d = pr.GetComponent<DamageObject>();
    if (i == 1) { d.knockForce *= 1.5f; sb.Append("bash knock x1.5; "); }
    if (i == 2) { d.holdingBuffPower = 180; skills[i].SkillCoolTime = 6f; sb.Append("guard def 180; "); }
    foreach (var r in pr.GetComponentsInChildren<SpriteRenderer>(true)) r.color = new Color(0.85f, 0.7f, 0.95f, r.color.a);
    PrefabUtility.SaveAsPrefabAsset(pr, dst); PrefabUtility.UnloadPrefabContents(pr);
    var arr = skills[i].skillprefab; arr[0] = AssetDatabase.LoadAssetAtPath<GameObject>(dst); skills[i].skillprefab = arr;
}
foreach (var g in new[]{ w.RightWeqponSprite, w.leftWeqponSprite })
    if (g != null) foreach (var r in g.GetComponentsInChildren<SpriteRenderer>(true)) r.color = new Color(0.85f, 0.7f, 0.95f);
PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root);
var map = GameObject.Find("Map").transform.Find("Lv1_NormalForest");
int nd = 0;
foreach (var dm in map.GetComponentsInChildren<DropManager>(true))
{
    if (!(dm.name.StartsWith("RatCheif") || dm.name.StartsWith("CreepingTerror"))) continue;
    var so = new SerializedObject(dm); var arr = so.FindProperty("dropItems");
    bool has = false; for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).FindPropertyRelative("itemName").stringValue == "ShellShield") has = true;
    if (has) continue;
    arr.arraySize++; var e = arr.GetArrayElementAtIndex(arr.arraySize - 1);
    e.FindPropertyRelative("itemName").stringValue = "ShellShield"; e.FindPropertyRelative("dropProbability").floatValue = 0.02f; e.FindPropertyRelative("offsetY").floatValue = 0.5f;
    so.ApplyModifiedPropertiesWithoutUndo(); nd++;
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return sb.ToString() + "drops+" + nd;
