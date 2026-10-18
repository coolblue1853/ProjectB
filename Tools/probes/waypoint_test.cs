// 웨이포인트: 지도 열기(스크린샷용) 또는 이동. __KEY__ = open | travel:<맵이름> | state
var k = "__KEY__";
if (k == "open") { WaypointMap.OpenNow(); return "opened"; }
if (k.StartsWith("travel:")) { var ok = WaypointMap.TravelTo(k.Substring(7)); return "travel " + k.Substring(7) + " -> " + ok; }
var sb = new System.Text.StringBuilder("active maps: ");
foreach (Transform m in GameObject.Find("Map").transform) if (m.gameObject.activeSelf) sb.Append(m.name + "(visited=" + WaypointMap.IsVisited(m.name) + ") ");
return sb + "| player=" + PlayerController.instance.transform.position.ToString("F0") + " load=" + AscentLoad.CurrentLoad;
