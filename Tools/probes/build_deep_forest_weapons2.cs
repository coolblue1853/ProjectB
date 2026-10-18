// ⚠ Q-415(2026-10-19) 이후 몹 HP/방어·무기 피해·칭호(grantsRank)는 MonsterSheet.csv · apply_zone_tier_weapons.cs · Resources/Boss 가 기준이다. 이 프로브를 다시 돌리면 옛 값으로 되돌아가므로, 재실행 뒤 apply_zone_tier_weapons.cs 를 다시 돌리고 grantsRank(엔트 1·문지기 2·AbyssWizard 3)를 확인할 것.
// ③구역 깊은 숲 계보 무기 2차 (TowerWorld 12.2). 에디트 모드, 재실행 안전 (원본 판정 기준으로 다시 계산)
//  - Phantom Scythe (유령): ScareSide 복제. Phantom Reap = 관통 베기(최대 8명, 다 맞혀도 안 사라짐), Phantom Glide = 순간이동 베기 + PhantomVeil(무적 0.45s·잔상)
//  - Fae Wand (요정): ScareStaff 복제. Fairy Dust Bolt = 분홍 요정 가루 유도탄 4발(HomingOrb), Fae Blessing = 회복 장판(HealGround 복제, 분홍), Starlight Lance = 분홍 톤
//  - 아이콘·요정 가루 프레임 임포트(gen_deep_forest_gear2.py), 밤 유령·요정 스포너 드롭 2%
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/";
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
foreach (var n in new[]{ "PhantomScythe", "FaeWand" })
{
    string p = "Assets/Resources/Item/" + n + ".png";
    AssetDatabase.ImportAsset(p);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode;
    imp.SaveAndReimport();
}
// 요정 가루 프레임 (16px × 4, 16ppu)
string dustPath = "Assets/Art/Weapons/FaeDust.png";
AssetDatabase.ImportAsset(dustPath);
{
    var imp = (TextureImporter)AssetImporter.GetAtPath(dustPath);
    imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Multiple; imp.spritePixelsPerUnit = 16f;
    imp.filterMode = FilterMode.Point; imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false;
#pragma warning disable 618
    var metas = new SpriteMetaData[4];
    for (int i = 0; i < 4; i++) { metas[i].name = "FaeDust_" + i; metas[i].rect = new Rect(i * 16, 0, 16, 16); metas[i].alignment = (int)SpriteAlignment.Center; }
    imp.spritesheet = metas;
#pragma warning restore 618
    imp.SaveAndReimport();
}
sb.Append("icons; ");

// 판정 복제: 원본 폴더 src 의 프리팹을 dstFolder 로 복제 → 수치·색 조정 → after 로 추가 개조
System.Func<GameObject, string, float, Color, System.Action<GameObject>, GameObject> cloneHit = (src, folder, ratioMul, tint, after) => {
    System.IO.Directory.CreateDirectory(DO + folder);
    string dst = DO + folder + "/" + folder + src.name + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    var d = pr.GetComponent<DamageObject>(); var sd = src.GetComponent<DamageObject>();
    if (d != null && sd != null) { d.dmgRatio = sd.dmgRatio * ratioMul; }
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
// 재실행이면 복제본이 꽂혀 있다 → 원본 폴더의 같은 이름으로 되돌린다
System.Func<GameObject, string, string, GameObject> origOf = (g, prefix, srcFolder) => {
    if (g == null || !g.name.StartsWith(prefix)) return g;
    var o = AssetDatabase.LoadAssetAtPath<GameObject>(DO + srcFolder + "/" + g.name.Substring(prefix.Length) + ".prefab");
    return o != null ? o : g; };

// ── 1) Phantom Scythe ──
var ghostTint = new Color(0.8f, 0.72f, 1f, 0.78f);
var ps = openCopy("ScareSide", "PhantomScythe");
var pw = ps.GetComponent<Weapon>();
pw.minDmg = 14; pw.maxDmg = 22; pw.critPer = 10; pw.critDmg = 20;
pw.energyColor = new Color(0.72f, 0.6f, 1f);
var pNames = new System.Collections.Generic.Dictionary<string, string>{ {"LSkill","Phantom Reap"}, {"RSkill","Spectral Eruption"}, {"LSkill (1)","Wraith Whirl"}, {"RSkill (1)","Phantom Glide"} };
System.Action<GameObject> pierce = pr => { var d = pr.GetComponent<DamageObject>(); d.maxDamagedEnemies = 8; d.isDeletByMaxHit = false; };
System.Action<GameObject> veil = pr => { pierce(pr); if (pr.GetComponent<PhantomVeil>() == null) pr.AddComponent<PhantomVeil>(); };
foreach (var s in ps.GetComponentsInChildren<Skill>(true))
{
    if (pNames.ContainsKey(s.name)) s.skillName = pNames[s.name];
    var arr = s.skillprefab;
    for (int i = 0; i < arr.Length; i++)
    {
        arr[i] = origOf(arr[i], "PhantomScythe", "ScareSide");
        if (arr[i] == null || arr[i].GetComponent<DamageObject>() == null) continue;
        System.Action<GameObject> after = null; float mul = 1f;
        if (s.name == "LSkill") { after = pierce; mul = 1.15f; }
        else if (s.name == "RSkill (1)") { after = veil; }
        arr[i] = cloneHit(arr[i], "PhantomScythe", mul, ghostTint, after);
    }
    s.skillprefab = arr;
    s.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/PhantomScythe.png");
}
tintHands(pw, new Color(0.8f, 0.75f, 1f, 0.85f));
saveClose(ps);
sb.Append("PhantomScythe; ");

// ── 2) Fae Wand ──
string F = DO + "FaeWand/";
System.IO.Directory.CreateDirectory(F);
var touchGo = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "Common/OrbTouch.prefab"); // 보이지 않는 작은 판정 (수정하지 않고 참조만)
var dust = new System.Collections.Generic.List<Sprite>();
foreach (var o in AssetDatabase.LoadAllAssetsAtPath(dustPath)) if (o is Sprite) dust.Add((Sprite)o);
dust.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
var refSr = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "ScareStaff/ScareTotem.prefab").GetComponent<SpriteRenderer>();
var orb = new GameObject("FaeOrb");
{
    var sr = orb.AddComponent<SpriteRenderer>(); sr.sprite = dust[0]; sr.sortingLayerID = refSr.sortingLayerID; sr.sortingOrder = refSr.sortingOrder + 2;
    var fb = orb.AddComponent<SpriteFlipbook>(); fb.Frames = dust.ToArray();
    var fso = new SerializedObject(fb); fso.FindProperty("_frameTime").floatValue = 0.08f; fso.ApplyModifiedPropertiesWithoutUndo();
    var lg = new GameObject("Glow"); lg.transform.SetParent(orb.transform, false);
    var l = lg.AddComponent<UnityEngine.Rendering.Universal.Light2D>(); l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
    l.color = new Color(1f, 0.55f, 0.8f); l.intensity = 0.9f; l.pointLightOuterRadius = 1.3f; l.shadowsEnabled = false;
    orb.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
    var ho = orb.AddComponent<HomingOrb>(); var hso = new SerializedObject(ho);
    hso.FindProperty("_hitPrefab").objectReferenceValue = touchGo; hso.FindProperty("_damageScale").floatValue = 0.42f; hso.FindProperty("_speed").floatValue = 8f;
    hso.ApplyModifiedPropertiesWithoutUndo();
}
var orbPrefab = PrefabUtility.SaveAsPrefabAsset(orb, F + "FaeOrb.prefab"); UnityEngine.Object.DestroyImmediate(orb);
string castPath = F + "FaeBoltCast.prefab";
if (!System.IO.File.Exists(castPath)) AssetDatabase.CopyAsset(DO + "ScareStaff/SummonSacre.prefab", castPath);
{
    var r = PrefabUtility.LoadPrefabContents(castPath);
    var ss = r.GetComponent<SummonSkill>(); ss.summonObject = orbPrefab; ss.summonCount = 4; ss.summonDistance = 0.3f; ss.yPos = 0.4f;
    var s0 = r.GetComponent<SpriteRenderer>(); if (s0 != null) s0.color = new Color(1f, 1f, 1f, 0f);
    PrefabUtility.SaveAsPrefabAsset(r, castPath); PrefabUtility.UnloadPrefabContents(r);
}
var boltCast = AssetDatabase.LoadAssetAtPath<GameObject>(castPath);
var pink = new Color(1f, 0.7f, 0.88f, 1f);
var healSrc = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "LeafStaff/HealGround.prefab");
var faeHeal = cloneHit(healSrc, "FaeWand", 1f, pink, pr => { var d = pr.GetComponent<DamageObject>(); d.healCount = 60; }); // 소량: 원본(100틱)의 60%

var fw = openCopy("ScareStaff", "FaeWand");
var fww = fw.GetComponent<Weapon>();
fww.minDmg = 12; fww.maxDmg = 20; fww.skillDmg = 15; fww.critPer = 6;
fww.energyColor = new Color(1f, 0.55f, 0.8f);
var fsk = fw.GetComponentsInChildren<Skill>(true);
fsk[0].skillName = "Fairy Dust Bolt"; fsk[0].skillprefab = new GameObject[]{ boltCast }; fsk[0].SkillCoolTime = 1.2f; fsk[0].skillWaitTime = 0.2f;
fsk[0].skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/Fairy Dust.png");
fsk[1].skillName = "Fae Blessing"; fsk[1].skillprefab = new GameObject[]{ faeHeal }; fsk[1].SkillCoolTime = 12f;
// 회복 장판은 플레이어와 겹쳐야 회복된다 → LeafStaff 회복 스킬(CSkill)과 같은 경로의 피벗을 만들어 쓴다 (ScareStaff RSkill 피벗은 앞쪽이라 안 겹침)
{
    var leaf = PrefabUtility.LoadPrefabContents(W + "LeafStaff.prefab");
    var lp = leaf.transform.Find("CSkill").GetComponent<Skill>().skillPivot[0].transform;
    var lpath = AnimationUtility.CalculateTransformPath(lp, leaf.transform);
    Transform cur = fw.transform, lcur = leaf.transform;
    foreach (var part in lpath.Split('/'))
    {
        lcur = lcur.Find(part);
        var nx = cur.Find(part);
        if (nx == null) { nx = new GameObject(part).transform; nx.SetParent(cur, false); nx.localPosition = lcur.localPosition; nx.localRotation = lcur.localRotation; nx.localScale = lcur.localScale; }
        cur = nx;
    }
    PrefabUtility.UnloadPrefabContents(leaf);
    fsk[1].skillPivot = new GameObject[]{ cur.gameObject };
    sb.Append("healPivot " + lpath + "; ");
}
fsk[1].skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/FaeWand.png");
if (fsk.Length > 2)
{
    fsk[2].skillName = "Starlight Lance";
    var arr = fsk[2].skillprefab;
    for (int i = 0; i < arr.Length; i++)
    {
        arr[i] = origOf(arr[i], "FaeWand", "ScareStaff");
        if (arr[i] != null && arr[i].GetComponent<DamageObject>() != null) arr[i] = cloneHit(arr[i], "FaeWand", 1f, pink, null);
    }
    fsk[2].skillprefab = arr;
    fsk[2].skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/FaeWand.png");
}
tintHands(fww, new Color(1f, 0.8f, 0.92f));
saveClose(fw);
sb.Append("FaeWand skills=" + fsk.Length + "; ");

// ── 3) 드롭: 밤 유령 → Phantom Scythe 2%, 요정 → Fae Wand 2% ──
var map = GameObject.Find("Map").transform.Find("Lv1_DeepForest");
int nd = 0;
foreach (var dm in map.GetComponentsInChildren<DropManager>(true))
{
    string item = dm.name.StartsWith("Ghost") ? "PhantomScythe" : dm.name.StartsWith("Fairy") ? "FaeWand" : null;
    if (item == null) continue;
    var so = new SerializedObject(dm); var arr = so.FindProperty("dropItems");
    bool has = false; for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).FindPropertyRelative("itemName").stringValue == item) has = true;
    if (has) continue;
    arr.arraySize++; var e = arr.GetArrayElementAtIndex(arr.arraySize - 1);
    e.FindPropertyRelative("itemName").stringValue = item; e.FindPropertyRelative("dropProbability").floatValue = 0.02f; e.FindPropertyRelative("offsetY").floatValue = 0.5f;
    so.ApplyModifiedPropertiesWithoutUndo(); nd++;
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(map.gameObject.scene);
AssetDatabase.SaveAssets();
return sb.ToString() + "drops+" + nd;
