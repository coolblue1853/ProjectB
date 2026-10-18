// ⑤구역 산적 성채 계보 세트 (TowerWorld 15절, LineageGear 2절): Outlaw(OutlawArmor: 단검병·궁수 + Cutthroat Dagger·Bandit Shortbow) /
// Iron Raider(RaiderArmor: 방패병·횃불 순찰병 + Raider Buckler). build_swamp_sets.cs 와 같은 방식.
// Leaf 방어구 5부위 프리팹 복제 + 스탯, 아이콘·착용 텍스처 임포트(make_forest_sets.py Outlaw Raider 결과). 부위 드롭(0.3%)은 build_bandit_fortress.cs 가 넣는다. 에디트 모드, 재실행 안전
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
// 부위별 (armor, hp, crit, move) — Outlaw = 치명·이동(기습), Raider = 방어·HP(쇠 방패)
int[,] outS = { { 16, 150, 4, 0 }, { 28, 360, 0, 0 }, { 16, 150, 0, 3 }, { 14, 130, 6, 0 }, { 14, 110, 0, 6 } };
int[,] raidS = { { 26, 190, 0, 0 }, { 44, 440, 0, 0 }, { 26, 190, 0, 0 }, { 20, 150, 0, 0 }, { 20, 130, 0, 3 } };
foreach (var key in new[]{ "Outlaw", "Raider" })
{
    var st = key == "Outlaw" ? outS : raidS;
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
