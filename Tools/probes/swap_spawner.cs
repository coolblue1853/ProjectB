// 테스트: 스포너(KEY) 몬스터를 다른 프리팹으로 교체 (플레이 중, 해당 맵 진입 전 실행). 템플릿 스포너(TPL)의 공격 키를 복사한다
var key = "__KEY__"; var prefabName = "__PREFAB__"; var deadName = "__DEAD__"; var tplSpawner = "__TPL__";
var B = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var f = typeof(EnemySpawner).GetField("objectInfos", B);
EnemySpawner target = null, tpl = null;
foreach (var sp in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (sp.name == key) target = sp; if (sp.name == tplSpawner) tpl = sp; }
if (target == null || tpl == null) return "spawner not found";
var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/" + prefabName + ".prefab");
var dead = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Lv2/" + deadName + ".prefab");
var tArr = (System.Array)f.GetValue(tpl);
var elemType = tArr.GetType().GetElementType();
var list = new System.Collections.ArrayList();
foreach (var o in tArr)
{
    var n = (string)elemType.GetField("objectName").GetValue(o);
    var copy = System.Activator.CreateInstance(elemType, true);
    elemType.GetField("objectName").SetValue(copy, n);
    elemType.GetField("count").SetValue(copy, elemType.GetField("count").GetValue(o));
    var pf = (GameObject)elemType.GetField("perfab").GetValue(o);
    if (n == "enemy") pf = prefab; else if (n == "dead") pf = dead;
    elemType.GetField("perfab").SetValue(copy, pf);
    list.Add(copy);
}
var arr = System.Array.CreateInstance(elemType, list.Count); list.CopyTo(arr);
f.SetValue(target, arr);
target.isDaytimeSpowner = TimeChange.instance.isDaytime;
return "swapped " + key + " -> " + prefabName + " keys=" + list.Count + " active=" + target.gameObject.activeInHierarchy;
