// 구역 회귀 소크 집계: 게임 시간, 낮/밤, 활성 몬스터 수(보스 포함), 플레이어 위치, 감시기 이상 수
var tc = TimeChange.instance;
int n = 0, boss = 0;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
{
    if (!e.isActiveAndEnabled) continue;
    n++;
    if (e.name.Contains("Boss") || e.GetComponent<BossPhase>() != null) boss++;
}
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var ic = System.Globalization.CultureInfo.InvariantCulture;
return "t=" + Time.time.ToString("F1", ic) + " day=" + tc.isDaytime + " mobs=" + n + " boss=" + boss + " pos=" + pc.transform.position.x.ToString("F0", ic) + "," + pc.transform.position.y.ToString("F0", ic) + " anomalies=" + RuntimeMonitor.AnomalyCount;
