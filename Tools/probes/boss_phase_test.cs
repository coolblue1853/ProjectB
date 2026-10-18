// 보스 페이즈: __BOSS__ 보스를 꺼내 HP 를 49% 로 깎아 2페이즈, 24% 로 3페이즈 전환 확인 (애니 속도·배너 로그)
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
var boss = sp.GetOb(key); boss.transform.SetParent(null); boss.transform.position = PlayerController.instance.transform.position + Vector3.right * 4f;
var eh = boss.GetComponent<EnemyHealth>(); var anim = boss.GetComponent<Animator>(); var bp = boss.GetComponent<BossPhase>();
sb.Append("boss=" + boss.name + " hp=" + eh.nowHP + "/" + eh.maxHP + " phaseComp=" + (bp != null) + "; ");
var w = new int[] { 1, 1, 0, 0, 0, 100, 0, 0, 0, 0 };
int toHalf = eh.nowHP - Mathf.RoundToInt(eh.maxHP * 0.49f);
var dmg = new int[] { toHalf, toHalf, 0, 0, 0, 100, 0, 0, 0, 0 };
eh.damage2Enemy(dmg, 0f, 0f, Vector2.zero, 0f, false, false, 0.05f, 1f);
sb.Append("after49%: hp=" + eh.nowHP + " phase=" + (bp != null ? bp.Phase : -1) + " animSpeed=" + (anim != null ? anim.speed : -1f) + "; ");
for (int i = 0; i < 40 && eh.nowHP > eh.maxHP * 0.24f; i++) eh.damage2Enemy(new int[] { 400, 400, 0, 0, 0, 100, 0, 0, 0, 0 }, 0f, 0f, Vector2.zero, 0f, false, false, 0.05f, 1f);
sb.Append("after24%: hp=" + eh.nowHP + " phase=" + (bp != null ? bp.Phase : -1) + " animSpeed=" + (anim != null ? anim.speed : -1f) + " dmgMul=" + (bp != null ? bp.DamageMultiplier : 0f));
DG.Tweening.DOVirtual.DelayedCall(0.4f, () => Time.timeScale = 0f, false);
return sb.ToString();
