// 2층계 ②구역 무너진 성벽 계보 무기 (Floor2Plan 5.1, TowerWorld 17.1). 에디트 모드, 재실행 안전 (원본 공유 프리팹 미수정, 판정은 DamageObject/<무기>/ 복제본)
//  - Rampart Sword (성벽 기사, RustedSword 기반 22~33): A Shield Rush = 내려베기 1.3배·넉백 1.6배·경직 0.45s, S Knight's Cleave = 베기 1.4배 크기·1.25배
//  - Sentry Halberd (미늘창 파수병, Wood Spear 기반 23~34): A Halberd Sweep = 찌르기 1.35배 크기·6명·넉백 1.5배, S Crescent Reap = 돌진 + 땅을 미는 초승달(관통 5)
//  - Gargoyle Wing (가고일, ScareStaff 기반 21~31): A Stone Feather = 돌 깃털 5발 부채꼴, D Gargoyle Dive = 작은 가고일 2기 활강 돌격(SummonRunner)
//  - Skyfall Talon (가고일 군주 유니크, ScareClow 기반 30~46): A Talon Storm = 갈퀴 1.3배 + 돌 깃털 3발, D Gargoyle Host = 가고일 3기 활강
//  - 아이콘(gen_rampart_items.py) 임포트, 보스 보상 Resources/Boss/GargoyleLord_BD.asset
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/";
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
foreach (var n in new[]{ "RampartSword", "SentryHalberd", "GargoyleWing", "SkyfallTalon", "Rampart Shard", "Halberd Fragment", "Gargoyle Feather", "Storm Wisp" })
{
    string p = "Assets/Resources/Item/" + n + ".png";
    AssetDatabase.ImportAsset(p);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode;
    imp.SaveAndReimport();
}
sb.Append("icons; ");

// ── 공용 도우미 (build_outskirts_weapons.cs 와 같은 방식) ──
System.Func<GameObject, string, float, Color, System.Action<GameObject>, GameObject> cloneHit = (src, folder, ratioMul, tint, after) => {
    System.IO.Directory.CreateDirectory(DO + folder);
    string dst = DO + folder + "/" + folder + src.name + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    var d = pr.GetComponent<DamageObject>(); var sd = src.GetComponent<DamageObject>();
    if (d != null && sd != null) { d.dmgRatio = sd.dmgRatio * ratioMul; d.knockForce = sd.knockForce; d.knockbackDir = sd.knockbackDir; d.hitCount = sd.hitCount; d.maxDamagedEnemies = sd.maxDamagedEnemies; d.stiffnessTime = sd.stiffnessTime; d.holdingBuffPower = sd.holdingBuffPower; }
    pr.transform.localScale = src.transform.localScale; // 크기 배율은 원본 기준(재실행 누적 방지)
    var srs = src.GetComponentsInChildren<SpriteRenderer>(true); var drs = pr.GetComponentsInChildren<SpriteRenderer>(true);
    for (int i = 0; i < drs.Length && i < srs.Length; i++) { var c = srs[i].color; drs[i].color = new Color(c.r * tint.r, c.g * tint.g, c.b * tint.b, c.a * tint.a); }
    if (after != null) after(pr);
    PrefabUtility.SaveAsPrefabAsset(pr, dst); PrefabUtility.UnloadPrefabContents(pr);
    return AssetDatabase.LoadAssetAtPath<GameObject>(dst); };
System.Func<string, string, GameObject> openCopy = (srcName, dstName) => {
    string path = W + dstName + ".prefab";
    if (!System.IO.File.Exists(path)) AssetDatabase.CopyAsset(W + srcName + ".prefab", path);
    var go = PrefabUtility.LoadPrefabContents(path); go.name = dstName; return go; };
System.Action<GameObject> saveClose = go => { PrefabUtility.SaveAsPrefabAsset(go, W + go.name + ".prefab"); PrefabUtility.UnloadPrefabContents(go); };
System.Action<Weapon, Color> tintHands = (w, c) => {
    foreach (var g in new[]{ w.RightWeqponSprite, w.leftWeqponSprite })
        if (g != null) foreach (var r in g.GetComponentsInChildren<SpriteRenderer>(true)) r.color = c; };
// 무기 하나 개조: 스킬 오브젝트 이름 → (새 스킬 이름, 쿨, 배율, after). 판정은 원본 무기의 판정에서 다시 복제(재실행해도 값이 누적되지 않게 원본 기준)
System.Action<GameObject, string, string, Color, string, System.Collections.Generic.Dictionary<string, object[]>> remake = (wgo, srcWeapon, folder, tint, icon, spec) => {
    var srcRoot = AssetDatabase.LoadAssetAtPath<GameObject>(W + srcWeapon + ".prefab");
    foreach (var s in wgo.GetComponentsInChildren<Skill>(true))
    {
        object[] o = spec.ContainsKey(s.name) ? spec[s.name] : null;
        if (o != null) { s.skillName = (string)o[0]; if ((float)o[1] > 0f) s.SkillCoolTime = (float)o[1]; }
        s.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/" + icon + ".png");
        if (o != null && o[2] == null) continue; // 판정을 따로 지정하는 스킬
        var srcT = srcRoot.transform.Find(s.name); var srcSk = srcT != null ? srcT.GetComponent<Skill>() : null;
        if (srcSk == null) continue;
        var arr = new GameObject[srcSk.skillprefab.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            var sp = srcSk.skillprefab[i];
            arr[i] = (sp == null || sp.GetComponent<DamageObject>() == null) ? sp : cloneHit(sp, folder, o != null ? (float)o[2] : 1f, tint, o != null ? (System.Action<GameObject>)o[3] : null);
        }
        s.skillprefab = arr;
    }
};
System.Action<Weapon, int, int, int, int, Color> stats = (w, mn, mx, cp, cd, energy) => { w.minDmg = mn; w.maxDmg = mx; w.critPer = cp; w.critDmg = cd; w.energyColor = energy; };
System.Func<GameObject, string> skillList = g => { var s2 = new System.Text.StringBuilder(); foreach (var s in g.GetComponentsInChildren<Skill>(true)) s2.Append(s.name + "=" + s.skillName + "(" + s.SkillCoolTime + "s," + s.skillprefab.Length + ") "); return s2.ToString(); };
var refSr = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "ScareStaff/ScareTotem.prefab").GetComponent<SpriteRenderer>();
System.Func<string, string, GameObject, int, float, float, GameObject> cast = (folder, name, obj, count, dist, yPos) => {
    System.IO.Directory.CreateDirectory(DO + folder);
    string p = DO + folder + "/" + name + ".prefab";
    if (!System.IO.File.Exists(p)) AssetDatabase.CopyAsset(DO + "ScareStaff/SummonSacre.prefab", p);
    var r = PrefabUtility.LoadPrefabContents(p);
    var ss = r.GetComponent<SummonSkill>(); ss.summonObject = obj; ss.summonCount = count; ss.summonDistance = dist; ss.yPos = yPos;
    var s0 = r.GetComponent<SpriteRenderer>(); if (s0 != null) s0.color = new Color(1f, 1f, 1f, 0f);
    PrefabUtility.SaveAsPrefabAsset(r, p); PrefabUtility.UnloadPrefabContents(r);
    return AssetDatabase.LoadAssetAtPath<GameObject>(p); };
string RF = DO + "Rampart/";
System.IO.Directory.CreateDirectory(RF);
System.Func<string, Sprite[]> sheet = p => { var l = new System.Collections.Generic.List<Sprite>(); foreach (var o in AssetDatabase.LoadAllAssetsAtPath(p)) if (o is Sprite) l.Add((Sprite)o); l.Sort((a, b) => string.CompareOrdinal(a.name, b.name)); return l.ToArray(); };
// 보이지 않는 작은 피격 판정(공용 OrbTouch 복제, 원본 값 기준)
System.Func<string, float, GameObject> touch = (name, knockMul) => {
    string p = RF + name + ".prefab";
    if (!System.IO.File.Exists(p)) AssetDatabase.CopyAsset(DO + "Common/OrbTouch.prefab", p);
    var t = PrefabUtility.LoadPrefabContents(p); t.name = name;
    var src0 = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "Common/OrbTouch.prefab").GetComponent<DamageObject>();
    var d = t.GetComponent<DamageObject>(); d.maxDamagedEnemies = 1; d.knockForce = Mathf.Abs(src0.knockForce) * knockMul; d.stiffnessTime = Mathf.Max(src0.stiffnessTime, 0.3f);
    PrefabUtility.SaveAsPrefabAsset(t, p); PrefabUtility.UnloadPrefabContents(t);
    return AssetDatabase.LoadAssetAtPath<GameObject>(p); };
// StraightArrow 투사체: (이름, 그림, 판정, 피해, 속도, 수명, 각도, 퍼짐, 관통, 지면 통과, 상자, 크기)
System.Func<string, Sprite, GameObject, float, float, float, float, float, int, bool, Vector2, float, GameObject> missile = (name, sp, hit, dmg, speed, life, angle, jitter, pierce, ignoreTerrain, box, scale) => {
    var g = new GameObject(name);
    g.layer = hit.layer;
    var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = sp; sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 2;
    var bc = g.AddComponent<BoxCollider2D>(); bc.isTrigger = true; bc.size = box;
    var rb = g.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.useFullKinematicContacts = true;
    var a = g.AddComponent<StraightArrow>(); var so = new SerializedObject(a);
    so.FindProperty("_hitPrefab").objectReferenceValue = hit; so.FindProperty("_damageScale").floatValue = dmg; so.FindProperty("_speed").floatValue = speed;
    so.FindProperty("_life").floatValue = life; so.FindProperty("_angle").floatValue = angle; so.FindProperty("_angleJitter").floatValue = jitter; so.FindProperty("_pierce").intValue = pierce;
    so.FindProperty("_isSkill").boolValue = true; so.FindProperty("_ignoreTerrain").boolValue = ignoreTerrain;
    so.FindProperty("_spawnJitter").vector2Value = new Vector2(0f, jitter > 0f ? 0.2f : 0f);
    so.ApplyModifiedPropertiesWithoutUndo();
    g.transform.localScale = new Vector3(scale, scale, 1f);
    var pf0 = PrefabUtility.SaveAsPrefabAsset(g, RF + name + ".prefab"); UnityEngine.Object.DestroyImmediate(g); return pf0; };
// 활강하는 작은 가고일(SummonRunner, 가고일 Run 프레임)
var gargRun = sheet("Assets/Art/Monsters/Gargoyle/Run.png");
if (!System.IO.File.Exists(RF + "DiverStab.prefab")) AssetDatabase.CopyAsset(DO + "DoubleDagger/DaggerPowerSteb.prefab", RF + "DiverStab.prefab");
var diverStab = AssetDatabase.LoadAssetAtPath<GameObject>(RF + "DiverStab.prefab");
System.Func<string, float, Color, GameObject> diver = (name, dmg, col) => {
    var g = new GameObject(name);
    var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = gargRun[0]; sr.color = col;
    sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 1;
    var fb = g.AddComponent<SpriteFlipbook>(); fb.Frames = gargRun;
    var fso = new SerializedObject(fb); fso.FindProperty("_frameTime").floatValue = 0.06f; fso.ApplyModifiedPropertiesWithoutUndo();
    var r = g.AddComponent<SummonRunner>(); var rso = new SerializedObject(r);
    rso.FindProperty("_speed").floatValue = 8f; rso.FindProperty("_life").floatValue = 1.5f; rso.FindProperty("_attackInterval").floatValue = 0.3f; rso.FindProperty("_damageScale").floatValue = dmg;
    rso.FindProperty("_attackPrefab").objectReferenceValue = diverStab; // 쌍단검 원본 판정의 복제본(원본 조정이 소환물로 번지지 않게, 리뷰 반영)
    rso.ApplyModifiedPropertiesWithoutUndo();
    g.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    var pf0 = PrefabUtility.SaveAsPrefabAsset(g, RF + name + ".prefab"); UnityEngine.Object.DestroyImmediate(g); return pf0; };
System.Action<GameObject, GameObject, int, float> spawnOn = (pr, prefab, count, yOff) => {
    var sp = pr.GetComponent<SpawnOnStart>(); if (sp == null) sp = pr.AddComponent<SpawnOnStart>();
    var so = new SerializedObject(sp); so.FindProperty("_prefab").objectReferenceValue = prefab; so.FindProperty("_count").intValue = count; so.FindProperty("_spacing").floatValue = 0f; so.FindProperty("_yOffset").floatValue = yOff; so.ApplyModifiedPropertiesWithoutUndo(); };
var steelTint = new Color(0.9f, 0.95f, 1.05f, 1f);
var stoneTint = new Color(0.78f, 0.82f, 0.92f, 1f);

var featherSp = sheet("Assets/Art/Monsters/Gargoyle/GargoyleFeather.png");
var crescentSp = sheet("Assets/Art/Monsters/HalberdSentry/HalberdCrescent.png");
var featherTouch = touch("FeatherTouch", 0.8f);
var crescentTouch = touch("CrescentTouch", 1.3f);
var featherShot = missile("StoneFeatherShot", featherSp[0], featherTouch, 0.6f, 17f, 0.6f, 0f, 8f, 1, false, new Vector2(1.6f, 0.8f), 0.45f);
var crescentWave = missile("CrescentReapWave", crescentSp[Mathf.Min(2, crescentSp.Length - 1)], crescentTouch, 1.1f, 12f, 0.7f, 0f, 0f, 5, true, new Vector2(1.8f, 2f), 0.6f);
var gargDiver = diver("GargoyleDiver", 0.55f, new Color(0.85f, 0.9f, 1f, 0.95f));
var hostDiver = diver("GargoyleHostDiver", 0.65f, new Color(0.75f, 0.85f, 1f, 1f));

// ── 1) Rampart Sword (성벽 기사) — RustedSword 기반 ──
var rs = openCopy("RustedSword", "RampartSword");
stats(rs.GetComponent<Weapon>(), 22, 33, 6, 15, new Color(0.75f, 0.85f, 1f));
remake(rs, "RustedSword", "RampartSword", steelTint, "RampartSword", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Shield Rush", 3f, 1.3f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); d.knockForce *= 1.6f; d.stiffnessTime = Mathf.Max(d.stiffnessTime, 0.45f); }) }},
    {"RSkill", new object[]{ "Knight's Cleave", 3f, 1.25f, (System.Action<GameObject>)(pr => { pr.transform.localScale = new Vector3(pr.transform.localScale.x * 1.4f, pr.transform.localScale.y * 1.4f, pr.transform.localScale.z); }) }} });
tintHands(rs.GetComponent<Weapon>(), new Color(0.92f, 0.95f, 1f));
sb.Append("RampartSword: " + skillList(rs) + "; ");
saveClose(rs);

// ── 2) Sentry Halberd (미늘창 파수병) — Wood Spear 기반 ──
var sh = openCopy("Wood Spear", "SentryHalberd");
stats(sh.GetComponent<Weapon>(), 23, 34, 5, 15, new Color(0.8f, 0.88f, 1f));
remake(sh, "Wood Spear", "SentryHalberd", steelTint, "SentryHalberd", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Halberd Sweep", 2.5f, 1.2f, (System.Action<GameObject>)(pr => {
        pr.transform.localScale = new Vector3(pr.transform.localScale.x * 1.35f, pr.transform.localScale.y * 1.35f, pr.transform.localScale.z);
        var d = pr.GetComponent<DamageObject>(); d.maxDamagedEnemies = Mathf.Max(d.maxDamagedEnemies, 6); d.knockForce *= 1.5f; }) }},
    {"RSkill", new object[]{ "Crescent Reap", 3.5f, 1.1f, (System.Action<GameObject>)(pr => spawnOn(pr, crescentWave, 1, -0.2f)) }} });
tintHands(sh.GetComponent<Weapon>(), new Color(0.9f, 0.93f, 1f));
sb.Append("SentryHalberd: " + skillList(sh) + "; ");
saveClose(sh);

// ── 3) Gargoyle Wing (가고일) — ScareStaff 기반 ──
var featherCast = cast("Rampart", "StoneFeatherCast", featherShot, 5, 0.3f, 0.4f);
var diveCast = cast("Rampart", "GargoyleDiveCast", gargDiver, 2, 0.5f, 0.8f);
var gw = openCopy("ScareStaff", "GargoyleWing");
stats(gw.GetComponent<Weapon>(), 21, 31, 8, 20, new Color(0.7f, 0.85f, 1f));
remake(gw, "ScareStaff", "GargoyleWing", stoneTint, "GargoyleWing", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Stone Feather", 1.4f, null, null }},
    {"RSkill", new object[]{ "Petrify Bolt", 0f, 1.1f, null }},
    {"DSkill", new object[]{ "Gargoyle Dive", 9f, null, null }} });
{
    var L = gw.transform.Find("LSkill").GetComponent<Skill>(); L.skillprefab = new GameObject[]{ featherCast }; L.skillWaitTime = 0.15f;
    L.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/Gargoyle Feather.png");
    var D = gw.transform.Find("DSkill").GetComponent<Skill>(); D.skillprefab = new GameObject[]{ diveCast }; D.skillWaitTime = 0.3f;
}
tintHands(gw.GetComponent<Weapon>(), new Color(0.72f, 0.76f, 0.86f));
sb.Append("GargoyleWing: " + skillList(gw) + "; ");
saveClose(gw);

// ── 4) Skyfall Talon (가고일 군주 유니크) — ScareClow 기반 ──
var hostCast = cast("Rampart", "GargoyleHostCast", hostDiver, 3, 0.5f, 0.8f);
var st = openCopy("ScareClow", "SkyfallTalon");
var stw = st.GetComponent<Weapon>();
stats(stw, 30, 46, 10, 30, new Color(0.65f, 0.85f, 1f));
remake(st, "ScareClow", "SkyfallTalon", stoneTint, "SkyfallTalon", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Talon Storm", 2.5f, 1.3f, null }},
    {"RSkill", new object[]{ "Skyfall Rend", 3f, 1.2f, null }},
    {"Dskill", new object[]{ "Gargoyle Host", 10f, null, null }} });
{
    var L = st.transform.Find("LSkill").GetComponent<Skill>();
    // 첫 판정에만 돌 깃털 3발(판정 6개 모두에 붙이면 18발)
    if (L.skillprefab.Length > 0 && L.skillprefab[0] != null)
    {
        string p0 = AssetDatabase.GetAssetPath(L.skillprefab[0]);
        if (!p0.StartsWith(DO + "SkyfallTalon/")) return "FAIL: Talon Storm 첫 판정이 복제본이 아님(원본 공유 프리팹 보호) " + p0;
        var pr = PrefabUtility.LoadPrefabContents(p0); spawnOn(pr, featherShot, 3, 0.3f); PrefabUtility.SaveAsPrefabAsset(pr, p0); PrefabUtility.UnloadPrefabContents(pr);
    }
    var D = st.transform.Find("Dskill").GetComponent<Skill>(); D.skillprefab = new GameObject[]{ hostCast }; D.skillWaitTime = 0.3f;
    D.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/Gargoyle Feather.png");
}
tintHands(stw, new Color(0.74f, 0.78f, 0.9f));
sb.Append("SkyfallTalon: " + skillList(st) + "; ");
saveClose(st);

// ── 5) 보스 보상: 확정 1개 = 계보 무기 3종 중, 유니크 Skyfall Talon 8%·천장 12, 칭호 없음(구역 보스) ──
string bp = "Assets/Resources/Boss/GargoyleLord_BD.asset";
var rw = AssetDatabase.LoadAssetAtPath<BossReward>(bp);
if (rw == null) { rw = ScriptableObject.CreateInstance<BossReward>(); AssetDatabase.CreateAsset(rw, bp); }
rw.displayName = "Gargoyle Lord";
rw.setPieces = new[]{ "RampartSword", "SentryHalberd", "GargoyleWing" };
rw.uniqueItem = "SkyfallTalon"; rw.uniqueChance = 0.08f; rw.pity = 12;
rw.stoneMin = 4; rw.stoneMax = 7; rw.firstKillStones = 8; rw.grantsRank = 0;
foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/GargoyleLord/Idle.png")) if (o is Sprite && o.name == "Idle_0") rw.portrait = (Sprite)o; // 초상 에셋이 없어 도트 첫 프레임
if (rw.portrait == null) sb.Append("[초상 없음] ");
EditorUtility.SetDirty(rw);
AssetDatabase.SaveAssets();
sb.Append("boss reward");
return sb.ToString();
