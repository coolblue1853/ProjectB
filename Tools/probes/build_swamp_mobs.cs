// ⚠ Q-415(2026-10-19) 이후 몹 HP/방어·무기 피해·칭호(grantsRank)는 MonsterSheet.csv · apply_zone_tier_weapons.cs · Resources/Boss 가 기준이다. 이 프로브를 다시 돌리면 옛 값으로 되돌아가므로, 재실행 뒤 apply_zone_tier_weapons.cs 를 다시 돌리고 grantsRank(엔트 1·문지기 2·AbyssWizard 3)를 확인할 것.
// 1층계 ④구역 '늪지' 몬스터 (gen_swamp_mobs.py 시트 → 프레임 크기별 슬라이스 → MonsterBuilder). 에디트 모드, 재실행 안전
// 낮: 개구리(RatThief 템플릿)·꽃게(RatThief, 방어 높음)·악어(RatThief, 64px) / 밤: 모기(FlowerMole=비행)·슬라임(LeftMole=느림) / 보스: 거대 슬라임(PassiveTerror)
var reimportSince = System.DateTime.Now.AddSeconds(-1); // B-139: 끝에서 이 시각 이후 저장된 프리팹·컨트롤러 재임포트
var sb = new System.Text.StringBuilder();
string root = "Assets/Art/Monsters/";
AssetDatabase.Refresh();
var sizes = new System.Collections.Generic.Dictionary<string, int>{ {"SwampFrog",48}, {"MarshCrab",48}, {"BogCroc",64}, {"SwampMosquito",48}, {"BogSlime",48}, {"GiantSlime",96} };
foreach (var kv in sizes)
foreach (var anim in new[]{ "Idle", "Run", "Attack1", "Attack2", "Take Hit", "Death" })
{
    string path = root + kv.Key + "/" + anim + ".png";
    var imp = AssetImporter.GetAtPath(path) as TextureImporter;
    if (imp == null) { if (anim != "Attack2") sb.Append("missing " + path + "; "); continue; }
    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    int fs = tex.height, n = tex.width / fs; // 시트는 한 줄 → 프레임 = 정사각형(높이)
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
System.Func<string, string, int, int, bool, bool, string[], string> build = (name, tpl, hp, def, armor, boss, map) => {
    var S = new ProjectB.EditorTools.MonsterBuilder.Spec();
    S.Name = name + "_BD"; S.Template = tpl; S.SheetFolder = root + name; S.MaxHp = hp; S.Def = def; S.SuperArmor = armor; S.Boss = boss;
    if (boss) S.WorldPxPerUnit = 18f; // 보스는 일반 몹보다 1.33배 크게 (약 5유닛)
    for (int i = 0; i + 1 < map.Length; i += 2) { if (map[i] == "death") S.DeathSheet = map[i + 1]; else S.ClipMap[map[i]] = map[i + 1]; }
    return ProjectB.EditorTools.MonsterBuilder.Build(S); };
sb.AppendLine(build("SwampFrog", "RatThief_BD", 260, 6, false, false, new[]{ "RatThiefStay","Idle","RatThiefWalk","Run","RatThiefHit","Take Hit","RatThiefAttack","Attack1","death","Death" }));
sb.AppendLine(build("MarshCrab", "RatThief_BD", 300, 14, false, false, new[]{ "RatThiefStay","Idle","RatThiefWalk","Run","RatThiefHit","Take Hit","RatThiefAttack","Attack1","death","Death" }));
sb.AppendLine(build("BogCroc", "RatThief_BD", 520, 10, false, false, new[]{ "RatThiefStay","Idle","RatThiefWalk","Run","RatThiefHit","Take Hit","RatThiefAttack","Attack1","death","Death" }));
sb.AppendLine(build("SwampMosquito", "FlowerMole_BD", 150, 4, false, false, new[]{ "FlowerMoleStay","Idle","FlowerMoleWalk","Run","FlowerMoleHit","Take Hit","FlowerMoleAttack","Attack1","death","Death" }));
sb.AppendLine(build("BogSlime", "LeftMole_BD", 260, 8, false, false, new[]{ "LeafMoleStay","Idle","LeafMoleWalk","Run","LeafMoleHit","Take Hit","LeafMoleAttack","Attack1","death","Death" }));
sb.AppendLine(build("GiantSlime", "PassiveTerror_BD", 8000, 20, true, true, new[]{ "PT_Stay","Idle","PT_Walk","Run","ScaryCrowHit","Take Hit","PTAttackOne","Attack1","PTAttackTwo","Attack2","death","Death" }));

// 밤 몹 발광: 피격 머티리얼 교체와 무관하게 Light2D 로 (wisp_glow.cs 와 같은 방식)
System.Action<string, Color, float, float> glow = (mob, c, inten, rad) => {
    string path = "Assets/Prefab/Enemy_BD/" + mob + "_BD.prefab";
    var pr = PrefabUtility.LoadPrefabContents(path);
    var old = pr.transform.Find(mob + "Glow"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    var go = new GameObject(mob + "Glow"); go.transform.SetParent(pr.transform, false);
    var l = go.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
    l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
    l.shadowsEnabled = false; { var lso = new SerializedObject(l); lso.FindProperty("m_NormalMapQuality").intValue = (int)UnityEngine.Rendering.Universal.Light2D.NormalMapQuality.Disabled; lso.ApplyModifiedPropertiesWithoutUndo(); } // 그림자·노멀맵 안 씀 (밤에 다수)
    l.color = c; l.intensity = inten; l.pointLightOuterRadius = rad; l.pointLightInnerRadius = 0.2f;
    PrefabUtility.SaveAsPrefabAsset(pr, path); PrefabUtility.UnloadPrefabContents(pr);
};
glow("SwampMosquito", new Color(1f, 0.45f, 0.4f), 0.35f, 1.6f); // 밤 습지에서 보이게 약한 붉은 빛
glow("BogSlime", new Color(0.6f, 1f, 0.65f), 0.35f, 2f);       // 밤 몹: 몸 속 핵이 희미하게 빛난다
glow("GiantSlime", new Color(0.75f, 1f, 0.5f), 0.55f, 4.5f);   // 밤 보스: 어둠 속 실루엣이 보이게
sb.Append(" reimported=" + ProjectB.EditorTools.MonsterBuilder.ReimportModifiedSince(reimportSince));
return sb.ToString();
