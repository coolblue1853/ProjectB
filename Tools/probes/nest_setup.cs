// 둥지 추적 검증 준비(FloorCoreContent 1절 ②, 플레이 중): __MAP__ 로 이동 → 낮 → 스포너 영역 트리거·주기 스폰 끄기(풀 유지) → 근처 몬스터 끄기 → 쿨타임 해제 → 시작 표식 앞으로 이동
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var phm0 = PlayerHealthManager.Instance; if (phm0 != null) phm0.nowHp = phm0.fullHP;
bool ok = MapInfo.GoToMap("__MAP__", pc.gameObject);
var tc = TimeChange.instance;
if (tc != null && !tc.isDaytime) { float target = (tc.endNighttime + 0.05f) * tc.totalTime; float add = target - tc.value * tc.totalTime; if (add < 0) add += tc.totalTime; tc.AddTime(add); }
NestHuntSite site = null;
foreach (var n in UnityEngine.Object.FindObjectsByType<NestHuntSite>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (n.transform.parent.name == "__MAP__") site = n;
if (site == null) return "no site in __MAP__ goto=" + ok;
var map = site.transform.parent;
foreach (var sp in map.GetComponentsInChildren<EnemySpawner>(true)) { sp.CancelInvoke(); foreach (var c in sp.GetComponents<Collider2D>()) c.enabled = false; sp.isInPlayer = false; }
int off = 0;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) { if (e != EnemySpawner.TrackedTarget) { e.gameObject.SetActive(false); off++; } }
site.ResetCooldown();
var rb = pc.GetComponent<Rigidbody2D>();
var log = new UnityEngine.GameObject("[NestSetup] __MAP__");
DG.Tweening.TweenSettingsExtensions.SetUpdate(DG.Tweening.DOVirtual.DelayedCall(0.8f, () =>
{
    var m0 = site.Mark(0);
    pc.transform.position = new Vector3(m0.transform.position.x - 6f, m0.transform.position.y + 1.5f, pc.transform.position.z);
    if (rb != null) rb.linearVelocity = Vector2.zero;
    var sb = new System.Text.StringBuilder(" day=" + (tc == null || tc.isDaytime));
    for (int i = 0; i < 4; i++) { var m = site.Mark(i); sb.Append(" m" + i + "@" + m.transform.position.x.ToString("F1") + "," + m.transform.position.y.ToString("F1")); }
    log.name += sb + " can0=" + site.CanInvestigate(0);
}), true);
return "goto " + ok + " enemiesOff=" + off;
