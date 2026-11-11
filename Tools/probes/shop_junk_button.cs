if (!UnityEditor.EditorApplication.isPlaying) { return "NOT_PLAYING"; }
var inv = InventoryManager.instance;
var shop = UnityEngine.Object.FindFirstObjectByType<ShopManager>(UnityEngine.FindObjectsInactive.Include);
var cat = CsvItemCatalog.Shared;
var sb = new System.Text.StringBuilder();
System.Action<bool,string> check = (ok, label) => { if (!ok) { throw new System.Exception("FAIL: " + label + " :: " + sb.ToString()); } sb.AppendLine("PASS " + label); };
Time.timeScale = 1f;
inv.maxBoxNum = 15;
int per = inv.maxHor * inv.maxVer;
var bag = new SlotContainer(cat, 5 * per, 15);
bag.PutAt(0, new ItemStack("Small Bone", 3)); bag.PutAt(1, new ItemStack("Wood", 4));
inv.Service.LoadFrom(SaveMapper.ToArray(bag, 5, per), ItemFlagSave.ToArray(bag, 5, per)); DatabaseManager.money = 1000; inv.SaveInventory();
if (shop.ShopUI.activeSelf) { shop.CloseShop(); }
inv.inventory.SetActive(false); UiLock.Set("InvGroup", false);
shop.OpenShop(); inv.state = "ShopCusorOn";
var conv = UnityEngine.Object.FindFirstObjectByType<InventoryConvenience>();
var plan = BulkSaleRules.Build(inv.Service.ScanModel(), cat);
check(plan.Count >= 1, "junk plan count>=1 (" + plan.Count + ")");
bool opened = conv.RequestBulkSaleConfirm(inv);
check(opened && ConfirmPopup.IsOpen, "confirm popup opened");
check(DatabaseManager.money == 1000, "no sale before confirm");
ConfirmPopup.Choose(false);
check(DatabaseManager.money == 1000 && inv.CountItem("Small Bone") == 3, "cancel keeps items");
conv.RequestBulkSaleConfirm(inv);
ConfirmPopup.Choose(true);
check(DatabaseManager.money == 1000 + plan.Price && inv.CountItem("Small Bone") == 0 && inv.CountItem("Wood") == 4, "yes sells junk only +" + plan.Price);
return sb.ToString();
