var inv = InventoryManager.instance;
var conv = UnityEngine.Object.FindFirstObjectByType<InventoryConvenience>();
var plan = BulkSaleRules.Build(inv.Service.ScanModel(), CsvItemCatalog.Shared);
int before = DatabaseManager.money;
bool opened = conv.RequestBulkSaleConfirm(inv);
bool open = ConfirmPopup.IsOpen;
ConfirmPopup.Choose(true);
return "opened=" + opened + " popup=" + open + " money " + before + "->" + DatabaseManager.money + " bone=" + inv.CountItem("Small Bone") + " wood=" + inv.CountItem("Wood") + " planPrice=" + plan.Price;
