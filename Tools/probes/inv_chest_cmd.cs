// 3c-3 검증(플레이 모드): 창고↔인벤에서 인벤 쪽이 모델 명령으로 처리되는지. I2CMove(키보드 이동)·드래그 창고→인벤 빈칸·인벤→창고 빈칸·
// 창고→인벤 합치기·인벤→창고 교환 뒤 OutsideChanges +0, 총량 보존, 저장(인벤·창고)=화면. 세이브는 프로브 밖에서 백업·복원.
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
var F = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var ch = UnityEngine.Object.FindFirstObjectByType<Chest>(FindObjectsInactive.Include);
if (ch == null) return "no chest";
inv.CheckNowChest(ch);
int per = inv.maxHor * inv.maxVer;
Transform page0 = inv.inventoryUI[0].transform;
Transform cpage = ch.inventoryUI[ch.nowBox].transform;
sb.AppendLine("chest page boxes=" + cpage.childCount + " nowBox=" + ch.nowBox);
GameObject dragCanvas = GameObject.FindWithTag("dragCanvas");
System.Action<ItemDrag> prep = (dr) => { if (dr.dragCanvus == null) dr.dragCanvus = dragCanvas; typeof(ItemDrag).GetField("itemCheck", F).SetValue(dr, dr.GetComponent<ItemCheck>()); };
var ev = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
System.Action<ItemCheck, Transform> drop = (it, to) =>
{
    ItemDrag d = it.GetComponent<ItemDrag>(); prep(d);
    ((UnityEngine.EventSystems.IBeginDragHandler)d).OnBeginDrag(ev);
    typeof(ItemDrag).GetField("changeParent", F).SetValue(d, to);
    ((UnityEngine.EventSystems.IEndDragHandler)d).OnEndDrag(ev);
};
System.Func<Transform, int> emptyIn = (pg) => { for (int s = 0; s < pg.childCount; s++) if (pg.GetChild(s).childCount == 0) return s; return -1; };
System.Func<Transform, string, ItemCheck> findIn = (pg, id) => { for (int s = 0; s < pg.childCount; s++) if (pg.GetChild(s).childCount > 0) { var c = pg.GetChild(s).GetChild(0).GetComponent<ItemCheck>(); if (c.name == id) return c; } return null; };
System.Func<Transform, string, int> countIn = (pg, id) => { int n = 0; for (int s = 0; s < pg.childCount; s++) if (pg.GetChild(s).childCount > 0) { var c = pg.GetChild(s).GetChild(0).GetComponent<ItemCheck>(); if (c.name == id) n += c.nowStack; } return n; };
inv.Service.SyncModelFromViews();
int o0 = inv.Service.OutsideChanges;

// 1) I2CMove: 인벤 Potion x3 → 창고
for (int i = 0; i < 3; i++) inv.CreatItem("Potion");
int invP0 = inv.CountItem("Potion"), chP0 = countIn(cpage, "Potion");
ItemCheck pot = findIn(page0, "Potion");
int pi = inv.Service.IndexOf(pot);
inv.nowBox = 0; inv.cusorCount[0] = pi % per;
typeof(InventoryManager).GetMethod("I2CMove", F).Invoke(inv, null);
inv.Service.SyncModelFromViews();
sb.AppendLine("I2C: inv " + invP0 + "->" + inv.CountItem("Potion") + " chest " + chP0 + "->" + countIn(cpage, "Potion") + " outside+" + (inv.Service.OutsideChanges - o0));

// 2) 드래그 창고 → 인벤 빈칸
ItemCheck cp = findIn(cpage, "Potion");
int ie = emptyIn(page0);
drop(cp, page0.GetChild(ie));
inv.Service.SyncModelFromViews();
sb.AppendLine("C2I drag empty: inSlot=" + (cp != null && cp.transform.parent == page0.GetChild(ie)) + " inv=" + inv.CountItem("Potion") + " chest=" + countIn(cpage, "Potion") + " outside+" + (inv.Service.OutsideChanges - o0));

// 3) 드래그 인벤 → 창고 빈칸
int ce = emptyIn(cpage);
drop(cp, cpage.GetChild(ce));
inv.Service.SyncModelFromViews();
sb.AppendLine("I2C drag empty: inChest=" + (cp.transform.parent == cpage.GetChild(ce)) + " inv=" + inv.CountItem("Potion") + " chest=" + countIn(cpage, "Potion") + " outside+" + (inv.Service.OutsideChanges - o0));

// 4) 창고 Potion → 인벤 Potion 합치기(인벤에 1개 만들어 두고)
inv.CreatItem("Potion");
ItemCheck ip = findIn(page0, "Potion");
int tot = inv.CountItem("Potion") + countIn(cpage, "Potion");
drop(cp, ip.transform.parent);
inv.Service.SyncModelFromViews();
sb.AppendLine("C2I merge: invSlot=" + ip.nowStack + " inv=" + inv.CountItem("Potion") + " chest=" + countIn(cpage, "Potion") + " total " + tot + "->" + (inv.CountItem("Potion") + countIn(cpage, "Potion")) + " outside+" + (inv.Service.OutsideChanges - o0));

// 5) 인벤 Potion ↔ 창고 다른 아이템 교환 (창고 빈칸에 Wood 를 I2C 로 하나 넣고)
inv.CreatItem("Cap");
ItemCheck cap = findIn(page0, "Cap");
int capIdx = inv.Service.IndexOf(cap);
inv.cusorCount[0] = capIdx % per;
typeof(InventoryManager).GetMethod("I2CMove", F).Invoke(inv, null);
ItemCheck ccap = findIn(cpage, "Cap");
ip = findIn(page0, "Potion");
Transform ipBox = ip.transform.parent, capBox = ccap.transform.parent;
drop(ip, capBox);
inv.Service.SyncModelFromViews();
ItemCheck nowInv = ipBox.childCount > 0 ? ipBox.GetChild(0).GetComponent<ItemCheck>() : null;
sb.AppendLine("I2C swap: invSlotNow=" + (nowInv != null ? nowInv.name : "-") + " chestSlotNow=" + (capBox.childCount > 0 ? capBox.GetChild(0).name : "-") + " childCounts=" + ipBox.childCount + "/" + capBox.childCount + " outside+" + (inv.Service.OutsideChanges - o0));

// 저장 = 화면
inv.SaveInventoryAndChest();
SlotContainer si = SaveMapper.FromArray(SaveManager.instance.datas.invenItem, CsvItemCatalog.Shared, 5, per);
SlotContainer ui = ViewScanner.BuildFrom(inv.inventoryUI, per, CsvItemCatalog.Shared);
SlotContainer sc = SaveMapper.FromArray(SaveManager.instance.datas.chestItem, CsvItemCatalog.Shared, 5, 30);
SlotContainer uc = ViewScanner.BuildFrom(ch.inventoryUI, 30, CsvItemCatalog.Shared);
int mi = 0, mc = 0;
for (int i = 0; i < ui.Capacity; i++) if (ui[i].Id != si[i].Id || ui[i].Count != si[i].Count) mi++;
for (int i = 0; i < uc.Capacity; i++) if (uc[i].Id != sc[i].Id || uc[i].Count != sc[i].Count) mc++;
sb.AppendLine("save mismatch inv=" + mi + " chest=" + mc);
inv.chest = null; inv.state = "";
return sb.ToString();
