// ⑥구역 탑 근교 계보 무기 (LineageGear 3절, TowerWorld 16절). 에디트 모드, 재실행 안전 (원본 공유 프리팹 미수정, 판정은 DamageObject/<무기>/ 복제본)
//  - Pebble Gauntlet (소형 골렘, Leather Knuckles 기반): A Rolling Charge = 대시 판정에 굴러 들이받는 피해·넉백
//  - Quakestone Maul (중형 골렘, SacreGreatSword 기반): S Quake Slam = 검기 강화, D Aftershock = CliffStaff 의 Cliff Quake 를 복사·강화(피해 1.6배, 1.25배 크기)
//  - Monolith Shield (대형 골렘, Shell Shield 기반): A Boulder Toss = 포물선 바위(StraightArrow _gravity), C Monolith Stance = 홀드 방어 +250 + 슈퍼아머
//  - Gatekeeper's Oath (문지기 레전더리, SacreGreatSword 기반): S Gate Cleave = 검기 + 땅을 미는 룬 충격파(관통 6), D Stone Sentinels = 굴러 돌격하는 소형 골렘 3(SummonRunner)
//  - 아이콘(gen_outskirts_items.py) 임포트, 보스 보상 Resources/Boss/Gatekeeper_BD.asset(칭호 1)
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/";
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
foreach (var n in new[]{ "PebbleGauntlet", "QuakestoneMaul", "MonolithShield", "GatekeepersOath", "Golem Pebble", "Quake Core", "Monolith Shard", "Ruin Mote" })
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

// 다른 무기의 스킬 오브젝트를 복사해 새 슬롯으로 붙인다(build_deep_forest_weapons 의 Ancient Wrath 방식: 원본과 같은 경로의 피벗·레이 시작점을 만든다)
System.Func<GameObject, string, string, string, int, Skill> addSkill = (wgo, newName, srcWeapon, srcSkillName, slot) => {
    var old0 = wgo.transform.Find(newName); if (old0 != null) UnityEngine.Object.DestroyImmediate(old0.gameObject);
    var srcRoot = PrefabUtility.LoadPrefabContents(W + srcWeapon + ".prefab");
    var srcSk = srcRoot.transform.Find(srcSkillName).GetComponent<Skill>();
    System.Func<Transform, Transform> ensure = t => {
        if (t == null) return null;
        string path = AnimationUtility.CalculateTransformPath(t, srcRoot.transform);
        Transform cur = wgo.transform, lcur = srcRoot.transform;
        foreach (var part in path.Split('/'))
        {
            lcur = lcur.Find(part);
            var nx = cur.Find(part);
            if (nx == null) { nx = new GameObject(part).transform; nx.SetParent(cur, false); nx.localPosition = lcur.localPosition; nx.localRotation = lcur.localRotation; nx.localScale = lcur.localScale; }
            cur = nx;
        }
        return cur; };
    var pivots = new System.Collections.Generic.List<GameObject>();
    if (srcSk.skillPivot != null) foreach (var pv in srcSk.skillPivot) { var e = ensure(pv != null ? pv.transform : null); pivots.Add(e != null ? e.gameObject : null); }
    var ray = srcSk.rayPositon != null ? ensure(srcSk.rayPositon.transform) : null;
    var go1 = new GameObject(newName); go1.transform.SetParent(wgo.transform, false);
    var sk = go1.AddComponent<Skill>();
    EditorUtility.CopySerialized(srcSk, sk);
    PrefabUtility.UnloadPrefabContents(srcRoot);
    sk.skillPivot = pivots.ToArray();
    if (ray != null) sk.rayPositon = ray.gameObject;
    var w0 = wgo.GetComponent<Weapon>();
    var slots = w0.skill; if (slots.Length <= slot) System.Array.Resize(ref slots, slot + 1); slots[slot] = sk; w0.skill = slots;
    return sk; };
var stoneTint = new Color(0.82f, 0.86f, 0.9f, 1f);
var runeTint = new Color(0.7f, 0.92f, 1f, 1f);
string OF = DO + "Outskirts/";
System.IO.Directory.CreateDirectory(OF);
System.Func<string, Sprite> firstSprite = p => { Sprite s0 = null; foreach (var o in AssetDatabase.LoadAllAssetsAtPath(p)) if (o is Sprite && (s0 == null || string.CompareOrdinal(o.name, s0.name) < 0)) s0 = (Sprite)o; return s0; };
// 보이지 않는 작은 피격 판정(Bandit Shortbow ArrowTouch 와 같은 원본)
System.Func<string, float, GameObject> touch = (name, knockMul) => {
    string p = OF + name + ".prefab";
    if (!System.IO.File.Exists(p)) AssetDatabase.CopyAsset(DO + "WispLantern/WispTouch.prefab", p);
    var t = PrefabUtility.LoadPrefabContents(p); t.name = name;
    // 원본 WispTouch 값 기준으로 다시 계산한다(재실행해도 넉백이 누적되지 않게)
    var src0 = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "WispLantern/WispTouch.prefab").GetComponent<DamageObject>();
    var d = t.GetComponent<DamageObject>(); d.maxDamagedEnemies = 1; d.knockForce = Mathf.Abs(src0.knockForce) * knockMul; d.stiffnessTime = Mathf.Max(src0.stiffnessTime, 0.35f);
    PrefabUtility.SaveAsPrefabAsset(t, p); PrefabUtility.UnloadPrefabContents(t);
    return AssetDatabase.LoadAssetAtPath<GameObject>(p); };
// StraightArrow 투사체(바위·충격파)
System.Func<string, Sprite, GameObject, float, float, float, float, float, int, bool, Vector2, float, GameObject> missile = (name, sp, hit, dmg, speed, life, angle, gravity, pierce, ignoreTerrain, box, scale) => {
    var g = new GameObject(name);
    g.layer = hit.layer;
    var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = sp; sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 2;
    var bc = g.AddComponent<BoxCollider2D>(); bc.isTrigger = true; bc.size = box;
    var rb = g.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.useFullKinematicContacts = true;
    var a = g.AddComponent<StraightArrow>(); var so = new SerializedObject(a);
    so.FindProperty("_hitPrefab").objectReferenceValue = hit; so.FindProperty("_damageScale").floatValue = dmg; so.FindProperty("_speed").floatValue = speed;
    so.FindProperty("_life").floatValue = life; so.FindProperty("_angle").floatValue = angle; so.FindProperty("_pierce").intValue = pierce;
    so.FindProperty("_isSkill").boolValue = true; so.FindProperty("_gravity").floatValue = gravity; so.FindProperty("_ignoreTerrain").boolValue = ignoreTerrain;
    so.ApplyModifiedPropertiesWithoutUndo();
    g.transform.localScale = new Vector3(scale, scale, 1f);
    var pf0 = PrefabUtility.SaveAsPrefabAsset(g, OF + name + ".prefab"); UnityEngine.Object.DestroyImmediate(g); return pf0; };

var rockTouch = touch("BoulderTouch", 1.6f);
var boulderSp = firstSprite("Assets/Art/Monsters/GolemLarge/Boulder.png");

// ── 1) Pebble Gauntlet (소형 골렘: 구르기 돌진) — Leather Knuckles 기반 ──
// 대시 판정(Dash)은 이동 표식이라 콜라이더가 없어 피해를 못 준다 → 대시와 함께 앞으로 구르는 돌(관통 4)을 SpawnOnStart 로 내보낸다
var rollStone = missile("RollingStone", boulderSp, rockTouch, 1.3f, 15f, 0.35f, 0f, 0f, 4, true, new Vector2(2.4f, 2.4f), 0.35f);
var pg = openCopy("Leather Knuckles", "PebbleGauntlet");
stats(pg.GetComponent<Weapon>(), 14, 21, 6, 12, new Color(0.7f, 0.85f, 0.95f));
remake(pg, "PebbleGauntlet", stoneTint, "PebbleGauntlet", new System.Collections.Generic.Dictionary<string, object[]>{
    // Dash 판정은 원본 피해 0(이동만) → 굴러 들이받는 피해·넉백을 준다
    {"LSkill", new object[]{ "Rolling Charge", 2.5f, 1f, (System.Action<GameObject>)(pr => {
        var sp = pr.GetComponent<SpawnOnStart>(); if (sp == null) sp = pr.AddComponent<SpawnOnStart>();
        var so = new SerializedObject(sp); so.FindProperty("_prefab").objectReferenceValue = rollStone; so.FindProperty("_count").intValue = 1; so.FindProperty("_spacing").floatValue = 0f; so.FindProperty("_yOffset").floatValue = 0.2f; so.ApplyModifiedPropertiesWithoutUndo(); }) }},
    {"RSkill", new object[]{ "Pebble Hop", 0f, 1f, null }},
    {"CSkill", new object[]{ "Rubble Uppercut", 0f, 1.2f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); d.knockForce *= 1.3f; }) }} });
tintHands(pg.GetComponent<Weapon>(), new Color(0.75f, 0.78f, 0.8f));
sb.Append("PebbleGauntlet: " + skillList(pg) + "; ");
saveClose(pg);

// ── 2) Quakestone Maul (중형 골렘: 지면 강타 + 충격파) — SacreGreatSword 기반 + D Aftershock = Cliff Quake 재사용 강화 ──
var qm = openCopy("SacreGreatSword", "QuakestoneMaul");
stats(qm.GetComponent<Weapon>(), 18, 28, 5, 20, new Color(0.55f, 0.85f, 1f));
var aft = addSkill(qm, "DSkill", "CliffStaff", "DSkill", 2);
var quakeSrcScale = aft.skillprefab[0] != null ? aft.skillprefab[0].transform.localScale : Vector3.one; // 원본 Cliff Quake 판정 크기(1.25배는 원본 기준 — 재실행 누적 방지)
remake(qm, "QuakestoneMaul", stoneTint, "QuakestoneMaul", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Tremor Pull", 0f, null, null }}, // AbsorbDark 원본에 누락 스크립트가 있어 복제 저장이 안 된다 → 원본 판정 그대로(이름만)
    {"RSkill", new object[]{ "Quake Slam", 3f, 1.35f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); d.knockForce *= 1.4f; d.stiffnessTime = Mathf.Max(d.stiffnessTime, 0.45f); }) }},
    {"DSkill", new object[]{ "Aftershock", 6f, 1.6f, (System.Action<GameObject>)(pr => { pr.transform.localScale = new Vector3(quakeSrcScale.x * 1.25f, quakeSrcScale.y * 1.25f, quakeSrcScale.z); var d = pr.GetComponent<DamageObject>(); d.stiffnessTime = Mathf.Max(d.stiffnessTime, 0.5f); }) }} });
aft.skillWaitTime = 0.3f;
tintHands(qm.GetComponent<Weapon>(), new Color(0.72f, 0.74f, 0.78f));
sb.Append("QuakestoneMaul: " + skillList(qm) + "; ");
saveClose(qm);

// ── 3) Monolith Shield (대형 골렘: 바위 투척 포물선 + 슈퍼아머 자세) — Shell Shield 기반 ──
var boulder = missile("BoulderToss", boulderSp, rockTouch, 1.6f, 12f, 1.6f, 38f, 24f, 2, false, new Vector2(2.2f, 2.2f), 0.4f);
var tossCast = cast("Outskirts", "BoulderTossCast", boulder, 1, 0f, 0.6f);
var ms = openCopy("ShellShield", "MonolithShield");
stats(ms.GetComponent<Weapon>(), 15, 22, 4, 15, new Color(0.6f, 0.8f, 0.9f));
remake(ms, "MonolithShield", stoneTint, "MonolithShield", new System.Collections.Generic.Dictionary<string, object[]>{
    {"ASkill", new object[]{ "Boulder Toss", 2.5f, null, null }},
    {"BSkill", new object[]{ "Granite Bash", 5f, 1.2f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); d.knockForce *= 1.4f; }) }},
    {"CSkill", new object[]{ "Monolith Stance", 6f, 1f, (System.Action<GameObject>)(pr => { var d = pr.GetComponent<DamageObject>(); if (d.isHoldingBuffObject) { d.holdingBuffPower = 250; d.addSuperArmor = true; } }) }} });
{
    var A = ms.transform.Find("ASkill").GetComponent<Skill>(); A.skillprefab = new GameObject[]{ tossCast }; A.skillWaitTime = 0.15f;
    A.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/Monolith Shard.png");
}
tintHands(ms.GetComponent<Weapon>(), new Color(0.78f, 0.8f, 0.84f));
sb.Append("MonolithShield: " + skillList(ms) + "; ");
saveClose(ms);

// ── 4) Gatekeeper's Oath (문지기 레전더리 특색 무기) — SacreGreatSword 기반 ──
//  R Gate Cleave = 검기 강화 + 땅을 타고 미는 룬 충격파(관통 6), D Stone Sentinels = 소형 골렘 3기가 굴러 돌격(SummonRunner)
var waveTouch = touch("ShockwaveTouch", 1.4f);
var waveSp = firstSprite("Assets/Art/Monsters/Gatekeeper/GateShockwave.png");
var wave = missile("OathShockwave", waveSp, waveTouch, 1.1f, 13f, 0.75f, 0f, 0f, 6, true, new Vector2(2f, 2.6f), 0.55f);
var rollFrames = new System.Collections.Generic.List<Sprite>();
foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/GolemSmall/Run.png")) if (o is Sprite) rollFrames.Add((Sprite)o);
rollFrames.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
var sentinel = new GameObject("StoneSentinel");
{
    var sr = sentinel.AddComponent<SpriteRenderer>(); sr.sprite = rollFrames[0]; sr.color = new Color(0.8f, 0.92f, 1f, 0.95f);
    sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 1;
    var fb = sentinel.AddComponent<SpriteFlipbook>(); fb.Frames = rollFrames.ToArray();
    var fso = new SerializedObject(fb); fso.FindProperty("_frameTime").floatValue = 0.06f; fso.ApplyModifiedPropertiesWithoutUndo();
    var r = sentinel.AddComponent<SummonRunner>(); var rso = new SerializedObject(r);
    rso.FindProperty("_speed").floatValue = 6.5f; rso.FindProperty("_life").floatValue = 1.8f; rso.FindProperty("_attackInterval").floatValue = 0.3f; rso.FindProperty("_damageScale").floatValue = 0.6f;
    rso.FindProperty("_attackPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "DoubleDagger/DaggerPowerSteb.prefab");
    rso.ApplyModifiedPropertiesWithoutUndo();
    sentinel.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
}
var sentinelPf = PrefabUtility.SaveAsPrefabAsset(sentinel, OF + "StoneSentinel.prefab"); UnityEngine.Object.DestroyImmediate(sentinel);
var sentinelCast = cast("Outskirts", "SentinelCast", sentinelPf, 3, 0.5f, -0.2f);
var go2 = openCopy("SacreGreatSword", "GatekeepersOath");
var gw = go2.GetComponent<Weapon>();
stats(gw, 26, 38, 10, 35, new Color(0.55f, 0.9f, 1f));
remake(go2, "GatekeepersOath", runeTint, "GatekeepersOath", new System.Collections.Generic.Dictionary<string, object[]>{
    {"LSkill", new object[]{ "Oath Bind", 0f, null, null }}, // AbsorbDark 원본 그대로(이름만)
    {"RSkill", new object[]{ "Gate Cleave", 4f, 1.5f, (System.Action<GameObject>)(pr => {
        var d = pr.GetComponent<DamageObject>(); d.knockForce *= 1.4f; d.stiffnessTime = Mathf.Max(d.stiffnessTime, 0.5f);
        var sp = pr.GetComponent<SpawnOnStart>(); if (sp == null) sp = pr.AddComponent<SpawnOnStart>();
        var so = new SerializedObject(sp); so.FindProperty("_prefab").objectReferenceValue = wave; so.FindProperty("_count").intValue = 1; so.FindProperty("_spacing").floatValue = 0f; so.FindProperty("_yOffset").floatValue = -0.3f; so.ApplyModifiedPropertiesWithoutUndo(); }) }} });
{
    var old2 = go2.transform.Find("DSkill"); if (old2 != null) UnityEngine.Object.DestroyImmediate(old2.gameObject);
    var staff = PrefabUtility.LoadPrefabContents(W + "ScareStaff.prefab");
    var srcSkill = staff.transform.Find("LSkill").GetComponent<Skill>();
    var dGo = new GameObject("DSkill"); dGo.transform.SetParent(go2.transform, false);
    var dSkill = dGo.AddComponent<Skill>();
    EditorUtility.CopySerialized(srcSkill, dSkill);
    PrefabUtility.UnloadPrefabContents(staff);
    var pivotBox = go2.transform.Find("PivotBox"); if (pivotBox == null) pivotBox = go2.transform;
    var piv = pivotBox.Find("SummonPivot");
    if (piv == null) { piv = new GameObject("SummonPivot").transform; piv.SetParent(pivotBox, false); piv.localPosition = new Vector3(0.6f, 0f, 0f); }
    dSkill.skillPivot = new GameObject[]{ piv.gameObject };
    dSkill.skillprefab = new GameObject[]{ sentinelCast };
    dSkill.skillName = "Stone Sentinels"; dSkill.SkillCoolTime = 10f; dSkill.skillWaitTime = 0.3f;
    dSkill.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/Golem Pebble.png");
    var slots = gw.skill; if (slots.Length < 3) System.Array.Resize(ref slots, 3); slots[2] = dSkill; gw.skill = slots;
}
tintHands(gw, new Color(0.75f, 0.9f, 1f));
sb.Append("GatekeepersOath: " + skillList(go2) + "; ");
saveClose(go2);

// ── 5) 보스 보상: 확정 1개 = 계보 무기 3종 중, 레전더리 Gatekeeper's Oath 6%·천장 15, 1층계 졸업 → 탐험가 칭호 1(서광 Dawn) ──
string bp = "Assets/Resources/Boss/Gatekeeper_BD.asset";
var rw = AssetDatabase.LoadAssetAtPath<BossReward>(bp);
if (rw == null) { rw = ScriptableObject.CreateInstance<BossReward>(); AssetDatabase.CreateAsset(rw, bp); }
rw.displayName = "Gatekeeper of the Tower";
rw.setPieces = new[]{ "PebbleGauntlet", "QuakestoneMaul", "MonolithShield" };
rw.uniqueItem = "GatekeepersOath"; rw.uniqueChance = 0.06f; rw.pity = 15;
rw.stoneMin = 4; rw.stoneMax = 7; rw.firstKillStones = 8; rw.grantsRank = 1;
foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Monsters/Gatekeeper/Idle.png")) if (o is Sprite && o.name == "Idle_0") rw.portrait = (Sprite)o; // 초상 에셋이 없어 문지기 도트 첫 프레임
if (rw.portrait == null) sb.Append("[초상 없음] ");
EditorUtility.SetDirty(rw);
// PassiveTerror(① 야간 임시 보스)의 칭호 1 승급은 1층계 졸업 보스로 옮긴다 (Q-413)
var pt = AssetDatabase.LoadAssetAtPath<BossReward>("Assets/Resources/Boss/PassiveTerror_BD.asset");
if (pt != null && pt.grantsRank == 1) { pt.grantsRank = 0; EditorUtility.SetDirty(pt); sb.Append("PassiveTerror rank 1→0; "); }
AssetDatabase.SaveAssets();
sb.Append("boss reward");
return sb.ToString();
