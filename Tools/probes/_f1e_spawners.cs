// 샛길 스포너 낮/밤 플래그 열거
var sb = new System.Text.StringBuilder();
foreach (var s in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { var p = s.transform.parent; string path = s.name; for (var t = p; t != null && path.Length < 120; t = t.parent) path = t.name + "/" + path; if (path.Contains("SideRoad")) sb.AppendLine(path + " day=" + s.isDaytimeSpowner); }
return TimeChange.instance != null ? "total=" + TimeChange.instance.totalTime + " ns=" + TimeChange.instance.startNighttime + " ne=" + TimeChange.instance.endNighttime + "\n" + sb : "no tc\n" + sb;
