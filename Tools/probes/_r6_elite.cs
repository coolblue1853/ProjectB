// R-6 정예 배치 확인: 샛길 맵의 RatChief_BD 정예 여부·HP·killKey 메시지 id 열거
var pc = PlayerController.instance; MapInfo.GoToMap("Lv1_NearForest_SideRoad", pc.gameObject);
var sb = new System.Text.StringBuilder(); int n = 0, elites = 0;
foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { n++; if (e.MonsterId == "RatChief_BD" || e.IsElite) { sb.AppendLine(e.name + " id=" + e.MonsterId + " elite=" + e.IsElite + " always=" + e.AlwaysElite + " hp=" + e.maxHP + " def=" + e.enemyDef + " kill=" + e.KillMessageId + " x=" + e.transform.position.x.ToString("0.0")); if (e.IsElite && e.MonsterId == "RatChief_BD") elites++; } }
return "mobs=" + n + " chiefElites=" + elites + "\n" + sb;
