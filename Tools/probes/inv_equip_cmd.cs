// 3c-1 검증(플레이 모드): 장비칸 ↔ 인벤이 모델 명령(TakeOut/PutIn)으로 처리되는지.
// 키보드 장착(빈 칸)·드래그 장착 교체·장비칸 → 빈 인벤 칸 드래그 해제·무기 키보드 교체 뒤 OutsideChanges 증가 0, 세이브(equipGear·invenItem)=화면.
// 세이브는 프로브 밖에서 백업·복원한다.
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
var F = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
int per = inv.maxHor * inv.maxVer;
Transform page0 = inv.inventoryUI[0].transform;
var use = typeof(InventoryManager).GetMethod("UseEquipment", F);
GameObject dragCanvas = GameObject.FindWithTag("dragCanvas");
System.Action<ItemDrag> prep = (dr) => { if (dr.dragCanvus == null) dr.dragCanvus = dragCanvas; typeof(ItemDrag).GetField("itemCheck", F).SetValue(dr, dr.GetComponent<ItemCheck>()); };
var ev = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
System.Func<string, ItemCheck> findInv = (id) => { foreach (var c in inv.EnumerateInventoryItems()) if (c.name == id) return c; return null; };
System.Func<int> emptySlot = () => { for (int s = 0; s < page0.childCount; s++) if (page0.GetChild(s).childCount == 0) return s; return -1; };
Transform head = inv.equipBox[2].transform;
inv.Service.SyncModelFromViews();
int o0 = inv.Service.OutsideChanges;
sb.AppendLine("head before: children=" + head.childCount);

// 1) 키보드 장착(빈 장비칸 가정 — 차 있으면 교체 경로)
inv.CreatItem("Cap");
ItemCheck capA = findInv("Cap");
int slotA = inv.Service.IndexOf(capA);
capA.itemData.upgrade = 4; inv.Service.SyncModelFromViews(); int oBase = inv.Service.OutsideChanges; o0 = oBase;
use.Invoke(inv, new object[] { capA });
inv.Service.SyncModelFromViews();
sb.AppendLine("key equip: inHead=" + (capA.transform.parent == head) + " slotEmpty=" + (page0.GetChild(slotA).childCount == 0) + " equipItemCheck=" + (inv.equipItemCheck[2] == capA) + " gear=" + SaveManager.instance.datas.equipGear[2, 0] + "/" + SaveManager.instance.datas.equipGear[2, 2] + " outside+" + (inv.Service.OutsideChanges - o0));

// 2) 드래그 장착 교체: capB 를 머리칸으로 → capA 가 capB 자리로
inv.CreatItem("Cap");
ItemCheck capB = findInv("Cap");
int slotB = inv.Service.IndexOf(capB);
ItemDrag d = capB.GetComponent<ItemDrag>(); prep(d);
((UnityEngine.EventSystems.IBeginDragHandler)d).OnBeginDrag(ev);
d.equipBox = head;
((UnityEngine.EventSystems.IEndDragHandler)d).OnEndDrag(ev);
inv.Service.SyncModelFromViews();
sb.AppendLine("drag swap: Bhead=" + (capB.transform.parent == head) + " AinSlotB=" + (capA.transform.parent == page0.GetChild(slotB)) + " A.upg=" + capA.itemData.upgrade + " equipItemCheck=B:" + (inv.equipItemCheck[2] == capB) + " outside+" + (inv.Service.OutsideChanges - o0));

// 3) 장비칸 → 빈 인벤 칸 드래그 해제
int e = emptySlot();
d = capB.GetComponent<ItemDrag>(); prep(d);
((UnityEngine.EventSystems.IBeginDragHandler)d).OnBeginDrag(ev);
typeof(ItemDrag).GetField("changeParent", F).SetValue(d, page0.GetChild(e));
((UnityEngine.EventSystems.IEndDragHandler)d).OnEndDrag(ev);
inv.Service.SyncModelFromViews();
sb.AppendLine("drag unequip: BinSlot=" + (capB.transform.parent == page0.GetChild(e)) + " headChildren=" + head.childCount + " gear='" + SaveManager.instance.datas.equipGear[2, 0] + "' equipItemCheck null=" + (inv.equipItemCheck[2] == null) + " outside+" + (inv.Service.OutsideChanges - o0));

// 4) 무기 키보드 교체: PickAxe 장착 → 원래 무기가 그 칸으로 → 원래 무기 다시 장착
ItemCheck oldW = inv.equipItemCheck[7];
inv.CreatItem("PickAxe");
ItemCheck pick = findInv("PickAxe");
int slotP = inv.Service.IndexOf(pick);
use.Invoke(inv, new object[] { pick });
bool oldBack = oldW != null && oldW.transform.parent == page0.GetChild(slotP);
use.Invoke(inv, new object[] { oldW });
inv.Service.SyncModelFromViews();
sb.AppendLine("weapon swap: oldInSlot=" + oldBack + " restored=" + (inv.equipItemCheck[7] == oldW) + " pickBack=" + (pick.transform.parent == page0.GetChild(slotP)) + " gear=" + SaveManager.instance.datas.equipGear[7, 0] + " outside+" + (inv.Service.OutsideChanges - o0));

// 저장 = 화면
inv.SaveInventory();
SlotContainer saved = SaveMapper.FromArray(SaveManager.instance.datas.invenItem, CsvItemCatalog.Shared, 5, per);
SlotContainer ui = ViewScanner.BuildFrom(inv.inventoryUI, per, CsvItemCatalog.Shared);
int mm = 0; for (int i = 0; i < ui.Capacity; i++) if (ui[i].Id != saved[i].Id || ui[i].Count != saved[i].Count || ui[i].Upgrade != saved[i].Upgrade) mm++;
sb.AppendLine("save/ui mismatch=" + mm + " Cap=" + inv.CountItem("Cap") + " PickAxe=" + inv.CountItem("PickAxe"));

inv.DeletItemByName("Cap", inv.CountItem("Cap"));
inv.DeletItemByName("PickAxe", inv.CountItem("PickAxe"));
return sb.ToString();
