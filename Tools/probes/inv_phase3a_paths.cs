// Phase 3a 남은 경로 검증(플레이 모드): 소비(ActiveConsum)·판매(SellItem)·나누기(SplitAt)·페이지 놓기(CreatItemSelected)·페이지 이동(MoveToPage)
// 모두 InventoryService 를 거치는지, 총량·티어·강화가 보존되는지, UI ↔ 모델 불일치 0 인지 본다. 세이브는 프로브 밖에서 백업·복원한다.
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
var t = typeof(InventoryManager);
int per = inv.maxHor * inv.maxVer;
sb.Append("per=" + per + " maxBoxNum=" + inv.maxBoxNum + " pages=");
for (int p = 0; p < inv.inventoryUI.Length; p++) sb.Append(inv.inventoryUI[p].transform.childCount + ",");
sb.AppendLine();
int potion0 = inv.CountItem("Potion");
int money0 = DatabaseManager.money;
System.Func<string, System.Collections.Generic.List<ItemCheck>> find = (id) =>
{
    var list = new System.Collections.Generic.List<ItemCheck>();
    foreach (var c in inv.EnumerateInventoryItems()) if (c.name == id) list.Add(c);
    return list;
};

// 1) 소비: 3개 만들고 커서를 그 칸에 두고 ActiveConsum
for (int i = 0; i < 3; i++) inv.CreatItem("Potion");
ItemCheck pot = find("Potion")[find("Potion").Count - 1];
int idx = inv.Service.IndexOf(pot);
inv.nowBox = idx / per; inv.cusorCount[inv.nowBox] = idx % per;
int beforeUse = inv.CountItem("Potion");
inv.ActiveConsum();
sb.AppendLine("consume: " + beforeUse + " -> " + inv.CountItem("Potion") + " (expect -1)");

// 2) 나누기: 같은 칸에서 1개 떼기 → 칸 2개, 총량 보존
pot = inv.inventoryUI[idx / per].transform.GetChild(idx % per).GetChild(0).GetComponent<ItemCheck>();
int stacksBefore = find("Potion").Count, totalBefore = inv.CountItem("Potion");
bool split = inv.Service.SplitAt(pot, 1, idx / per);
sb.AppendLine("split: ok=" + split + " stacks " + stacksBefore + "->" + find("Potion").Count + " total " + totalBefore + "->" + inv.CountItem("Potion") + " src=" + pot.nowStack);
bool splitBad = inv.Service.SplitAt(pot, pot.nowStack, idx / per);
sb.AppendLine("split all(=count) rejected=" + !splitBad);

// 3) 페이지 놓기: 티어·강화가 있는 데이터로 2개 놓기
ItemData d = pot.itemData.Clone(); d.tear = 2; d.upgrade = 3;
bool put = inv.CreatItemSelected("Potion", 0, 2, d);
ItemCheck placed = null;
foreach (var c in find("Potion")) if (c.itemData.tear == 2) placed = c;
sb.AppendLine("put: ok=" + put + " placed=" + (placed != null ? placed.nowStack + " t" + placed.itemData.tear + " u" + placed.itemData.upgrade + " sharedRef=" + object.ReferenceEquals(placed.itemData, d) : "null"));

// 4) 판매: placed 1개 판매 → 돈 +price, 수량 -1. 수량보다 많이 팔면 거절·돈 불변
t.GetField("detail", flags).SetValue(inv, placed);
inv.sellSlider.maxValue = 99;
inv.sellSlider.value = 1;
t.GetField("totalSellPrice", flags).SetValue(inv, 123);
int m1 = DatabaseManager.money, c1 = inv.CountItem("Potion");
t.GetMethod("SellItem", flags).Invoke(inv, null);
sb.AppendLine("sell1: money +" + (DatabaseManager.money - m1) + " count " + c1 + "->" + inv.CountItem("Potion"));
inv.sellSlider.value = 50;
int m2 = DatabaseManager.money, c2 = inv.CountItem("Potion");
t.GetMethod("SellItem", flags).Invoke(inv, null);
sb.AppendLine("sell over: money +" + (DatabaseManager.money - m2) + " count " + c2 + "->" + inv.CountItem("Potion") + " (expect +0, same)");

// 5) 페이지 이동(열린 칸이 두 페이지 이상일 때만)
if (inv.maxBoxNum > per)
{
    ItemCheck mv = find("Potion")[0];
    int from = inv.Service.IndexOf(mv);
    bool moved = inv.Service.MoveToPage(mv, 1);
    int to = -1;
    foreach (var c in find("Potion")) { int k = inv.Service.IndexOf(c); if (k >= per) to = k; }
    sb.AppendLine("moveToPage: ok=" + moved + " from=" + from + " to=" + to);
}
else
{
    sb.AppendLine("moveToPage: skip (열린 칸 " + inv.maxBoxNum + " <= " + per + ")");
    ItemCheck mv = find("Potion")[0];
    sb.AppendLine("moveToPage same page rejected=" + !inv.Service.MoveToPage(mv, inv.Service.IndexOf(mv) / per));
}

// 5b) 시트에 없는 이름은 거절(원본 삭제 → 소실 방지), source 를 주면 OnItemAdded 발행
bool unknown = inv.CreatItemSelected("__NoSuchItem__", 0, 1, null);
int evt = 0; string evtSrc = "";
System.Action<ItemStack, AddSource> h = (st, src) => { evt++; evtSrc = src.ToString(); };
InventoryService.OnItemAdded += h;
bool viaChest = inv.CreatItemSelected("Potion", 0, 1, null, AddSource.Chest);
bool noSrc = inv.CreatItemSelected("Potion", 0, 1, null);
InventoryService.OnItemAdded -= h;
sb.AppendLine("unknown rejected=" + !unknown + " chestPut=" + viaChest + " noSrcPut=" + noSrc + " events=" + evt + " src=" + evtSrc + " (expect 1, Chest)");
// 5c) 인벤+창고 한 번에 저장(창고 닫혀 있으면 인벤만)
inv.SaveInventoryAndChest();
SlotContainer saved = SaveMapper.FromArray(SaveManager.instance.datas.invenItem, CsvItemCatalog.Shared, 5, per);
sb.AppendLine("saveBoth: saved Potion=" + saved.Count("Potion") + " ui=" + inv.CountItem("Potion") + " chestOpen=" + (inv.chest != null));

// 6) UI ↔ 모델 불일치
SlotContainer m = ViewScanner.BuildFrom(inv.inventoryUI, per, CsvItemCatalog.Shared);
int mismatch = 0;
for (int p = 0; p < inv.inventoryUI.Length; p++)
{
    Transform page = inv.inventoryUI[p].transform;
    for (int s = 0; s < page.childCount && s < per; s++)
    {
        ItemStack ui = ViewScanner.Read(page.GetChild(s));
        int arr = (s < inv.inventoryArray.GetLength(0) && p < inv.inventoryArray.GetLength(1)) ? inv.inventoryArray[s, p] : (ui.IsEmpty ? 0 : 1);
        if ((arr == 1) != !ui.IsEmpty) mismatch++;
        if (page.GetChild(s).childCount > 1) mismatch++;
    }
}
sb.AppendLine("arr/view mismatch=" + mismatch);

// 정리
int extra = inv.CountItem("Potion") - potion0;
inv.DeletItemByName("Potion", extra);
DatabaseManager.money = money0;
sb.AppendLine("cleanup potion=" + inv.CountItem("Potion") + " (orig " + potion0 + ") money=" + DatabaseManager.money);
return sb.ToString();
