// 장비 프리셋 검증(BasicFeatures.md 11절, 플레이 중, 인벤토리를 연 뒤): 지금 장비 → 프리셋1, LeafStaff·StoneCap 장착 → 프리셋2, 1·2 번갈아 장착
var inv = InventoryManager.instance;
if (!inv.inventory.activeSelf) return "inventory closed";
var sb = new System.Text.StringBuilder();
System.Func<string> worn = () => { var w = inv.EquippedFor("Weapon"); var h = inv.EquippedFor("Head"); return "W=" + (w != null ? w.name : "-") + " H=" + (h != null ? h.name : "-"); };
sb.Append("start " + worn() + " | ");
int s1 = GearPresets.SaveCurrent(0);
inv.CreatItem("LeafStaff"); inv.CreatItem("StoneCap");
ItemCheck staff = null, cap = null;
foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == "LeafStaff") staff = c; if (c.name == "StoneCap") cap = c; }
inv.EquipFromInventory(staff); inv.EquipFromInventory(cap);
sb.Append("after equip " + worn() + " | ");
int s2 = GearPresets.SaveCurrent(1);
int miss;
int c1 = GearPresets.Apply(0, out miss);
sb.Append("apply1 changed=" + c1 + " miss=" + miss + " " + worn() + " | ");
int c2 = GearPresets.Apply(1, out miss);
sb.Append("apply2 changed=" + c2 + " miss=" + miss + " " + worn() + " | ");
int c3 = GearPresets.Apply(2, out miss);
sb.Append("apply3(empty) changed=" + c3 + " | saved=" + string.Join(" / ", ES3.Load<string[]>(GearPresets.SaveKey)) + " leafInBag=" + inv.CountItem("LeafStaff") + " swordInBag=" + inv.CountItem("RustedSword"));
return "s1=" + s1 + " s2=" + s2 + " " + sb;
