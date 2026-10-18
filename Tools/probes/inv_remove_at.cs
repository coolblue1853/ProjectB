// 판매·소비 경로(Service.RemoveAt): 칸에서 n개 차감, 0이 되면 칸이 비고 inventoryArray 도 0 으로
var inv = InventoryManager.instance;
int before = inv.CountItem("Potion");
inv.CreatItem("Potion"); inv.CreatItem("Potion");
ItemCheck target = null;
foreach (var c in inv.EnumerateInventoryItems()) if (c.name == "Potion") target = c;
int idx = inv.Service.IndexOf(target);
int per = inv.maxHor * inv.maxVer;
bool a = inv.Service.RemoveAt(target, 1);
int mid = inv.CountItem("Potion");
ItemCheck again = null;
foreach (var c in inv.EnumerateInventoryItems()) if (c.name == "Potion") again = c;
int remain = inv.CountItem("Potion");
bool b = again != null && inv.Service.RemoveAt(again, remain);
int slotArr = idx >= 0 ? inv.inventoryArray[idx % per, idx / per] : -9;
string r = "before=" + before + " idx=" + idx + " remove1=" + a + " mid=" + mid + " removeAll=" + b + " after=" + inv.CountItem("Potion") + " arr=" + slotArr;
for (int i = 0; i < before; i++) inv.CreatItem("Potion");
inv.SaveInventory();
return r + " restored=" + inv.CountItem("Potion");
