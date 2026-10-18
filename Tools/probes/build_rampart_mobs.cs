// 2층계 ②구역 '무너진 성벽' 몬스터 (gen_rampart_mobs.py 시트 → 슬라이스 → MonsterBuilder, Floor2Plan 5.1). 에디트 모드, 재실행 안전
// 낮: 성벽 기사(RatThief 템플릿=빠른 근접)·미늘창 파수병(LeftMole=느림, 판정 leaf → HalberdCrescent)·가고일(FlowerMole=원거리, 탄 → GargoyleFeather)
// 밤: + 구름 정령(FlowerMole, 탄 → CloudSpark, 노란 Light2D) / 보스: 가고일 군주(PassiveTerror, 128px, 판정 GargoyleTalon·GargoyleShard, 2페이즈 가고일 2 호출)
// 판정은 원본 공유 프리팹 복제 + OverrideController(build_ent_boss_roots.cs 방식). BT damageOb 캐시 GUID·fileID 도 새 판정으로 텍스트 교체
var reimportSince = System.DateTime.Now.AddSeconds(-1); // B-139
var sb = new System.Text.StringBuilder();
string root = "Assets/Art/Monsters/";
AssetDatabase.Refresh();
var sizes = new System.Collections.Generic.Dictionary<string, int>{ {"RampartKnight",64}, {"HalberdSentry",64}, {"Gargoyle",48}, {"CloudWisp",48}, {"GargoyleLord",128} };
foreach (var kv in sizes)
foreach (var anim in new[]{ "Idle", "Run", "Attack1", "Attack2", "Take Hit", "Death" })
{
    string path = root + kv.Key + "/" + anim + ".png";
    AssetDatabase.ImportAsset(path);
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
// Floor2Plan 3.2 T7: 평균 HP 610·방어 23 (기사 ×1.05, 파수병 탱커 ×1.45, 가고일 원거리 ×0.7, 정령 ×0.5·방어 ×0.4)
sb.AppendLine(build("RampartKnight", "RatThief_BD", 640, 24, false, false, rat));
sb.AppendLine(build("HalberdSentry", "LeftMole_BD", 880, 28, false, false, leaf));
sb.AppendLine(build("Gargoyle", "FlowerMole_BD", 420, 26, false, false, flower));
sb.AppendLine(build("CloudWisp", "FlowerMole_BD", 300, 10, false, false, flower));
sb.AppendLine(build("GargoyleLord", "PassiveTerror_BD", 18000, 34, true, true, new[]{ "PT_Stay","Idle","PT_Walk","Run","ScaryCrowHit","Take Hit","PTAttackOne","Attack1","PTAttackTwo","Attack2","death","Death" }));

// 정령: 노란 번개 빛, 군주: 눈빛(청백)
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
glow("CloudWisp", new Color(1f, 0.92f, 0.55f), 0.85f, 3f, new Vector2(0f, 0.6f));
glow("GargoyleLord", new Color(0.6f, 0.85f, 1f), 0.45f, 5f, new Vector2(0.8f, 3f));

// 보스 부하 호출: 2페이즈(HP 50%)에 가고일 2 (풀 키 minion = Gargoyle_BD, build_broken_rampart.cs 가 등록)
{
    string path = "Assets/Prefab/Enemy_BD/GargoyleLord_BD.prefab";
    var pr = PrefabUtility.LoadPrefabContents(path);
    var mc = pr.GetComponent<BossMinionCall>(); if (mc == null) mc = pr.AddComponent<BossMinionCall>();
    var mso = new SerializedObject(mc); mso.FindProperty("_bannerText").stringValue = "TAKES WING — The statues awaken!"; mso.FindProperty("_count").intValue = 2; mso.FindProperty("_spread").floatValue = 3f; mso.ApplyModifiedPropertiesWithoutUndo();
    PrefabUtility.SaveAsPrefabAsset(pr, path); PrefabUtility.UnloadPrefabContents(pr);
    sb.Append("minionCall; ");
}

// ── 판정 복제 ──
string SRC = "Assets/Prefab/Enemy/EnemyDamge/", DST = SRC + "Rampart/";
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
var talon = slice(root + "GargoyleLord/GargoyleTalon.png", 64, 16f);
var shard = slice(root + "GargoyleLord/GargoyleShard.png", 32, 12.5f);
var crescent = slice(root + "HalberdSentry/HalberdCrescent.png", 32, 12.5f);
var featherSp = slice(root + "Gargoyle/GargoyleFeather.png", 32, 12f);
var sparkSp = slice(root + "CloudWisp/CloudSpark.png", 32, 12f);

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
    for (int i = 0; i < keys.Length; i++) { nk[i].time = keys[i].time; nk[i].value = frames[Mathf.Min(i * frames.Length / Mathf.Max(1, keys.Length), frames.Length - 1)]; }
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
    sb.Append(dstName + " keys" + keys.Length + "/frames" + frames.Length + "; ");
    return AssetDatabase.LoadAssetAtPath<GameObject>(dst); };
make("ClowAttack", "GargoyleTalon", talon);
make("PassiveTerrorAttackPrefab", "GargoyleShard", shard);
make("LeafAttackPrefab", "HalberdCrescent", crescent);

// 직선 탄: FlowerAttackPrefab(발사 순간 플레이어 쪽 직선) 복제 → 애니메이터 제거, 스프라이트 교체 (BanditArrow 방식)
System.Action<string, Sprite> shot = (dstName, sp) => {
    string dst = DST + dstName + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(SRC + "FlowerAttackPrefab.prefab", dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    pr.name = dstName;
    var an = pr.GetComponent<Animator>(); if (an != null) UnityEngine.Object.DestroyImmediate(an);
    pr.GetComponent<SpriteRenderer>().sprite = sp;
    PrefabUtility.SaveAsPrefabAsset(pr, dst); PrefabUtility.UnloadPrefabContents(pr);
    sb.Append(dstName + "; ");
};
shot("GargoyleFeather", featherSp[0]);
shot("CloudSpark", sparkSp[0]);
AssetDatabase.SaveAssets();

// BT damageOb 캐시 참조 GUID·루트 fileID 교체(런타임은 풀 키로 꺼내므로 표시용 정리 — build_swamp_boss_attacks.cs 와 같음)
System.Func<string, string, string, int> swapGuid = (prefab, from, to) => {
    string gFrom = AssetDatabase.AssetPathToGUID(from), gTo = AssetDatabase.AssetPathToGUID(to);
    long idFrom, idTo; string g;
    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(AssetDatabase.LoadAssetAtPath<GameObject>(from), out g, out idFrom);
    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(AssetDatabase.LoadAssetAtPath<GameObject>(to), out g, out idTo);
    string a = "{fileID: " + idFrom + ", guid: " + gFrom, b = "{fileID: " + idTo + ", guid: " + gTo;
    string txt = System.IO.File.ReadAllText(prefab);
    int c = 0, i = 0;
    while ((i = txt.IndexOf(a, i, System.StringComparison.Ordinal)) >= 0) { c++; i += a.Length; }
    if (c > 0) { System.IO.File.WriteAllText(prefab, txt.Replace(a, b)); AssetDatabase.ImportAsset(prefab, ImportAssetOptions.ForceUpdate); }
    return c; };
string BD = "Assets/Prefab/Enemy_BD/";
sb.Append("guid lord clow=" + swapGuid(BD + "GargoyleLord_BD.prefab", SRC + "ClowAttack.prefab", DST + "GargoyleTalon.prefab"));
sb.Append(" fire=" + swapGuid(BD + "GargoyleLord_BD.prefab", SRC + "PassiveTerrorAttackPrefab.prefab", DST + "GargoyleShard.prefab"));
sb.Append(" sentry=" + swapGuid(BD + "HalberdSentry_BD.prefab", SRC + "LeafAttackPrefab.prefab", DST + "HalberdCrescent.prefab"));
sb.Append(" garg=" + swapGuid(BD + "Gargoyle_BD.prefab", SRC + "FlowerAttackPrefab.prefab", DST + "GargoyleFeather.prefab"));
sb.Append(" wisp=" + swapGuid(BD + "CloudWisp_BD.prefab", SRC + "FlowerAttackPrefab.prefab", DST + "CloudSpark.prefab") + "; ");
sb.Append(" reimported=" + ProjectB.EditorTools.MonsterBuilder.ReimportModifiedSince(reimportSince));
return sb.ToString();
