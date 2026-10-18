// 타격감 검증: 가장 가까운 몬스터 1회 타격 직후 플래시·히트스톱·흔들림 값
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
EnemyHealth t = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude))
{ var d = Vector2.Distance(e.transform.position, pc.transform.position); if (e.nowHP > 30 && d < best) { best = d; t = e; } }
if (t == null) return "no enemy";
int shakeBefore = HitFeedback.ShakeCount;
t.damage2Enemy(new int[] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 }, 0f, 0f, Vector2.zero, 0f, false, false, 0.05f, 1f);
var fx = t.GetComponent<SpriteHitFx>();
return t.name + " fx=" + (fx != null) + " ready=" + (fx != null && fx.IsReady) + " blend=" + (fx ? fx.CurrentFlashBlend : -1) + " animSpeed=" + (fx ? fx.CurrentAnimSpeed : -1)
    + " shakes+" + (HitFeedback.ShakeCount - shakeBefore) + " shader=" + t.GetComponent<SpriteRenderer>().sharedMaterial.shader.name;
