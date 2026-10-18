// ③구역 계보 세트 (TowerWorld 12.2): Ancient Grove(GroveArmor: 곰·엔트 + Bear Claw·Root Staff) / Will-o'-Fae(FaeArmor: 유령·요정 + Phantom Scythe·Fae Wand).
// build_forest_sets.cs 와 같은 방식.
// Leaf 방어구 5부위 프리팹 복제 + 스탯, 아이콘·착용 텍스처 임포트(make_forest_sets.py 결과), 부위 드롭(각 0.3%). 에디트 모드, 재실행 안전
var sb = new System.Text.StringBuilder();
var refIcon = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/LeafCap.png");
var refTex = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Sprite/LeafArmor.png");
System.Action<string, TextureImporter> copyImp = (p, r) => {
    AssetDatabase.ImportAsset(p);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    var st = new TextureImporterSettings(); r.ReadTextureSettings(st); imp.SetTextureSettings(st);
    imp.textureCompression = r.textureCompression; imp.filterMode = r.filterMode; imp.maxTextureSize = r.maxTextureSize; imp.SaveAndReimport(); };
string[] parts = { "Cap", "Armor", "LegArmor", "HandArmor", "Shoes" };
string[] areas = { "Head", "Chest", "Leg", "Hand", "Shoes" };
// 부위별 (armor, hp, crit, move)
int[,] grove = { { 18, 140, 0, 0 }, { 32, 340, 0, 0 }, { 18, 140, 0, 0 }, { 14, 120, 3, 0 }, { 14, 100, 0, 4 } };
int[,] fae = { { 12, 100, 0, 0 }, { 24, 260, 0, 0 }, { 12, 100, 0, 0 }, { 10, 90, 5, 0 }, { 10, 80, 0, 8 } };
foreach (var key in new[]{ "Grove", "Fae" })
{
    var st = key == "Grove" ? grove : fae;
    copyImp("Assets/Resources/Sprite/" + key + "Armor.png", refTex);
    for (int i = 0; i < parts.Length; i++)
    {
        copyImp("Assets/Resources/Item/" + key + parts[i] + ".png", refIcon);
        string dir = "Assets/Resources/" + areas[i] + "/";
        string src = dir + "Leaf" + parts[i] + ".prefab", dst = dir + key + parts[i] + ".prefab";
        if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(src, dst);
        var root = PrefabUtility.LoadPrefabContents(dst);
        root.name = key + parts[i];
        var eq = root.GetComponent<Equipment>();
        if (eq != null)
        {
            eq.armor = st[i, 0]; eq.hp = st[i, 1]; eq.critical = st[i, 2]; eq.moveSpeed = st[i, 3];
            eq.basicDmg = 0; eq.criticalDmg = 0; eq.incDmg = 0; eq.attSpeed = 0; eq.poisonDmg = 0; eq.isBleeding = false;
        }
        PrefabUtility.SaveAsPrefabAsset(root, dst); PrefabUtility.UnloadPrefabContents(root);
    }
    sb.Append(key + " set; ");
}
// 드롭: 낮 곰·엔트 → Grove 부위, 밤 유령·요정 → Fae 부위 (부위별 0.3%)
var map = GameObject.Find("Map").transform.Find("Lv1_DeepForest");
int nd = 0;
foreach (var dm in map.GetComponentsInChildren<DropManager>(true))
{
    string n = dm.name; string key = null;
    if (n.StartsWith("Bear") || n.StartsWith("Ent")) key = "Grove";
    else if (n.StartsWith("Ghost") || n.StartsWith("Fairy")) key = "Fae";
    if (key == null) continue;
    var so = new SerializedObject(dm); var arr = so.FindProperty("dropItems");
    foreach (var part in parts)
    {
        string item = key + part; bool has = false;
        for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).FindPropertyRelative("itemName").stringValue == item) has = true;
        if (has) continue;
        arr.arraySize++; var e = arr.GetArrayElementAtIndex(arr.arraySize - 1);
        e.FindPropertyRelative("itemName").stringValue = item; e.FindPropertyRelative("dropProbability").floatValue = 0.003f; e.FindPropertyRelative("offsetY").floatValue = 0.5f;
    }
    so.ApplyModifiedPropertiesWithoutUndo(); nd++;
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
return sb.ToString() + "drop tables " + nd;
