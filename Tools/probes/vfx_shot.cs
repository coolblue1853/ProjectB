// 평타 1회 + 치명 이펙트 + (장착 스킬 있으면) 시전 이펙트를 띄우고 0.15초 뒤 시간 정지 → 스크린샷용
var pc = PlayerController.instance;
var sb = new System.Text.StringBuilder();
var w = pc.GetComponentInChildren<Weapon>();
if (w != null) { w.MeleeAttack(); sb.Append("swing " + w.name + " vfx=" + w.swingVfx + "; "); }
EnemyHealth t = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
{ var d = Vector2.Distance(e.transform.position, pc.transform.position); if (e.nowHP > 0 && d < best) { best = d; t = e; } }
if (t != null) { HitFeedback.OnEnemyHit(t, true, false, false); sb.Append("crit on " + t.name + "; "); }
Vfx.Play(VfxId.__KEY__, pc.transform.position + new Vector3(pc.transform.localScale.x < 0 ? -2.5f : 2.5f, 1.2f, 0f), pc.transform.localScale.x < 0);
sb.Append("active=" + Vfx.ActiveCount);
DG.Tweening.DOVirtual.DelayedCall(0.15f, () => Time.timeScale = 0f, false);
return sb.ToString();
