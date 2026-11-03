var pc = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
var sb = new System.Text.StringBuilder();
bool day = TimeChange.instance == null || TimeChange.instance.isDaytime;
sb.AppendLine("day=" + day);
bool ok = MapInfo.GoToMap("__MAP__", pc.gameObject);
pc.transform.position = new UnityEngine.Vector3(__X__, __Y__, pc.transform.position.z);
sb.AppendLine("goto " + ok + " pos " + pc.transform.position);
foreach (var s in UnityEngine.Object.FindObjectsByType<MiniBossSite>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
  sb.AppendLine(s.name + " active=" + s.gameObject.activeInHierarchy + " alive=" + s.Alive);
foreach (var s in UnityEngine.Object.FindObjectsByType<NestHuntSite>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
  sb.AppendLine("nest " + s.transform.parent.name + " bonus=" + string.Join(",", s.BonusLoot));
return sb.ToString();
