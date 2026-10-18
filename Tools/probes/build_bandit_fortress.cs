// 1층계 ⑤구역 "산적 성채"(Lv1_BanditFortress, TowerWorld 15절): ②③④가 샛길·근교·Lv2 지형을 모두 썼으므로 외곽 샛길(Lv1_NearForest_SideRoad, x -753~-137) 레이아웃을 복제
// → 성채 색조(갈색·주황 횃불톤, 어둡게) + 고정 횃불 Light2D, 스포너 교체(낮: 단검병·궁수·방패병 / 밤: + 횃불 순찰병), 보스 스포너(산적 대장, 낮, 위쪽 망루 -533, 영역 = 맵 전체),
// 보스 풀에 부하(단검병 2 + 공격 stab + 시체) 추가, 궁수 탄(flower 키) → BanditArrow, 보스 판정(clow/fire 키) → ChiefCleave/ChiefAxe,
// 드롭(재료 30~35% + 계보 무기 2% + Outlaw/Raider 부위 0.3%), MapInfo, 포털 한 쌍(깊은 숲 가운데 Portal_ToFortress ↔ 성채 오른쪽 끝 Portal_FortressToDeep).
// 에디트 모드, 재실행하면 복제본·포털을 지우고 다시 만든다
var sb = new System.Text.StringBuilder();
foreach (var p in new[]{ "Assets/Prefab/Enemy_BD/BanditChief_BD.prefab", "Assets/Prefab/Enemy_BD/BanditArcher_BD.prefab" }) AssetDatabase.ImportAsset(p); // BT GUID 텍스트 교체 반영
var mapRoot = GameObject.Find("Map").transform;
var src = mapRoot.Find("Lv1_NearForest_SideRoad");
var deep = mapRoot.Find("Lv1_DeepForest");
if (src == null || deep == null || src.Find("Portal/PotalLv1_NearForest2") == null || deep.Find("Portal/Portal_DeepToNormal") == null)
    return "FAIL: 원본 맵/포털 없음 (아무것도 바꾸지 않음)";
var old = mapRoot.Find("Lv1_BanditFortress");
if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var oldP = deep.Find("Portal/Portal_ToFortress");
if (oldP != null) UnityEngine.Object.DestroyImmediate(oldP.gameObject);

var copy = UnityEngine.Object.Instantiate(src.gameObject, mapRoot);
copy.name = "Lv1_BanditFortress";
copy.transform.position = src.position;
copy.SetActive(false);
var info = copy.GetComponent<MapInfo>();
info.displayName = "Bandit Fortress"; info.zoneTitle = "Bandit Fortress"; info.floor = 1; info.ascentLoad = 30; info.fireflies = false;

// 포털: 마을행 유지, 오른쪽 끝(PotalLv1_NearForest2 자리)을 깊은 숲 귀환용으로, 나머지 삭제
Portal back = null;
foreach (var p in copy.GetComponentsInChildren<Portal>(true))
{
    if (p.name == "PotalLv1_NearForest") { p.closeMap = copy; continue; }
    if (p.name == "PotalLv1_NearForest2") { back = p; continue; }
    UnityEngine.Object.DestroyImmediate(p.gameObject);
}
var tpl = deep.Find("Portal/Portal_DeepToNormal").GetComponent<Portal>();
var go = UnityEngine.Object.Instantiate(tpl.gameObject, tpl.transform.parent);
go.name = "Portal_ToFortress";
go.transform.position = new Vector3(-305f, 1.3f, tpl.transform.position.z); // 깊은 숲 가운데 아래층(버섯 스폰 지면 0.7) — 진행 중 갈라지는 분기
var there = go.GetComponent<Portal>();
back.name = "Portal_FortressToDeep";
there.destination = back.transform; there.closeMap = deep.gameObject; there.openMap = copy;
back.destination = there.transform; back.closeMap = copy; back.openMap = deep.gameObject;
back.proCamera = tpl.proCamera; back.player = tpl.player;
foreach (var p in new[]{ go, back.gameObject }) p.SetActive(true); // B-119: 포털이 꺼진 채 복제되지 않게
sb.Append("portals " + go.transform.position.ToString("F1") + " <-> " + back.transform.position.ToString("F1") + " act " + go.activeSelf + "/" + back.gameObject.activeSelf + "; ");

// 스포너 교체 (풀 구성은 몬스터 BT 템플릿과 같은 샛길 원본 스포너에서; extra 로 키별 프리팹 교체·추가)
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var f = typeof(EnemySpawner).GetField("objectInfos", B);
System.Func<string, EnemySpawner> srcSpawner = n => { foreach (var s in src.GetComponentsInChildren<EnemySpawner>(true)) if (s.name == n) return s; return null; };
System.Func<string, GameObject> pf = p => AssetDatabase.LoadAssetAtPath<GameObject>(p);
var arrow = pf("Assets/Prefab/Enemy/EnemyDamge/Bandit/BanditArrow.prefab");
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
string[] cutD = withSet("Outlaw", new[]{ "Bandit Bandana", "0.35", "CutthroatDagger", "0.02", "Food", "0.1", "Potion", "0.04" });
string[] arcD = withSet("Outlaw", new[]{ "Broken Arrow", "0.35", "BanditShortbow", "0.02", "Potion", "0.04" });
string[] shdD = withSet("Raider", new[]{ "Iron Rivet", "0.35", "RaiderBuckler", "0.02", "T0_Ore", "0.1", "Potion", "0.04" });
string[] torD = withSet("Raider", new[]{ "Pitch Torch", "0.3", "Bandit Bandana", "0.1", "Potion", "0.04" });
string[] bossD = { "Bandit Bandana", "1", "Iron Rivet", "0.5", "Broken Arrow", "0.5", "Potion", "0.3" };
var arrowX = new System.Collections.Generic.Dictionary<string, object[]>{ {"flower", new object[]{ arrow, 6 }} };
// 원본 스포너 이름 → (새 이름, 템플릿 스포너, 몹, 드롭, 수)
var map = new System.Collections.Generic.Dictionary<string, object[]>{
    {"Rabbit", new object[]{ "Cutthroat1", "RatThief", "BanditCutthroat", cutD, 5 }},
    {"Rabbit2", new object[]{ "Archer1", "Flowermole1", "BanditArcher", arcD, 4 }},
    {"Rabbit2_2", new object[]{ "Shield1", "RatThief", "BanditShieldman", shdD, 3 }},
    {"Rabbit3", new object[]{ "Archer2", "Flowermole1", "BanditArcher", arcD, 4 }},
    {"Mole", new object[]{ "Cutthroat2", "RatThief", "BanditCutthroat", cutD, 4 }},
    {"Mole2", new object[]{ "Shield2", "RatThief", "BanditShieldman", shdD, 3 }},
    {"Mole3", new object[]{ "Cutthroat3", "RatThief", "BanditCutthroat", cutD, 5 }},
    {"Flowermole1", new object[]{ "Archer3", "Flowermole1", "BanditArcher", arcD, 5 }},
    {"Flowermole2", new object[]{ "Cutthroat4", "RatThief", "BanditCutthroat", cutD, 5 }},
    {"RatThief", new object[]{ "Torch1", "RatThief", "BanditTorchbearer", torD, 4 }},
    {"RatThief2", new object[]{ "Cutthroat5", "RatThief", "BanditCutthroat", cutD, 4 }},
    {"RatCheif", new object[]{ "Archer4", "Flowermole1", "BanditArcher", arcD, 4 }},
    {"RatCheif2", new object[]{ "Archer5", "Flowermole1", "BanditArcher", arcD, 4 }},
    {"ScareCrow", new object[]{ "Torch2", "RatThief", "BanditTorchbearer", torD, 4 }},
    {"ScareCrow2", new object[]{ "Shield3", "RatThief", "BanditShieldman", shdD, 3 }},
    {"ScareCrow3", new object[]{ "Torch3", "RatThief", "BanditTorchbearer", torD, 4 }},
    {"CreepingTerror", new object[]{ "Cutthroat6", "RatThief", "BanditCutthroat", cutD, 4 }},
    {"CreepingTerror2", new object[]{ "Torch4", "RatThief", "BanditTorchbearer", torD, 5 }} };
foreach (var s in copy.GetComponentsInChildren<EnemySpawner>(true))
{
    if (!map.ContainsKey(s.name)) { if (s.name != "BossSpawoner") sb.Append("[유지 " + s.name + "] "); continue; }
    var m = map[s.name];
    sb.Append(swap(s, (string)m[1], (string)m[2], (string)m[2] == "BanditArcher" ? arrowX : null));
    setDrops(s, (string[])m[3]);
    s.enemyCount = Mathf.Min((int)m[4], s.enemyPositon.transform.childCount);
    s.name = (string)m[0];
    EditorUtility.SetDirty(s);
}

// B-120: 스폰 지점이 영역 안쪽에 들어오는지(위아래 여유 2) 확인하고 모자라면 늘린다
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

// 보스: 원본 샛길 보스 스포너(밤, 위쪽 망루 -533/24.4)를 낮 스포너로 옮겨 산적 대장, 영역 = 맵 전체(x -755~-135)
var bossT = copy.transform.Find("NightTimeSpawn/BossSpawoner");
bossT.SetParent(copy.transform.Find("DayTimeSpawn"), true);
var boss = bossT.GetComponent<EnemySpawner>();
boss.isDaytimeSpowner = true;
var stab = (GameObject)null;
{
    var rt = srcSpawner("RatThief"); var arr = (System.Array)f.GetValue(rt); var et = arr.GetType().GetElementType();
    foreach (var o in arr) if ((string)et.GetField("objectName").GetValue(o) == "stab") stab = (GameObject)et.GetField("perfab").GetValue(o);
}
sb.Append(swap(boss, "BossSpawoner", "BanditChief", new System.Collections.Generic.Dictionary<string, object[]>{
    {"clow", new object[]{ pf("Assets/Prefab/Enemy/EnemyDamge/Bandit/ChiefCleave.prefab"), 1 }},
    {"fire", new object[]{ pf("Assets/Prefab/Enemy/EnemyDamge/Bandit/ChiefAxe.prefab"), 2 }},
    {"minion", new object[]{ pf("Assets/Prefab/Enemy_BD/BanditCutthroat_BD.prefab"), 2 }},
    {"minionDead", new object[]{ pf("Assets/Prefab/Enemy/Lv2/BanditCutthroatDeadBody.prefab"), 2 }},
    {"stab", new object[]{ stab, 3 }} }));
setDrops(boss, bossD);
var bbox = boss.GetComponent<BoxCollider2D>();
if (bbox != null)
{
    var ls = boss.transform.lossyScale;
    bbox.offset = new Vector2((-445f - boss.transform.position.x) / ls.x, (18f - boss.transform.position.y) / ls.y);
    bbox.size = new Vector2(620f / ls.x, 52f / ls.y);
}
sb.Append("boss @" + boss.enemyPositon.transform.GetChild(0).position.ToString("F1") + "; ");

// 성채 색조: 갈색·주황 횃불톤, 어둡게
var tone = new Color(0.66f, 0.5f, 0.4f);
foreach (var tm in copy.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true)) tm.color = tone;
var par = copy.transform.Find("Parrelax");
if (par != null) foreach (var sr in par.GetComponentsInChildren<SpriteRenderer>(true)) sr.color = new Color(sr.color.r * 0.62f, sr.color.g * 0.44f, sr.color.b * 0.34f, sr.color.a);
foreach (var name in new[]{ "Vegetation", "Props", "Deco" })
{
    var t = copy.transform.Find(name);
    if (t != null) foreach (var sr in t.GetComponentsInChildren<SpriteRenderer>(true)) sr.color = new Color(sr.color.r * 0.62f, sr.color.g * 0.48f, sr.color.b * 0.38f, sr.color.a);
}
// 고정 횃불 빛 (밤에 성채 윤곽이 주황으로 보이게): 낮·밤 스포너 지점 일부 위에 Light2D
var torches = new GameObject("FortressTorches"); torches.transform.SetParent(copy.transform, false);
float[,] tpos = { { -720, 5.1f }, { -666, 5.4f }, { -640, 6.5f }, { -599, 20.5f }, { -587, 31.3f }, { -533, 24.4f }, { -396, -0.2f }, { -380, 1.6f }, { -311, 6.9f }, { -297, 1.0f }, { -239, 17.9f }, { -227, -1.2f } };
for (int i = 0; i < tpos.GetLength(0); i++)
{
    var lg = new GameObject("Torch" + i); lg.transform.SetParent(torches.transform, false);
    lg.transform.position = new Vector3(tpos[i, 0], tpos[i, 1] + 2f, 0f);
    var l = lg.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
    l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point; l.shadowsEnabled = false;
    { var lso = new SerializedObject(l); lso.FindProperty("m_NormalMapQuality").intValue = (int)UnityEngine.Rendering.Universal.Light2D.NormalMapQuality.Disabled; lso.ApplyModifiedPropertiesWithoutUndo(); }
    l.color = new Color(1f, 0.6f, 0.28f); l.intensity = 0.7f; l.pointLightOuterRadius = 5f; l.pointLightInnerRadius = 0.5f;
}
sb.Append("torches " + tpos.GetLength(0) + "; ");

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(copy.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(copy.scene);
return sb.ToString();
