// 지정 프리팹(__BOSS__ 접두) 몬스터를 스포너 풀에서 꺼내 플레이어 옆 3유닛에 둔다 (BT가 그대로 추적·공격) — 텔레그래프·추적 확인용
var sb = new System.Text.StringBuilder();
EnemySpawner sp = null; string key = "enemy";
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var infosF = typeof(EnemySpawner).GetField("objectInfos", BF);
foreach (var s2 in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  foreach (var info in (System.Collections.IEnumerable)infosF.GetValue(s2)) {
    var pf = (GameObject)info.GetType().GetField("perfab").GetValue(info);
    var key0 = (string)info.GetType().GetField("objectName").GetValue(info);
    if (pf != null && key0 == "enemy" && pf.name.StartsWith("__BOSS__")) sp = s2; // 보스 프리팹을 "enemy" 키로 스폰하는 스포너
  }
}
if (sp == null) return sb + "no boss spawner";
var poolsF = typeof(EnemySpawner).GetField("pools", BF);
var dict = poolsF.GetValue(sp) as System.Collections.IDictionary;
if (dict == null || dict.Count == 0) typeof(EnemySpawner).GetMethod("Init", BF).Invoke(sp, null);
sb.Append("spawner=" + sp.name + " active=" + sp.gameObject.activeInHierarchy + "; ");
var mob = sp.GetOb(key);
mob.transform.SetParent(null); // 스포너가 비활성 맵 밑이면 Start·BT가 돌지 않는다
mob.transform.position = PlayerController.instance.transform.position + Vector3.right * 3f;
var eh = mob.GetComponent<EnemyHealth>(); eh.enemySpowner = sp;
return sb + "placed " + mob.name + " telegraph=" + (mob.GetComponent<EnemyTelegraph>() != null);
