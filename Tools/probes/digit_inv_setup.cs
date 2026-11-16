// 가방 열린 채 숫자 2: 퀵슬롯 사용 + 페이지 넘김 동시 여부(주말1 버그 3). 준비: 퀵슬롯1=Small HP Potion x10, 가방 열기, 페이지 0. 이어서 digit_inv_press → digit_inv_read
if (!UnityEditor.EditorApplication.isPlaying) { return "NOT_PLAYING"; }
var inv = InventoryManager.instance; var cat = CsvItemCatalog.Shared;
inv.maxBoxNum = 15; int per = inv.maxHor * inv.maxVer;
var bag = new SlotContainer(cat, 5 * per, 15);
bag.PutAt(0, new ItemStack("Small HP Potion", 10));
inv.Service.LoadFrom(SaveMapper.ToArray(bag, 5, per), ItemFlagSave.ToArray(bag, 5, per)); inv.SaveInventory();
var q = QuickSlotBar.Model; for (int i = 0; i < QuickSlotModel.SlotCount; i++) { q.Assign(i, null); }
q.Assign(0, "Small HP Potion"); q.Assign(1, "Small HP Potion");
if (!inv.inventory.activeSelf) { inv.OpenFromField(); }
inv.state = ""; inv.nowBox = 0;
return "open=" + inv.inventory.activeSelf + " box=" + inv.nowBox + " potions=" + inv.CountItem("Small HP Potion") + " isOpenUI=" + DatabaseManager.isOpenUI;
