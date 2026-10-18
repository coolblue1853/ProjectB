// 정예 테스트: __KEY__ = promote | kill. promote: 가장 가까운 몬스터 승격 후 정지(스크린샷용). kill: 정예 처치 후 1초 뒤 주변 드롭 목록
var pc = PlayerController.instance;
EnemyHealth t = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
{ var d = Vector2.Distance(e.transform.position, pc.transform.position); if (e.nowHP > 0 && d < best && ("__KEY__" == "promote" || e.IsElite)) { best = d; t = e; } }
if (t == null) return "no target";
if ("__KEY__" == "promote") {
  t.PromoteElite();
  DG.Tweening.DOVirtual.DelayedCall(0.5f, () => Time.timeScale = 0f, false);
  return "promoted " + t.name + " hp=" + t.nowHP + "/" + t.maxHP + " def=" + t.enemyDef + " scale=" + t.transform.localScale;
}
var pos = t.transform.position;
var w = new int[] { 99999, 99999, 0, 0, 0, 100, 0, 0, 0, 0 };
t.damage2Enemy(w, 0f, 0f, Vector2.zero, 0f, false, false, 0.05f, 1f);
DG.Tweening.DOVirtual.DelayedCall(1.2f, () => {
  var sb = new System.Text.StringBuilder("[PB][DBG] elite drops near " + pos + ": ");
  foreach (var di in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsSortMode.None))
    if (Vector2.Distance(di.transform.position, pos) < 4f) sb.Append(di.name + ", ");
  UnityEngine.Debug.Log(sb.ToString());
}, false);
return "killed " + t.name + " elite=" + t.IsElite;
