CookingStove k = null;
foreach (var s in UnityEngine.Object.FindObjectsByType<CookingStove>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (s.IsKitchen) k = s;
var inv = InventoryManager.instance;
QuantityPopup.ConfirmNow();
return "phase=" + k.Session.Phase + " count=" + k.Session.Count + " rabbit=" + inv.CountItem("Rabbit Meat") + " herb=" + inv.CountItem("GreenHurb") + " wood=" + inv.CountItem("Wood") + " qOpen=" + QuantityPopup.IsOpen;
