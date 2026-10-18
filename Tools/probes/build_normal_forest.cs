// 1층계 ②구역 "일반 숲"(Lv1_NormalForest): 외곽 샛길 맵을 복제 → 스포너 몬스터 교체(낮: 늑대·멧돼지·버섯 / 밤: 도깨비불·달팽이),
// 포털(샛길 오른쪽 끝 ↔ 일반 숲), 짙은 숲 색조, MapInfo. 에디트 모드, 재실행하면 기존 복제본을 지우고 다시 만든다
var sb = new System.Text.StringBuilder();
var mapRoot = GameObject.Find("Map").transform;
var src = mapRoot.Find("Lv1_NearForest_SideRoad");
var old = mapRoot.Find("Lv1_NormalForest");
if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var oldP = src.Find("Portal/Portal_ToNormalForest");
if (oldP != null) UnityEngine.Object.DestroyImmediate(oldP.gameObject);

var copy = UnityEngine.Object.Instantiate(src.gameObject, mapRoot);
copy.name = "Lv1_NormalForest";
copy.transform.position = src.position;
copy.SetActive(false);

// MapInfo
var info = copy.GetComponent<MapInfo>();
info.displayName = "Normal Forest"; info.zoneTitle = "Normal Forest"; info.floor = 1; info.ascentLoad = 10; info.fireflies = true;

// 포털: 복제본은 '샛길로 돌아가는' 포털 하나 + 마을 포털만 남긴다
Portal back = null;
foreach (var p in copy.GetComponentsInChildren<Portal>(true))
{
    if (p.name == "Portal_ToLv2") { back = p; continue; }
    if (p.name == "PotalLv1_NearForest") { p.closeMap = copy; continue; } // 마을행 유지
    UnityEngine.Object.DestroyImmediate(p.gameObject);
}
var tpl = src.Find("Portal/Portal_ToLv2").GetComponent<Portal>();
var go = UnityEngine.Object.Instantiate(tpl.gameObject, tpl.transform.parent);
go.name = "Portal_ToNormalForest";
var there = go.GetComponent<Portal>();
var pos = src.Find("Portal/PotalLv1_NearForest2").position + new Vector3(-26f, 0f, 0f);
go.transform.position = pos;
back.name = "Portal_ToSideRoad";
back.transform.position = pos;
there.destination = back.transform; there.closeMap = src.gameObject; there.openMap = copy;
back.destination = there.transform; back.closeMap = copy; back.openMap = src.gameObject;
sb.Append("portals @" + pos.ToString("F0") + "; ");

// 스포너 교체
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var f = typeof(EnemySpawner).GetField("objectInfos", B);
System.Func<string, EnemySpawner> srcSpawner = n => { foreach (var s in src.GetComponentsInChildren<EnemySpawner>(true)) if (s.name == n) return s; return null; };
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
foreach (var s in copy.GetComponentsInChildren<EnemySpawner>(true))
{
    string n = s.name;
    if (n.StartsWith("Rabbit")) sb.Append(swap(s, "RatThief", "ForestWolf_BD", "ForestWolfDeadBody"));
    else if (n.StartsWith("Mole")) sb.Append(swap(s, "RatThief", "WildBoar_BD", "WildBoarDeadBody"));
    else if (n.StartsWith("Flowermole")) sb.Append(swap(s, "Mole", "SporeShroom_BD", "SporeShroomDeadBody"));
    else if (n.StartsWith("RatThief") || n.StartsWith("ScareCrow")) sb.Append(swap(s, "Flowermole1", "Wisp_BD", "WispDeadBody"));
    else if (n.StartsWith("RatCheif") || n.StartsWith("CreepingTerror")) sb.Append(swap(s, "Mole", "ForestSnail_BD", "ForestSnailDeadBody"));
    else if (n.StartsWith("Boss")) { UnityEngine.Object.DestroyImmediate(s.gameObject); sb.Append("boss removed; "); continue; } // SetActive(false) 는 PoolCheck.AbleSpwaner 가 다시 켠다
}

// 짙은 숲 색조 (같은 지형이라도 구역이 바뀐 느낌)
var deep = new Color(0.72f, 0.86f, 0.72f);
foreach (var tm in copy.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true)) tm.color = deep;
foreach (var sr in copy.transform.Find("Parrelax").GetComponentsInChildren<SpriteRenderer>(true)) sr.color = new Color(sr.color.r * 0.7f, sr.color.g * 0.85f, sr.color.b * 0.75f, sr.color.a);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(copy.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(copy.scene);
return sb.ToString();
