var sb = new System.Text.StringBuilder();
sb.AppendLine("lv=" + LevelSystem.Level + " exp=" + LevelSystem.Exp + " money=" + DatabaseManager.money);
foreach (var g in UnityEngine.Object.FindObjectsByType<NpcQuestGiver>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
    sb.AppendLine(g.NpcId + " @" + g.transform.position + " mark='" + g.MarkState + "' act=" + g.gameObject.activeInHierarchy);
sb.AppendLine("state=" + NpcQuestLog.Serialize());
return sb.ToString();
