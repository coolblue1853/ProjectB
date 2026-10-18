var inv = InventoryManager.instance;
var chest = UnityEngine.Object.FindFirstObjectByType<Chest>(FindObjectsInactive.Include);
typeof(Chest).GetMethod("C2IMove", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(chest, null);
string r = "after C2I next frame: inv Wood=" + inv.CountItem("Wood");
inv.DeletItemByName("Wood", inv.CountItem("Wood"));
return r;
