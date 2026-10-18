// 1층계 ⑥구역 '탑 근교' 몬스터 (gen_outskirts_mobs.py 시트 → 슬라이스 → MonsterBuilder). 에디트 모드, 재실행 안전
// 낮: 소형 골렘(RatThief 템플릿=빠른 근접, 구르기)·중형 골렘(LeftMole=느림, 지면 강타 판정 GolemQuake)·대형 골렘(FlowerMole=원거리, 슈퍼아머, 탄 → GolemBoulder)
// 밤: + 유적 정령(FlowerMole, 탄 → RuinBolt, 청록 Light2D) / 보스: 탑의 문지기(PassiveTerror, 128px, 판정 GateCleave·GateShockwave, 2페이즈 소형 골렘 호출)
// 보스·대형 골렘·정령·중형 골렘 BT 의 damageOb GUID 는 이 프로브 뒤에 텍스트로 교체한다(반환값에 GUID 출력)
var reimportSince = System.DateTime.Now.AddSeconds(-1); // B-139: 끝에서 이 시각 이후 저장된 프리팹·컨트롤러 재임포트
var sb = new System.Text.StringBuilder();
string root = "Assets/Art/Monsters/";
AssetDatabase.Refresh();
var sizes = new System.Collections.Generic.Dictionary<string, int>{ {"GolemSmall",48}, {"GolemMedium",64}, {"GolemLarge",80}, {"RuinSpirit",48}, {"Gatekeeper",128} };
foreach (var kv in sizes)
foreach (var anim in new[]{ "Idle", "Run", "Attack1", "Attack2", "Take Hit", "Death" })
{
    string path = root + kv.Key + "/" + anim + ".png";
    var imp = AssetImporter.GetAtPath(path) as TextureImporter;
    if (imp == null) { if (anim != "Attack2") sb.Append("missing " + path + "; "); continue; }
    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    int fs = tex.height, n = tex.width / fs;
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
    if (boss) S.WorldPxPerUnit = 18f;
    for (int i = 0; i + 1 < map.Length; i += 2) { if (map[i] == "death") S.DeathSheet = map[i + 1]; else S.ClipMap[map[i]] = map[i + 1]; }
    return ProjectB.EditorTools.MonsterBuilder.Build(S); };
string[] rat = { "RatThiefStay","Idle","RatThiefWalk","Run","RatThiefHit","Take Hit","RatThiefAttack","Attack1","death","Death" };
string[] flower = { "FlowerMoleStay","Idle","FlowerMoleWalk","Run","FlowerMoleHit","Take Hit","FlowerMoleAttack","Attack1","death","Death" };
string[] leaf = { "LeafMoleStay","Idle","LeafMoleWalk","Run","LeafMoleHit","Take Hit","LeafMoleAttack","Attack1","death","Death" };
sb.AppendLine(build("GolemSmall", "RatThief_BD", 360, 14, false, false, rat));
sb.AppendLine(build("GolemMedium", "LeftMole_BD", 620, 20, false, false, leaf));
sb.AppendLine(build("GolemLarge", "FlowerMole_BD", 950, 28, true, false, flower));
sb.AppendLine(build("RuinSpirit", "FlowerMole_BD", 170, 6, false, false, flower));
sb.AppendLine(build("Gatekeeper", "PassiveTerror_BD", 15000, 28, true, true, new[]{ "PT_Stay","Idle","PT_Walk","Run","ScaryCrowHit","Take Hit","PTAttackOne","Attack1","PTAttackTwo","Attack2","death","Death" }));

// 정령: 청록 빛(밤 유적의 작은 빛), 문지기: 바이저 룬 빛
System.Action<string, Color, float, float, Vector2> glow = (mob, c, inten, rad, off) => {
    string path = "Assets/Prefab/Enemy_BD/" + mob + "_BD.prefab";
    var pr = PrefabUtility.LoadPrefabContents(path);
    var old = pr.transform.Find(mob + "Glow"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    var go = new GameObject(mob + "Glow"); go.transform.SetParent(pr.transform, false); go.transform.localPosition = off;
    var l = go.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
    l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
    l.shadowsEnabled = false; { var lso = new SerializedObject(l); lso.FindProperty("m_NormalMapQuality").intValue = (int)UnityEngine.Rendering.Universal.Light2D.NormalMapQuality.Disabled; lso.ApplyModifiedPropertiesWithoutUndo(); }
    l.color = c; l.intensity = inten; l.pointLightOuterRadius = rad; l.pointLightInnerRadius = 0.2f;
    PrefabUtility.SaveAsPrefabAsset(pr, path); PrefabUtility.UnloadPrefabContents(pr);
};
glow("RuinSpirit", new Color(0.45f, 0.85f, 1f), 0.9f, 3f, new Vector2(0f, 0.6f));
glow("Gatekeeper", new Color(0.55f, 0.88f, 1f), 0.5f, 5.5f, new Vector2(0f, 2f));

// 보스 부하 호출: 2페이즈(HP 50%)에 소형 골렘 2 (풀 키 minion = GolemSmall_BD, build_tower_outskirts.cs 가 등록)
{
    string path = "Assets/Prefab/Enemy_BD/Gatekeeper_BD.prefab";
    var pr = PrefabUtility.LoadPrefabContents(path);
    var mc = pr.GetComponent<BossMinionCall>(); if (mc == null) mc = pr.AddComponent<BossMinionCall>();
    var mso = new SerializedObject(mc); mso.FindProperty("_bannerText").stringValue = "AWAKENED — The stones rise!"; mso.FindProperty("_count").intValue = 2; mso.ApplyModifiedPropertiesWithoutUndo();
    var et = pr.GetComponent<BossEncounterTalk>(); if (et == null) et = pr.AddComponent<BossEncounterTalk>();
    PrefabUtility.SaveAsPrefabAsset(pr, path); PrefabUtility.UnloadPrefabContents(pr);
    sb.Append("minionCall+encounterTalk; ");
}

// ── 판정 복제 (build_ent_boss_roots.cs 방식: 원본 공유 프리팹 미수정, 원본 컨트롤러 + OverrideController 로 클립만 교체) ──
string SRC = "Assets/Prefab/Enemy/EnemyDamge/", DST = SRC + "Outskirts/", ART = root + "Gatekeeper/";
System.IO.Directory.CreateDirectory(DST);
System.Func<string, int, float, Sprite[]> slice = (p, fs, ppu) => {
    AssetDatabase.ImportAsset(p);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
    int n = tex.width / fs;
    string nm = System.IO.Path.GetFileNameWithoutExtension(p);
    imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Multiple; imp.spritePixelsPerUnit = ppu;
    imp.filterMode = FilterMode.Point; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false;
#pragma warning disable 618
    var metas = new SpriteMetaData[n];
    for (int i = 0; i < n; i++) { metas[i].name = nm + "_" + i; metas[i].rect = new Rect(i * fs, 0, fs, fs); metas[i].alignment = (int)SpriteAlignment.Center; }
    imp.spritesheet = metas;
#pragma warning restore 618
    imp.SaveAndReimport();
    var list = new System.Collections.Generic.List<Sprite>();
    foreach (var o in AssetDatabase.LoadAllAssetsAtPath(p)) if (o is Sprite) list.Add((Sprite)o);
    list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
    return list.ToArray(); };
var cleave = slice(ART + "GateCleave.png", 64, 16f);
var wave = slice(ART + "GateShockwave.png", 32, 12.5f);
var quake = slice(root + "GolemMedium/QuakeBurst.png", 32, 12.5f);
var boulderSp = slice(root + "GolemLarge/Boulder.png", 32, 12f);
var boltSp = slice(root + "RuinSpirit/RuinBolt.png", 32, 12f);

System.Func<string, string, Sprite[], GameObject> make = (srcName, dstName, frames) => {
    string src = SRC + srcName + ".prefab", dst = DST + dstName + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(src, dst);
    var srcGo = AssetDatabase.LoadAssetAtPath<GameObject>(src);
    var baseCtrl = srcGo.GetComponent<Animator>().runtimeAnimatorController;
    var srcClip = baseCtrl.animationClips[0];
    string clipPath = DST + dstName + ".anim";
    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
    if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
    clip.frameRate = srcClip.frameRate;
    var binding = AnimationUtility.GetObjectReferenceCurveBindings(srcClip)[0];
    var keys = AnimationUtility.GetObjectReferenceCurve(srcClip, binding);
    var nk = new ObjectReferenceKeyframe[keys.Length];
    for (int i = 0; i < keys.Length; i++) { nk[i].time = keys[i].time; nk[i].value = frames[Mathf.Min(i, frames.Length - 1)]; }
    AnimationUtility.SetObjectReferenceCurve(clip, binding, nk);
    AnimationUtility.SetAnimationClipSettings(clip, AnimationUtility.GetAnimationClipSettings(srcClip));
    string ocPath = DST + dstName + ".overrideController";
    var oc = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(ocPath);
    if (oc == null) { oc = new AnimatorOverrideController(baseCtrl); AssetDatabase.CreateAsset(oc, ocPath); }
    oc.runtimeAnimatorController = baseCtrl;
    oc[srcClip] = clip;
    EditorUtility.SetDirty(oc); EditorUtility.SetDirty(clip);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    pr.name = dstName;
    pr.GetComponent<Animator>().runtimeAnimatorController = oc;
    var sr = pr.GetComponent<SpriteRenderer>(); sr.sprite = frames[0]; sr.color = Color.white;
    PrefabUtility.SaveAsPrefabAsset(pr, dst); PrefabUtility.UnloadPrefabContents(pr);
    sb.Append(dstName + " keys" + keys.Length + "/frames" + frames.Length + " guid " + AssetDatabase.AssetPathToGUID(dst) + "; ");
    return AssetDatabase.LoadAssetAtPath<GameObject>(dst); };
make("ClowAttack", "GateCleave", cleave);
make("PassiveTerrorAttackPrefab", "GateShockwave", wave);
make("LeafAttackPrefab", "GolemQuake", quake);

// 대형 골렘 바위·정령 탄: FlowerAttackPrefab(발사 순간 플레이어 쪽으로 곧게 나는 탄) 복제 → 애니메이터 제거, 스프라이트 교체 (BanditArrow 방식)
System.Action<string, Sprite> shot = (dstName, sp) => {
    string dst = DST + dstName + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(SRC + "FlowerAttackPrefab.prefab", dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    pr.name = dstName;
    var an = pr.GetComponent<Animator>(); if (an != null) UnityEngine.Object.DestroyImmediate(an);
    pr.GetComponent<SpriteRenderer>().sprite = sp;
    PrefabUtility.SaveAsPrefabAsset(pr, dst); PrefabUtility.UnloadPrefabContents(pr);
    sb.Append(dstName + " guid " + AssetDatabase.AssetPathToGUID(dst) + "; ");
};
shot("GolemBoulder", boulderSp[0]);
shot("RuinBolt", boltSp[0]);
sb.Append(" reimported=" + ProjectB.EditorTools.MonsterBuilder.ReimportModifiedSince(reimportSince));
return sb.ToString();
