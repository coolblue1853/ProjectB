// 1층계 ⑥구역 "탑 근교"(Lv1_TowerOutskirts, TowerWorld 16절): 근교 원본 지형(Lv1_NearForest, x -437~-137)을 복제 → 돌·유적 회색 톤 + 룬 빛 Light2D,
// 스포너 교체(낮: 소형·중형·대형 골렘 / 밤: 골렘 + 유적 정령), 보스 스포너(탑의 문지기, 낮, 왼쪽 끝 -412, 영역 = 맵 전체, 2페이즈 부하 = 소형 골렘),
// 드롭(재료 30~35% + 계보 무기 2% + Stoneborn 부위 0.3%), MapInfo(부하 40), 포털: 늪지 오른쪽 끝 Portal_ToOutskirts ↔ 근교 오른쪽 Portal_OutskirtsToSwamp,
// 근교 왼쪽 끝 Portal_ToTower(→ 2층계 Lv2_MagicCliffs 입구, TowerGatePortal: 문지기 첫 처치 전에는 꺼짐). 에디트 모드, 재실행하면 복제본·포털을 지우고 다시 만든다
var sb = new System.Text.StringBuilder();
foreach (var p in new[]{ "Gatekeeper", "GolemSmall", "GolemMedium", "GolemLarge", "RuinSpirit" }) AssetDatabase.ImportAsset("Assets/Prefab/Enemy_BD/" + p + "_BD.prefab"); // BT GUID 텍스트 교체 반영
var mapRoot = GameObject.Find("Map").transform;
var src = mapRoot.Find("Lv1_NearForest");
var side = mapRoot.Find("Lv1_NearForest_SideRoad");
var swamp = mapRoot.Find("Lv1_Swamp");
var lv2 = mapRoot.Find("Lv2_MagicCliffs");
if (src == null || side == null || swamp == null || lv2 == null || src.Find("Portal/PotalLv1_NearForest2") == null || swamp.Find("Portal/Portal_SwampToDeep") == null || lv2.Find("Portal/Portal_ToLv1") == null)
    return "FAIL: 원본 맵/포털 없음 (아무것도 바꾸지 않음)";
var old = mapRoot.Find("Lv1_TowerOutskirts");
if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var oldP = swamp.Find("Portal/Portal_ToOutskirts");
if (oldP != null) UnityEngine.Object.DestroyImmediate(oldP.gameObject);

var copy = UnityEngine.Object.Instantiate(src.gameObject, mapRoot);
copy.name = "Lv1_TowerOutskirts";
copy.transform.position = src.position;
copy.SetActive(false);
var info = copy.GetComponent<MapInfo>();
info.displayName = "Tower Outskirts"; info.zoneTitle = "Tower Outskirts"; info.floor = 1; info.ascentLoad = 40; info.fireflies = false;

// 포털: 마을행(왼쪽 -425) 유지, 오른쪽 끝(PotalLv1_NearForest2 자리)을 늪지 귀환용으로, 왼쪽 끝 바깥(-432.5)에 탑 입구(2층계행)
Portal back = null, town = null;
foreach (var p in copy.GetComponentsInChildren<Portal>(true))
{
    if (p.name == "PotalLv1_NearForest") { p.closeMap = copy; town = p; continue; }
    if (p.name == "PotalLv1_NearForest2") { back = p; continue; }
    UnityEngine.Object.DestroyImmediate(p.gameObject);
}
var tpl = swamp.Find("Portal/Portal_SwampToDeep").GetComponent<Portal>();
var go = UnityEngine.Object.Instantiate(tpl.gameObject, tpl.transform.parent);
go.name = "Portal_ToOutskirts";
go.transform.position = new Vector3(-186f, tpl.transform.position.y, tpl.transform.position.z); // 늪지 오른쪽 끝(보스 자리 -200 너머, 벽 -179 안쪽)
var there = go.GetComponent<Portal>();
back.name = "Portal_OutskirtsToSwamp";
there.destination = back.transform; there.closeMap = swamp.gameObject; there.openMap = copy;
back.destination = there.transform; back.closeMap = copy; back.openMap = swamp.gameObject;
back.proCamera = tpl.proCamera; back.player = tpl.player;
// 탑 입구: 2층계 입구 포털(Portal_ToLv1) 자리로 도착. 2층계 쪽 귀환 포털은 그대로(내려가기는 자유 — 기존 샛길 연결 유지, Q-414)
var gate = UnityEngine.Object.Instantiate(back.gameObject, back.transform.parent);
gate.name = "Portal_ToTower";
gate.transform.position = new Vector3(-432.5f, town.transform.position.y, back.transform.position.z);
var gp = gate.GetComponent<Portal>();
gp.destination = lv2.Find("Portal/Portal_ToLv1"); gp.closeMap = copy; gp.openMap = lv2.gameObject;
foreach (var p in new[]{ go, back.gameObject, gate }) p.SetActive(true); // B-119: 꺼진 채 복제되지 않게(탑 입구는 TowerGatePortal 이 런타임에 끈다)
var holder = back.transform.parent.gameObject;
var tg = holder.GetComponent<TowerGatePortal>(); if (tg == null) tg = holder.AddComponent<TowerGatePortal>();
{ var so = new SerializedObject(tg); so.FindProperty("_portal").objectReferenceValue = gate; so.FindProperty("_bossName").stringValue = "Gatekeeper_BD"; so.ApplyModifiedPropertiesWithoutUndo(); }
sb.Append("portals swamp " + go.transform.position.ToString("F1") + " <-> " + back.transform.position.ToString("F1") + ", gate " + gate.transform.position.ToString("F1") + " -> " + gp.destination.position.ToString("F1") + "; ");

// 스포너 교체 (풀 구성은 몬스터 BT 템플릿과 같은 샛길 원본 스포너에서; extra 로 키별 프리팹 교체·추가)
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var f = typeof(EnemySpawner).GetField("objectInfos", B);
System.Func<string, EnemySpawner> srcSpawner = n => { foreach (var s in side.GetComponentsInChildren<EnemySpawner>(true)) if (s.name == n) return s; return null; };
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
string[] smallD = withSet("Stone", new[]{ "Golem Pebble", "0.35", "PebbleGauntlet", "0.02", "T0_Ore", "0.1", "Potion", "0.04" });
string[] medD = withSet("Stone", new[]{ "Quake Core", "0.33", "QuakestoneMaul", "0.02", "T0_Ore", "0.12", "Potion", "0.04" });
string[] largeD = withSet("Stone", new[]{ "Monolith Shard", "0.33", "MonolithShield", "0.02", "Potion", "0.05" });
string[] spiritD = withSet("Stone", new[]{ "Ruin Mote", "0.3", "Potion", "0.04" });
string[] bossD = { "Golem Pebble", "1", "Quake Core", "0.5", "Monolith Shard", "0.5", "Ruin Mote", "0.5", "Potion", "0.3" };
var quakeX = new System.Collections.Generic.Dictionary<string, object[]>{ {"leaf", new object[]{ pf("Assets/Prefab/Enemy/EnemyDamge/Outskirts/GolemQuake.prefab"), 6 }} };
var boulderX = new System.Collections.Generic.Dictionary<string, object[]>{ {"flower", new object[]{ pf("Assets/Prefab/Enemy/EnemyDamge/Outskirts/GolemBoulder.prefab"), 6 }} };
var boltX = new System.Collections.Generic.Dictionary<string, object[]>{ {"flower", new object[]{ pf("Assets/Prefab/Enemy/EnemyDamge/Outskirts/RuinBolt.prefab"), 6 }} };
// 원본 스포너 이름 → (새 이름, 템플릿 스포너, 몹, 드롭, 수, 판정 교체)
var map = new System.Collections.Generic.Dictionary<string, object[]>{
    {"Rabbit", new object[]{ "GolemSmall1", "RatThief", "GolemSmall", smallD, 5, null }},
    {"Rabbit2", new object[]{ "GolemMedium1", "Mole", "GolemMedium", medD, 3, quakeX }},
    {"Rabbit2_2", new object[]{ "GolemSmall2", "RatThief", "GolemSmall", smallD, 4, null }},
    {"Rabbit3", new object[]{ "GolemLarge1", "Flowermole1", "GolemLarge", largeD, 3, boulderX }},
    {"Mole", new object[]{ "GolemMedium2", "Mole", "GolemMedium", medD, 3, quakeX }},
    {"RatThief", new object[]{ "GolemSmall3", "RatThief", "GolemSmall", smallD, 4, null }},
    {"RatThief2", new object[]{ "RuinSpirit1", "Flowermole1", "RuinSpirit", spiritD, 5, boltX }},
    {"RatCheif", new object[]{ "GolemLarge2", "Flowermole1", "GolemLarge", largeD, 2, boulderX }},
    {"RatCheif2", new object[]{ "RuinSpirit2", "Flowermole1", "RuinSpirit", spiritD, 6, boltX }},
    {"ScareCrow", new object[]{ "GolemMedium3", "Mole", "GolemMedium", medD, 3, quakeX }} };
foreach (var s in copy.GetComponentsInChildren<EnemySpawner>(true))
{
    if (!map.ContainsKey(s.name)) { sb.Append("[유지 " + s.name + "] "); continue; }
    var m = map[s.name];
    sb.Append(swap(s, (string)m[1], (string)m[2], (System.Collections.Generic.Dictionary<string, object[]>)m[5]));
    setDrops(s, (string[])m[3]);
    s.enemyCount = Mathf.Min((int)m[4], s.enemyPositon.transform.childCount);
    s.name = (string)m[0];
    EditorUtility.SetDirty(s);
}

// 보스: 샛길 보스 스포너를 복제해 낮 스포너 밑으로, 스폰 지점 왼쪽 끝(-412, 1) — 탑 입구 앞을 지키는 문지기. 영역 = 맵 전체(x -470~-130)
var bossSrc = side.Find("NightTimeSpawn/BossSpawoner").GetComponent<EnemySpawner>();
var bossGo = UnityEngine.Object.Instantiate(bossSrc.gameObject, copy.transform.Find("DayTimeSpawn"));
bossGo.name = "BossSpawoner";
var boss = bossGo.GetComponent<EnemySpawner>();
var spawnPt = boss.enemyPositon.transform.GetChild(0);
bossGo.transform.position += new Vector3(-412f, 1f, 0f) - spawnPt.position;
boss.isDaytimeSpowner = true;
var stab = (GameObject)null;
{
    var rt = srcSpawner("RatThief"); var arr = (System.Array)f.GetValue(rt); var et = arr.GetType().GetElementType();
    foreach (var o in arr) if ((string)et.GetField("objectName").GetValue(o) == "stab") stab = (GameObject)et.GetField("perfab").GetValue(o);
}
sb.Append(swap(boss, "BossSpawoner", "Gatekeeper", new System.Collections.Generic.Dictionary<string, object[]>{
    {"clow", new object[]{ pf("Assets/Prefab/Enemy/EnemyDamge/Outskirts/GateCleave.prefab"), 1 }},
    {"fire", new object[]{ pf("Assets/Prefab/Enemy/EnemyDamge/Outskirts/GateShockwave.prefab"), 2 }},
    {"minion", new object[]{ pf("Assets/Prefab/Enemy_BD/GolemSmall_BD.prefab"), 2 }},
    {"minionDead", new object[]{ pf("Assets/Prefab/Enemy/Lv2/GolemSmallDeadBody.prefab"), 2 }},
    {"stab", new object[]{ stab, 3 }} }));
setDrops(boss, bossD);
var bbox = bossGo.GetComponent<BoxCollider2D>();
if (bbox != null)
{
    var ls = boss.transform.lossyScale;
    bbox.offset = new Vector2((-300f - boss.transform.position.x) / ls.x, bbox.offset.y);
    bbox.size = new Vector2(340f / ls.x, bbox.size.y);
}
sb.Append("boss @" + spawnPt.position.ToString("F1") + " area " + (bbox != null ? bbox.bounds.min.x.ToString("F0") + "~" + bbox.bounds.max.x.ToString("F0") + " y" + bbox.bounds.min.y.ToString("F0") + "~" + bbox.bounds.max.y.ToString("F0") : "-") + "; ");

// 돌·유적 회색 톤: 채도를 빼고 약간 푸른 회색
var tone = new Color(0.74f, 0.6f, 0.66f); // 풀 초록을 눌러 회갈색 돌빛으로(곱셈 색조라 채도는 못 뺀다)
foreach (var tm in copy.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true)) tm.color = tone;
System.Func<Color, float, Color> grey = (c, k) => { float l = (c.r + c.g + c.b) / 3f; return new Color(Mathf.Lerp(c.r, l, 0.7f) * k, Mathf.Lerp(c.g, l, 0.7f) * k, Mathf.Min(1f, Mathf.Lerp(c.b, l, 0.7f) * k * 1.05f), c.a); };
var par = copy.transform.Find("Parrelax");
if (par != null) foreach (var sr in par.GetComponentsInChildren<SpriteRenderer>(true)) sr.color = grey(sr.color, 0.66f);
foreach (var name in new[]{ "Vegetation", "Props", "Deco" })
{
    var t = copy.transform.Find(name);
    if (t != null) foreach (var sr in t.GetComponentsInChildren<SpriteRenderer>(true)) sr.color = grey(sr.color, 0.72f);
}
// 유적 룬 빛(청록, 약하게): 밤에 유적 윤곽
var runes = new GameObject("RuinRunes"); runes.transform.SetParent(copy.transform, false);
float[,] rpos = { { -418, 1 }, { -396, 1 }, { -372, 15 }, { -340, 3 }, { -314, 8 }, { -290, 2 }, { -262, 13 }, { -244, -2 } };
for (int i = 0; i < rpos.GetLength(0); i++)
{
    var lg = new GameObject("Rune" + i); lg.transform.SetParent(runes.transform, false);
    lg.transform.position = new Vector3(rpos[i, 0], rpos[i, 1] + 1.5f, 0f);
    var l = lg.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
    l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point; l.shadowsEnabled = false;
    { var lso = new SerializedObject(l); lso.FindProperty("m_NormalMapQuality").intValue = (int)UnityEngine.Rendering.Universal.Light2D.NormalMapQuality.Disabled; lso.ApplyModifiedPropertiesWithoutUndo(); }
    l.color = new Color(0.45f, 0.85f, 1f); l.intensity = 0.55f; l.pointLightOuterRadius = 4.5f; l.pointLightInnerRadius = 0.4f;
}
sb.Append("runes " + rpos.GetLength(0) + "; ");

// 왼쪽 끝 보이지 않는 벽: 근교 원본 지형은 왼쪽(-437) 너머에 벽이 없어 보스 넉백에 밀리면 맵 밖으로 떨어진다(플레이 검증에서 발견, TechDebt B-130)
var edge = new GameObject("EdgeWall_Left"); edge.transform.SetParent(copy.transform, false);
edge.layer = LayerMask.NameToLayer("Wall");
edge.transform.position = new Vector3(-438f, 20f, 0f);
var ebox = edge.AddComponent<BoxCollider2D>(); ebox.size = new Vector2(1f, 60f);
sb.Append("edge wall; ");

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(copy.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(copy.scene);
return sb.ToString();
