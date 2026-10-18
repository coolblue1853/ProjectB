// 플레이 중 핵심 상태 요약 (플레이어/카메라/적/스포너)
var sb = new System.Text.StringBuilder();
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var cam = Camera.main;
sb.AppendLine("playing=" + EditorApplication.isPlaying + " t=" + Time.time.ToString("F1"));
if (pc) sb.AppendLine("player pos=" + pc.transform.position + " active=" + pc.gameObject.activeInHierarchy);
if (cam) sb.AppendLine("camera pos=" + cam.transform.position + " ortho=" + cam.orthographicSize);
var hm = PlayerHealthManager.Instance;
if (hm != null) sb.AppendLine("hp=" + hm.nowHp + "/" + hm.fullHP);
var enemies = UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude);
sb.AppendLine("active enemies=" + enemies.Length);
foreach (var e in enemies) sb.AppendLine("  " + e.name + " hp=" + e.nowHP + " pos=" + e.transform.position);
var sp = UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include);
int act = 0; foreach (var s in sp) if (s.isActiveAndEnabled) act++;
sb.AppendLine("spawners total=" + sp.Length + " enabled=" + act);
return sb.ToString();
