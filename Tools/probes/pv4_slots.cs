var o = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
var panel = inv.inventory.transform.Find("ExtraEquipPanel");
System.Func<string> labels = () => { var s = ""; foreach (var t in panel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true)) if (t.name=="Label") s += "[" + t.text + "] "; return s; };
o.Append("init " + labels());
foreach (var s in SlotRules.Extras) o.Append("\n hint " + s + ": " + ExtraEquip.UnlockHint(s));
float c0 = PlayerStats.Get(StatId.CritRate), a0 = PlayerStats.Get(StatId.AttackSpeed), d0 = PlayerStats.Get(StatId.DropRate), f0 = PlayerStats.Get(StatId.Def), cd0 = PlayerStats.Get(StatId.CritDmg), m0 = PlayerStats.Get(StatId.MoveSpeed);
for (int f = 1; f <= 4; f++) ExtraEquipRewards.OnFloorBossFirstKill(f);
ExtraEquipRewards.OnBestiaryMilestone("B04");
o.Append("\nunlocked " + labels());
foreach (var w in new[]{"EarringPlain","BraceletPlain","CharmPlain","StonePlain"}) o.Append("\n inv " + w + "=" + inv.CountItem(w));
// 잠금 상태 밖에서 추가 아이템
inv.CreatItem("EarringHunter", false, null, AddSource.Other);
inv.CreatItem("BraceletSwift", false, null, AddSource.Other);
foreach (var w in new[]{"EarringPlain","EarringHunter","BraceletPlain","BraceletSwift","CharmPlain","StonePlain"}) {
  ItemCheck it = null;
  foreach (var c in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (c.itemData != null && c.itemData.name == w && c.transform.IsChildOf(inv.transform)) { it = c; break; }
  o.Append("\n " + w + " found=" + (it!=null));
  if (it != null) o.Append(" handled=" + ExtraEquip.TryHandleEquip(it));
}
o.Append("\nafter " + labels());
o.Append("\nstat dCrit=" + (PlayerStats.Get(StatId.CritRate)-c0) + " dCritDmg=" + (PlayerStats.Get(StatId.CritDmg)-cd0) + " dAtt=" + (PlayerStats.Get(StatId.AttackSpeed)-a0) + " dMove=" + (PlayerStats.Get(StatId.MoveSpeed)-m0) + " dDrop=" + (PlayerStats.Get(StatId.DropRate)-d0) + " dDef=" + (PlayerStats.Get(StatId.Def)-f0));
o.Append("\nequipped " + ExtraEquip.Get(EquipSlot.Earring)?.name + "," + ExtraEquip.Get(EquipSlot.Bracelet)?.name + "," + ExtraEquip.Get(EquipSlot.Charm)?.name + "," + ExtraEquip.Get(EquipSlot.Stone)?.name);
ExtraEquip.Unequip(EquipSlot.Earring);
o.Append("\nunequip Earring dCrit=" + (PlayerStats.Get(StatId.CritRate)-c0));
return o.ToString();
