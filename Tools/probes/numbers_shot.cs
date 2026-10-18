// 숫자 스타일 확인용: 근처 몬스터에 일반·치명·추가타·독 숫자를 띄우고 시간을 멈춘다 (스크린샷 후 numbers_resume.cs로 재개)
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
EnemyHealth t = null; float best = 1e9f;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude))
{ var d = Vector2.Distance(e.transform.position, pc.transform.position); if (e.nowHP > 0 && d < best) { best = d; t = e; } }
if (t == null) return "no enemy";
var basePos = t.transform.position + Vector3.up;
HitFeedback.SpawnNumber(t.damageNumber, basePos + new Vector3(-1.2f, 0, 0), 12, HitFeedback.NumberKind.Normal);
HitFeedback.SpawnNumber(t.damageNumber, basePos + new Vector3(0f, 0.3f, 0), 37, HitFeedback.NumberKind.Crit);
HitFeedback.SpawnNumber(t.damageNumber, basePos + new Vector3(0f, 0.3f, 0), 9, HitFeedback.NumberKind.Extra);
HitFeedback.SpawnNumber(t.damageNumber, basePos + new Vector3(1.2f, 0, 0), 4, HitFeedback.NumberKind.Poison);
Time.timeScale = 0f;
return "spawned near " + t.name;
