// 2층계 ②구역 "무너진 성벽"(Lv2_BrokenRampart, Floor2Plan 5.1, TowerWorld 17절): 외곽 샛길(Lv1_NearForest_SideRoad, x -753~-137) 레이아웃 복제
// → 하늘·흰 돌 색조 + 2층계 소품(gen_floor2_props.py: 총안 성벽·부서진 기둥·찢긴 깃발·구름), 스포너 교체(낮: 성벽 기사·미늘창 파수병·가고일 / 밤: 가고일·구름 정령·기사),
// 보스 스포너(가고일 군주, 밤, 위쪽 망루 -533, 영역 = 맵 전체, 2페이즈 가고일 2), 드롭(재료 30~35% + 계보 무기 2% + Rampart/Sky 부위 0.3%), MapInfo(floor 2, 부하 35),
// 포털: 마법 절벽 오른쪽 끝 Portal_ToRampart(x -186) ↔ 성벽 오른쪽 끝 Portal_RampartToCliffs. 에디트 모드, 재실행하면 복제본·포털을 지우고 다시 만든다
var sb = new System.Text.StringBuilder();
foreach (var p in new[]{ "GargoyleLord", "HalberdSentry", "Gargoyle", "CloudWisp", "RampartKnight" }) AssetDatabase.ImportAsset("Assets/Prefab/Enemy_BD/" + p + "_BD.prefab");
var mapGo = GameObject.Find("Map");
if (mapGo == null) return "FAIL: Map 없음";
var mapRoot = mapGo.transform;
var src = mapRoot.Find("Lv1_NearForest_SideRoad");
var lv2 = mapRoot.Find("Lv2_MagicCliffs");
if (src == null || lv2 == null || src.Find("Portal/PotalLv1_NearForest2") == null || lv2.Find("Portal/Portal_ToLv1") == null || src.Find("NightTimeSpawn/BossSpawoner") == null)
    return "FAIL: 원본 맵/포털 없음 (아무것도 바꾸지 않음)";
var old = mapRoot.Find("Lv2_BrokenRampart");
if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var oldP = lv2.Find("Portal/Portal_ToRampart");
if (oldP != null) UnityEngine.Object.DestroyImmediate(oldP.gameObject);

var copy = UnityEngine.Object.Instantiate(src.gameObject, mapRoot);
copy.name = "Lv2_BrokenRampart";
copy.transform.position = src.position;
copy.SetActive(false);
var info = copy.GetComponent<MapInfo>();
info.displayName = "Broken Rampart"; info.zoneTitle = "Broken Rampart"; info.floor = 2; info.ascentLoad = 35; info.fireflies = false;

// 포털: 마을행 유지(Q-417), 오른쪽 끝(PotalLv1_NearForest2 자리)을 마법 절벽 귀환용으로, 나머지(지름길 봉인 포함) 삭제
Portal back = null;
foreach (var p in copy.GetComponentsInChildren<Portal>(true))
{
    if (p.name == "PotalLv1_NearForest") { p.closeMap = copy; continue; }
    if (p.name == "PotalLv1_NearForest2") { back = p; continue; }
    UnityEngine.Object.DestroyImmediate(p.gameObject);
}
var tpl = lv2.Find("Portal/Portal_ToLv1").GetComponent<Portal>();
var go = UnityEngine.Object.Instantiate(tpl.gameObject, tpl.transform.parent);
go.name = "Portal_ToRampart";
go.transform.position = new Vector3(-186f, tpl.transform.position.y, tpl.transform.position.z); // 마법 절벽 오른쪽 끝(보스 제단 -200 너머, 벽 -179 안쪽 — 늪지 Portal_ToOutskirts 와 같은 자리)
foreach (var ps in go.GetComponents<PortalSeal>()) UnityEngine.Object.DestroyImmediate(ps);
var there = go.GetComponent<Portal>();
back.name = "Portal_RampartToCliffs";
there.destination = back.transform; there.closeMap = lv2.gameObject; there.openMap = copy;
back.destination = there.transform; back.closeMap = copy; back.openMap = lv2.gameObject;
back.proCamera = tpl.proCamera; back.player = tpl.player;
foreach (var p in new[]{ go, back.gameObject }) p.SetActive(true); // B-119
sb.Append("portals " + go.transform.position.ToString("F1") + " <-> " + back.transform.position.ToString("F1") + "; ");

// 스포너 교체 (풀 구성은 몬스터 BT 템플릿과 같은 샛길 원본 스포너에서; extra 로 키별 프리팹 교체·추가)
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var f = typeof(EnemySpawner).GetField("objectInfos", B);
System.Func<string, EnemySpawner> srcSpawner = n => { foreach (var s in src.GetComponentsInChildren<EnemySpawner>(true)) if (s.name == n) return s; return null; };
System.Func<string, GameObject> pf = p => AssetDatabase.LoadAssetAtPath<GameObject>(p);
System.Func<EnemySpawner, string, string, System.Collections.Generic.Dictionary<string, object[]>, string> swap = (target, tplName, mob, extra) => {
    var tp = srcSpawner(tplName);
    var prefab = pf("Assets/Prefab/Enemy_BD/" + mob + "_BD.prefab");
    var dead = pf("Assets/Prefab/Enemy/Lv2/" + mob + "DeadBody.prefab");
    if (tp == null || prefab == null) return target.name + " FAIL; ";
    var tArr = (System.Array)f.GetValue(tp);
    var et = tArr.GetType().GetElementType();
    var list = new System.Collections.ArrayList();
    var seen = new System.Collections.Generic.HashSet<string>();
    System.Action<string, GameObject, int> add = (key, p, cnt) => {
        var c = System.Activator.CreateInstance(et, true);
        et.GetField("objectName").SetValue(c, key); et.GetField("perfab").SetValue(c, p); et.GetField("count").SetValue(c, cnt);
        list.Add(c); seen.Add(key); };
    foreach (var o in tArr)
    {
        var n = (string)et.GetField("objectName").GetValue(o);
        var p = (GameObject)et.GetField("perfab").GetValue(o);
        int cnt = (int)et.GetField("count").GetValue(o);
        if (n == "enemy") p = prefab; else if (n == "dead" && dead != null) p = dead;
        if (extra != null && extra.ContainsKey(n)) { p = (GameObject)extra[n][0]; cnt = (int)extra[n][1]; }
        add(n, p, cnt);
    }
    if (extra != null) foreach (var kv in extra) if (!seen.Contains(kv.Key)) add(kv.Key, (GameObject)kv.Value[0], (int)kv.Value[1]);
    var arr = System.Array.CreateInstance(et, list.Count); list.CopyTo(arr);
    f.SetValue(target, arr);
    EditorUtility.SetDirty(target);
    return target.name + "->" + mob + "; ";
};
System.Func<string, string[], string[]> withSet = (key, list) => {
    var l = new System.Collections.Generic.List<string>(list);
    foreach (var part in new[]{ "Cap", "Armor", "LegArmor", "HandArmor", "Shoes" }) { l.Add(key + part); l.Add("0.003"); }
    return l.ToArray(); };
System.Action<EnemySpawner, string[]> setDrops = (s, list) => {
    var dm = s.GetComponent<DropManager>(); if (dm == null) return;
    var so = new SerializedObject(dm); var arr = so.FindProperty("dropItems"); arr.arraySize = list.Length / 2;
    for (int i = 0; i < list.Length / 2; i++) { var e = arr.GetArrayElementAtIndex(i); e.FindPropertyRelative("itemName").stringValue = list[i * 2]; e.FindPropertyRelative("dropProbability").floatValue = float.Parse(list[i * 2 + 1], System.Globalization.CultureInfo.InvariantCulture); e.FindPropertyRelative("offsetY").floatValue = 0.5f; }
    so.ApplyModifiedPropertiesWithoutUndo(); };
string[] knightD = withSet("Rampart", new[]{ "Rampart Shard", "0.35", "RampartSword", "0.02", "T0_Ore", "0.12", "Potion", "0.05" });
string[] sentryD = withSet("Rampart", new[]{ "Halberd Fragment", "0.33", "SentryHalberd", "0.02", "T0_Ore", "0.12", "Potion", "0.05" });
string[] gargD = withSet("Sky", new[]{ "Gargoyle Feather", "0.33", "GargoyleWing", "0.02", "Potion", "0.05" });
string[] wispD = withSet("Sky", new[]{ "Storm Wisp", "0.3", "Potion", "0.04" });
string[] bossD = { "Gargoyle Feather", "1", "Rampart Shard", "0.5", "Halberd Fragment", "0.5", "Storm Wisp", "0.5", "Potion", "0.3" };
var crescent = pf("Assets/Prefab/Enemy/EnemyDamge/Rampart/HalberdCrescent.prefab");
var feather = pf("Assets/Prefab/Enemy/EnemyDamge/Rampart/GargoyleFeather.prefab");
var spark = pf("Assets/Prefab/Enemy/EnemyDamge/Rampart/CloudSpark.prefab");
var talonPf = pf("Assets/Prefab/Enemy/EnemyDamge/Rampart/GargoyleTalon.prefab");
var shardPf = pf("Assets/Prefab/Enemy/EnemyDamge/Rampart/GargoyleShard.prefab");
var minionPf = pf("Assets/Prefab/Enemy_BD/Gargoyle_BD.prefab");
var minionDeadPf = pf("Assets/Prefab/Enemy/Lv2/GargoyleDeadBody.prefab");
if (crescent == null || feather == null || spark == null || talonPf == null || shardPf == null || minionPf == null || minionDeadPf == null) { UnityEngine.Object.DestroyImmediate(copy); UnityEngine.Object.DestroyImmediate(go); return "FAIL: 판정·부하 프리팹 없음 — build_rampart_mobs.cs 먼저 (복제본 되돌림, 씬 미저장)"; }
var sentryX = new System.Collections.Generic.Dictionary<string, object[]>{ {"leaf", new object[]{ crescent, 6 }} };
var featherX = new System.Collections.Generic.Dictionary<string, object[]>{ {"flower", new object[]{ feather, 6 }} };
var sparkX = new System.Collections.Generic.Dictionary<string, object[]>{ {"flower", new object[]{ spark, 6 }} };
// 원본 스포너 이름 → (새 이름, 템플릿 스포너, 몹, 드롭, 수, 판정 교체)
var map = new System.Collections.Generic.Dictionary<string, object[]>{
    {"Rabbit", new object[]{ "Knight1", "RatThief", "RampartKnight", knightD, 5, null }},
    {"Rabbit2", new object[]{ "Sentry1", "Mole", "HalberdSentry", sentryD, 3, sentryX }},
    {"Rabbit2_2", new object[]{ "Gargoyle1", "Flowermole1", "Gargoyle", gargD, 3, featherX }},
    {"Rabbit3", new object[]{ "Knight2", "RatThief", "RampartKnight", knightD, 4, null }},
    {"Mole", new object[]{ "Sentry2", "Mole", "HalberdSentry", sentryD, 3, sentryX }},
    {"Mole2", new object[]{ "Knight3", "RatThief", "RampartKnight", knightD, 4, null }},
    {"Mole3", new object[]{ "Sentry3", "Mole", "HalberdSentry", sentryD, 3, sentryX }},
    {"Flowermole1", new object[]{ "Gargoyle2", "Flowermole1", "Gargoyle", gargD, 4, featherX }},
    {"Flowermole2", new object[]{ "Knight4", "RatThief", "RampartKnight", knightD, 5, null }},
    {"RatThief", new object[]{ "Gargoyle3", "Flowermole1", "Gargoyle", gargD, 4, featherX }},
    {"RatThief2", new object[]{ "Wisp1", "Flowermole1", "CloudWisp", wispD, 4, sparkX }},
    {"RatCheif", new object[]{ "Knight5", "RatThief", "RampartKnight", knightD, 3, null }},
    {"RatCheif2", new object[]{ "Gargoyle4", "Flowermole1", "Gargoyle", gargD, 4, featherX }},
    {"ScareCrow", new object[]{ "Wisp2", "Flowermole1", "CloudWisp", wispD, 4, sparkX }},
    {"ScareCrow2", new object[]{ "Gargoyle5", "Flowermole1", "Gargoyle", gargD, 4, featherX }},
    {"ScareCrow3", new object[]{ "Wisp3", "Flowermole1", "CloudWisp", wispD, 4, sparkX }},
    {"CreepingTerror", new object[]{ "Knight6", "RatThief", "RampartKnight", knightD, 4, null }},
    {"CreepingTerror2", new object[]{ "Wisp4", "Flowermole1", "CloudWisp", wispD, 5, sparkX }} };
foreach (var s in copy.GetComponentsInChildren<EnemySpawner>(true))
{
    if (!map.ContainsKey(s.name)) { if (s.name != "BossSpawoner") sb.Append("[유지 " + s.name + "] "); continue; }
    var m = map[s.name];
    sb.Append(swap(s, (string)m[1], (string)m[2], (System.Collections.Generic.Dictionary<string, object[]>)m[5]));
    setDrops(s, (string[])m[3]);
    s.enemyCount = Mathf.Min((int)m[4], s.enemyPositon.transform.childCount);
    s.name = (string)m[0];
    EditorUtility.SetDirty(s);
}

// B-120: 스폰 지점이 영역 안쪽에 들어오는지(위아래 여유) 확인하고 모자라면 늘린다
foreach (var s in copy.GetComponentsInChildren<EnemySpawner>(true))
{
    var bx = s.GetComponent<BoxCollider2D>(); if (bx == null || s.name == "BossSpawoner") continue;
    float lo = float.MaxValue, hi = float.MinValue;
    foreach (Transform c in s.enemyPositon.transform) { lo = Mathf.Min(lo, c.position.y); hi = Mathf.Max(hi, c.position.y); }
    float sy = s.transform.lossyScale.y;
    float cy = s.transform.position.y + bx.offset.y * sy, h = bx.size.y * sy;
    float bLo = cy - h / 2f, bHi = cy + h / 2f;
    float nLo = Mathf.Min(bLo, lo - 2f), nHi = Mathf.Max(bHi, hi + 3f);
    if (nLo < bLo - 0.01f || nHi > bHi + 0.01f)
    {
        bx.size = new Vector2(bx.size.x, (nHi - nLo) / sy);
        bx.offset = new Vector2(bx.offset.x, ((nLo + nHi) / 2f - s.transform.position.y) / sy);
        sb.Append(s.name + " box y " + nLo.ToString("F0") + "~" + nHi.ToString("F0") + "; ");
    }
}

// 보스: 원본 샛길 보스 스포너(밤, 위쪽 망루 -533/24.4) 그대로 가고일 군주(석상은 밤에 깨어난다, Q-417), 영역 = 맵 전체(x -755~-135)
var boss = copy.transform.Find("NightTimeSpawn/BossSpawoner").GetComponent<EnemySpawner>();
boss.isDaytimeSpowner = false;
sb.Append(swap(boss, "BossSpawoner", "GargoyleLord", new System.Collections.Generic.Dictionary<string, object[]>{
    {"clow", new object[]{ talonPf, 1 }},
    {"fire", new object[]{ shardPf, 2 }},
    {"minion", new object[]{ minionPf, 2 }},
    {"minionDead", new object[]{ minionDeadPf, 2 }},
    {"flower", new object[]{ feather, 4 }} }));
setDrops(boss, bossD);
var bbox = boss.GetComponent<BoxCollider2D>();
if (bbox != null)
{
    var ls = boss.transform.lossyScale;
    bbox.offset = new Vector2((-445f - boss.transform.position.x) / ls.x, (18f - boss.transform.position.y) / ls.y);
    bbox.size = new Vector2(620f / ls.x, 52f / ls.y);
}
sb.Append("boss @" + boss.enemyPositon.transform.GetChild(0).position.ToString("F1") + "; ");

// 하늘·흰 돌 색조: 풀 초록을 눌러 푸른 회백색, 원경은 채도를 빼고 푸른 안개
var tone = new Color(0.86f, 0.9f, 1f);
foreach (var tm in copy.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true)) tm.color = tone;
System.Func<Color, float, Color> haze = (c, k) => { float l = (c.r + c.g + c.b) / 3f; return new Color(Mathf.Lerp(c.r, l, 0.75f) * k * 0.92f, Mathf.Lerp(c.g, l, 0.75f) * k * 0.97f, Mathf.Min(1f, Mathf.Lerp(c.b, l, 0.75f) * k * 1.1f), c.a); };
var par = copy.transform.Find("Parrelax");
if (par != null) foreach (var sr in par.GetComponentsInChildren<SpriteRenderer>(true)) sr.color = haze(sr.color, sr.name.Contains("cloud") ? 1f : 0.85f);
foreach (var name in new[]{ "Vegetation", "Props", "Deco" })
{
    var t = copy.transform.Find(name);
    if (t != null) foreach (var sr in t.GetComponentsInChildren<SpriteRenderer>(true)) sr.color = haze(sr.color, 0.9f);
}
{ var dt = copy.transform.Find("Deco"); if (dt != null) { var tm = dt.GetComponent<UnityEngine.Tilemaps.Tilemap>(); if (tm != null) tm.color = haze(Color.white, 0.92f); } }

// 2층계 소품: 지면 레이캐스트(같은 지형의 샛길 원본 콜라이더 사용)로 총안 성벽·부서진 기둥·깃발을 세우고, 하늘에 구름
string PR = "Assets/Art/Floor2/";
var propNames = new[]{ "RampartWall", "RampartWallBroken", "BrokenPillar", "TornBanner", "Cloud1", "Cloud2" };
var sprites = new System.Collections.Generic.Dictionary<string, Sprite>();
foreach (var n in propNames)
{
    string p = PR + n + ".png";
    AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceSynchronousImport);
    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
    imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single; imp.filterMode = FilterMode.Point;
    imp.textureCompression = TextureImporterCompression.Uncompressed; imp.mipmapEnabled = false; imp.spritePixelsPerUnit = 24;
    var st = new TextureImporterSettings(); imp.ReadTextureSettings(st); st.spriteAlignment = (int)SpriteAlignment.BottomCenter; imp.SetTextureSettings(st);
    imp.SaveAndReimport();
    sprites[n] = AssetDatabase.LoadAssetAtPath<Sprite>(p);
}
var propRoot = new GameObject("RampartProps"); propRoot.transform.SetParent(copy.transform, false);
var rng = new System.Random(23);
var groundMask = LayerMask.GetMask("Ground");
Physics2D.SyncTransforms();
int placed = 0, clouds = 0;
for (float x = -748f; x < -142f; x += 7f + (float)rng.NextDouble() * 6f)
{
    // 같은 좌표에 겹친 다른 맵(마법 절벽 등)이 켜져 있어도 샛길 원본 지형만 본다(리뷰 반영)
    RaycastHit2D hit = new RaycastHit2D(); bool found = false;
    foreach (var h in Physics2D.RaycastAll(new Vector2(x, 45f), Vector2.down, 80f, groundMask))
    {
        if (h.collider != null && h.collider.transform.IsChildOf(src)) { hit = h; found = true; break; }
    }
    if (!found) continue;
    double roll = rng.NextDouble();
    string n = roll < 0.32 ? "RampartWall" : roll < 0.6 ? "RampartWallBroken" : roll < 0.85 ? "BrokenPillar" : "TornBanner";
    var g = new GameObject(n); g.transform.SetParent(propRoot.transform, false);
    g.transform.position = new Vector3(x, hit.point.y - 0.05f, 0f);
    var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = sprites[n]; sr.sortingOrder = -2;
    if (rng.NextDouble() < 0.5) sr.flipX = true;
    placed++;
    if (rng.NextDouble() < 0.45)
    {
        string cn = rng.NextDouble() < 0.5 ? "Cloud1" : "Cloud2";
        var c = new GameObject(cn); c.transform.SetParent(propRoot.transform, false);
        c.transform.position = new Vector3(x + (float)rng.NextDouble() * 6f, hit.point.y + 7f + (float)rng.NextDouble() * 6f, 0f);
        var cr = c.AddComponent<SpriteRenderer>(); cr.sprite = sprites[cn]; cr.sortingOrder = -15; cr.color = new Color(1f, 1f, 1f, 0.75f);
        clouds++;
    }
}
if (placed == 0) sb.Append("[WARN 소품 0개 — 샛길 원본이 꺼져 있으면 지면 레이캐스트가 안 맞는다] ");
sb.Append("props " + placed + " clouds " + clouds + "; ");
// 성벽 위 달빛(밤에 흰 돌 윤곽)
var lights = new GameObject("RampartMoonlight"); lights.transform.SetParent(copy.transform, false);
float[,] lpos = { { -720, 5.1f }, { -640, 6.5f }, { -587, 31.3f }, { -533, 24.4f }, { -396, -0.2f }, { -311, 6.9f }, { -239, 17.9f } };
for (int i = 0; i < lpos.GetLength(0); i++)
{
    var lg = new GameObject("Moon" + i); lg.transform.SetParent(lights.transform, false);
    lg.transform.position = new Vector3(lpos[i, 0], lpos[i, 1] + 3f, 0f);
    var l = lg.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
    l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point; l.shadowsEnabled = false;
    { var lso = new SerializedObject(l); lso.FindProperty("m_NormalMapQuality").intValue = (int)UnityEngine.Rendering.Universal.Light2D.NormalMapQuality.Disabled; lso.ApplyModifiedPropertiesWithoutUndo(); }
    l.color = new Color(0.72f, 0.82f, 1f); l.intensity = 0.5f; l.pointLightOuterRadius = 7f; l.pointLightInnerRadius = 1f;
}
sb.Append("moonlights " + lpos.GetLength(0) + "; ");

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(copy.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(copy.scene);
return sb.ToString();
