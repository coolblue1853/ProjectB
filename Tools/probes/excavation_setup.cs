// 발굴 검증 준비(MiniGames.md, 플레이 중): __MAP__ 맵으로 이동 → 낮 → 스포너·근처 몬스터 끄기(피격 취소 방지, 테스트 전용) → 첫 지점 x 로 이동(y 는 지점 위 3, 떨어져 착지)
// 유적은 첫 Update 에 지면에 붙으므로 0.5초 뒤 한 번 더 그 자리로 옮긴다. 결과는 "[ExcSetup] ..." 오브젝트 이름
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var phm0 = PlayerHealthManager.Instance; if (phm0 != null) phm0.nowHp = phm0.fullHP;
bool ok = MapInfo.GoToMap("__MAP__", pc.gameObject);
var tc = TimeChange.instance;
if (tc != null && !tc.isDaytime) { float target = (tc.endNighttime + 0.05f) * tc.totalTime; float add = target - tc.value * tc.totalTime; if (add < 0) add += tc.totalTime; tc.AddTime(add); }
ExcavationSite node = null;
foreach (var n in UnityEngine.Object.FindObjectsByType<ExcavationSite>(FindObjectsSortMode.None)) if (n.transform.parent.name == "__MAP__" && (node == null || n.name.CompareTo(node.name) < 0)) node = n;
if (node == null) return "no node in __MAP__ goto=" + ok;
var map = node.transform.parent;
foreach (string nm in new[] { "DayTimeSpawn", "NightTimeSpawn" }) { var t = map.Find(nm); if (t != null) t.gameObject.SetActive(false); }
int off = 0;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) { if (Mathf.Abs(e.transform.position.x - node.transform.position.x) < 60f) { e.gameObject.SetActive(false); off++; } }
node.ResetCooldown();
var rb = pc.GetComponent<Rigidbody2D>();
var log = new UnityEngine.GameObject("[ExcSetup] __MAP__");
DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.DOVirtual.DelayedCall(0.5f, () =>
{
    pc.transform.position = new Vector3(node.transform.position.x, node.transform.position.y + 1.5f, pc.transform.position.z);
    if (rb != null) rb.linearVelocity = Vector2.zero;
    log.name += " node=" + node.name + "@" + node.transform.position;
}), true);
return "goto " + ok + " node " + node.name + " enemiesOff=" + off;
