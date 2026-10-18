// 보스 등장 연출(W2): __BOSS__ 보스를 꺼내 플레이어 오른쪽 6칸에 둔다 → BossIntro (카메라 타깃 추가·줌아웃·배너)
var sb = new System.Text.StringBuilder();
EnemySpawner sp = null; string key = "enemy";
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var infosF = typeof(EnemySpawner).GetField("objectInfos", BF);
foreach (var s2 in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  foreach (var info in (System.Collections.IEnumerable)infosF.GetValue(s2)) {
    var pf = (GameObject)info.GetType().GetField("perfab").GetValue(info);
    var key0 = (string)info.GetType().GetField("objectName").GetValue(info);
    if (pf != null && key0 == "enemy" && pf.name.StartsWith("__BOSS__")) sp = s2;
  }
}
if (sp == null) return "no boss spawner";
var dict = typeof(EnemySpawner).GetField("pools", BF).GetValue(sp) as System.Collections.IDictionary;
if (dict == null || dict.Count == 0) typeof(EnemySpawner).GetMethod("Init", BF).Invoke(sp, null);
var boss = sp.GetOb(key); boss.transform.SetParent(null); boss.transform.position = PlayerController.instance.transform.position + Vector3.right * 6f;
return "boss=" + boss.name + " active=" + EnemyHealth.ActiveBossCount + " targets=" + Com.LuisPedroFonseca.ProCamera2D.ProCamera2D.Instance.CameraTargets.Count;
