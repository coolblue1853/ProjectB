var sb = new System.Text.StringBuilder();
foreach (var f in UnityEngine.Object.FindObjectsByType<FastTravelNpc>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
  var g = f.GetComponent<NpcQuestGiver>(); var t = f.GetComponent<TalkNpc>();
  sb.AppendLine(f.name + " stop=" + f.StopId + " giver=" + (g != null ? g.NpcId : "-") + " talk=" + (t != null) + " pos=" + f.transform.position.x.ToString("F1") + "," + f.transform.position.y.ToString("F1"));
}
DatabaseManager.money = 1000;
var town = FastTravelStops.Find("Town"); var side = FastTravelStops.Find("Lv1_NearForest_SideRoad"); var deep = FastTravelStops.Find("Lv1_DeepForest");
sb.AppendLine("open deep=" + FastTravelState.IsOpen(deep) + " eval deep=" + FastTravelFlow.Evaluate(deep, 100) + " eval side=" + FastTravelFlow.Evaluate(side, 75));
FastTravelFlow.Open("Town");
sb.AppendLine("menu open=" + NpcMenu.IsOpen + " count=" + NpcMenu.Count);
NpcMenu.PickCurrent();
sb.AppendLine("after pick: confirm open=" + ConfirmPopup.IsOpen + " menu=" + NpcMenu.IsOpen);
if (ConfirmPopup.IsOpen) { ConfirmPopup.Choose(true); }
sb.AppendLine("last=" + FastTravelFlow.LastTravelled + " money=" + DatabaseManager.money + " saved last=" + FastTravelState.Last);
return sb.ToString();
