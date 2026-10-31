// 무기 계열 1차(WeaponTypes.md 5.1) 대표 무기 5종 빌더 — 편집 모드, 재실행 안전(원본 공유 프리팹 미수정, 판정은 DamageObject/<무기>/ 복제본)
//  HuntersLongbow(활, BanditShortbow 기반, ChargeShot) / GnarledClub(둔기, QuakestoneMaul 기반, StaggerOnHit)
//  WoodcuttersAxe(도끼, CrocJawAxe 기반, ArmorCleave) / TwinFang(쌍검, DoubleDagger 기반, 4타 + FinisherBonus) / VineLash(채찍, FrogTongueWhip 기반, 긴 판정 + TipBonus)
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/";
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
// 아이콘: 기존 아이콘 색조만 바꾼다(임시, 정식 도트는 후속)
System.Action<string, string, Color> icon = (src, dst, tint) => {
    string dp = "Assets/Resources/Item/" + dst + ".png";
    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
    t.LoadImage(System.IO.File.ReadAllBytes("Assets/Resources/Item/" + src + ".png"));
    var px = t.GetPixels();
    for (int i = 0; i < px.Length; i++) px[i] = new Color(Mathf.Clamp01(px[i].r * tint.r), Mathf.Clamp01(px[i].g * tint.g), Mathf.Clamp01(px[i].b * tint.b), px[i].a);
    t.SetPixels(px); t.Apply();
    System.IO.File.WriteAllBytes(dp, t.EncodeToPNG());
    AssetDatabase.ImportAsset(dp);
    var imp = (TextureImporter)AssetImporter.GetAtPath(dp);
    imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode;
    imp.SaveAndReimport(); };
icon("BanditShortbow", "HuntersLongbow", new Color(0.78f, 1f, 0.7f, 1f));
icon("QuakestoneMaul", "GnarledClub", new Color(1f, 0.8f, 0.55f, 1f));
icon("CrocJawAxe", "WoodcuttersAxe", new Color(1f, 0.78f, 0.6f, 1f));
icon("FangDagger", "TwinFang", new Color(0.7f, 0.85f, 1f, 1f));
icon("FrogTongueWhip", "VineLash", new Color(0.8f, 0.95f, 0.6f, 1f));
sb.Append("icons; ");

// 판정 복제: 폴더/폴더+원본이름.prefab, 배율·크기 조정
System.Func<GameObject, string, float, Color, System.Action<GameObject>, GameObject> cloneHit = (src, folder, ratioMul, tint, after) => {
    System.IO.Directory.CreateDirectory(DO + folder);
    string dst = DO + folder + "/" + folder + src.name + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    var d = pr.GetComponent<DamageObject>(); var sd = src.GetComponent<DamageObject>();
    if (d != null && sd != null) { d.dmgRatio = sd.dmgRatio * ratioMul; d.knockForce = sd.knockForce; d.knockbackDir = sd.knockbackDir; d.hitCount = sd.hitCount; d.maxDamagedEnemies = sd.maxDamagedEnemies; d.stiffnessTime = sd.stiffnessTime; d.holdingBuffPower = sd.holdingBuffPower; }
    pr.transform.localScale = src.transform.localScale; // 재실행 때 크기 누적 방지
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
System.Action<Weapon, int, int, int, int, Color> stats = (w, mn, mx, cp, cd, energy) => { w.minDmg = mn; w.maxDmg = mx; w.critPer = cp; w.critDmg = cd; w.energyColor = energy; };
System.Action<Weapon, string> setKind = (w, k) => { var so = new SerializedObject(w); so.FindProperty("_kind").enumValueIndex = (int)(WeaponKind)System.Enum.Parse(typeof(WeaponKind), k); so.ApplyModifiedPropertiesWithoutUndo(); };
// 스킬 슬롯 정리: 남길 스킬 오브젝트 이름 순서대로 슬롯 0.. 에 놓고 나머지는 지운다. 이름·쿨·배율(판정 복제본) 지정
System.Action<GameObject, string, Color, string, string[], object[][]> setSkills = (wgo, folder, tint, iconName, keep, spec) => {
    var w = wgo.GetComponent<Weapon>();
    var all = new System.Collections.Generic.List<Skill>(wgo.GetComponentsInChildren<Skill>(true));
    var kept = new Skill[4];
    for (int i = 0; i < keep.Length; i++) kept[i] = all.Find(s => s.name == keep[i]);
    foreach (var s in all) { bool k = System.Array.IndexOf(kept, s) >= 0; if (!k) UnityEngine.Object.DestroyImmediate(s.gameObject); }
    for (int i = 0; i < keep.Length; i++)
    {
        var s = kept[i]; var o = spec[i];
        s.skillName = (string)o[0]; if ((float)o[1] > 0f) s.SkillCoolTime = (float)o[1];
        var arr = s.skillprefab;
        if (o.Length > 3 && o[3] != null) arr = (GameObject[])o[3];
        for (int j = 0; j < arr.Length; j++)
        {
            if (arr[j] == null || arr[j].GetComponent<DamageObject>() == null || arr[j].name.StartsWith(folder)) continue;
            arr[j] = cloneHit(arr[j], folder, (float)o[2], tint, null);
        }
        s.skillprefab = arr;
        s.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/" + iconName + ".png");
    }
    w.skill = kept; };

// ── 1) Hunter's Longbow (활) ──
{
    string F = "HuntersLongbow";
    System.IO.Directory.CreateDirectory(DO + F);
    string arrowPath = DO + F + "/HuntersArrow.prefab", shotPath = DO + F + "/HuntersBowShot.prefab";
    if (!System.IO.File.Exists(arrowPath)) AssetDatabase.CopyAsset(DO + "BanditShortbow/ArrowBasic.prefab", arrowPath);
    var ar = PrefabUtility.LoadPrefabContents(arrowPath);
    var so = new SerializedObject(ar.GetComponent<StraightArrow>());
    so.FindProperty("_speed").floatValue = 20f; so.FindProperty("_life").floatValue = 0.8f; so.FindProperty("_damageScale").floatValue = 1.1f; so.ApplyModifiedPropertiesWithoutUndo();
    foreach (var r in ar.GetComponentsInChildren<SpriteRenderer>(true)) r.color = new Color(0.85f, 1f, 0.8f, 1f);
    PrefabUtility.SaveAsPrefabAsset(ar, arrowPath); PrefabUtility.UnloadPrefabContents(ar);
    if (!System.IO.File.Exists(shotPath)) AssetDatabase.CopyAsset(DO + "BanditShortbow/BowShot.prefab", shotPath);
    var sp = PrefabUtility.LoadPrefabContents(shotPath);
    var sso = new SerializedObject(sp.GetComponent<SpawnOnStart>()); sso.FindProperty("_prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(arrowPath); sso.ApplyModifiedPropertiesWithoutUndo();
    PrefabUtility.SaveAsPrefabAsset(sp, shotPath); PrefabUtility.UnloadPrefabContents(sp);
    var g = openCopy("BanditShortbow", F);
    var w = g.GetComponent<Weapon>();
    stats(w, 13, 20, 12, 25, new Color(0.7f, 1f, 0.6f));
    w.attackPrefab = new GameObject[]{ AssetDatabase.LoadAssetAtPath<GameObject>(shotPath) };
    w.attckSpeed = new float[]{ 0.55f };
    setSkills(g, F, new Color(0.85f, 1f, 0.8f, 1f), F, new[]{ "LSkill", "RSkill" }, new[]{
        new object[]{ "Hunter's Volley", 2f, 1f }, new object[]{ "Venom Rain", 8f, 1f } });
    if (g.GetComponent<ChargeShot>() == null) g.AddComponent<ChargeShot>();
    setKind(w, "Bow");
    tintHands(w, new Color(0.8f, 1f, 0.75f));
    sb.Append("HuntersLongbow; "); saveClose(g);
}

// ── 2) Gnarled Club (둔기) ──
{
    string F = "GnarledClub";
    var g = openCopy("QuakestoneMaul", F);
    var w = g.GetComponent<Weapon>();
    stats(w, 15, 23, 5, 20, new Color(0.85f, 0.65f, 0.35f));
    var brown = new Color(1f, 0.82f, 0.6f, 1f);
    System.Action<GameObject> thick = pr => { var d = pr.GetComponent<DamageObject>(); d.maxDamagedEnemies = 1; d.hitCount = 1; d.knockForce *= 1.5f; pr.transform.localScale = pr.transform.localScale * 0.68f; };
    var h = new GameObject[2];
    for (int i = 0; i < 2; i++) h[i] = cloneHit(w.attackPrefab[i], F, 1f, brown, thick);
    w.attackPrefab = h; w.attckSpeed = new float[]{ 0.65f, 0.9f };
    setSkills(g, F, brown, F, new[]{ "RSkill", "DSkill" }, new[]{
        new object[]{ "Club Smash", 4f, 0.8f }, new object[]{ "Root Tremor", 7f, 0.8f } });
    if (g.GetComponent<StaggerOnHit>() == null) g.AddComponent<StaggerOnHit>();
    setKind(w, "Mace");
    tintHands(w, new Color(1f, 0.8f, 0.6f));
    sb.Append("GnarledClub; "); saveClose(g);
}

// ── 3) Woodcutter's Axe (도끼) ──
{
    string F = "WoodcuttersAxe";
    var g = openCopy("CrocJawAxe", F);
    var w = g.GetComponent<Weapon>();
    stats(w, 14, 21, 6, 20, new Color(0.8f, 0.55f, 0.3f));
    var rust = new Color(1f, 0.8f, 0.62f, 1f);
    System.Action<GameObject> chop = pr => { var d = pr.GetComponent<DamageObject>(); d.maxDamagedEnemies = 2; d.hitCount = 1; pr.transform.localScale = pr.transform.localScale * 0.7f; };
    var h = new GameObject[2];
    for (int i = 0; i < 2; i++) h[i] = cloneHit(w.attackPrefab[i], F, 1f, rust, chop);
    w.attackPrefab = h; w.attckSpeed = new float[]{ 0.5f, 0.7f };
    setSkills(g, F, rust, F, new[]{ "LSkill", "RSkill (1)" }, new[]{
        new object[]{ "Timber Chop", 3f, 0.85f }, new object[]{ "Log Roller", 5f, 0.85f } });
    if (g.GetComponent<ArmorCleave>() == null) g.AddComponent<ArmorCleave>();
    setKind(w, "Axe");
    tintHands(w, new Color(1f, 0.8f, 0.65f));
    sb.Append("WoodcuttersAxe; "); saveClose(g);
}

// ── 4) Twin Fang (쌍검, 4타) ──
{
    string F = "TwinFang";
    var g = openCopy("DoubleDagger", F);
    var w = g.GetComponent<Weapon>();
    stats(w, 9, 14, 8, 20, new Color(0.6f, 0.8f, 1f));
    var steel = new Color(0.8f, 0.9f, 1f, 1f);
    var s0 = cloneHit(AssetDatabase.LoadAssetAtPath<GameObject>(DO + "DoubleDagger/DaggerSteb.prefab"), F, 0.9f, steel, null);
    var s2 = cloneHit(AssetDatabase.LoadAssetAtPath<GameObject>(DO + "DoubleDagger/DaggerVashDown.prefab"), F, 0.9f, steel, null);
    w.maxComboCount = 4;
    w.attackPrefab = new GameObject[]{ s0, s0, s2, s2 };
    w.attackPivot = new GameObject[]{ w.attackPivot[0], w.attackPivot[1], w.attackPivot[2], w.attackPivot[2] };
    w.attckSpeed = new float[]{ 0.25f, 0.25f, 0.25f, 0.4f };
    w.attackAnimName = new string[]{ "RightHandstabbing_OneHand", "LeftHandStab", "RightHandVashDown_OneHand", "CrossAttack" };
    w.spriteReverse = new bool[]{ false, false, false, false };
    var snd = w.weaponSound; w.weaponSound = new string[]{ snd[0], snd[1], snd[2], snd[2] };
    // 스킬: A 교차 4연(Vicious Assault 앞 4개), S 뒤로 돌아 찌르기(Cutthroat Backstab 판정 재사용)
    var cutt = AssetDatabase.LoadAssetAtPath<GameObject>(W + "CutthroatDagger.prefab");
    var cl = cutt.transform.Find("LSkill").GetComponent<Skill>();
    var vic = g.transform.Find("LSkill").GetComponent<Skill>();
    var four = new GameObject[]{ vic.skillprefab[0], vic.skillprefab[1], vic.skillprefab[2], vic.skillprefab[3] };
    setSkills(g, F, steel, F, new[]{ "LSkill", "RSkill" }, new[]{
        new object[]{ "Cross Fang", 3f, 0.9f, four }, new object[]{ "Turn and Thrust", 0f, 1f, new GameObject[]{ cl.skillprefab[0] } } });
    var rs = g.transform.Find("RSkill").GetComponent<Skill>();
    rs.skillAnim = cl.skillAnim; rs.skillWaitTime = cl.skillWaitTime; rs.attckSpeed = cl.attckSpeed; rs.SkillCoolTime = 4f; rs.isCancleAttack = cl.isCancleAttack;
    rs.skillprefab = new GameObject[]{ cl.skillprefab[0] }; // 원본 판정을 그대로 재사용(복제하지 않음)
    if (g.GetComponent<FinisherBonus>() == null) g.AddComponent<FinisherBonus>();
    setKind(w, "Dual");
    tintHands(w, new Color(0.8f, 0.9f, 1f));
    sb.Append("TwinFang " + w.attackPrefab.Length + "/" + w.attackPivot.Length + "/" + w.attckSpeed.Length + "/" + w.attackAnimName.Length + "/" + w.spriteReverse.Length + "/" + w.weaponSound.Length + "; "); saveClose(g);
}

// ── 5) Vine Lash (채찍) ──
{
    string F = "VineLash";
    var g = openCopy("FrogTongueWhip", F);
    var w = g.GetComponent<Weapon>();
    stats(w, 11, 17, 6, 15, new Color(0.55f, 0.85f, 0.35f));
    var vine = new Color(0.85f, 1f, 0.7f, 1f);
    System.Action<GameObject> longer = pr => { var s = pr.transform.localScale; pr.transform.localScale = new Vector3(s.x * WeaponGimmickRules.WhipLengthMul, s.y, s.z); };
    var lash = cloneHit(w.attackPrefab[0], F, 1f, vine, longer);
    w.maxComboCount = 2;
    w.attackPrefab = new GameObject[]{ lash, lash };
    w.attackPivot = new GameObject[]{ w.attackPivot[0], w.attackPivot[0] };
    w.attckSpeed = new float[]{ 0.45f, 0.5f };
    w.attackAnimName = new string[]{ w.attackAnimName[0], "RightHandVashDown_OneHand" };
    w.spriteReverse = new bool[]{ false, false };
    var snd = w.weaponSound; w.weaponSound = new string[]{ snd[0], snd[0] };
    setSkills(g, F, vine, F, new[]{ "LSkill", "RSkill" }, new[]{
        new object[]{ "Vine Pull", 2f, 1f }, new object[]{ "Pounce Lash", 0f, 1f } });
    if (g.GetComponent<TipBonus>() == null) g.AddComponent<TipBonus>();
    setKind(w, "Whip");
    tintHands(w, new Color(0.85f, 1f, 0.7f));
    sb.Append("VineLash; "); saveClose(g);
}
AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
return sb.ToString();
