// 3c-2 검증(플레이 모드): 강화(EnhanceService.TryEnhance)가 모델 명령(SetUpgrade→ReplaceAt)으로 처리되는지.
// 같은 ItemCheck 유지, 모델·세이브 강화값 = 화면, OutsideChanges +0. 세이브는 프로브 밖에서 백업·복원.
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
int per = inv.maxHor * inv.maxVer;
int money0 = DatabaseManager.money;
int stones0 = inv.CountItem(EnhanceRules.StoneName);
DatabaseManager.money = 999999;
inv.BeginSaveBatch();
for (int i = 0; i < 10; i++) inv.CreatItem(EnhanceRules.StoneName);
inv.EndSaveBatch();
inv.CreatItem("Cap");
ItemCheck cap = null; foreach (var c in inv.EnumerateInventoryItems()) if (c.name == "Cap") cap = c;
int idx = inv.Service.IndexOf(cap);
inv.Service.SyncModelFromViews();
int o0 = inv.Service.OutsideChanges;
string log = "";
for (int t = 0; t < 3; t++)
{
    EnhanceRules.Result r; int lv;
    bool ok = EnhanceService.TryEnhance(cap, out r, out lv);
    log += " try" + t + "=" + ok + "/" + r + "/+" + lv;
}
SlotContainer m = inv.Service.ScanModel();
SlotContainer saved = SaveMapper.FromArray(SaveManager.instance.datas.invenItem, CsvItemCatalog.Shared, 5, per);
bool same = inv.Service.IndexOf(cap) == idx;
sb.AppendLine("enhance:" + log);
sb.AppendLine("sameObject=" + same + " view=+" + cap.itemData.upgrade + " model=+" + m[idx].Upgrade + " saved=+" + saved[idx].Upgrade + " outside+" + (inv.Service.OutsideChanges - o0));

inv.DeletItemByName("Cap", 1);
int extra = inv.CountItem(EnhanceRules.StoneName) - stones0;
if (extra > 0) inv.DeletItemByName(EnhanceRules.StoneName, extra);
DatabaseManager.money = money0;
inv.SaveInventory();
return sb.ToString();
