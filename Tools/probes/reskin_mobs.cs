// 몹 외형 교체(ArtStyle.md): 64px GPT 시트로 다시 슬라이스 → MonsterBuilder 재빌드. 에디트 모드, 재실행 안전
// 기존 프리팹의 HP·방어·슈퍼아머·보스 값을 읽어 그대로 넘기므로 Q-415 수치가 되돌아가지 않는다.
// 사용: 아래 mobs 배열에 "이름:템플릿" 을 적고 실행. 템플릿 rat|flower|leaf|boss
var sb = new System.Text.StringBuilder();
string root = "Assets/Art/Monsters/";
var mobs = new[]{ "ForestWolf:rat", "WildBoar:rat", "ForestBear:rat", "Ent:leaf" };
// 프레임 크기 = 시트 높이(몹마다 64·84·128 등)
AssetDatabase.Refresh();
var maps = new System.Collections.Generic.Dictionary<string, string[]>();
maps["rat"] = new[]{ "RatThief_BD", "RatThiefStay","Idle","RatThiefWalk","Run","RatThiefHit","Take Hit","RatThiefAttack","Attack1","death","Death" };
maps["flower"] = new[]{ "FlowerMole_BD", "FlowerMoleStay","Idle","FlowerMoleWalk","Run","FlowerMoleHit","Take Hit","FlowerMoleAttack","Attack1","death","Death" };
maps["leaf"] = new[]{ "LeftMole_BD", "LeafMoleStay","Idle","LeafMoleWalk","Run","LeafMoleHit","Take Hit","LeafMoleAttack","Attack1","death","Death" };
maps["boss"] = new[]{ "PassiveTerror_BD", "PT_Stay","Idle","PT_Walk","Run","ScaryCrowHit","Take Hit","PTAttackOne","Attack1","PTAttackTwo","Attack2","death","Death" };
foreach (var entry in mobs)
{
    string mob = entry.Split(':')[0];
    string[] map = maps[entry.Split(':')[1]];
    foreach (var anim in new[]{ "Idle", "Run", "Attack1", "Attack2", "Take Hit", "Death" })
    {
        string path = root + mob + "/" + anim + ".png";
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { continue; }
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        int fs = tex.height;
        int n = tex.width / fs;
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.filterMode = FilterMode.Point; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false;
#pragma warning disable 618
        var metas = new SpriteMetaData[n];
        for (int i = 0; i < n; i++) { metas[i].name = anim.Replace(" ", "") + "_" + i; metas[i].rect = new Rect(i * fs, 0, fs, fs); metas[i].alignment = (int)SpriteAlignment.BottomCenter; }
        imp.spritesheet = metas;
#pragma warning restore 618
        imp.SaveAndReimport();
    }
    var old = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/" + mob + "_BD.prefab");
    var eh = old != null ? old.GetComponent<EnemyHealth>() : null;
    if (eh == null) { sb.AppendLine("no prefab/EnemyHealth " + mob); continue; }
    var S = new ProjectB.EditorTools.MonsterBuilder.Spec();
    S.Name = mob + "_BD"; S.Template = map[0]; S.SheetFolder = root + mob;
    S.MaxHp = eh.maxHP; S.Def = eh.enemyDef; S.SuperArmor = eh.isSuperArmor; S.Boss = eh.isBossMob;
    if (S.Boss) { S.WorldPxPerUnit = 18f; }
    for (int i = 1; i + 1 < map.Length; i += 2)
    {
        if (map[i] == "death") { S.DeathSheet = map[i + 1]; }
        else if (System.IO.File.Exists(root + mob + "/" + map[i + 1] + ".png")) { S.ClipMap[map[i]] = map[i + 1]; }
        else { S.ClipMap[map[i]] = "Attack1"; }
    }
    sb.AppendLine(ProjectB.EditorTools.MonsterBuilder.Build(S));
}
return sb.ToString();
