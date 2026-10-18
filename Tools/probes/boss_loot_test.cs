// __BOSS__ = 보스 프리팹 이름 접두(PassiveTerror / AbyssWizard).
// 보스 보상 테스트 (세이브 백업 필수): ① BossLoot.Roll 13회 — 12회 안에 고유템 보장 확인 ② PassiveTerror 스포너 경로로 보스 1마리 꺼내 처치 → 확정 드롭
var sb = new System.Text.StringBuilder();
var r = Resources.Load<BossReward>("Boss/PassiveTerror_BD");
ES3.DeleteKey("bossSinceUnique.TEST"); ES3.DeleteKey("bossFirstKill.TEST");
int firstUnique = -1;
for (int i = 1; i <= 13; i++) { var l = BossLoot.Roll("TEST", r); if (firstUnique < 0 && l.Contains(r.uniqueItem)) firstUnique = i; if (i == 1) sb.Append("first roll items=" + l.Count + "; "); }
sb.Append("unique at kill #" + firstUnique + "; ");
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
var boss = sp.GetOb(key); var eh = boss.GetComponent<EnemyHealth>();
boss.transform.position = PlayerController.instance.transform.position + Vector3.right * 4f;
var slotF = typeof(EnemySpawner).GetField("enemySlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
var slots = (GameObject[])slotF.GetValue(sp); slots[0] = boss; eh.enemyNum = 0;
var m = typeof(EnemySpawner).GetMethod("OnEnemyReleased", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var evt = typeof(EnemyHealth).GetEvent("OnReleasedToPool");
var del = System.Delegate.CreateDelegate(evt.EventHandlerType, sp, m);
evt.RemoveEventHandler(eh, del); evt.AddEventHandler(eh, del);
var pos = boss.transform.position;
eh.ReleaseEnemy(true, true);
var near = new System.Collections.Generic.List<string>();
foreach (var di in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsSortMode.None)) if (Vector2.Distance(di.transform.position, pos) < 6f) near.Add(di.name);
return sb + "boss drops(" + near.Count + "): " + string.Join(", ", near);
