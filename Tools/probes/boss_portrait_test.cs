// __BOSS__ = 보스 프리팹 이름 접두(PassiveTerror / AbyssWizard).
// 보스 HP바 초상 확인: 보스 스포너에서 보스를 꺼내 플레이어 옆에 두고 HP바 UI를 켠 뒤 정지
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
var boss = sp.GetOb(key); var eh = boss.GetComponent<EnemyHealth>();
boss.transform.position = PlayerController.instance.transform.position + Vector3.right * 5f;
foreach (var t in boss.GetComponentsInChildren<Transform>(true)) if (t.GetComponentInParent<Canvas>(true) != null) t.gameObject.SetActive(true);
var bar = boss.transform.Find("UI");
var portrait = boss.GetComponentInChildren<UnityEngine.UI.Outline>(true);
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => Time.timeScale = 0f, false);
return sb + "boss=" + boss.name + " portrait=" + (portrait != null ? portrait.name : "none");
