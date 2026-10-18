// 가장 가까운 몬스터에 독 2중첩 + 출혈 1회 → HP 감소 기록. 두 번째 호출 시 처치하여 DoT가 반환된 개체를 건드리지 않는지 확인
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
EnemyHealth t = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude))
{ var d = Vector2.Distance(e.transform.position, pc.transform.position); if (e.nowHP > 0 && d < best) { best = d; t = e; } }
if (t == null) return "no enemy";
t.CreatPoisonPrefab(2, 0.3f, 5);
t.CreatPoisonPrefab(2, 0.3f, 5);
t.CreatBleedingPrefab(1, 0.3f, 3);
return t.name + " hp after first ticks=" + t.nowHP + " bleeding=" + t.isBleeding + " dots=" + t.GetComponentsInChildren<DotEffectBase>().Length;
