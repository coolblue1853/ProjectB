// 낚시 검증 준비(MiniGames.md, 플레이 중): 늪(Lv1_Swamp)으로 이동 → 첫 낚시터 앞에 세움 → 낮 → 근처 몬스터 끄기(피격 취소 방지, 테스트 전용)
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var phm0 = PlayerHealthManager.Instance; if (phm0 != null) phm0.nowHp = phm0.fullHP;
bool ok = MapInfo.GoToMap("Lv1_Swamp", pc.gameObject);
pc.transform.position = new Vector3(-330f, -178.6f, pc.transform.position.z);
var rb = pc.GetComponent<Rigidbody2D>();
if (rb != null) rb.linearVelocity = Vector2.zero;
var tc = TimeChange.instance;
if (tc != null && !tc.isDaytime) { float target = (tc.endNighttime + 0.05f) * tc.totalTime; float add = target - tc.value * tc.totalTime; if (add < 0) add += tc.totalTime; tc.AddTime(add); }
int off = 0;
foreach (var sp in UnityEngine.Object.FindObjectsByType<FishingSpot>(FindObjectsSortMode.None)) { var map = sp.transform.parent; foreach (string n in new[] { "DayTimeSpawn", "NightTimeSpawn" }) { var t = map.Find(n); if (t != null) t.gameObject.SetActive(false); } }
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
{ if (Mathf.Abs(e.transform.position.x - pc.transform.position.x) < 60f && Mathf.Abs(e.transform.position.y - pc.transform.position.y) < 30f) { e.gameObject.SetActive(false); off++; } }
var sb = new System.Text.StringBuilder();
foreach (var s in UnityEngine.Object.FindObjectsByType<FishingSpot>(FindObjectsSortMode.None)) sb.Append(s.name + "@" + s.transform.position + " ");
return "goto " + ok + " pos " + pc.transform.position + " enemiesOff=" + off + " spots: " + sb + " rod=" + FishingSave.LoadRod();
