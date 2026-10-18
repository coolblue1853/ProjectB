// ⚠ Q-415(2026-10-19) 이후 몹 HP/방어·무기 피해·칭호(grantsRank)는 MonsterSheet.csv · apply_zone_tier_weapons.cs · Resources/Boss 가 기준이다. 이 프로브를 다시 돌리면 옛 값으로 되돌아가므로, 재실행 뒤 apply_zone_tier_weapons.cs 를 다시 돌리고 grantsRank(엔트 1·문지기 2·AbyssWizard 3)를 확인할 것.
// ③구역 깊은 숲 계보 무기 (TowerWorld 12·13절). 에디트 모드, 재실행 안전
//  - Bear Claw (곰): ScareClow 복제, 돌진 할퀴기·올려베기 판정 복제본을 넉백 1.5배·경직 0.5s·피해 1.3배로
//  - Root Staff (엔트): LeafStaff 복제, Rising Roots → Ancient Roots (큰 뿌리 DeepRoot, 기본 5줄기·최대 8줄기, 쿨 2s)
//  - Heartwood Greatsword (고대숲의 엔트 고유): SacreGreatSword 복제 + D 스킬 Ancient Wrath(거대 뿌리 8줄기 빠른 파도, 쿨 9s)
//  - 아이콘 임포트, Resources/Boss/AncientEnt_BD.asset (확정 보상·고유템 천장)
var sb = new System.Text.StringBuilder();
string W = "Assets/Resources/Weapon/", DO = W + "DamageObject/";
var refImp = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Item/Wood.png");
var refSet = new TextureImporterSettings(); refImp.ReadTextureSettings(refSet);
foreach (var n in new[]{ "BearClaw", "RootStaff", "HeartwoodGreatsword", "Bear Pelt", "Heartwood Chip", "Ectoplasm", "Fairy Dust" })
{
    string p = "Assets/Resources/Item/" + n + ".png";
    AssetDatabase.ImportAsset(p);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    imp.SetTextureSettings(refSet); imp.textureCompression = refImp.textureCompression; imp.filterMode = refImp.filterMode;
    imp.SaveAndReimport();
}
sb.Append("icons; ");

// 판정 프리팹 복제 + 수치 조정 (원본 공유 프리팹은 건드리지 않는다)
System.Func<GameObject, string, float, float, float, Color, GameObject> cloneHit = (src, folder, knockMul, stiff, ratioMul, tint) => {
    System.IO.Directory.CreateDirectory(DO + folder);
    string dst = DO + folder + "/" + folder + src.name + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    var d = pr.GetComponent<DamageObject>(); var sd = src.GetComponent<DamageObject>(); // 재실행해도 원본 기준으로 계산
    if (d != null) { d.knockForce = sd.knockForce * knockMul; d.stiffnessTime = Mathf.Max(sd.stiffnessTime, stiff); d.dmgRatio = sd.dmgRatio * ratioMul; }
    var srs = src.GetComponentsInChildren<SpriteRenderer>(true); var drs = pr.GetComponentsInChildren<SpriteRenderer>(true);
    for (int i = 0; i < drs.Length && i < srs.Length; i++) { var c = srs[i].color; drs[i].color = new Color(c.r * tint.r, c.g * tint.g, c.b * tint.b, c.a); }
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

// ── 1) Bear Claw ──
var bear = openCopy("ScareClow", "BearClaw");
var bw = bear.GetComponent<Weapon>();
bw.minDmg = 13; bw.maxDmg = 20; bw.critPer = 8; bw.critDmg = 25;
bw.energyColor = new Color(0.85f, 0.6f, 0.35f);
var brown = new Color(1f, 0.82f, 0.62f);
var made = new System.Collections.Generic.Dictionary<GameObject, GameObject>();
var bNames = new System.Collections.Generic.Dictionary<string, string>{ {"LSkill","Mauling Rush"}, {"RSkill","Rending Upswipe"}, {"Dskill","Bear Roar"} };
foreach (var s in bear.GetComponentsInChildren<Skill>(true))
{
    if (bNames.ContainsKey(s.name)) s.skillName = bNames[s.name];
    if (s.name == "Dskill") continue; // 공속 버프는 그대로 (포효)
    var arr = s.skillprefab;
    for (int i = 0; i < arr.Length; i++)
    {
        if (arr[i] == null || arr[i].GetComponent<DamageObject>() == null) continue;
        // 재실행이면 이미 BearClaw 복제본이 꽂혀 있다 → ScareClow 원본으로 되돌려 원본 기준으로 다시 계산 (수치 누적 방지)
        if (arr[i].name.StartsWith("BearClaw")) { var orig = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "ScareClow/" + arr[i].name.Substring(8) + ".prefab"); if (orig != null) arr[i] = orig; }
        if (!made.ContainsKey(arr[i])) made[arr[i]] = cloneHit(arr[i], "BearClaw", 1.5f, 0.5f, 1.3f, brown);
        arr[i] = made[arr[i]];
    }
    s.skillprefab = arr;
    s.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/BearClaw.png");
}
tintHands(bw, brown);
saveClose(bear);
sb.Append("BearClaw hits=" + made.Count + "; ");

// ── 2) DeepRoot (강화 뿌리) / GiantRoot (보스 고유) ──
var rootSrc = AssetDatabase.LoadAssetAtPath<GameObject>(DO + "LeafStaff/RootAttack.prefab");
System.Func<string, float, float, int, Color, GameObject> makeRoot = (nm, scale, ratio, hits, tint) => {
    System.IO.Directory.CreateDirectory(DO + "RootStaff");
    string dst = DO + "RootStaff/" + nm + ".prefab";
    if (!System.IO.File.Exists(dst)) AssetDatabase.CopyAsset(DO + "LeafStaff/RootAttack.prefab", dst);
    var pr = PrefabUtility.LoadPrefabContents(dst);
    pr.name = nm;
    var ls = rootSrc.transform.localScale; pr.transform.localScale = new Vector3(ls.x * scale, ls.y * scale, ls.z);
    var d = pr.GetComponent<DamageObject>(); d.dmgRatio = ratio; d.hitCount = hits; d.knockForce = 14f;
    var r = pr.GetComponent<SpriteRenderer>(); r.color = tint;
    PrefabUtility.SaveAsPrefabAsset(pr, dst); PrefabUtility.UnloadPrefabContents(pr);
    return AssetDatabase.LoadAssetAtPath<GameObject>(dst); };
var deepRoot = makeRoot("DeepRoot", 1.35f, 2.6f, 3, new Color(0.85f, 1f, 0.8f));
var giantRoot = makeRoot("GiantRoot", 1.9f, 3.2f, 3, new Color(0.75f, 0.9f, 0.85f));

// 다단 뿌리 스킬 배열 세팅: prefab[0] 은 쓰이지 않고 1..count 가 순서대로 나간다
System.Action<Skill, GameObject, GameObject, GameObject, int, float, float, float> setRoots = (s, prefab, pivot, ray, count, gap, step, first) => {
    var pf = new GameObject[count + 1]; var pv = new GameObject[count + 1];
    for (int i = 0; i <= count; i++) { pf[i] = prefab; pv[i] = pivot; }
    s.skillprefab = pf; s.skillPivot = pv; s.rayPositon = ray;
    var iv = new float[count]; var dir = new Vector2[count]; var dist = new float[count];
    iv[0] = 0.1f;
    for (int i = 0; i < count; i++) { if (i > 0) iv[i] = gap; dir[i] = Vector2.right; dist[i] = first + step * i; }
    s.interval = iv; s.direction = dir; s.distance = dist; };

// ── 3) Root Staff ──
var rs = openCopy("LeafStaff", "RootStaff");
var rw = rs.GetComponent<Weapon>();
rw.minDmg = 14; rw.maxDmg = 22; rw.critPer = 5; rw.critDmg = 10;
rw.energyColor = new Color(0.55f, 0.85f, 0.45f);
var rsk = rs.transform.Find("RSkill").GetComponent<Skill>();
setRoots(rsk, deepRoot, rsk.skillPivot[0], rsk.rayPositon, 8, 0.22f, 1.5f, 2f);
rsk.skillName = "Ancient Roots"; rsk.objectMaxCount = 6; rsk.isEffectMaxAdd = true; rsk.SkillCoolTime = 2f;
rsk.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/RootStaff.png");
rs.transform.Find("LSkill").GetComponent<Skill>().skillName = "Root Step";
rs.transform.Find("CSkill").GetComponent<Skill>().skillName = "Sap Mend";
tintHands(rw, new Color(0.8f, 0.7f, 0.55f));
saveClose(rs);
sb.Append("RootStaff; ");

// ── 4) Heartwood Greatsword (보스 고유) ──
var hg = openCopy("SacreGreatSword", "HeartwoodGreatsword");
var hw = hg.GetComponent<Weapon>();
hw.minDmg = 22; hw.maxDmg = 32; hw.critPer = 10; hw.critDmg = 30;
hw.energyColor = new Color(1f, 0.67f, 0.25f);
var old = hg.transform.Find("DSkill"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var leaf = PrefabUtility.LoadPrefabContents(W + "LeafStaff.prefab");
var srcSkill = leaf.transform.Find("RSkill").GetComponent<Skill>();
var pivotPath = AnimationUtility.CalculateTransformPath(srcSkill.skillPivot[0].transform, leaf.transform);
var rayPath = AnimationUtility.CalculateTransformPath(srcSkill.rayPositon.transform, leaf.transform);
System.Func<string, Transform> ensure = path => {
    Transform cur = hg.transform; Transform lcur = leaf.transform;
    foreach (var part in path.Split('/'))
    {
        lcur = lcur.Find(part);
        var nx = cur.Find(part);
        if (nx == null) { nx = new GameObject(part).transform; nx.SetParent(cur, false); nx.localPosition = lcur.localPosition; nx.localRotation = lcur.localRotation; nx.localScale = lcur.localScale; }
        cur = nx;
    }
    return cur; };
// LeafStaff 와 같은 경로의 피벗·레이 시작점을 대검에 만든다(이미 있으면 대검 것을 그대로 쓴다)
var hPivot = ensure(pivotPath); var hRay = ensure(rayPath);
var dGo = new GameObject("DSkill"); dGo.transform.SetParent(hg.transform, false);
var dSkill = dGo.AddComponent<Skill>();
EditorUtility.CopySerialized(srcSkill, dSkill);
PrefabUtility.UnloadPrefabContents(leaf);
setRoots(dSkill, giantRoot, hPivot.gameObject, hRay.gameObject, 8, 0.08f, 1.6f, 1.5f);
dSkill.skillName = "Ancient Wrath"; dSkill.isEffectMaxAdd = false; dSkill.objectMaxCount = 9; dSkill.SkillCoolTime = 9f; dSkill.skillWaitTime = 0.2f; dSkill.skillNum = 2;
dSkill.skillImage = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Item/HeartwoodGreatsword.png");
var slots = hw.skill; for (int i = 0; i < slots.Length; i++) if (slots[i] == null) slots[i] = null; // 지운 DSkill 의 Missing 참조 정리
if (slots.Length < 3) System.Array.Resize(ref slots, 3); slots[2] = dSkill; hw.skill = slots;
hg.transform.Find("LSkill").GetComponent<Skill>().skillName = "Sap Drain";
hg.transform.Find("RSkill").GetComponent<Skill>().skillName = "Grove Cleave";
tintHands(hw, new Color(0.75f, 0.9f, 0.85f));
saveClose(hg);
sb.Append("Heartwood (pivot " + pivotPath + ", ray " + rayPath + "); ");

// ── 5) 보스 보상 ──
string bp = "Assets/Resources/Boss/AncientEnt_BD.asset";
var br = AssetDatabase.LoadAssetAtPath<BossReward>(bp);
if (br == null) { br = ScriptableObject.CreateInstance<BossReward>(); AssetDatabase.CreateAsset(br, bp); }
br.displayName = "Ancient Ent";
br.setPieces = new[]{ "BearClaw", "RootStaff" };
br.uniqueItem = "HeartwoodGreatsword"; br.uniqueChance = 0.08f; br.pity = 12;
br.stoneMin = 3; br.stoneMax = 5; br.firstKillStones = 5; br.grantsRank = 0;
br.portrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/RPGMonstersIcon/default size/no shadow/4 Forest/forest_409.png");
if (br.portrait == null) sb.Append("[초상 없음] ");
EditorUtility.SetDirty(br);
AssetDatabase.SaveAssets();
sb.Append("boss reward");
return sb.ToString();
