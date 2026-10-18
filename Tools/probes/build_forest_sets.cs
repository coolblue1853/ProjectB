// ②구역 계보 세트 (TowerWorld 12.1): Pack Hunter(WolfArmor: 늑대·멧돼지) / Night Lantern(WispArmor: 도깨비불·달팽이).
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
int[,] wolf = { { 10, 80, 0, 0 }, { 20, 220, 0, 0 }, { 10, 80, 0, 0 }, { 8, 80, 4, 0 }, { 8, 60, 0, 6 } };
int[,] wisp = { { 14, 100, 0, 0 }, { 26, 280, 0, 0 }, { 14, 100, 0, 0 }, { 10, 100, 2, 0 }, { 10, 80, 0, 3 } };
foreach (var key in new[]{ "Wolf", "Wisp" })
{
    var st = key == "Wolf" ? wolf : wisp;
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
// 드롭: 낮 늑대·멧돼지 → Wolf 부위, 밤 도깨비불·달팽이 → Wisp 부위 (부위별 0.3%)
var map = GameObject.Find("Map").transform.Find("Lv1_NormalForest");
int nd = 0;
foreach (var dm in map.GetComponentsInChildren<DropManager>(true))
{
    string n = dm.name; string key = null;
    if (n.StartsWith("Rabbit") || n.StartsWith("Mole")) key = "Wolf";
    else if (n.StartsWith("RatThief") || n.StartsWith("ScareCrow") || n.StartsWith("RatCheif") || n.StartsWith("CreepingTerror")) key = "Wisp";
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
