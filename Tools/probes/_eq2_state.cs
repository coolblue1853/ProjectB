var sb = new System.Text.StringBuilder();
sb.Append("unlockKey=" + (ES3.KeyExists("equip.extraUnlock") ? ES3.Load<int>("equip.extraUnlock").ToString() : "none"));
sb.Append(" features=" + (ES3.KeyExists("features.unlocked") ? string.Join(",", ES3.Load<string[]>("features.unlocked")) : "none"));
sb.Append(" extra=" + (ES3.KeyExists("equip.extra") ? string.Join("|", ES3.Load<string[]>("equip.extra")) : "none"));
var inv = InventoryManager.instance;
sb.Append(" inv=" + (inv != null));
if (inv != null) { foreach (int b in new[] {1,2,3,5,6,7,8,9}) sb.Append(" b" + b + "=" + (inv.equipBox[b].activeSelf ? "on" : "off") + ":" + (inv.equipBox[b].transform.childCount > 0 ? inv.equipBox[b].transform.GetChild(0).name : "-")); }
sb.Append(" earr=" + (ExtraEquip.Get(EquipSlot.Earring) != null ? ExtraEquip.Get(EquipSlot.Earring).name : "-") + " earrOpen=" + ExtraEquip.IsUnlocked(EquipSlot.Earring) + " charmOpen=" + ExtraEquip.IsUnlocked(EquipSlot.Charm));
return sb.ToString();
