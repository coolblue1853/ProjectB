var sb = new System.Text.StringBuilder(); int a = 0;
foreach (var s in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { if (s.gameObject.activeInHierarchy) { a++; if (a < 40) sb.Append(s.name + "=" + s.MonsterProfile + " "); } }
return "activeSpawners=" + a + " " + sb + " | map active=" + (GameObject.Find("Lv1_NearForest_SideRoad") != null && GameObject.Find("Lv1_NearForest_SideRoad").activeInHierarchy);
