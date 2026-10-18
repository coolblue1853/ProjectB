// ⚠ Q-415(2026-10-19) 이후 몹 HP/방어·무기 피해·칭호(grantsRank)는 MonsterSheet.csv · apply_zone_tier_weapons.cs · Resources/Boss 가 기준이다. 이 프로브를 다시 돌리면 옛 값으로 되돌아가므로, 재실행 뒤 apply_zone_tier_weapons.cs 를 다시 돌리고 grantsRank(엔트 1·문지기 2·AbyssWizard 3)를 확인할 것.
// 1층계 ②구역 몬스터 (gen_forest_mobs.py 시트 → 48px 슬라이스 → MonsterBuilder). 에디트 모드, 재실행 안전
var sb = new System.Text.StringBuilder();
string root = "Assets/Art/Monsters/";
AssetDatabase.Refresh();
foreach (var mob in new[]{ "ForestWolf", "WildBoar", "Wisp", "ForestSnail" })
foreach (var anim in new[]{ "Idle", "Run", "Attack1", "Take Hit", "Death" })
{
    string path = root + mob + "/" + anim + ".png";
    var imp = AssetImporter.GetAtPath(path) as TextureImporter;
    if (imp == null) { sb.Append("missing " + path + "; "); continue; }
    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    int n = tex.width / 48;
    imp.textureType = TextureImporterType.Sprite;
    imp.spriteImportMode = SpriteImportMode.Multiple;
    imp.filterMode = FilterMode.Point; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false;
#pragma warning disable 618
    var metas = new SpriteMetaData[n];
    for (int i = 0; i < n; i++) { metas[i].name = anim.Replace(" ", "") + "_" + i; metas[i].rect = new Rect(i * 48, 0, 48, 48); metas[i].alignment = (int)SpriteAlignment.BottomCenter; }
    imp.spritesheet = metas;
#pragma warning restore 618
    imp.SaveAndReimport();
}
System.Func<string, string, int, int, string[], string> build = (name, tpl, hp, def, map) => {
    var S = new ProjectB.EditorTools.MonsterBuilder.Spec();
    S.Name = name + "_BD"; S.Template = tpl; S.SheetFolder = root + name; S.MaxHp = hp; S.Def = def;
    for (int i = 0; i + 1 < map.Length; i += 2) { if (map[i] == "death") S.DeathSheet = map[i + 1]; else S.ClipMap[map[i]] = map[i + 1]; }
    return ProjectB.EditorTools.MonsterBuilder.Build(S); };
sb.AppendLine(build("ForestWolf", "RatThief_BD", 140, 3, new[]{ "RatThiefStay","Idle","RatThiefWalk","Run","RatThiefHit","Take Hit","RatThiefAttack","Attack1","death","Death" }));
sb.AppendLine(build("WildBoar", "RatThief_BD", 220, 6, new[]{ "RatThiefStay","Idle","RatThiefWalk","Run","RatThiefHit","Take Hit","RatThiefAttack","Attack1","death","Death" }));
sb.AppendLine(build("Wisp", "FlowerMole_BD", 90, 2, new[]{ "FlowerMoleStay","Idle","FlowerMoleWalk","Run","FlowerMoleHit","Take Hit","FlowerMoleAttack","Attack1","death","Death" }));
sb.AppendLine(build("ForestSnail", "LeftMole_BD", 180, 10, new[]{ "LeafMoleStay","Idle","LeafMoleWalk","Run","LeafMoleHit","Take Hit","LeafMoleAttack","Attack1","death","Death" }));
return sb.ToString();
