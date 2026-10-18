// 1층계 ④구역 "늪지"(Lv1_Swamp, TowerWorld 14절): ②(샛길 복제)·③(근교 복제)과 다른 지형인 Lv2_MagicCliffs 레이아웃(x -420~-180, 발판·사다리)을 복제 →
// 늪 색조(탁한 녹갈색, 어둡게), 스포너 교체(낮: 개구리·꽃게·악어·포자버섯 / 밤: 모기·슬라임), 보스 스포너(거대 슬라임, 밤, 오른쪽 끝, 영역 = 맵 전체),
// 드롭 테이블(재료 + 계보 무기 2% + 방어구 부위 0.3%), MapInfo, 포털 한 쌍(깊은 숲 왼쪽 끝 Portal_ToSwamp ↔ 늪지 왼쪽 Portal_SwampToDeep).
// 에디트 모드, 재실행하면 복제본·포털을 지우고 다시 만든다
var sb = new System.Text.StringBuilder();
var mapRoot = GameObject.Find("Map").transform;
var src = mapRoot.Find("Lv2_MagicCliffs");
var side = mapRoot.Find("Lv1_NearForest_SideRoad");
var deep = mapRoot.Find("Lv1_DeepForest");
if (src == null || side == null || deep == null || src.GetComponent<MapInfo>() == null || src.Find("Portal/Portal_ToLv1") == null || deep.Find("Portal/Portal_DeepToNormal") == null)
    return "FAIL: 원본 맵/포털 없음 (아무것도 바꾸지 않음)";
var old = mapRoot.Find("Lv1_Swamp");
if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var oldP = deep.Find("Portal/Portal_ToSwamp");
if (oldP != null) UnityEngine.Object.DestroyImmediate(oldP.gameObject);

var copy = UnityEngine.Object.Instantiate(src.gameObject, mapRoot);
copy.name = "Lv1_Swamp";
copy.transform.position = src.position;
copy.SetActive(false);

var info = copy.GetComponent<MapInfo>();
info.displayName = "Swamp"; info.zoneTitle = "Swamp"; info.floor = 1; info.ascentLoad = 30; info.fireflies = true;

// 포털: 늪지 왼쪽 Lv1행 포털을 깊은 숲 귀환용으로, 깊은 숲 왼쪽 끝(마을 포털 바깥쪽)에 늪지행 포털 신설
var back = copy.transform.Find("Portal/Portal_ToLv1").GetComponent<Portal>();
foreach (var p in copy.GetComponentsInChildren<Portal>(true)) if (p != back) UnityEngine.Object.DestroyImmediate(p.gameObject);
var tpl = deep.Find("Portal/Portal_DeepToNormal").GetComponent<Portal>();
var go = UnityEngine.Object.Instantiate(tpl.gameObject, tpl.transform.parent);
go.name = "Portal_ToSwamp";
var townP = deep.Find("Portal/PotalLv1_NearForest");
go.transform.position = new Vector3(-432.5f, townP.position.y, tpl.transform.position.z); // 깊은 숲 왼쪽 끝 (보스 구역 너머, 마을 포털 -425 바깥)
var there = go.GetComponent<Portal>();
back.name = "Portal_SwampToDeep";
there.destination = back.transform; there.closeMap = deep.gameObject; there.openMap = copy;
back.destination = there.transform; back.closeMap = copy; back.openMap = deep.gameObject;
back.proCamera = tpl.proCamera; back.player = tpl.player;
sb.Append("portals " + go.transform.position.ToString("F1") + " <-> " + back.transform.position.ToString("F1") + "; ");

// 스포너 교체 (풀 구성은 몬스터 BT 템플릿과 같은 샛길 원본 스포너에서 가져온다 — build_deep_forest 와 같은 방식)
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
// 드롭 테이블: "이름", "확률" 쌍. 방어구 부위는 낮=Bog / 밤=Gloom 세트(각 0.3%)
System.Func<string, string[], string[]> withSet = (key, list) => {
    var l = new System.Collections.Generic.List<string>(list);
    foreach (var part in new[]{ "Cap", "Armor", "LegArmor", "HandArmor", "Shoes" }) { l.Add(key + part); l.Add("0.003"); }
    return l.ToArray(); };
System.Action<EnemySpawner, string[]> setDrops = (s, list) => {
    var dm = s.GetComponent<DropManager>(); if (dm == null) return;
    var so = new SerializedObject(dm); var arr = so.FindProperty("dropItems"); arr.arraySize = list.Length / 2;
    for (int i = 0; i < list.Length / 2; i++) { var e = arr.GetArrayElementAtIndex(i); e.FindPropertyRelative("itemName").stringValue = list[i * 2]; e.FindPropertyRelative("dropProbability").floatValue = float.Parse(list[i * 2 + 1], System.Globalization.CultureInfo.InvariantCulture); e.FindPropertyRelative("offsetY").floatValue = 0.5f; }
    so.ApplyModifiedPropertiesWithoutUndo(); };
string[] frogD = withSet("Bog", new[]{ "Frog Skin", "0.35", "FrogTongueWhip", "0.02", "Food", "0.1", "Potion", "0.04" });
string[] crabD = withSet("Bog", new[]{ "Crab Pincer", "0.35", "PetalPincer", "0.02", "Potion", "0.04" });
string[] crocD = withSet("Bog", new[]{ "Croc Tooth", "0.35", "CrocJawAxe", "0.02", "Food", "0.1", "Potion", "0.04" });
string[] shroomD = { "Slime Gel", "0.08", "Small Tuber", "0.15", "GreenHurb", "0.02", "Potion", "0.04" };
string[] mosqD = withSet("Gloom", new[]{ "Blood Sting", "0.3", "BloodstingRapier", "0.02", "Potion", "0.04" });
string[] slimeD = withSet("Gloom", new[]{ "Slime Gel", "0.33", "SlimeMace", "0.02", "Potion", "0.04" });
string[] bossD = { "Slime Gel", "1", "Blood Sting", "0.5", "Frog Skin", "0.5", "Potion", "0.3" };
var map = new System.Collections.Generic.Dictionary<string, string[]>{
    {"L2_A_Shroom", new[]{ "Shroom1", "Mole", "SporeShroom_BD", "SporeShroomDeadBody" }},
    {"L2_B_Goblin", new[]{ "Frog1", "RatThief", "SwampFrog_BD", "SwampFrogDeadBody" }},
    {"L2_B_ShroomUp", new[]{ "Crab1", "RatThief", "MarshCrab_BD", "MarshCrabDeadBody" }},
    {"L2_C_Goblin", new[]{ "Croc1", "RatThief", "BogCroc_BD", "BogCrocDeadBody" }},
    {"L2_A_EyeN", new[]{ "Mosquito1", "Flowermole1", "SwampMosquito_BD", "SwampMosquitoDeadBody" }},
    {"L2_B_SkelN", new[]{ "Slime1", "Mole", "BogSlime_BD", "BogSlimeDeadBody" }},
    {"L2_B_GoblinN", new[]{ "Mosquito2", "Flowermole1", "SwampMosquito_BD", "SwampMosquitoDeadBody" }},
    {"L2_C_SkelN", new[]{ "Slime2", "Mole", "BogSlime_BD", "BogSlimeDeadBody" }},
    {"L2_D_BossN", new[]{ "BossSpawoner", "BossSpawoner", "GiantSlime_BD", "GiantSlimeDeadBody" }} };
foreach (var s in copy.GetComponentsInChildren<EnemySpawner>(true))
{
    if (s.name == "L2_C_EliteN") { UnityEngine.Object.DestroyImmediate(s.gameObject); sb.Append("elite removed; "); continue; } // Lv2 정예(Death Bringer) 자리는 쓰지 않는다
    if (!map.ContainsKey(s.name)) { sb.Append("[미매핑 " + s.name + "] "); continue; }
    var m = map[s.name];
    sb.Append(swap(s, m[1], m[2], m[3]));
    string n = m[0];
    setDrops(s, n.StartsWith("Shroom") ? shroomD : n.StartsWith("Frog") ? frogD : n.StartsWith("Crab") ? crabD : n.StartsWith("Croc") ? crocD
        : n.StartsWith("Mosquito") ? mosqD : n.StartsWith("Slime") ? slimeD : bossD);
    if (n.StartsWith("Croc")) { s.enemyCount = 3; EditorUtility.SetDirty(s); } // 덩치 큰 몹은 밀도를 줄인다
    s.name = n;
}

// 보스: 오른쪽 끝(원본 Lv2 보스 자리), 밤. 영역을 맵 전체(x -422~-178)로 — 영역 밖으로 끌어내 체력 리셋하는 공략 방지(B-117 이후 ③과 같은 규칙)
var boss = copy.transform.Find("NightTimeSpawn/BossSpawoner").GetComponent<EnemySpawner>();
var bbox = boss.GetComponent<BoxCollider2D>();
if (bbox != null) { bbox.offset = new Vector2(-300f - boss.transform.position.x, bbox.offset.y); bbox.size = new Vector2(244f, bbox.size.y); }
boss.isDaytimeSpowner = false;
sb.Append("boss @" + boss.enemyPositon.transform.GetChild(0).position.ToString("F0") + "; ");

// 늪 색조: 탁한 녹갈색, 어둡게 (바다 배경은 늪물처럼)
var bog = new Color(0.46f, 0.52f, 0.36f);
foreach (var tm in copy.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true)) tm.color = bog;
var bg = copy.transform.Find("Background");
if (bg != null) foreach (var sr in bg.GetComponentsInChildren<SpriteRenderer>(true)) sr.color = new Color(sr.color.r * 0.42f, sr.color.g * 0.5f, sr.color.b * 0.32f, sr.color.a);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(copy.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(copy.scene);
return sb.ToString();
