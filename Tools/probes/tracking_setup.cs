// 흔적 추적 검증 준비(MiniGames.md ⑥, 플레이 중): __MAP__ 맵으로 이동 → 낮 → 스포너 영역 트리거·주기 스폰 끄기(풀은 유지 — 추적 정예 소환에 필요, 테스트 전용)
// → 근처 몬스터 끄기(피격 취소 방지) → 첫 지점으로 이동. 결과는 "[TrkSetup] ..." 오브젝트 이름
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var phm0 = PlayerHealthManager.Instance; if (phm0 != null) phm0.nowHp = phm0.fullHP;
bool ok = MapInfo.GoToMap("__MAP__", pc.gameObject);
var tc = TimeChange.instance;
if (tc != null && !tc.isDaytime) { float target = (tc.endNighttime + 0.05f) * tc.totalTime; float add = target - tc.value * tc.totalTime; if (add < 0) add += tc.totalTime; tc.AddTime(add); }
TrackingSpot node = null;
foreach (var n in UnityEngine.Object.FindObjectsByType<TrackingSpot>(FindObjectsSortMode.None)) if (n.transform.parent.name == "__MAP__" && (node == null || n.name.CompareTo(node.name) < 0)) node = n;
if (node == null) return "no spot in __MAP__ goto=" + ok;
var map = node.transform.parent;
foreach (var sp in map.GetComponentsInChildren<EnemySpawner>(true)) { sp.CancelInvoke(); foreach (var c in sp.GetComponents<Collider2D>()) c.enabled = false; sp.isInPlayer = false; }
int off = 0;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) { if (e != EnemySpawner.TrackedTarget && Mathf.Abs(e.transform.position.x - node.transform.position.x) < 80f) { e.gameObject.SetActive(false); off++; } }
node.ResetCooldown();
var rb = pc.GetComponent<Rigidbody2D>();
var log = new UnityEngine.GameObject("[TrkSetup] __MAP__");
DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.DOVirtual.DelayedCall(0.5f, () =>
{
    pc.transform.position = new Vector3(node.transform.position.x, node.transform.position.y + 1.5f, pc.transform.position.z);
    if (rb != null) rb.linearVelocity = Vector2.zero;
    log.name += " node=" + node.name + "@" + node.transform.position + " ready=" + node.IsReady;
}), true);
return "goto " + ok + " node " + node.name + " enemiesOff=" + off;
