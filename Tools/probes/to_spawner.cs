// __S__ = 원하는 스포너 이름 일부(예: Rabbit), 없으면 아무거나
// 활성 맵의 몹 스포너 트리거 안으로 플레이어를 옮긴다(스포너는 플레이어가 들어와야 몹을 낸다)
var pc = PlayerController.instance; EnemySpawner best = null;
foreach (var s in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude)) { if (best == null || (s.name.Contains("__S__") && !best.name.Contains("__S__"))) best = s; }
if (best == null) return "no spawner";
var col = best.GetComponent<Collider2D>();
pc.transform.position = col != null ? (Vector3)col.bounds.center + new Vector3(0f, 0f, 0f) : best.transform.position;
return "moved to " + best.name;
