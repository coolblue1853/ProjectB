// 계보 무기 2호 Tusk Knuckles (TowerWorld 12.1): 멧돼지 → 너클. Leather Knuckles 복제 + 돌진(Dash) 판정을 넉백 2배 전용 복제로 교체.
// 아이콘 임포트, 멧돼지 스포너 드롭에 2% 추가. 에디트 모드, 재실행 안전
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/";
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
AssetDatabase.ImportAsset("Assets/Resources/Item/TuskKnuckles.png");
var imp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/TuskKnuckles.png");
imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode; imp.SaveAndReimport();

string path = W + "TuskKnuckles.prefab";
if (!System.IO.File.Exists(path)) AssetDatabase.CopyAsset(W + "Leather Knuckles.prefab", path);
var root = PrefabUtility.LoadPrefabContents(path);
root.name = "TuskKnuckles";
var w = root.GetComponent<Weapon>();
w.minDmg = 12; w.maxDmg = 18; w.critPer = 5; w.critDmg = 10;
w.energyColor = new Color(0.95f, 0.85f, 0.6f);
var names = new[]{ "Boar Charge", "Hoof Step", "Tusk Uppercut" };
var skills = root.GetComponentsInChildren<Skill>(true);
System.IO.Directory.CreateDirectory(DO + "TuskKnuckles");
for (int i = 0; i < skills.Length && i < names.Length; i++)
{
    skills[i].skillName = names[i];
    if (i == 0 || i == 2)
    {
        // 돌진·올려치기 판정: 원본 공유 프리팹을 복제해 넉백만 키운다 (다른 무기에 영향 없음)
        var src = skills[i].skillprefab[0];
        string dst = DO + "TuskKnuckles/Tusk" + src.name + ".prefab";
        if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dst);
        var pr = PrefabUtility.LoadPrefabContents(dst);
        var d = pr.GetComponent<DamageObject>();
        d.knockForce *= 2f; d.stiffnessTime = Mathf.Max(d.stiffnessTime, 0.4f);
        PrefabUtility.SaveAsPrefabAsset(pr, dst); PrefabUtility.UnloadPrefabContents(pr);
        var arr = skills[i].skillprefab; arr[0] = AssetDatabase.LoadAssetAtPath<GameObject>(dst); skills[i].skillprefab = arr;
        sb.Append(names[i] + " knock x2; ");
    }
}
// 무기 스프라이트를 상아·갈색 톤으로
foreach (var g in new[]{ w.RightWeqponSprite, w.leftWeqponSprite })
    if (g != null) foreach (var r in g.GetComponentsInChildren<SpriteRenderer>(true)) r.color = new Color(1f, 0.9f, 0.75f);
PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root);

var map = GameObject.Find("Map").transform.Find("Lv1_NormalForest");
int nd = 0;
foreach (var dm in map.GetComponentsInChildren<DropManager>(true))
{
    if (!dm.name.StartsWith("Mole")) continue;
    var so = new SerializedObject(dm); var arr = so.FindProperty("dropItems");
    bool has = false; for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).FindPropertyRelative("itemName").stringValue == "TuskKnuckles") has = true;
    if (has) continue;
    arr.arraySize++; var e = arr.GetArrayElementAtIndex(arr.arraySize - 1);
    e.FindPropertyRelative("itemName").stringValue = "TuskKnuckles"; e.FindPropertyRelative("dropProbability").floatValue = 0.02f; e.FindPropertyRelative("offsetY").floatValue = 0.5f;
    so.ApplyModifiedPropertiesWithoutUndo(); nd++;
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return sb.ToString() + "drops+" + nd;
