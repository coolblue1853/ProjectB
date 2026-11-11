int k = 0; foreach (var e in UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) { if (e.gameObject.activeInHierarchy && e.nowHP > 0 && e.MonsterId == "RatThief_BD") { e.onlyDamage2Enemy(99999); k++; if (k >= 2) break; } }
return "killed=" + k + " Q106=" + NpcQuestLog.Progress(NpcQuestCatalog.Find("Q106")) + " day=" + TimeChange.instance.isDaytime;
