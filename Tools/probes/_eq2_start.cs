var inv = InventoryManager.instance;
ExtraEquip.Unequip(EquipSlot.Earring);
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
typeof(InventoryManager).GetMethod("EquipStartOutfit", flags).Invoke(inv, null);
var sb = new System.Text.StringBuilder();
foreach (int b in new[] {1,3,5,7}) sb.Append(" b" + b + "=" + (inv.equipBox[b].transform.childCount > 0 ? inv.equipBox[b].transform.GetChild(0).GetComponent<ItemCheck>().itemData.name : "-"));
var e = ExtraEquip.Get(EquipSlot.Earring);
sb.Append(" earr=" + (e != null ? e.name : "-") + " setStack=" + string.Join(",", System.Linq.Enumerable.Select(DatabaseManager.setsEffectStack, kv => kv.Key + ":" + kv.Value)));
return sb.ToString();
