// Phase 2 브리지 검증: UI 슬롯 ↔ ViewScanner 모델 칸 단위 비교 + 저장 → SaveMapper 로드 왕복 비교 (플레이 모드)
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
int per = inv.maxHor * inv.maxVer;
inv.CreatItem("Wood"); inv.CreatItem("Wood"); inv.CreatItem("Cap");
SlotContainer m = ViewScanner.BuildFrom(inv.inventoryUI, per, CsvItemCatalog.Shared);
int mismatch = 0, items = 0;
for (int p = 0; p < 5; p++)
{
    Transform page = inv.inventoryUI[p].transform;
    for (int s = 0; s < page.childCount && s < per; s++)
    {
        ItemStack ui = ViewScanner.Read(page.GetChild(s));
        ItemStack md = m[p * per + s];
        if (!ui.IsEmpty) items++;
        if (ui.Id != md.Id || ui.Count != md.Count || ui.Tier != md.Tier || ui.Upgrade != md.Upgrade) mismatch++;
    }
}
sb.AppendLine("scan: items=" + items + " mismatch=" + mismatch + " Wood model=" + m.Count("Wood") + " ui=" + inv.CountItem("Wood"));
inv.SaveInventory();
SlotContainer back = SaveMapper.FromArray(SaveManager.instance.datas.invenItem, CsvItemCatalog.Shared, 5, per);
int rt = 0;
for (int i = 0; i < m.Capacity; i++)
{
    ItemStack a = m[i], b = back[i];
    if (a.Id != b.Id || a.Count != b.Count || a.Tier != b.Tier || a.Upgrade != b.Upgrade) rt++;
}
sb.AppendLine("save roundtrip mismatch=" + rt);
inv.DeletItemByName("Wood", 2); inv.DeletItemByName("Cap", 1); inv.SaveInventory();
return sb.ToString();
