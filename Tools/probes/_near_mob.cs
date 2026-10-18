// 테스트 보조: 이름이 __M__ 로 시작하는 몹 중 가장 낮은 층(y 최소) 몹의 3유닛 왼쪽으로 플레이어를 옮긴다(발판 끝에서 떨어지지 않게)
var pc = PlayerController.instance; EnemyHealth best = null;
foreach (var e in EnemyHealth.Active) { if (e.IsMonster && !e.isBossMob && e.name.StartsWith("__M__") && (best == null || e.transform.position.y < best.transform.position.y)) best = e; }
if (best == null) return "none";
pc.transform.position = best.transform.position + new Vector3(-3f, 0.3f, 0f);
return "at " + best.name + " " + best.transform.position.ToString("F1");
