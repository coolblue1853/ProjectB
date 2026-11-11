CookingStove k = null;
foreach (var s in UnityEngine.Object.FindObjectsByType<CookingStove>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (s.IsKitchen) k = s;
var inv = InventoryManager.instance;
return k.Session == null ? "session null" : ("phase=" + k.Session.Phase + " t=" + k.Session.Elapsed + " round=" + k.Session.Round + " resultId=" + k.ResultId + " n=" + k.ResultCount + " lost=" + k.Lost + " herbRoast=" + inv.CountItem("Herb Roasted Rabbit") + " burnt=" + inv.CountItem("Burnt Meal"));
