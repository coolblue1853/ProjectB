// 1층계 ③구역 "깊은 숲"(Lv1_DeepForest): 쓰지 않던 원본 근교 지형(Lv1_NearForest)을 복제 → 돌 유적 색조(어두운 청록·회색),
// 스포너 교체(낮: 숲곰·엔트·고블린·포자버섯 / 밤: 유령·요정), 보스 스포너(고대숲의 엔트, 낮, 왼쪽 끝), 드롭 테이블, MapInfo,
// 포털 한 쌍(일반 숲 왼쪽 끝 Portal_ToDeepForest ↔ 깊은 숲 오른쪽 Portal_DeepToNormal). 에디트 모드, 재실행하면 복제본을 지우고 다시 만든다
var sb = new System.Text.StringBuilder();
var mapRoot = GameObject.Find("Map").transform;
var src = mapRoot.Find("Lv1_NearForest");
var side = mapRoot.Find("Lv1_NearForest_SideRoad");
var normal = mapRoot.Find("Lv1_NormalForest");
if (src == null || side == null || normal == null || src.GetComponent<MapInfo>() == null || src.Find("Portal/PotalLv1_NearForest2") == null || normal.Find("Portal/Portal_ToSideRoad") == null)
    return "FAIL: 원본 맵/포털 없음 (아무것도 바꾸지 않음)";
var old = mapRoot.Find("Lv1_DeepForest");
if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var oldP = normal.Find("Portal/Portal_ToDeepForest");
if (oldP != null) UnityEngine.Object.DestroyImmediate(oldP.gameObject);

var copy = UnityEngine.Object.Instantiate(src.gameObject, mapRoot);
copy.name = "Lv1_DeepForest";
copy.transform.position = src.position;
copy.SetActive(false);

var info = copy.GetComponent<MapInfo>();
info.displayName = "Deep Forest"; info.zoneTitle = "Deep Forest"; info.floor = 1; info.ascentLoad = 20; info.fireflies = true;

// 포털: 마을행(왼쪽)은 유지, 오른쪽 끝 포털을 일반 숲 귀환용으로
Portal back = null;
foreach (var p in copy.GetComponentsInChildren<Portal>(true))
{
    if (p.name == "PotalLv1_NearForest") { p.closeMap = copy; continue; }
    if (p.name == "PotalLv1_NearForest2") { back = p; continue; }
    UnityEngine.Object.DestroyImmediate(p.gameObject);
}
var tpl = normal.Find("Portal/Portal_ToSideRoad").GetComponent<Portal>();
var go = UnityEngine.Object.Instantiate(tpl.gameObject, tpl.transform.parent);
go.name = "Portal_ToDeepForest";
go.transform.position = new Vector3(-722.3f, -0.4f, tpl.transform.position.z); // 일반 숲 왼쪽 끝(원본 Lv2 포털 자리)
var there = go.GetComponent<Portal>();
back.name = "Portal_DeepToNormal";
there.destination = back.transform; there.closeMap = normal.gameObject; there.openMap = copy;
back.destination = there.transform; back.closeMap = copy; back.openMap = normal.gameObject;
back.proCamera = tpl.proCamera; back.player = tpl.player;
sb.Append("portals " + go.transform.position.ToString("F0") + " <-> " + back.transform.position.ToString("F0") + "; ");

// 스포너 교체 (풀 구성은 몬스터 BT 템플릿과 같은 원본 스포너에서 가져온다 — build_normal_forest 와 같은 방식)
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var f = typeof(EnemySpawner).GetField("objectInfos", B);
System.Func<string, EnemySpawner> srcSpawner = n => { foreach (var s in side.GetComponentsInChildren<EnemySpawner>(true)) if (s.name == n) return s; return null; };
System.Func<EnemySpawner, string, string, string, string> swap = (target, tplName, prefabName, deadName) => {
    var tp = srcSpawner(tplName);
    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/" + prefabName + ".prefab");
    var dead = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Lv2/" + deadName + ".prefab");
    if (tp == null || prefab == null) return target.name + " FAIL; ";
    var tArr = (System.Array)f.GetValue(tp);
    var et = tArr.GetType().GetElementType();
    var list = new System.Collections.ArrayList();
    foreach (var o in tArr)
    {
        var n = (string)et.GetField("objectName").GetValue(o);
        var c = System.Activator.CreateInstance(et, true);
        et.GetField("objectName").SetValue(c, n);
        et.GetField("count").SetValue(c, et.GetField("count").GetValue(o));
        var pf = (GameObject)et.GetField("perfab").GetValue(o);
        if (n == "enemy") pf = prefab; else if (n == "dead" && dead != null) pf = dead;
        et.GetField("perfab").SetValue(c, pf);
        list.Add(c);
    }
    var arr = System.Array.CreateInstance(et, list.Count); list.CopyTo(arr);
    f.SetValue(target, arr);
    EditorUtility.SetDirty(target);
    return target.name + "->" + prefabName + "; ";
};
// 드롭 테이블: "이름", "확률" 쌍
System.Action<EnemySpawner, string[]> setDrops = (s, list) => {
    var dm = s.GetComponent<DropManager>(); if (dm == null) return;
    var so = new SerializedObject(dm); var arr = so.FindProperty("dropItems"); arr.arraySize = list.Length / 2;
    for (int i = 0; i < list.Length / 2; i++) { var e = arr.GetArrayElementAtIndex(i); e.FindPropertyRelative("itemName").stringValue = list[i * 2]; e.FindPropertyRelative("dropProbability").floatValue = float.Parse(list[i * 2 + 1], System.Globalization.CultureInfo.InvariantCulture); e.FindPropertyRelative("offsetY").floatValue = 0.5f; }
    so.ApplyModifiedPropertiesWithoutUndo(); };
string[] bearD = { "Bear Pelt", "0.35", "BearClaw", "0.02", "Food", "0.1", "Potion", "0.04" };
string[] entD = { "Heartwood Chip", "0.35", "RootStaff", "0.02", "Wood", "0.2", "Potion", "0.04" };
string[] gobD = { "Bear Pelt", "0.08", "T0_Ore", "0.15", "Food", "0.08", "Potion", "0.04" };
string[] shroomD = { "Heartwood Chip", "0.08", "Small Tuber", "0.15", "GreenHurb", "0.02", "Potion", "0.04" };
string[] ghostD = { "Ectoplasm", "0.3", "Potion", "0.04" };
string[] fairyD = { "Fairy Dust", "0.3", "GreenHurb", "0.02", "Potion", "0.04" };
string[] bossD = { "Heartwood Chip", "1", "Ectoplasm", "0.5", "Fairy Dust", "0.5", "Potion", "0.3" };
foreach (var s in copy.GetComponentsInChildren<EnemySpawner>(true))
{
    string n = s.name;
    if (n == "Rabbit") { sb.Append(swap(s, "RatThief", "ForestBear_BD", "ForestBearDeadBody")); setDrops(s, bearD); }
    else if (n == "Rabbit2") { sb.Append(swap(s, "RatThief", "CliffGoblin_BD", "CliffGoblinDeadBody")); setDrops(s, gobD); }
    else if (n == "Rabbit2_2") { sb.Append(swap(s, "Mole", "SporeShroom_BD", "SporeShroomDeadBody")); setDrops(s, shroomD); }
    else if (n == "Rabbit3") { sb.Append(swap(s, "Mole", "Ent_BD", "EntDeadBody")); setDrops(s, entD); }
    else if (n == "Mole") { sb.Append(swap(s, "RatThief", "ForestBear_BD", "ForestBearDeadBody")); setDrops(s, bearD); }
    else if (n == "RatThief" || n == "RatCheif" || n == "ScareCrow") { sb.Append(swap(s, "Flowermole1", "Ghost_BD", "GhostDeadBody")); setDrops(s, ghostD); }
    else if (n == "RatThief2" || n == "RatCheif2") { sb.Append(swap(s, "Flowermole1", "Fairy_BD", "FairyDeadBody")); setDrops(s, fairyD); }
}
// 덩치 큰 몹은 밀도를 줄인다 (일반 숲 tune 과 같은 이유: 진입 직후 몰림)
foreach (var s in copy.GetComponentsInChildren<EnemySpawner>(true))
    if (s.name == "Rabbit" || s.name == "Rabbit3" || s.name == "Mole") { s.enemyCount = Mathf.Max(2, s.enemyCount / 2); EditorUtility.SetDirty(s); }

// 스포너 이름을 실제 몹 이름으로 (GameLog.Spawn 가독성). 원본 이름 매칭은 위에서 끝났다
var rename = new System.Collections.Generic.Dictionary<string, string>{ {"Rabbit","Bear1"}, {"Mole","Bear2"}, {"Rabbit2","Goblin1"}, {"Rabbit2_2","Shroom1"}, {"Rabbit3","Ent1"},
    {"RatThief","Ghost1"}, {"RatCheif","Ghost2"}, {"ScareCrow","Ghost3"}, {"RatThief2","Fairy1"}, {"RatCheif2","Fairy2"} };
foreach (var s in copy.GetComponentsInChildren<EnemySpawner>(true)) if (rename.ContainsKey(s.name)) s.name = rename[s.name];

// 보스 스포너: 샛길 보스 스포너를 복제해 낮 스포너 밑으로, 스폰 지점이 왼쪽 끝 (-412, 1) 에 오도록 옮긴다
var bossSrc = side.Find("NightTimeSpawn/BossSpawoner").GetComponent<EnemySpawner>();
var bossGo = UnityEngine.Object.Instantiate(bossSrc.gameObject, copy.transform.Find("DayTimeSpawn"));
bossGo.name = "BossSpawoner";
var boss = bossGo.GetComponent<EnemySpawner>();
var spawnPt = boss.enemyPositon.transform.GetChild(0);
bossGo.transform.position += new Vector3(-412f, 1f, 0f) - spawnPt.position;
boss.isDaytimeSpowner = true;
var bbox = bossGo.GetComponent<BoxCollider2D>();
// 영역을 맵 전체(x -470 ~ -130, 양끝은 벽)로: 보스를 영역 밖으로 끌어내 사라지게(체력 리셋) 하는 공략을 막는다
if (bbox != null) { bbox.offset = new Vector2(-300f - bossGo.transform.position.x, bbox.offset.y); bbox.size = new Vector2(340f, bbox.size.y); }
sb.Append(swap(boss, "BossSpawoner", "AncientEnt_BD", "AncientEntDeadBody"));
setDrops(boss, bossD);
sb.Append("boss @" + spawnPt.position.ToString("F0") + " area " + (bbox != null ? bbox.bounds.min.x.ToString("F0") + "~" + bbox.bounds.max.x.ToString("F0") : "-") + "; ");

// 돌 유적 색조: 어둡고 청록·회색
var ruin = new Color(0.5f, 0.62f, 0.64f);
foreach (var tm in copy.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true)) tm.color = ruin;
var par = copy.transform.Find("Parrelax");
if (par != null) foreach (var sr in par.GetComponentsInChildren<SpriteRenderer>(true)) sr.color = new Color(sr.color.r * 0.55f, sr.color.g * 0.68f, sr.color.b * 0.7f, sr.color.a);
foreach (var name in new[]{ "Vegetation", "Props", "Deco" })
{
    var t = copy.transform.Find(name);
    if (t != null) foreach (var sr in t.GetComponentsInChildren<SpriteRenderer>(true)) sr.color = new Color(sr.color.r * 0.55f, sr.color.g * 0.68f, sr.color.b * 0.7f, sr.color.a);
}

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(copy.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(copy.scene);
return sb.ToString();
