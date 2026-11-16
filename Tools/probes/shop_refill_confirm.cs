// 상점 H 충전 확인창(주말1 버그 4): 물약만 기본 대상, 응급식량·귀환 주문서 제외, 확인창 예/아니오, 총액 일치. 플레이 중 실행
if (!UnityEditor.EditorApplication.isPlaying) { return "NOT_PLAYING"; }
var inv = InventoryManager.instance;
var shop = UnityEngine.Object.FindFirstObjectByType<ShopManager>(UnityEngine.FindObjectsInactive.Include);
var cat = CsvItemCatalog.Shared;
var sb = new System.Text.StringBuilder(); int checks = 0;
System.Action<bool,string> check = (ok, label) => { if (!ok) { throw new System.Exception("FAIL: " + label + " :: " + sb.ToString()); } checks++; sb.AppendLine("PASS " + label); };
TimeChange.instance.LockDaytime = true; Time.timeScale = 1f;
inv.maxBoxNum = 15;
int per = inv.maxHor * inv.maxVer;
System.Action<SlotContainer> loadBag = (b) => { inv.Service.LoadFrom(SaveMapper.ToArray(b, 5, per), ItemFlagSave.ToArray(b, 5, per)); };
var bag = new SlotContainer(cat, 5 * per, 15);
bag.PutAt(0, new ItemStack("Small HP Potion", 10)); bag.PutAt(1, new ItemStack("Recall Scroll", 1)); bag.PutAt(2, new ItemStack("Emergency Rations", 1));
loadBag(bag); DatabaseManager.money = 100000; inv.SaveInventory();
if (shop.ShopUI.activeSelf) { shop.CloseShop(); }
inv.inventory.SetActive(false); UiLock.Set("InvGroup", false);
shop.OpenShop(); inv.state = "ShopCusorOn";
var q = QuickSlotBar.Model; check(q != null, "quick model");
for (int i = 0; i < QuickSlotModel.SlotCount; i++) { q.Assign(i, null); }
q.Assign(0, "Small HP Potion"); q.Assign(1, "Recall Scroll"); q.Assign(2, "Emergency Rations");
sb.AppendLine("sells recall=" + ShopStockExtender.Sells("Recall Scroll") + " rations=" + ShopStockExtender.Sells("Emergency Rations"));
var plan = ShopConvenience.BuildRefillPlan();
sb.AppendLine("lines=" + plan.Lines.Count + " skipped=" + plan.SkippedOther + " total=" + plan.TotalGold);
check(plan.Lines.Count == 1 && plan.Lines[0].Id == "Small HP Potion", "plan = potion only");
check(plan.TotalGold == plan.Lines[0].Amount * 25 && plan.Lines[0].Amount >= 89, "amount x 25G " + plan.Lines[0].Amount);
long g0 = DatabaseManager.money;
// H 눌렀을 때: 확인창이 뜨고 아직 아무것도 사지 않는다
ShopConvenience.RequestRefill();
check(ConfirmPopup.IsOpen, "confirm opened");
check(DatabaseManager.money == g0 && inv.CountItem("Small HP Potion") == 10, "nothing bought before confirm");
ConfirmPopup.Choose(false);
check(!ConfirmPopup.IsOpen && DatabaseManager.money == g0 && inv.CountItem("Small HP Potion") == 10, "No = nothing bought");
ShopConvenience.RequestRefill();
ConfirmPopup.Choose(true);
check(inv.CountItem("Small HP Potion") == 10 + plan.Lines[0].Amount, "Yes = potion refilled to max stack");
check(inv.CountItem("Recall Scroll") == 1 && inv.CountItem("Emergency Rations") == 1, "scroll and rations untouched");
check(g0 - DatabaseManager.money == plan.TotalGold, "spent exactly plan total " + plan.TotalGold);
ShopConvenience.RequestRefill();
check(!ConfirmPopup.IsOpen, "full -> no confirm, warn only");
for (int i = 0; i < QuickSlotModel.SlotCount; i++) { q.Assign(i, null); }
sb.AppendLine("checks=" + checks);
return sb.ToString();
