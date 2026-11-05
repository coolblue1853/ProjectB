string wn = "FangDagger";
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex(wn));
box.LoadPrefab(wn, "Weapon", d.tfName);
return "equipping " + wn;
