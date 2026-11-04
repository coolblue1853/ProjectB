var o = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
ExtraEquipRewards.OnFloorBossFirstKill(1);
ExtraEquipRewards.OnBestiaryMilestone("B04"); // 중복 → 추가 지급 없어야
ExtraEquipRewards.OnFloorBossFirstKill(2);
foreach (var s in SlotRules.Extras) o.Append(s + "=" + ExtraEquip.IsUnlocked(s) + " ");
o.Append("\nTitleWanderer=" + inv.CountItem("TitleWanderer") + " AuraFaintGlow=" + inv.CountItem("AuraFaintGlow"));
foreach (var w in new[]{"TitleWanderer","AuraFaintGlow"}) {
  ItemCheck it = null;
  foreach (var c in UnityEngine.Object.FindObjectsByType<ItemCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (c.itemData != null && c.itemData.name == w && c.transform.IsChildOf(inv.transform)) { it = c; break; }
  o.Append("\n" + w + " found=" + (it!=null));
  if (it != null) o.Append(" handled=" + ExtraEquip.TryHandleEquip(it));
}
o.Append("\nequipped Title=" + (ExtraEquip.Get(EquipSlot.Title)?.name) + " Aura=" + (ExtraEquip.Get(EquipSlot.Aura)?.name));
var panel = inv.inventory.transform.Find("ExtraEquipPanel");
foreach (var t in panel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true)) if (t.name=="Label") o.Append("\n label '" + t.text + "'");
return o.ToString();
