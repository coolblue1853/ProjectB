// REWORK-2 S2 우클릭·호버·상점 마우스 확인 → Logs/playtest/s2.txt
var pc = PlayerController.instance; if (pc == null) return "no player";
PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
if (!GameObject.Find("Map").transform.Find("Town").gameObject.activeInHierarchy) MapInfo.GoToMap("Town", pc.gameObject);
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
System.Action<string> log = s => sb.AppendLine(s);
var steps = new System.Collections.Generic.List<System.Func<string>>();
System.Func<string, UnityEngine.EventSystems.PointerEventData.InputButton, string> click = (nm, btn) => {
  ItemCheck it = null; foreach (var c in inv.EnumerateInventoryItems()) { if (c.name == nm) { it = c; break; } }
  if (it == null) return "click: no " + nm;
  var ed = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = btn, clickCount = 1 };
  UnityEngine.EventSystems.ExecuteEvents.Execute(it.gameObject, ed, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
  return "clicked " + btn + " " + nm;
};
System.Func<string, string> hover = nm => {
  ItemCheck it = null; foreach (var c in inv.EnumerateInventoryItems()) { if (c.name == nm) { it = c; break; } }
  if (it == null) return "hover: no " + nm;
  var ed = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
  UnityEngine.InputSystem.LowLevel.InputState.Change(UnityEngine.InputSystem.Mouse.current.position, (Vector2)it.transform.position);
  UnityEngine.EventSystems.ExecuteEvents.Execute(it.gameObject, ed, UnityEngine.EventSystems.ExecuteEvents.pointerEnterHandler);
  return "hover " + nm + " showing=" + HoverTip.IsShowing;
};
steps.Add(() => {
  foreach (var n in new[] { "Potion", "Wood", "LeafArmor" }) { while (inv.CountItem(n) > 0) { inv.Service.Remove(n, inv.CountItem(n)); } }
  inv.state = ""; inv.chest = null; DatabaseManager.money = 1000;
  for (int i = 0; i < 5; i++) inv.CreatItem("Potion");
  for (int i = 0; i < 3; i++) inv.CreatItem("Wood");
  inv.CreatItem("LeafArmor");
  inv.inventory.SetActive(true); inv.nowBox=0; inv.OpenBox(0); UiLock.Set("InvGroup", true); inv.cusor.SetActive(true);
  return "setup potion=" + inv.CountItem("Potion") + " wood=" + inv.CountItem("Wood") + " cap=" + inv.CountItem("LeafArmor");
});
steps.Add(null); steps.Add(null);
steps.Add(() => hover("LeafArmor")); steps.Add(null);
steps.Add(() => { return "tipShowing=" + HoverTip.IsShowing; });
steps.Add(null); steps.Add(null); steps.Add(null);
steps.Add(() => { ScreenCapture.CaptureScreenshot("Logs/playtest/s2_tip.png"); return "shot queued"; });
steps.Add(() => { HoverTip.Hide(); return click("Potion", UnityEngine.EventSystems.PointerEventData.InputButton.Right) + " -> potion=" + inv.CountItem("Potion"); });
steps.Add(null);
steps.Add(() => click("LeafArmor", UnityEngine.EventSystems.PointerEventData.InputButton.Right) + " -> cap in inv=" + inv.CountItem("LeafArmor") + " worn=" + (inv.EquippedFor("Chest") != null ? inv.EquippedFor("Chest").name : "null") + " state=" + inv.state);
steps.Add(null);
steps.Add(() => { var sm = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include); UiLock.Set("InvGroup", false); sm.OpenShop(); return "shop open=" + inv.ShopGameObject.activeSelf + " state=" + inv.state + " closeX=" + (sm.ShopUI.transform.Find("CloseX") != null); });
steps.Add(null); steps.Add(null);
steps.Add(() => click("Wood", UnityEngine.EventSystems.PointerEventData.InputButton.Right) + " -> popupOpen=" + QuantityPopup.IsOpen + " val=" + QuantityPopup.CurrentValue + " state=" + inv.state);
steps.Add(null);
steps.Add(() => { int m0 = DatabaseManager.money; QuantityPopup.ConfirmNow(); return "sell confirm money " + m0 + "->" + DatabaseManager.money + " wood=" + inv.CountItem("Wood"); });
steps.Add(null); steps.Add(null); steps.Add(null);
steps.Add(() => {
  var sm = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
  var items = sm.LV1craftBox[0].GetComponentsInChildren<ItemCheck>(false);
  var slot = items[items.Length > 1 ? 1 : 0]; var m = slot.GetComponent<ShopSlotMouse>();
  var ed = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = UnityEngine.EventSystems.PointerEventData.InputButton.Left, clickCount = 1 };
  UnityEngine.EventSystems.ExecuteEvents.Execute(slot.gameObject, ed, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
  string a = "stock item " + slot.name + " hasMouse=" + (m != null) + " 1stclick: detail=" + sm.isBuyDetail + " popup=" + QuantityPopup.IsOpen + " invState=" + inv.state;
  UnityEngine.EventSystems.ExecuteEvents.Execute(slot.gameObject, ed, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
  return a + " | 2ndclick: popup=" + QuantityPopup.IsOpen + " state=" + inv.state;
});
steps.Add(null);
steps.Add(() => { int m0 = DatabaseManager.money; int p0 = inv.CountItem("Potion"); QuantityPopup.ConfirmNow(); return "buy confirm money " + m0 + "->" + DatabaseManager.money + " stateAfter=" + inv.state; });
steps.Add(null); steps.Add(null);
steps.Add(() => { var sm = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include); var ed = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { scrollDelta = new Vector2(0, -1) }; var items = sm.LV1craftBox[0].GetComponentsInChildren<ItemCheck>(false); string before = sm.nowShopItem != null ? sm.nowShopItem.name : "null"; UnityEngine.EventSystems.ExecuteEvents.Execute(items[0].gameObject, ed, UnityEngine.EventSystems.ExecuteEvents.scrollHandler); return "wheel before=" + before + " after=" + sm.nowShopItem.name + " pages=" + sm.LV1craftBox[0].transform.childCount; });
steps.Add(() => { var sm = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include); var b = sm.ShopUI.transform.Find("CloseX").GetComponent<UnityEngine.UI.Button>(); b.onClick.Invoke(); return "closeX -> invOpen=" + inv.inventory.activeSelf + " shop=" + inv.ShopGameObject.activeSelf; });
steps.Add(null);
steps.Add(() => { inv.inventory.SetActive(false); UiLock.Set("InvGroup", false); return "cleanup"; });
int idx = 0; double next = UnityEditor.EditorApplication.timeSinceStartup + 2;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
    double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < next) return;
    next = now + 0.15;
    if (idx >= steps.Count) { System.IO.File.WriteAllText("Logs/playtest/s2.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
    var st = steps[idx++]; if (st == null) return;
    string r = null; try { r = st(); } catch (System.Exception e) { r = "EX " + e.Message + e.StackTrace.Split('\n')[0]; }
    if (r != null) log(r);
};
UnityEditor.EditorApplication.update += upd; return "started " + steps.Count;
