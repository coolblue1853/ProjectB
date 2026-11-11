CookingStove k = null;
foreach (var s in UnityEngine.Object.FindObjectsByType<CookingStove>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (s.IsKitchen) k = s;
var inv = InventoryManager.instance;
bool too = k.TryStartCooking(3);
bool ok = k.TryStartCooking(2);
return "too=" + too + " ok=" + ok + " count=" + k.Session.Count + " rabbit=" + inv.CountItem("Rabbit Meat") + " herb=" + inv.CountItem("GreenHurb");
