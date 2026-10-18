// ⑤구역 산적 성채 계보 무기 (LineageGear 2절, TowerWorld 15절). 에디트 모드, 재실행 안전 (원본 공유 프리팹 미수정, 판정은 DamageObject/<무기>/ 복제본)
//  - Cutthroat Dagger (단검병, DoubleDagger 기반): A Backstab = 앞쪽 가장 가까운 적 뒤로 순간 이동해 돌아서 찌르기(BackstabStep: 백어택 치명 +40%·치피 +30%)
//  - Bandit Shortbow (궁수, ScareStaff 기반, 신규 원거리): 평타 = 직선 화살(StraightArrow, 근접 판정 끔), A Triple Shot = 화살 3발, S Poison Volley = 위에서 쏟아지는 독화살 비 6발
//  - Raider Buckler (방패병, Shell Shield 기반): A Raider Thrust, S Shield Shove = 방패 밀치기(넉백 1.6배·경직 0.5s), D Counter Stance = Shell Guard 재사용(방어 +200 홀드)
//  - Chief's War Banner (산적 대장 유니크, Wood Spear 기반): A Banner Thrust, S Raid Charge, F Rally the Gang = 산적 그림자 3명 돌격(SummonRunner)
//  - 아이콘(gen_bandit_items.py) 임포트, 보스 보상 Resources/Boss/BanditChief_BD.asset
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/";
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
foreach (var n in new[]{ "CutthroatDagger", "BanditShortbow", "RaiderBuckler", "ChiefsWarBanner", "Bandit Bandana", "Broken Arrow", "Iron Rivet", "Pitch Torch" })
{
    string p = "Assets/Resources/Item/" + n + ".png";
    AssetDatabase.ImportAsset(p);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode;
    imp.SaveAndReimport();
}
sb.Append("icons; ");

// ── 공용 도우미 (build_swamp_weapons.cs 와 같은 방식) ──
System.Func<GameObject, string, float, Color, System.Action<GameObject>, GameObject> cloneHit = (src, folder, ratioMul, tint, after) => {
    System.IO.Directory.CreateDirectory(DO + folder);
    string dst = DO + folder + "/" + folder + src.name + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    var d = pr.GetComponent<DamageObject>(); var sd = src.GetComponent<DamageObject>();
    if (d != null && sd != null) { d.dmgRatio = sd.dmgRatio * ratioMul; d.knockForce = sd.knockForce; d.knockbackDir = sd.knockbackDir; d.hitCount = sd.hitCount; d.maxDamagedEnemies = sd.maxDamagedEnemies; d.stiffnessTime = sd.stiffnessTime; d.holdingBuffPower = sd.holdingBuffPower; }
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
// 무기 하나 개조: 스킬 오브젝트 이름 → (새 스킬 이름, 쿨, 배율, after). 모든 판정을 색만 바꾼 복제본으로 교체
System.Action<GameObject, string, Color, string, System.Collections.Generic.Dictionary<string, object[]>> remake = (wgo, folder, tint, icon, spec) => {
    foreach (var s in wgo.GetComponentsInChildren<Skill>(true))
    {
        object[] o = spec.ContainsKey(s.name) ? spec[s.name] : null;
        if (o != null) { s.skillName = (string)o[0]; if ((float)o[1] > 0f) s.SkillCoolTime = (float)o[1]; }
        if (o != null && o[2] == null) continue; // 판정을 따로 지정하는 스킬(복제하지 않음)
        var arr = s.skillprefab;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == null || arr[i].GetComponent<DamageObject>() == null || arr[i].name.StartsWith(folder)) continue; // 이미 이 무기 복제본이면 그대로(재실행)
            arr[i] = cloneHit(arr[i], folder, o != null ? (float)o[2] : 1f, tint, o != null ? (System.Action<GameObject>)o[3] : null);
        }
        s.skillprefab = arr;
        s.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/" + icon + ".png");
    }
};
System.Action<Weapon, int, int, int, int, Color> stats = (w, mn, mx, cp, cd, energy) => { w.minDmg = mn; w.maxDmg = mx; w.critPer = cp; w.critDmg = cd; w.energyColor = energy; };
System.Func<GameObject, string> skillList = g => { var s2 = new System.Text.StringBuilder(); foreach (var s in g.GetComponentsInChildren<Skill>(true)) s2.Append(s.name + "=" + s.skillName + "(" + s.SkillCoolTime + "s," + s.skillprefab.Length + ") "); return s2.ToString(); };
var refSr = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "ScareStaff/ScareTotem.prefab").GetComponent<SpriteRenderer>();
System.Func<string, string, GameObject, int, float, float, GameObject> cast = (folder, name, obj, count, dist, yPos) => {
    string p = DO + folder + "/" + name + ".prefab";
    if (!System.IO.File.Exists(p)) AssetDatabase.CopyAsset(DO + "ScareStaff/SummonSacre.prefab", p);
    var r = PrefabUtility.LoadPrefabContents(p);
    var ss = r.GetComponent<SummonSkill>(); ss.summonObject = obj; ss.summonCount = count; ss.summonDistance = dist; ss.yPos = yPos;
    var s0 = r.GetComponent<SpriteRenderer>(); if (s0 != null) s0.color = new Color(1f, 1f, 1f, 0f);
    PrefabUtility.SaveAsPrefabAsset(r, p); PrefabUtility.UnloadPrefabContents(r);
    return AssetDatabase.LoadAssetAtPath<GameObject>(p); };

// ── 1) Cutthroat Dagger ──
var crimson = new Color(1f, 0.55f, 0.55f, 1f);
var cd1 = openCopy("DoubleDagger", "CutthroatDagger");
stats(cd1.GetComponent<Weapon>(), 13, 19, 15, 30, new Color(0.85f, 0.2f, 0.25f));
remake(cd1, "CutthroatDagger", crimson, "CutthroatDagger", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Backstab", 3f, 0.8f, (System.Action<GameObject>)(pr => { if (pr.GetComponent<BackstabStep>() == null) pr.AddComponent<BackstabStep>(); var d = pr.GetComponent<DamageObject>(); d.maxDamagedEnemies = 2; d.stiffnessTime = Mathf.Max(d.stiffnessTime, 0.4f); }) }},
    {"RSkill", new object[]{ "Shadow Step", 0f, 1f, null }},
    {"RSkill (1)", new object[]{ "Cutpurse Stride", 0f, 1f, null }} });
{   // Vicious Assault 는 같은 찌르기 10회 → Backstab 은 한 번(순간 이동도 한 번)
    var ls = cd1.transform.Find("LSkill").GetComponent<Skill>();
    ls.skillprefab = new GameObject[]{ ls.skillprefab[0] };
}
tintHands(cd1.GetComponent<Weapon>(), new Color(0.9f, 0.7f, 0.7f));
sb.Append("CutthroatDagger: " + skillList(cd1) + "; ");
saveClose(cd1);

// ── 2) Bandit Shortbow (화살) ──
string BF = DO + "BanditShortbow/";
System.IO.Directory.CreateDirectory(BF);
var arrowSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Monsters/BanditArcher/Arrow.png");
if (arrowSp == null) foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/BanditArcher/Arrow.png")) if (o is Sprite) arrowSp = (Sprite)o;
System.Func<string, System.Action<GameObject>, GameObject> touch = (name, after) => {
    string p = BF + name + ".prefab";
    if (!System.IO.File.Exists(p)) AssetDatabase.CopyAsset(DO + "WispLantern/WispTouch.prefab", p);
    var t = PrefabUtility.LoadPrefabContents(p); t.name = name;
    var d = t.GetComponent<DamageObject>(); d.maxDamagedEnemies = 1; d.knockForce = Mathf.Abs(d.knockForce) * 0.6f;
    if (after != null) after(t);
    PrefabUtility.SaveAsPrefabAsset(t, p); PrefabUtility.UnloadPrefabContents(t);
    return AssetDatabase.LoadAssetAtPath<GameObject>(p); };
var arrowTouch = touch("ArrowTouch", null);
var poisonTouch = touch("PoisonArrowTouch", t => { var d = t.GetComponent<DamageObject>(); d.isPosionAttack = true; d.poisonDamage = 4; d.damageInterval = 0.5f; d.damageCount = 6; });
System.Func<string, GameObject, float, float, float, float, float, Vector2, int, bool, Color, GameObject> arrow = (name, hit, dmg, speed, life, angle, jitter, spawn, pierce, isSkill, col) => {
    var g = new GameObject(name);
    g.layer = arrowTouch.layer;
    var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = arrowSp; sr.color = col; sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 2;
    var bc = g.AddComponent<BoxCollider2D>(); bc.isTrigger = true; bc.size = new Vector2(2.6f, 0.6f);
    var rb = g.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.useFullKinematicContacts = true;
    var a = g.AddComponent<StraightArrow>(); var so = new SerializedObject(a);
    so.FindProperty("_hitPrefab").objectReferenceValue = hit; so.FindProperty("_damageScale").floatValue = dmg; so.FindProperty("_speed").floatValue = speed;
    so.FindProperty("_life").floatValue = life; so.FindProperty("_angle").floatValue = angle; so.FindProperty("_angleJitter").floatValue = jitter;
    so.FindProperty("_spawnJitter").vector2Value = spawn; so.FindProperty("_pierce").intValue = pierce; so.FindProperty("_isSkill").boolValue = isSkill;
    so.ApplyModifiedPropertiesWithoutUndo();
    g.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
    var pf = PrefabUtility.SaveAsPrefabAsset(g, BF + name + ".prefab"); UnityEngine.Object.DestroyImmediate(g); return pf; };
var arrowBasic = arrow("ArrowBasic", arrowTouch, 1f, 16f, 0.55f, 0f, 0f, Vector2.zero, 0, false, Color.white);
var arrowSkill = arrow("ArrowSkill", arrowTouch, 0.75f, 18f, 0.6f, 0f, 6f, new Vector2(0f, 0.15f), 1, true, new Color(1f, 0.9f, 0.75f));
var arrowRain = arrow("ArrowRain", poisonTouch, 0.55f, 14f, 0.6f, -75f, 4f, new Vector2(1.8f, 0.4f), 1, true, new Color(0.7f, 1f, 0.55f)); // 앞 0~3.6유닛에 떨어진다(땅에 닿으면 사라짐)
var tripleCast = cast("BanditShortbow", "TripleShotCast", arrowSkill, 3, 0.3f, 0.35f);
var rainCast = cast("BanditShortbow", "PoisonVolleyCast", arrowRain, 6, 1f, 3f);
// 평타: DarkAttack 복제본 — 근접 판정·그림은 끄고 화살 한 발을 쏜다
string shotPath = BF + "BowShot.prefab";
if (!System.IO.File.Exists(shotPath)) AssetDatabase.CopyAsset(DO + "ScareStaff/DarkAttack.prefab", shotPath);
{
    if (!System.IO.File.Exists(DO + "ScareStaff/DarkAttack.prefab")) sb.Append("[DarkAttack 경로 확인] ");
    var r = PrefabUtility.LoadPrefabContents(shotPath); r.name = "BowShot";
    foreach (var s0 in r.GetComponentsInChildren<SpriteRenderer>(true)) s0.color = new Color(1f, 1f, 1f, 0f);
    foreach (var c in r.GetComponentsInChildren<Collider2D>(true)) c.enabled = false;
    var sp = r.GetComponent<SpawnOnStart>(); if (sp == null) sp = r.AddComponent<SpawnOnStart>();
    var so = new SerializedObject(sp); so.FindProperty("_prefab").objectReferenceValue = arrowBasic; so.FindProperty("_count").intValue = 1; so.FindProperty("_spacing").floatValue = 0f; so.FindProperty("_yOffset").floatValue = 0f; so.ApplyModifiedPropertiesWithoutUndo();
    PrefabUtility.SaveAsPrefabAsset(r, shotPath); PrefabUtility.UnloadPrefabContents(r);
}
var bowShot = AssetDatabase.LoadAssetAtPath<GameObject>(shotPath);
var bow = openCopy("ScareStaff", "BanditShortbow");
var bw = bow.GetComponent<Weapon>();
stats(bw, 12, 19, 10, 20, new Color(0.95f, 0.75f, 0.4f));
bw.skillDmg = 10;
bw.attackPrefab = new GameObject[]{ bowShot };
remake(bow, "BanditShortbow", new Color(1f, 0.75f, 0.45f, 1f), "BanditShortbow", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Triple Shot", 0f, null, null }},
    {"RSkill", new object[]{ "Poison Volley", 0f, null, null }},
    {"DSkill", new object[]{ "Pitch Shot", 0f, 1.1f, null }} });
{
    var L = bow.transform.Find("LSkill").GetComponent<Skill>(); L.skillName = "Triple Shot"; L.skillprefab = new GameObject[]{ tripleCast }; L.SkillCoolTime = 1.5f; L.skillWaitTime = 0.15f;
    var R = bow.transform.Find("RSkill").GetComponent<Skill>(); R.skillName = "Poison Volley"; R.skillprefab = new GameObject[]{ rainCast }; R.SkillCoolTime = 8f; R.skillWaitTime = 0.2f;
    L.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/Broken Arrow.png");
    R.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/BanditShortbow.png");
}
{   // 손에 든 지팡이 그림 → 활 아이콘(같은 대각선 구도). 아이콘 256px 은 원본 512px 의 절반이라 배율 2배
    var bowSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/BanditShortbow.png");
    foreach (var g in new[]{ bw.RightWeqponSprite, bw.leftWeqponSprite })
        if (g != null) foreach (var r in g.GetComponentsInChildren<SpriteRenderer>(true))
            if (r.sprite != null && r.sprite.name == "ScareStaff" && bowSp != null) { float k = r.sprite.rect.width / bowSp.rect.width * bowSp.pixelsPerUnit / r.sprite.pixelsPerUnit; r.sprite = bowSp; r.transform.localScale = new Vector3(r.transform.localScale.x * k, r.transform.localScale.y * k, r.transform.localScale.z); r.color = Color.white; }
}
sb.Append("BanditShortbow: " + skillList(bow) + "; ");
saveClose(bow);

// ── 3) Raider Buckler (Shell Shield 기반, Counter Stance = Shell Guard 재사용) ──
var iron = new Color(0.9f, 0.8f, 0.7f, 1f);
var rb1 = openCopy("ShellShield", "RaiderBuckler");
stats(rb1.GetComponent<Weapon>(), 12, 18, 5, 15, new Color(0.85f, 0.7f, 0.45f));
remake(rb1, "RaiderBuckler", iron, "RaiderBuckler", new System.Collections.Generic.Dictionary<string, object[]>{
    {"ASkill", new object[]{ "Raider Thrust", 3f, 1.15f, null }},
    {"BSkill", new object[]{ "Shield Shove", 4f, 1.3f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); d.knockForce *= 1.6f; d.stiffnessTime = Mathf.Max(d.stiffnessTime, 0.5f); }) }},
    {"CSkill", new object[]{ "Counter Stance", 6f, 1f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); if (d.isHoldingBuffObject) d.holdingBuffPower = 200; }) }} });
tintHands(rb1.GetComponent<Weapon>(), new Color(0.95f, 0.85f, 0.75f));
sb.Append("RaiderBuckler: " + skillList(rb1) + "; ");
saveClose(rb1);

// ── 4) Chief's War Banner (보스 유니크) + 산적 그림자 소환 ──
var runFrames = new System.Collections.Generic.List<Sprite>();
foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/BanditCutthroat/Run.png")) if (o is Sprite) runFrames.Add((Sprite)o);
runFrames.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
string CF = DO + "ChiefsWarBanner/";
System.IO.Directory.CreateDirectory(CF);
var shadow = new GameObject("BanditShadow");
{
    var sr = shadow.AddComponent<SpriteRenderer>(); sr.sprite = runFrames[0]; sr.color = new Color(0.42f, 0.32f, 0.5f, 0.85f);
    sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 1;
    var fb = shadow.AddComponent<SpriteFlipbook>(); fb.Frames = runFrames.ToArray();
    var fso = new SerializedObject(fb); fso.FindProperty("_frameTime").floatValue = 0.07f; fso.ApplyModifiedPropertiesWithoutUndo();
    var r = shadow.AddComponent<SummonRunner>(); var rso = new SerializedObject(r);
    rso.FindProperty("_speed").floatValue = 5.5f; rso.FindProperty("_life").floatValue = 2f; rso.FindProperty("_attackInterval").floatValue = 0.3f; rso.FindProperty("_damageScale").floatValue = 0.5f;
    rso.FindProperty("_attackPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "DoubleDagger/DaggerPowerSteb.prefab");
    rso.ApplyModifiedPropertiesWithoutUndo();
    shadow.transform.localScale = new Vector3(0.62f, 0.62f, 1f);
}
var shadowPf = PrefabUtility.SaveAsPrefabAsset(shadow, CF + "BanditShadow.prefab"); UnityEngine.Object.DestroyImmediate(shadow);
var rallyCast = cast("ChiefsWarBanner", "RallyCast", shadowPf, 3, 0.4f, -0.2f);
var wb = openCopy("Wood Spear", "ChiefsWarBanner");
var ww = wb.GetComponent<Weapon>();
stats(ww, 22, 32, 8, 25, new Color(0.95f, 0.3f, 0.25f));
remake(wb, "ChiefsWarBanner", new Color(1f, 0.6f, 0.55f, 1f), "ChiefsWarBanner", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Banner Thrust", 2f, 1.3f, null }},
    {"RSkill", new object[]{ "Raid Charge", 0f, 1.3f, null }} });
{
    var old = wb.transform.Find("FSkill"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    var staff = PrefabUtility.LoadPrefabContents(W + "ScareStaff.prefab");
    var srcSkill = staff.transform.Find("LSkill").GetComponent<Skill>();
    var fGo = new GameObject("FSkill"); fGo.transform.SetParent(wb.transform, false);
    var fSkill = fGo.AddComponent<Skill>();
    EditorUtility.CopySerialized(srcSkill, fSkill);
    PrefabUtility.UnloadPrefabContents(staff);
    var pivotBox = wb.transform.Find("PivotBox"); if (pivotBox == null) pivotBox = wb.transform;
    var piv = pivotBox.Find("SummonPivot");
    if (piv == null) { piv = new GameObject("SummonPivot").transform; piv.SetParent(pivotBox, false); piv.localPosition = new Vector3(0.6f, 0f, 0f); }
    fSkill.skillPivot = new GameObject[]{ piv.gameObject };
    fSkill.skillprefab = new GameObject[]{ rallyCast };
    fSkill.skillName = "Rally the Gang"; fSkill.SkillCoolTime = 9f; fSkill.skillWaitTime = 0.3f;
    fSkill.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/ChiefsWarBanner.png");
    var slots = ww.skill; if (slots.Length < 4) System.Array.Resize(ref slots, 4); slots[3] = fSkill; ww.skill = slots;
}
{   // 손에 든 창 그림 → 깃발 아이콘
    var bSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/ChiefsWarBanner.png");
    foreach (var g in new[]{ ww.RightWeqponSprite, ww.leftWeqponSprite })
        if (g != null) foreach (var r in g.GetComponentsInChildren<SpriteRenderer>(true))
            if (r.sprite != null && r.sprite.name == "Wood Spear" && bSp != null) { float k = r.sprite.rect.width / bSp.rect.width * bSp.pixelsPerUnit / r.sprite.pixelsPerUnit; r.sprite = bSp; r.transform.localScale = new Vector3(r.transform.localScale.x * k, r.transform.localScale.y * k, r.transform.localScale.z); r.color = Color.white; }
}
sb.Append("ChiefsWarBanner: " + skillList(wb) + "; ");
saveClose(wb);

// ── 5) 보스 보상 ──
string bp = "Assets/Resources/Boss/BanditChief_BD.asset";
var rw = AssetDatabase.LoadAssetAtPath<BossReward>(bp);
if (rw == null) { rw = ScriptableObject.CreateInstance<BossReward>(); AssetDatabase.CreateAsset(rw, bp); }
rw.displayName = "Bandit Chief";
rw.setPieces = new[]{ "CutthroatDagger", "BanditShortbow", "RaiderBuckler" };
rw.uniqueItem = "ChiefsWarBanner"; rw.uniqueChance = 0.08f; rw.pity = 12;
rw.stoneMin = 3; rw.stoneMax = 6; rw.firstKillStones = 6; rw.grantsRank = 0;
foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/BanditChief/Idle.png")) if (o is Sprite && o.name == "Idle_0") rw.portrait = (Sprite)o; // 사람형 초상 에셋이 없어 대장 도트 첫 프레임
if (rw.portrait == null) sb.Append("[초상 없음] ");
EditorUtility.SetDirty(rw);
AssetDatabase.SaveAssets();
sb.Append("boss reward");
return sb.ToString();
