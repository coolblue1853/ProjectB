// 1층계 ③구역 '깊은 숲' 몬스터 (gen_deep_forest_mobs.py 시트 → 프레임 크기별 슬라이스 → MonsterBuilder). 에디트 모드, 재실행 안전
// 낮: 숲곰(지상 근접)·엔트(느린 탱커) / 밤: 유령(비행, 반투명)·요정(작은 비행, 빛) / 보스: 고대숲의 엔트(PassiveTerror 템플릿)
var reimportSince = System.DateTime.Now.AddSeconds(-1); // B-139: 끝에서 이 시각 이후 저장된 프리팹·컨트롤러 재임포트
var sb = new System.Text.StringBuilder();
string root = "Assets/Art/Monsters/";
AssetDatabase.Refresh();
var sizes = new System.Collections.Generic.Dictionary<string, int>{ {"ForestBear",64}, {"Ent",64}, {"Ghost",48}, {"Fairy",48}, {"AncientEnt",96} };
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
sb.AppendLine(build("ForestBear", "RatThief_BD", 400, 8, false, false, new[]{ "RatThiefStay","Idle","RatThiefWalk","Run","RatThiefHit","Take Hit","RatThiefAttack","Attack1","death","Death" }));
sb.AppendLine(build("Ent", "LeftMole_BD", 500, 10, false, false, new[]{ "LeafMoleStay","Idle","LeafMoleWalk","Run","LeafMoleHit","Take Hit","LeafMoleAttack","Attack1","death","Death" }));
sb.AppendLine(build("Ghost", "FlowerMole_BD", 160, 4, false, false, new[]{ "FlowerMoleStay","Idle","FlowerMoleWalk","Run","FlowerMoleHit","Take Hit","FlowerMoleAttack","Attack1","death","Death" }));
sb.AppendLine(build("Fairy", "FlowerMole_BD", 110, 3, false, false, new[]{ "FlowerMoleStay","Idle","FlowerMoleWalk","Run","FlowerMoleHit","Take Hit","FlowerMoleAttack","Attack1","death","Death" }));
sb.AppendLine(build("AncientEnt", "PassiveTerror_BD", 6000, 18, true, true, new[]{ "PT_Stay","Idle","PT_Walk","Run","ScaryCrowHit","Take Hit","PTAttackOne","Attack1","PTAttackTwo","Attack2","death","Death" }));

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
glow("Ghost", new Color(0.75f, 0.85f, 1f), 0.6f, 2.5f);   // 약하게
glow("Fairy", new Color(1f, 0.92f, 0.6f), 0.8f, 2.5f);
sb.Append(" reimported=" + ProjectB.EditorTools.MonsterBuilder.ReimportModifiedSince(reimportSince));
return sb.ToString();
