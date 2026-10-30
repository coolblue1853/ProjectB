// Phase 3b 검증(플레이 모드): 키보드 교환(ActiveChangeCursor)·드래그 인벤→인벤 놓기가 InventoryService.MoveSlot 으로
// 이동·교환·합치기 되는지, 드래그 중 저장이 미뤄졌다가 놓을 때 1회로 풀리는지(B-219), 저장 = UI 인지 본다. 세이브는 프로브 밖에서 백업·복원.
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
var F = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var t = typeof(InventoryManager);
int per = inv.maxHor * inv.maxVer;
Transform page0 = inv.inventoryUI[0].transform;
System.Func<int, string> at = (s) => { ItemStack st = ViewScanner.Read(page0.GetChild(s)); return st.IsEmpty ? "-" : st.Id + "x" + st.Count; };
System.Func<int> empty = () => { for (int s = 0; s < page0.childCount; s++) if (page0.GetChild(s).childCount == 0) return s; return -1; };
System.Func<string, int> slotOf = (id) => { for (int s = 0; s < page0.childCount; s++) { ItemStack st = ViewScanner.Read(page0.GetChild(s)); if (st.Id == id) return s; } return -1; };
var change = t.GetMethod("ActiveChangeCursor", F);
System.Action<int, int> keyMove = (a, b) =>
{
    inv.state = ""; inv.nowBox = 0;
    inv.cusorCount[0] = a; change.Invoke(inv, null);
    inv.cusorCount[0] = b; change.Invoke(inv, null);
};
int wood0 = inv.CountItem("Wood"), pot0 = inv.CountItem("Potion");

// A) 키보드: 빈칸으로 이동
inv.CreatItem("Potion"); inv.CreatItem("Potion"); inv.CreatItem("Potion");
int p = slotOf("Potion"), e = empty();
keyMove(p, e);
sb.AppendLine("key move: [" + p + "]=" + at(p) + " [" + e + "]=" + at(e) + " state='" + inv.state + "'");

// B) 키보드: 다른 아이템과 교환
int w = slotOf("Wood");
if (w < 0) { inv.CreatItem("Wood"); w = slotOf("Wood"); }
p = slotOf("Potion");
string wb = at(w), pb = at(p);
keyMove(w, p);
sb.AppendLine("key swap: before [" + w + "]=" + wb + " [" + p + "]=" + pb + " after [" + w + "]=" + at(w) + " [" + p + "]=" + at(p));

// C) 키보드: 같은 스택 합치기 (Potion 3 → 나누어 2+1 → 합쳐 3)
p = slotOf("Potion");
ItemCheck pc = page0.GetChild(p).GetChild(0).GetComponent<ItemCheck>();
inv.Service.SplitAt(pc, 1, 0);
int p2 = -1; for (int s = 0; s < page0.childCount; s++) if (s != p && ViewScanner.Read(page0.GetChild(s)).Id == "Potion") p2 = s;
keyMove(p2, p);
int stacks = 0; for (int s = 0; s < page0.childCount; s++) if (ViewScanner.Read(page0.GetChild(s)).Id == "Potion") stacks++;
sb.AppendLine("key merge: [" + p + "]=" + at(p) + " [" + p2 + "]=" + at(p2) + " potionStacks=" + stacks + " total=" + inv.CountItem("Potion"));

// D) 드래그: 인벤 칸 → 인벤 칸(교환) + 저장 미룸/해제
var ev = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
var depthF = t.GetField("_saveBatchDepth", F);
GameObject dragCanvas = GameObject.FindWithTag("dragCanvas");
System.Action<ItemDrag> prep = (dr) => { if (dr.dragCanvus == null) dr.dragCanvus = dragCanvas; typeof(ItemDrag).GetField("itemCheck", F).SetValue(dr, dr.GetComponent<ItemCheck>()); };
sb.AppendLine("dragCanvas=" + (dragCanvas != null) + " invActive=" + inv.inventory.activeSelf);
w = slotOf("Wood"); p = slotOf("Potion");
ItemCheck wc = page0.GetChild(w).GetChild(0).GetComponent<ItemCheck>();
ItemDrag drag = wc.GetComponent<ItemDrag>(); prep(drag);
((UnityEngine.EventSystems.IBeginDragHandler)drag).OnBeginDrag(ev);
int depthDuring = (int)depthF.GetValue(inv);
inv.SaveInventory(); // 드래그 중 다른 경로의 저장 → 미뤄져야 함
SlotContainer savedDuring = SaveMapper.FromArray(SaveManager.instance.datas.invenItem, CsvItemCatalog.Shared, 5, per);
typeof(ItemDrag).GetField("changeParent", F).SetValue(drag, page0.GetChild(p));
((UnityEngine.EventSystems.IEndDragHandler)drag).OnEndDrag(ev);
int depthAfter = (int)depthF.GetValue(inv);
SlotContainer saved = SaveMapper.FromArray(SaveManager.instance.datas.invenItem, CsvItemCatalog.Shared, 5, per);
sb.AppendLine("drag swap: [" + w + "]=" + at(w) + " [" + p + "]=" + at(p) + " depth during=" + depthDuring + " after=" + depthAfter
    + " savedWoodDuring=" + savedDuring.Count("Wood") + " savedWood=" + saved.Count("Wood") + " ui=" + inv.CountItem("Wood"));

// E) 드래그 중 창고 저장 요청은 미뤄짐, 비활성(파괴 등)되면 배치만 풀림(원위치는 하지 않음 — 비활성 중 부모 변경 불가)
w = slotOf("Wood");
wc = page0.GetChild(w).GetChild(0).GetComponent<ItemCheck>();
drag = wc.GetComponent<ItemDrag>(); prep(drag);
((UnityEngine.EventSystems.IBeginDragHandler)drag).OnBeginDrag(ev);
bool deferred = inv.DeferChestSave();
typeof(ItemDrag).GetMethod("OnDisable", F).Invoke(drag, null);
int depthE = (int)depthF.GetValue(inv);
typeof(ItemDrag).GetMethod("ReturnToOrigin", F).Invoke(drag, null); // 프로브 정리용
sb.AppendLine("drag disable: chestDeferred=" + deferred + " depth=" + depthE + " back=" + (wc.transform.parent == page0.GetChild(w)) + " pagesAlive=" + InventoryManager.PagesAlive(inv.inventoryUI) + " nullPage=" + InventoryManager.PagesAlive(new GameObject[] { null }));

// 저장 = UI, 한 칸 두 개 없음
inv.SaveInventory();
SlotContainer fin = SaveMapper.FromArray(SaveManager.instance.datas.invenItem, CsvItemCatalog.Shared, 5, per);
SlotContainer ui = ViewScanner.BuildFrom(inv.inventoryUI, per, CsvItemCatalog.Shared);
int mm = 0, dbl = 0;
for (int i = 0; i < ui.Capacity; i++) { if (ui[i].Id != fin[i].Id || ui[i].Count != fin[i].Count) mm++; }
for (int s = 0; s < page0.childCount; s++) if (page0.GetChild(s).childCount > 1) dbl++;
sb.AppendLine("save/ui mismatch=" + mm + " doubleSlots=" + dbl + " wood " + wood0 + "->" + inv.CountItem("Wood"));

inv.DeletItemByName("Potion", inv.CountItem("Potion") - pot0);
if (inv.CountItem("Wood") > wood0) inv.DeletItemByName("Wood", inv.CountItem("Wood") - wood0);
return sb.ToString();
