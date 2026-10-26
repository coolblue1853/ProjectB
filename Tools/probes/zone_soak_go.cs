// 구역 회귀 소크 한 단계: __DN__(day/night)로 시간을 맞추고 __MAP__ 으로 이동(MapInfo.GoToMap) → 그 시간대 일반 스포너 중 슬롯이 가장 많은 곳의 스폰 지점으로 옮긴다
// (스포너는 플레이어가 영역 안에 있어야 스폰한다 — 입구만 보면 스폰 0 이 정상인 맵이 있다). 반환: 시작 게임 시간
var tc = TimeChange.instance;
bool wantNight = "__DN__" == "night";
if (wantNight) { tc.LockDaytime = false; } // 낮 고정(기본 켜짐)을 풀어야 밤으로 넘어간다
float target = ((wantNight ? tc.startNighttime : tc.endNighttime) + 0.05f) * tc.totalTime;
float add = target - tc.value * tc.totalTime; if (add < 0) add += tc.totalTime;
tc.AddTime(add);
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
bool ok = MapInfo.GoToMap("__MAP__", pc.gameObject);
string at = "entry";
var map = GameObject.Find("Map").transform.Find("__MAP__");
if (!ok || map == null) { return Time.time.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + " ok=False at=none"; }
EnemySpawner best = null;
foreach (var sp in map.GetComponentsInChildren<EnemySpawner>(true))
{
    if (sp.isDaytimeSpowner == wantNight || sp.enemyPositon == null || sp.enemyPositon.transform.childCount == 0) continue;
    if (sp.name.Contains("Boss")) continue;
    if (best == null || sp.enemyCount > best.enemyCount) best = sp;
}
if (best != null)
{
    pc.transform.position = best.enemyPositon.transform.GetChild(0).position + new Vector3(0f, 1f, 0f);
    at = best.name;
}
return Time.time.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + " ok=" + ok + " at=" + at;
