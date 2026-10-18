// 2층계 ②구역 무너진 성벽 계보 세트 (Floor2Plan 5.1, TowerWorld 17.1): Rampart Guard(RampartArmor: 성벽 기사·미늘창 파수병 + Rampart Sword·Sentry Halberd),
// Skybound(SkyArmor: 가고일·구름 정령 + Gargoyle Wing). build_outskirts_sets.cs 와 같은 방식: Leaf 방어구 5부위 프리팹 복제 + 스탯, 아이콘·착용 텍스처 임포트(make_forest_sets.py Rampart Sky 결과).
// 부위 드롭(0.3%)은 build_broken_rampart.cs 가 넣는다. 에디트 모드, 재실행 안전
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
// 부위별 (armor, hp, crit, move) — Rampart Guard = 기사 판금(방어·HP), Skybound = 가벼운 깃(치명·이동)
int[,] rampS = { { 34, 230, 0, 0 }, { 56, 540, 0, 0 }, { 34, 230, 0, 0 }, { 28, 180, 2, 0 }, { 28, 160, 0, 2 } };
int[,] skyS = { { 20, 120, 3, 2 }, { 34, 260, 3, 0 }, { 20, 120, 2, 3 }, { 16, 90, 4, 0 }, { 16, 80, 0, 5 } };
foreach (var key in new[]{ "Rampart", "Sky" })
{
    var st = key == "Rampart" ? rampS : skyS;
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
AssetDatabase.SaveAssets();
return sb.ToString();
