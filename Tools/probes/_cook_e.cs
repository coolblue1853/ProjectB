CookingStove k = null;
foreach (var s in UnityEngine.Object.FindObjectsByType<CookingStove>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (s.IsKitchen) k = s;
var inv = InventoryManager.instance;
bool began = k.Begin();
bool started = k.TryStartCooking(3);
return "began=" + began + " started=" + started + " count=" + k.Session.Count + " rabbit=" + inv.CountItem("Rabbit Meat") + " herb=" + inv.CountItem("GreenHurb");
