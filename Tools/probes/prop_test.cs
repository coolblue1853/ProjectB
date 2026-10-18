// 파괴 오브젝트: 가장 가까운 BreakableProp 옆으로 이동 → Hit 2회 → 드롭 목록, 0.4초 뒤 정지(스크린샷)
var pc = PlayerController.instance; BreakableProp best = null; float bd = 1e9f;
foreach (var p in UnityEngine.Object.FindObjectsByType<BreakableProp>(FindObjectsSortMode.None)) { var d = Vector2.Distance(p.transform.position, pc.transform.position); if (d < bd) { bd = d; best = p; } }
if (best == null) return "no prop";
pc.transform.position = best.transform.position + new Vector3(-1.6f, 0.3f, 0f);
var sr = best.GetComponent<SpriteRenderer>();
best.Hit(pc.transform.position); string f1 = sr.sprite.name;
best.Hit(pc.transform.position); string f2 = sr.sprite.name;
var pos = best.transform.position; var sb = new System.Text.StringBuilder("prop=" + best.name + " f1=" + f1 + " f2=" + f2 + " drops: ");
DG.Tweening.DOVirtual.DelayedCall(0.4f, () => {
  foreach (var di in UnityEngine.Object.FindObjectsByType<DropItemCheck>(FindObjectsSortMode.None)) if (Vector2.Distance(di.transform.position, pos) < 3f) UnityEngine.Debug.Log("[PB][DBG] prop drop " + di.name);
  Time.timeScale = 0f; }, false);
return sb.ToString();
