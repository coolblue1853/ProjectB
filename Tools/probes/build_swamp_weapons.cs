// ④구역 늪지 계보 무기 (LineageGear 1절, TowerWorld 14절). 에디트 모드, 재실행 안전 (원본 판정 기준으로 다시 계산, 원본 공유 프리팹 미수정)
//  - Frog Tongue Whip (개구리, Wood Spear 기반): A Tongue Lash = 찌르기 판정 복제본 넉백 음수(적을 앞으로 당김)·3명 / S Leap Slam = 돌진 찌르기 강화
//  - Bloodsting Rapier (모기, RatCheifDagger 기반): A Bloodsting = 교차 찌르기 복제본 + LifeStealHit(피해 20% 회복) / S Swarm Dart
//  - Slime Mace (슬라임, SacreGreatSword 기반): S Slime Smash = 검기 복제본 + SpawnOnStart(튀는 슬라임 2, SummonRunner 바운스) / A Gel Gather
//  - Croc Jaw Axe (악어, ScareSide 기반): A Jaw Shake = 내려베기 3타 / R2 Death Roll = 미끄러지며 베기 강화
//  - Petal Pincer (꽃게, Leather Knuckles 기반): D Pincer Flurry = 올려치기 복제본 3타
//  - Primordial Gel (거대 슬라임 유니크, SacreGreatSword 기반): S Gel Tsunami = 검기 + 큰 슬라임 3, 처치 시 35% 슬라임 소환(KillSummonProc)
//  - 아이콘(gen_swamp_items.py) 임포트, 보스 보상 Resources/Boss/GiantSlime_BD.asset
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/";
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
foreach (var n in new[]{ "FrogTongueWhip", "BloodstingRapier", "SlimeMace", "PrimordialGel", "CrocJawAxe", "PetalPincer", "Frog Skin", "Crab Pincer", "Croc Tooth", "Blood Sting", "Slime Gel" })
{
    string p = "Assets/Resources/Item/" + n + ".png";
    AssetDatabase.ImportAsset(p);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode;
    imp.SaveAndReimport();
}
sb.Append("icons; ");

// 판정 복제 (build_deep_forest_weapons2 와 같은 방식): 원본 → DO/folder/folder+원본이름 → 배율·색 → after 개조
System.Func<GameObject, string, float, Color, System.Action<GameObject>, GameObject> cloneHit = (src, folder, ratioMul, tint, after) => {
    System.IO.Directory.CreateDirectory(DO + folder);
    string dst = DO + folder + "/" + folder + src.name + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    var d = pr.GetComponent<DamageObject>(); var sd = src.GetComponent<DamageObject>();
    if (d != null && sd != null) { d.dmgRatio = sd.dmgRatio * ratioMul; d.knockForce = sd.knockForce; d.knockbackDir = sd.knockbackDir; d.hitCount = sd.hitCount; d.maxDamagedEnemies = sd.maxDamagedEnemies; }
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
System.Func<GameObject, string, string, GameObject> origOf = (g, prefix, srcFolder) => {
    if (g == null || !g.name.StartsWith(prefix)) return g;
    var o = AssetDatabase.LoadAssetAtPath<GameObject>(DO + srcFolder + "/" + g.name.Substring(prefix.Length) + ".prefab");
    return o != null ? o : g; };
// 무기 하나 개조: 슬롯 이름(LSkill 등) → (새 스킬 이름, 쿨, 배율, after). 모든 판정을 색만 바꾼 복제본으로 교체(원본 공유 프리팹 보호)
System.Action<GameObject, string, string, Color, string, System.Collections.Generic.Dictionary<string, object[]>> remake = (wgo, folder, srcFolder, tint, icon, spec) => {
    foreach (var s in wgo.GetComponentsInChildren<Skill>(true))
    {
        object[] o = spec.ContainsKey(s.name) ? spec[s.name] : null;
        if (o != null) { s.skillName = (string)o[0]; if ((float)o[1] > 0f) s.SkillCoolTime = (float)o[1]; }
        var arr = s.skillprefab;
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = origOf(arr[i], folder, srcFolder);
            if (arr[i] == null || arr[i].GetComponent<DamageObject>() == null) continue;
            arr[i] = cloneHit(arr[i], folder, o != null ? (float)o[2] : 1f, tint, o != null ? (System.Action<GameObject>)o[3] : null);
        }
        s.skillprefab = arr;
        s.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/" + icon + ".png");
    }
};
System.Action<Weapon, int, int, int, int, Color> stats = (w, mn, mx, cp, cd, energy) => { w.minDmg = mn; w.maxDmg = mx; w.critPer = cp; w.critDmg = cd; w.energyColor = energy; };

// ── 튀는 슬라임 소환물 (BogSlime Run 프레임 + SummonRunner 바운스, 보이지 않는 작은 판정 Common/OrbTouch 참조) ──
string SD = DO + "SlimeMace/";
System.IO.Directory.CreateDirectory(SD);
var touchGo = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "Common/OrbTouch.prefab");
var refSr = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "ScareStaff/ScareTotem.prefab").GetComponent<SpriteRenderer>();
var slimeRun = new System.Collections.Generic.List<Sprite>();
foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/BogSlime/Run.png")) if (o is Sprite) slimeRun.Add((Sprite)o);
slimeRun.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
var giantRun = new System.Collections.Generic.List<Sprite>();
foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/GiantSlime/Run.png")) if (o is Sprite) giantRun.Add((Sprite)o);
giantRun.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
System.Func<string, Sprite[], float, Color, float, float, GameObject> hopper = (name, frames, scale, col, dmgScale, life) => {
    var go = new GameObject(name);
    var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = frames[0]; sr.color = col; sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 1;
    var fb = go.AddComponent<SpriteFlipbook>(); fb.Frames = frames;
    var fso = new SerializedObject(fb); fso.FindProperty("_frameTime").floatValue = 0.08f; fso.ApplyModifiedPropertiesWithoutUndo();
    var r = go.AddComponent<SummonRunner>(); var rso = new SerializedObject(r);
    rso.FindProperty("_speed").floatValue = 3.5f; rso.FindProperty("_life").floatValue = life; rso.FindProperty("_attackInterval").floatValue = 0.4f;
    rso.FindProperty("_damageScale").floatValue = dmgScale; rso.FindProperty("_attackPrefab").objectReferenceValue = touchGo;
    rso.FindProperty("_bounceHeight").floatValue = 0.9f; rso.FindProperty("_bouncePeriod").floatValue = 0.38f;
    rso.ApplyModifiedPropertiesWithoutUndo();
    go.transform.localScale = new Vector3(scale, scale, 1f);
    var pf = PrefabUtility.SaveAsPrefabAsset(go, SD + name + ".prefab"); UnityEngine.Object.DestroyImmediate(go); return pf; };
var slimeHopper = hopper("SlimeHopper", slimeRun.ToArray(), 0.45f, new Color(0.85f, 1f, 0.85f, 0.9f), 0.45f, 2.2f);
var gelHopper = hopper("GelHopper", giantRun.ToArray(), 0.4f, new Color(0.9f, 1f, 0.8f, 0.9f), 0.6f, 2.6f);
System.Func<GameObject, int, float, System.Action<GameObject>> spawner = (pf, count, spacing) => pr => {
    var s = pr.GetComponent<SpawnOnStart>(); if (s == null) s = pr.AddComponent<SpawnOnStart>();
    var so = new SerializedObject(s); so.FindProperty("_prefab").objectReferenceValue = pf; so.FindProperty("_count").intValue = count; so.FindProperty("_spacing").floatValue = spacing; so.FindProperty("_yOffset").floatValue = 0.2f;
    so.ApplyModifiedPropertiesWithoutUndo(); };
sb.Append("hoppers; ");

// ── 1) Frog Tongue Whip ──
var tongue = new Color(1f, 0.62f, 0.72f, 1f);
System.Action<GameObject> pull = pr => { var d = pr.GetComponent<DamageObject>(); d.knockForce = -Mathf.Abs(d.knockForce) * 1.8f; d.knockbackDir = new Vector2(1f, -0.2f); d.maxDamagedEnemies = 3; d.stiffnessTime = Mathf.Max(d.stiffnessTime, 0.4f); };
var fw = openCopy("Wood Spear", "FrogTongueWhip");
stats(fw.GetComponent<Weapon>(), 13, 20, 6, 15, new Color(1f, 0.55f, 0.65f));
remake(fw, "FrogTongueWhip", "WoodSpear", tongue, "FrogTongueWhip", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Tongue Lash", 2f, 1.1f, pull }},
    {"RSkill", new object[]{ "Leap Slam", 0f, 1.25f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); d.knockForce *= 1.4f; }) }} });
tintHands(fw.GetComponent<Weapon>(), new Color(0.75f, 1f, 0.7f));
saveClose(fw); sb.Append("FrogTongueWhip; ");

// ── 2) Bloodsting Rapier ──
var blood = new Color(1f, 0.45f, 0.45f, 1f);
System.Action<GameObject> drain = pr => { var l = pr.GetComponent<LifeStealHit>(); if (l == null) l = pr.AddComponent<LifeStealHit>(); var so = new SerializedObject(l); so.FindProperty("_ratio").floatValue = 0.2f; so.ApplyModifiedPropertiesWithoutUndo(); };
var br = openCopy("RatCheifDagger", "BloodstingRapier");
stats(br.GetComponent<Weapon>(), 12, 18, 12, 20, new Color(0.9f, 0.2f, 0.25f));
remake(br, "BloodstingRapier", "RatCheifDagger", blood, "BloodstingRapier", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Bloodsting", 1.5f, 1.1f, drain }},
    {"RSkill", new object[]{ "Swarm Dart", 0f, 1.1f, null }},
    {"CSkill", new object[]{ "Swarm Call", 0f, 1f, null }} });
tintHands(br.GetComponent<Weapon>(), new Color(0.85f, 0.7f, 0.7f));
saveClose(br); sb.Append("BloodstingRapier; ");

// ── 3) Slime Mace ──
var gel = new Color(0.6f, 1f, 0.65f, 1f);
var sm = openCopy("SacreGreatSword", "SlimeMace");
stats(sm.GetComponent<Weapon>(), 15, 24, 5, 15, new Color(0.45f, 0.9f, 0.55f));
remake(sm, "SlimeMace", "GreatSword", gel, "SlimeMace", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Gel Gather", 0f, 1f, null }},
    {"RSkill", new object[]{ "Slime Smash", 6f, 1.15f, spawner(slimeHopper, 2, 0.8f) }} });
tintHands(sm.GetComponent<Weapon>(), new Color(0.7f, 1f, 0.75f));
saveClose(sm); sb.Append("SlimeMace; ");

// ── 4) Croc Jaw Axe ──
var olive = new Color(0.72f, 0.9f, 0.55f, 1f);
var ca = openCopy("ScareSide", "CrocJawAxe");
stats(ca.GetComponent<Weapon>(), 15, 23, 6, 20, new Color(0.55f, 0.75f, 0.35f));
remake(ca, "CrocJawAxe", "ScareSide", olive, "CrocJawAxe", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Jaw Shake", 3f, 0.85f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); d.hitCount = 3; d.stiffnessTime = Mathf.Max(d.stiffnessTime, 0.5f); }) }},
    {"RSkill", new object[]{ "Bog Eruption", 0f, 1f, null }},
    {"LSkill (1)", new object[]{ "Tail Whirl", 0f, 1f, null }},
    {"RSkill (1)", new object[]{ "Death Roll", 5f, 1.2f, null }} });
tintHands(ca.GetComponent<Weapon>(), new Color(0.75f, 0.9f, 0.6f));
saveClose(ca); sb.Append("CrocJawAxe; ");

// ── 5) Petal Pincer ──
var shell = new Color(1f, 0.6f, 0.5f, 1f);
var pp = openCopy("Leather Knuckles", "PetalPincer");
stats(pp.GetComponent<Weapon>(), 13, 19, 8, 15, new Color(1f, 0.5f, 0.4f));
remake(pp, "PetalPincer", "Leather Knucle", shell, "PetalPincer", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Crab Scuttle", 0f, 1f, null }},
    {"RSkill", new object[]{ "Shell Back", 0f, 1f, null }},
    {"CSkill", new object[]{ "Pincer Flurry", 2f, 0.6f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); d.hitCount = 3; d.isHitIntervelEven = false; d.hitIntervalTime = 0.1f; d.holdingTime = Mathf.Max(d.holdingTime, 0.6f); d.maxDamagedEnemies = 2; }) }} });
tintHands(pp.GetComponent<Weapon>(), new Color(1f, 0.7f, 0.62f));
saveClose(pp); sb.Append("PetalPincer; ");

// ── 6) Primordial Gel (보스 유니크) ──
var murk = new Color(0.62f, 0.8f, 0.45f, 1f);
var pg = openCopy("SacreGreatSword", "PrimordialGel");
stats(pg.GetComponent<Weapon>(), 22, 34, 8, 25, new Color(0.7f, 0.85f, 0.3f));
remake(pg, "PrimordialGel", "GreatSword", murk, "PrimordialGel", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Primordial Pull", 0f, 1.3f, null }},
    {"RSkill", new object[]{ "Gel Tsunami", 7f, 1.4f, spawner(gelHopper, 3, 1.1f) }} });
{
    var kp = pg.GetComponent<KillSummonProc>(); if (kp == null) kp = pg.AddComponent<KillSummonProc>();
    var so = new SerializedObject(kp); so.FindProperty("_summonPrefab").objectReferenceValue = slimeHopper; so.FindProperty("_chance").floatValue = 0.35f; so.ApplyModifiedPropertiesWithoutUndo();
}
tintHands(pg.GetComponent<Weapon>(), new Color(0.7f, 0.85f, 0.5f));
saveClose(pg); sb.Append("PrimordialGel; ");

// ── 7) 보스 보상 ──
string bp = "Assets/Resources/Boss/GiantSlime_BD.asset";
var rw = AssetDatabase.LoadAssetAtPath<BossReward>(bp);
if (rw == null) { rw = ScriptableObject.CreateInstance<BossReward>(); AssetDatabase.CreateAsset(rw, bp); }
rw.displayName = "Giant Slime";
rw.setPieces = new[]{ "SlimeMace", "BloodstingRapier", "FrogTongueWhip" };
rw.uniqueItem = "PrimordialGel"; rw.uniqueChance = 0.08f; rw.pity = 12;
rw.stoneMin = 3; rw.stoneMax = 6; rw.firstKillStones = 6; rw.grantsRank = 0;
rw.portrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/RPGMonstersIcon/default size/no shadow/5 Field/field_501.png");
if (rw.portrait == null) sb.Append("[초상 없음] ");
EditorUtility.SetDirty(rw);
AssetDatabase.SaveAssets();
sb.Append("boss reward");
return sb.ToString();
