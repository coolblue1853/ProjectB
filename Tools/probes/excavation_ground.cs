// 발굴 유적 자리 재기(MiniGames.md ③, 플레이 중): __MAP__ 로 이동 → 포털·채집 지점·유적 x 와 x −440~−270(10 간격) 지면 높이(위 20 에서 아래로 Ground 레이캐스트)
var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
bool ok = MapInfo.GoToMap("__MAP__", pc.gameObject);
var map = GameObject.Find("Map").transform.Find("__MAP__");
var sb = new System.Text.StringBuilder("goto=" + ok + "\n");
foreach (var p in map.GetComponentsInChildren<Portal>(true)) sb.Append("portal x=" + p.transform.position.x.ToString("F0") + "\n");
foreach (var n in map.GetComponentsInChildren<GatherNode>(true)) sb.Append("gather x=" + n.transform.position.x.ToString("F0") + "\n");
foreach (var n in map.GetComponentsInChildren<ExcavationSite>(true)) sb.Append("site x=" + n.transform.position.x.ToString("F0") + " y=" + n.transform.position.y.ToString("F1") + "\n");
var filter = new ContactFilter2D(); filter.SetLayerMask(LayerMask.GetMask("Ground")); filter.useTriggers = false;
var hits = new RaycastHit2D[8];
for (float x = -440f; x <= -270f; x += 10f)
{
    int k = Physics2D.Raycast(new Vector2(x, 20f), Vector2.down, filter, hits, 60f);
    sb.Append(x.ToString("F0") + ":");
    for (int i = 0; i < k; i++) sb.Append(" " + hits[i].point.y.ToString("F1"));
    sb.Append("\n");
}
return sb.ToString();
