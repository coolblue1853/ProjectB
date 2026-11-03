// 1층계 신규 맵 2장(Floor1Completion 14-3): 깊은 숲 심부 Lv1_DeepForestCore, 안개 늪 Lv1_MistSwamp.
// 둘 다 Lv1_Swamp(Lv2 마법 절벽 레이아웃 복제본, x -420~-180)를 복제해 색조 변형 후 스포너를 교체한다. 새 아트 없음.
// 포털: 깊은 숲 Portal_ToCore(오른쪽 끝) <-> 심부 Portal_CoreToDeep(왼쪽), 늪 Portal_ToMist(중앙 -308) <-> 안개 늪 Portal_MistToSwamp(왼쪽).
// 에디트 모드, 재실행하면 복제본/포털을 지우고 다시 만든다. 씬은 저장하지 않는다(SampleScene 은 커밋 대상 아님).
var sb = new System.Text.StringBuilder();
if (EditorApplication.isPlaying) return "FAIL: 플레이 중";
var mapRoot = GameObject.Find("Map").transform;
var swamp = mapRoot.Find("Lv1_Swamp");
var deep = mapRoot.Find("Lv1_DeepForest");
if (swamp == null || deep == null || swamp.Find("Portal/Portal_SwampToDeep") == null || deep.Find("Portal/Portal_DeepToNormal") == null)
    return "FAIL: 원본 없음";
foreach (var n in new[]{ "Lv1_DeepForestCore", "Lv1_MistSwamp" }) { var o = mapRoot.Find(n); if (o != null) UnityEngine.Object.DestroyImmediate(o.gameObject); }
foreach (var pr in new[]{ deep.Find("Portal/Portal_ToCore"), swamp.Find("Portal/Portal_ToMist") }) if (pr != null) UnityEngine.Object.DestroyImmediate(pr.gameObject);

var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var infosField = typeof(EnemySpawner).GetField("objectInfos", B);

System.Func<string, GameObject> clone = name => {
    var c = UnityEngine.Object.Instantiate(swamp.gameObject, mapRoot);
    c.name = name; c.transform.position = swamp.position; c.SetActive(false);
    var stray = c.transform.Find("Portal/Portal_ToMist"); if (stray != null) UnityEngine.Object.DestroyImmediate(stray.gameObject); // 늪 본 맵에 이미 만든 안개 늪행 포털은 복제본에 필요 없다
    return c;
};
System.Func<EnemySpawner, EnemySpawner, string> copyFrom = (dst, src) => {
    var sArr = (System.Array)infosField.GetValue(src);
    var et = sArr.GetType().GetElementType();
    var list = new System.Collections.ArrayList();
    foreach (var o in sArr) {
        var c = System.Activator.CreateInstance(et, true);
        foreach (var fi in et.GetFields(B)) fi.SetValue(c, fi.GetValue(o));
        list.Add(c);
    }
    var arr = System.Array.CreateInstance(et, list.Count); list.CopyTo(arr);
    infosField.SetValue(dst, arr);
    var dropField = typeof(DropManager).GetField("dropItems", B);
    var sdrops = (System.Array)dropField.GetValue(src.GetComponent<DropManager>());
    var clonedDrops = (System.Array)sdrops.Clone();
    dropField.SetValue(dst.GetComponent<DropManager>(), clonedDrops);
    EditorUtility.SetDirty(dst.GetComponent<DropManager>());
    EditorUtility.SetDirty(dst);
    return dst.name + "<-" + src.name + "(drops " + clonedDrops.Length + ") ";
};
System.Action<GameObject> dropBoss = c => { var b = c.transform.Find("NightTimeSpawn/BossSpawoner"); if (b != null) UnityEngine.Object.DestroyImmediate(b.gameObject); };
System.Action<GameObject, Color> tint = (c, col) => { foreach (var tm in c.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true)) tm.color = col; };
System.Action<Portal, Portal, GameObject, GameObject, string, string> link = (a, b, mapA, mapB, labA, labB) => {
    a.destination = b.transform; a.closeMap = mapA; a.openMap = mapB; a.labelOverride = labA;
    b.destination = a.transform; b.closeMap = mapB; b.openMap = mapA; b.labelOverride = labB;
    a.gameObject.SetActive(true); b.gameObject.SetActive(true);
};

// ===== 안개 늪 =====
var mist = clone("Lv1_MistSwamp");
dropBoss(mist);
var mInfo = mist.GetComponent<MapInfo>();
mInfo.displayName = "Mist Swamp"; mInfo.zoneTitle = "Mist Swamp"; mInfo.ascentLoad = 30; mInfo.ambience = AmbienceKind.Swamp;
mInfo.groundMoveMul = 0.8f; mInfo.mistFog = true; mInfo.fireflies = false;
tint(mist, new Color(0.52f, 0.64f, 0.6f));
var mout = mist.transform.Find("Portal/Portal_ToOutskirts"); if (mout != null) UnityEngine.Object.DestroyImmediate(mout.gameObject);
var mBack = mist.transform.Find("Portal/Portal_SwampToDeep").GetComponent<Portal>(); mBack.name = "Portal_MistToSwamp";
var swTpl = swamp.Find("Portal/Portal_SwampToDeep").GetComponent<Portal>();
var toMistGo = UnityEngine.Object.Instantiate(swTpl.gameObject, swTpl.transform.parent); toMistGo.name = "Portal_ToMist";
toMistGo.transform.position = new Vector3(-308f, swTpl.transform.position.y, swTpl.transform.position.z);
var toMist = toMistGo.GetComponent<Portal>();
mBack.proCamera = swTpl.proCamera; mBack.player = swTpl.player;
link(toMist, mBack, swamp.gameObject, mist, "Mist Swamp", "Swamp");
// 깊은 물 구덩이 2곳: 지면 타일 열을 비운다(뒤의 sea 배경이 물로 보인다). 낙사는 FallRescue 가 직전 안전 위치로 되돌린다
var mground = mist.transform.Find("BaseTile").GetComponent<UnityEngine.Tilemaps.Tilemap>();
foreach (var gap in new[]{ new[]{ -376, -368 }, new[]{ -232, -226 } })
{
    for (int x = gap[0]; x <= gap[1]; x++)
    {
        int cx = x; // 맵 루트 (0,0), 칸 1u: 비활성 맵은 layoutGrid 가 null 이라 WorldToCell 을 못 쓴다
        for (int y = mground.cellBounds.yMin; y < mground.cellBounds.yMax; y++) mground.SetTile(new Vector3Int(cx, y, 0), null);
    }
}
sb.Append("mist pits ground tiles at -372: " + mground.HasTile(new Vector3Int(-372, -181, 0)) + ", at -300: " + mground.HasTile(new Vector3Int(-300, -181, 0)) + "; ");

// ===== 깊은 숲 심부 =====
var core = clone("Lv1_DeepForestCore");
dropBoss(core);
var cInfo = core.GetComponent<MapInfo>();
var dInfo = deep.GetComponent<MapInfo>();
cInfo.displayName = "Deep Forest Core"; cInfo.zoneTitle = "Deep Forest Core"; cInfo.ascentLoad = dInfo.ascentLoad + 5; cInfo.ambience = AmbienceKind.DeepForest; cInfo.fireflies = true;
cInfo.groundMoveMul = 1f; cInfo.mistFog = false;
tint(core, new Color(0.34f, 0.5f, 0.38f));
var bgc = core.transform.Find("Background");
foreach (var sr in bgc.GetComponentsInChildren<SpriteRenderer>(true)) { if (sr.name == "sea") sr.enabled = false; else sr.color = new Color(sr.color.r * 0.55f, sr.color.g * 0.7f, sr.color.b * 0.55f, sr.color.a); }
var cout = core.transform.Find("Portal/Portal_ToOutskirts"); if (cout != null) UnityEngine.Object.DestroyImmediate(cout.gameObject);
var cBack = core.transform.Find("Portal/Portal_SwampToDeep").GetComponent<Portal>(); cBack.name = "Portal_CoreToDeep";
var dTpl = deep.Find("Portal/Portal_DeepToNormal").GetComponent<Portal>();
var toCoreGo = UnityEngine.Object.Instantiate(dTpl.gameObject, dTpl.transform.parent); toCoreGo.name = "Portal_ToCore";
bool deepWas = deep.gameObject.activeSelf; deep.gameObject.SetActive(true); Physics2D.SyncTransforms();
float groundTop = float.NaN;
foreach (var h in Physics2D.RaycastAll(new Vector2(-150f, 60f), Vector2.down, 120f, LayerMask.GetMask("Ground"))) if (float.IsNaN(groundTop) || h.point.y < groundTop) groundTop = h.point.y;
deep.gameObject.SetActive(deepWas);
if (float.IsNaN(groundTop)) groundTop = -3.6f;
toCoreGo.transform.position = new Vector3(-150f, groundTop + 1.2f, dTpl.transform.position.z);
var toCore = toCoreGo.GetComponent<Portal>();
cBack.proCamera = dTpl.proCamera; cBack.player = dTpl.player;
link(toCore, cBack, deep.gameObject, core, "Deep Forest Core", "Deep Forest");
sb.Append("deep portal y=" + toCoreGo.transform.position.y.ToString("F2") + " (top " + groundTop.ToString("F2") + "); ");
// 스포너: 스웜프 이름 -> 심부 이름 / 깊은 숲 원본 스포너
var mapping = new System.Collections.Generic.Dictionary<string, string[]>();
mapping["Shroom1"] = new[]{ "Shroom1", "Shroom1" }; mapping["Frog1"] = new[]{ "Ent1", "Ent1" }; mapping["Crab1"] = new[]{ "Goblin1", "Goblin1" };
mapping["Croc1"] = new[]{ "Ent2", "Ent1" }; mapping["Mosquito1"] = new[]{ "Fairy1", "Fairy1" }; mapping["Slime1"] = new[]{ "Ghost1", "Ghost1" };
mapping["Mosquito2"] = new[]{ "Fairy2", "Fairy2" }; mapping["Slime2"] = new[]{ "Ghost2", "Ghost2" };
foreach (var s in core.GetComponentsInChildren<EnemySpawner>(true))
{
    if (!mapping.ContainsKey(s.name)) { sb.Append("[미매핑 " + s.name + "] "); continue; }
    EnemySpawner src = null;
    foreach (var d in deep.GetComponentsInChildren<EnemySpawner>(true)) if (d.name == mapping[s.name][1]) { src = d; break; }
    if (src == null) { sb.Append("[원본 없음 " + mapping[s.name][1] + "] "); continue; }
    sb.Append(copyFrom(s, src));
    s.name = mapping[s.name][0];
}
return sb.ToString();
