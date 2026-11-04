var o = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
foreach (var w in new[]{"BanditPlunderMedal","ChiefsLastStandRing"}) {
  if (inv.CountItem(w) < 1) inv.CreatItem(w);
  ItemCheck it = null;
  foreach (var c in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (c.itemData != null && c.itemData.name == w && c.transform.IsChildOf(inv.transform)) { it = c; break; }
  if (it != null) typeof(InventoryManager).GetMethod("UseEquipment", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public).Invoke(inv, new object[]{it});
  o.Append(w + " itemFound=" + (it!=null) + " equipped=" + AccessoryGimmick.IsEquipped(w) + "\n");
}
return o.ToString();
