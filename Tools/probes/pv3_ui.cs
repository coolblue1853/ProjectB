var o = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
o.Append("ExtraEquip unlocked: ");
foreach (var s in SlotRules.Extras) o.Append(s + "=" + ExtraEquip.IsUnlocked(s) + " ");
inv.inventory.SetActive(true);
var panel = inv.inventory.transform.Find("ExtraEquipPanel");
o.Append("\npanel=" + (panel != null) + " active=" + (panel != null && panel.gameObject.activeInHierarchy));
if (panel != null) {
  foreach (var t in panel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true)) o.Append("\n " + t.name + " '" + t.text + "'");
  var box = panel.Find("Box0");
  UnityEngine.EventSystems.ExecuteEvents.Execute(box.gameObject, new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current), UnityEngine.EventSystems.ExecuteEvents.pointerEnterHandler);
  o.Append("\n tip after hover Box0: '" + panel.Find("Tip").GetComponent<TMPro.TextMeshProUGUI>().text + "'");
}
return o.ToString();
