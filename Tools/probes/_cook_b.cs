CookingStove k = null;
foreach (var s in UnityEngine.Object.FindObjectsByType<CookingStove>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (s.IsKitchen) k = s;
var inv = InventoryManager.instance;
bool began = k.Begin();
string r = "began=" + began + " phase=" + (k.Session != null ? k.Session.Phase.ToString() + " recipes=" + k.Session.Recipes.Count : "none");
bool req = k.RequestStart();
r += " req=" + req + " popupOpen=" + k.PopupOpen + " qOpen=" + QuantityPopup.IsOpen + " qVal=" + QuantityPopup.CurrentValue;
return r;
