var o = new System.Text.StringBuilder();
InventoryManager.instance.inventory.SetActive(false);
var fx = GameObject.Find("ExtraEquipFx");
o.Append("fx=" + (fx!=null));
if (fx!=null) {
  foreach (var l in fx.GetComponentsInChildren<LineRenderer>(true)) o.Append("\n ring '" + l.name + "' active=" + l.gameObject.activeInHierarchy + " enabled=" + l.enabled + " pts=" + l.positionCount + " pos=" + l.transform.position);
  foreach (var t in fx.GetComponentsInChildren<TMPro.TextMeshPro>(true)) o.Append("\n title '" + t.name + "' '" + t.text + "' active=" + t.gameObject.activeInHierarchy + " pos=" + t.transform.position);
}
o.Append("\nplayer=" + PlayerController.instance.transform.position);
return o.ToString();
