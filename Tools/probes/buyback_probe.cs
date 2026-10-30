// 되사기 검증(BasicFeatures.md 12절, 플레이 중): StoneCap(+3) 지급 → 판매 경로(SellItemUI→SellItem) → 골드·목록 → 되사기 → 티어·강화 보존·골드 복구
var inv = InventoryManager.instance;
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
inv.CreatItem("StoneCap");
ItemCheck cap = null;
foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == "StoneCap") cap = c; }
inv.Service.SetUpgrade(cap, 3);
int m0 = DatabaseManager.money;
inv.detail = cap;
inv.SellItemUI();
typeof(InventoryManager).GetMethod("SellItem", BF).Invoke(inv, null);
int m1 = DatabaseManager.money;
string mid = "sold: money " + m0 + "->" + m1 + " caps=" + inv.CountItem("StoneCap") + " list=" + Buyback.Model.Count;
bool ok = Buyback.TryBuyBackLast();
ItemCheck back = null;
foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == "StoneCap") back = c; }
bool again = Buyback.TryBuyBackLast();
return mid + " | buyback=" + ok + " money=" + DatabaseManager.money + " caps=" + inv.CountItem("StoneCap") + " up=" + (back != null ? back.itemData.upgrade : -1) + " list=" + Buyback.Model.Count + " secondTry=" + again;
