// REWORK-1 S3 칸 드래그 교환·이동·합치기 확인 → Logs/playtest/s3_drag.txt
var pc = PlayerController.instance; if (pc == null) return "no player";
PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
int pg = 0; System.Func<UnityEngine.Transform> PG = () => inv.inventoryUI[pg].transform;
var steps = new System.Collections.Generic.List<System.Func<string>>();
System.Func<int, string> slot = i => { var b = PG().GetChild(i); if (b.childCount == 0) return "[" + i + ":-]"; var c = b.GetChild(0).GetComponent<ItemCheck>(); return "[" + i + ":" + c.name + "x" + c.nowStack + "]"; };
System.Func<string, int> find = nm => { for (int i = 0; i < PG().childCount; i++) { var b = PG().GetChild(i); if (b.childCount > 0 && b.GetChild(0).GetComponent<ItemCheck>().name.StartsWith(nm)) return i; } return -1; };
System.Func<int, int, string> drag = (f, t) => {
  var page = PG(); var it = page.GetChild(f).GetChild(0);
  var es = UnityEngine.EventSystems.EventSystem.current;
  var ed = new UnityEngine.EventSystems.PointerEventData(es) { button = UnityEngine.EventSystems.PointerEventData.InputButton.Left, position = (Vector2)it.position };
  var go = it.gameObject;
  UnityEngine.EventSystems.ExecuteEvents.Execute(go, ed, UnityEngine.EventSystems.ExecuteEvents.beginDragHandler);
  ed.position = (Vector2)page.GetChild(t).position;
  UnityEngine.EventSystems.ExecuteEvents.Execute(go, ed, UnityEngine.EventSystems.ExecuteEvents.dragHandler);
  UnityEngine.EventSystems.ExecuteEvents.Execute(go, ed, UnityEngine.EventSystems.ExecuteEvents.endDragHandler);
  return "drag " + f + "->" + t;
};
int p = -1, w = -1, e = -1;
steps.Add(() => {
  foreach (var n in new[] { "Potion", "Wood" }) { while (inv.CountItem(n) > 0) { inv.Service.Remove(n, inv.CountItem(n)); } }
  inv.state = ""; inv.chest = null;
  for (int i = 0; i < 3; i++) inv.CreatItem("Potion");
  for (int i = 0; i < 2; i++) inv.CreatItem("Wood");
  inv.inventory.SetActive(true); inv.nowBox = 0; inv.OpenBox(0); UiLock.Set("InvGroup", true); inv.cusor.SetActive(true);
  return "created";
});
steps.Add(null); steps.Add(null);
steps.Add(() => {
  for (int q = 0; q < inv.inventoryUI.Length; q++) { pg = q; if (find("Potion") >= 0) break; }
  inv.nowBox = pg; for (int q2 = 0; q2 < inv.inventoryUI.Length; q2++) inv.inventoryUI[q2].SetActive(q2 == pg);
  p = find("Potion"); w = -1; for (int i = 0; i < PG().childCount; i++) { if (i != p && PG().GetChild(i).childCount > 0) { w = i; break; } }
  for (int i = 0; i < PG().childCount; i++) { if (PG().GetChild(i).childCount == 0 && i != p && i != w && inv.Service.IndexOfBox(PG().GetChild(i)) >= 0) { e = i; break; } }
  return "setup p=" + p + " w=" + w + " empty=" + e + " " + slot(p) + slot(w);
});
steps.Add(null); steps.Add(null);
steps.Add(() => drag(p, e) + " => " + slot(p) + slot(e) + " dragging=" + ItemDrag.IsDragging); steps.Add(null);
steps.Add(() => { int t = p; p = e; e = t; return "after-move " + slot(p) + slot(e) + slot(w); });
steps.Add(() => drag(p, w) + " => " + slot(p) + slot(w)); steps.Add(null);
steps.Add(() => { int t = p; p = w; w = t; return "after-swap potion@" + p + " wood@" + w + " " + slot(p) + slot(w); });
steps.Add(() => { var wi = PG().GetChild(p).GetChild(0).GetComponent<ItemCheck>(); bool ok = inv.Service.SplitAt(wi, 1, pg); return "split=" + ok; });
steps.Add(null); steps.Add(null);
steps.Add(() => { int w2 = -1; for (int i = 0; i < PG().childCount; i++) { var b = PG().GetChild(i); if (i != p && b.childCount > 0 && b.GetChild(0).GetComponent<ItemCheck>().name.StartsWith("Potion")) { w2 = i; break; } } e = w2; return "second potion@" + w2 + " " + slot(p) + slot(w2); });
steps.Add(() => drag(e, p) + " => " + slot(p) + slot(e)); steps.Add(null);
steps.Add(() => "final wood total=" + inv.CountItem("Wood") + " potion=" + inv.CountItem("Potion"));
steps.Add(() => { foreach (var n in new[] { "Potion", "Wood" }) { while (inv.CountItem(n) > 0) { inv.Service.Remove(n, inv.CountItem(n)); } } inv.inventory.SetActive(false); UiLock.Set("InvGroup", false); return "cleanup"; });
int idx = 0; double next = UnityEditor.EditorApplication.timeSinceStartup + 2;
UnityEditor.EditorApplication.CallbackFunction upd = null;
upd = () => {
    double now = UnityEditor.EditorApplication.timeSinceStartup; if (now < next) return;
    next = now + 0.15;
    if (idx >= steps.Count) { System.IO.File.WriteAllText("Logs/playtest/s3_drag.txt", sb.ToString()); UnityEditor.EditorApplication.update -= upd; return; }
    var st = steps[idx++]; if (st == null) return;
    string r = null; try { r = st(); } catch (System.Exception ex) { r = "EX " + ex.Message + ex.StackTrace.Split('\n')[0]; }
    if (r != null) sb.AppendLine(r);
};
UnityEditor.EditorApplication.update += upd; return "started " + steps.Count;
