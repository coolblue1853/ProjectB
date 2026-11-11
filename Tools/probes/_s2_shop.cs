var sm = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
if (sm == null) return "no ShopManager (playing=" + Application.isPlaying + ")";
string sb = "";
sb += "playing=" + Application.isPlaying + " boxes=" + sm.LV1craftBox.Length + "\n";
var items = sm.LV1craftBox[0].GetComponentsInChildren<ItemCheck>(true);
sb += "items=" + items.Length + "\n";
for (int i = 0; i < Mathf.Min(3, items.Length); i++) {
  var it = items[i]; var g = it.GetComponent<UnityEngine.UI.Graphic>();
  sb += it.transform.parent.parent.name + "/" + it.transform.parent.name + "/" + it.name + " graphic=" + (g!=null? g.GetType().Name + " ray=" + g.raycastTarget : "none") + " children=" + it.transform.childCount + "\n";
  for (int c = 0; c < it.transform.childCount; c++) { var cg = it.transform.GetChild(c).GetComponent<UnityEngine.UI.Graphic>(); sb += "   child " + it.transform.GetChild(c).name + " " + (cg!=null? cg.GetType().Name+" ray="+cg.raycastTarget:"none") + "\n"; }
}
return sb;
