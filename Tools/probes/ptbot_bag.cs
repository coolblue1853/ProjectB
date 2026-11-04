// PT-BOT 가방 점검(INV-EXP-3): 구역별 슬롯 점유율 로그 + 가득 차면 잡템 일괄 판매 → 그래도 가득이면 가방 확장 구매.
// 사용: bash Tools/u.sh cs Tools/probes/ptbot_bag.cs  (플레이 중, 폴링 루프에서 N초마다 호출). 로그: Logs/ptbot/bag.log
// 한 줄: 시각|맵|사용/해금|점유율|이벤트(full·sell·expand)  — 가득 참 횟수/판매/확장은 보고용으로 grep 집계.
var inv = InventoryManager.instance;
if (inv == null) { return "no inventory"; }
var model = inv.Service.ScanModel();
int unlocked = model.Unlocked;
int used = 0;
for (int i = 0; i < unlocked; i++) { if (!model[i].IsEmpty) { used++; } }
float occ = unlocked > 0 ? (float)used / unlocked : 1f;
string map = ZoneProgress.CurrentMap ?? "?";
string ev = "";
if (used >= unlocked - 1)
{
    ev += "full ";
    var plan = BulkSaleRules.Build(model, CsvItemCatalog.Shared);
    if (plan.Count > 0)
    {
        int ps = inv.maxHor * inv.maxVer;
        inv.BeginSaveBatch();
        try
        {
            foreach (int index in plan.Indices)
            {
                var box = inv.inventoryUI[index / ps].transform.GetChild(index % ps);
                var item = box.GetChild(0).GetComponent<ItemCheck>();
                int count = item.nowStack;
                int price = item.itemData.price * count;
                if (!item.itemData.isLocked && inv.Service.RemoveAt(item, count)) { DatabaseManager.money += price; }
            }
            inv.SaveInventory();
        }
        finally { inv.EndSaveBatch(); }
        ev += "sell(" + plan.Count + "개," + plan.Price + "G) ";
    }
    model = inv.Service.ScanModel();
    used = 0;
    for (int i = 0; i < model.Unlocked; i++) { if (!model[i].IsEmpty) { used++; } }
    if (used >= model.Unlocked - 1)
    {
        int before = BagExpansion.Unlocked;
        if (BagExpansion.TryBuy()) { ev += "expand(" + before + ">" + BagExpansion.Unlocked + ") "; }
        else { ev += "expand-blocked "; }
    }
}
string line = System.DateTime.Now.ToString("HH:mm:ss") + "|" + map + "|" + used + "/" + inv.Service.ScanModel().Unlocked + "|" + (int)(occ * 100) + "%|" + ev.Trim();
System.IO.Directory.CreateDirectory("Logs/ptbot");
System.IO.File.AppendAllText("Logs/ptbot/bag.log", line + "\n");
return line;
