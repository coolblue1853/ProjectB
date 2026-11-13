var sb = new System.Text.StringBuilder();
foreach (var s in UnityEngine.Object.FindObjectsByType<ZoneReportSign>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  var map = s.GetComponentInParent<MapInfo>(true);
  var mapId = (string)typeof(ZoneReportSign).GetField("_mapId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(s);
  var f = QuestEntrance.ZoneSign(mapId);
  int av = QuestService.Query(f.WithTab(QuestListRules.Tab.Available)).Items.Count;
  int ac = QuestService.Query(f.WithTab(QuestListRules.Tab.Active)).Items.Count;
  var ids = new System.Collections.Generic.List<string>();
  foreach (var d in QuestService.Query(f.WithTab(QuestListRules.Tab.Available)).Items) ids.Add(d.Id + "(" + d.Giver + ")");
  sb.AppendLine((map != null ? map.name : "-") + " mapId=" + mapId + " avail=" + av + " active=" + ac + " [" + string.Join(",", ids) + "]");
}
return sb.ToString();
