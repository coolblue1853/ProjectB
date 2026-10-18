// 창고 저장(SaveMapper 경로) → 배열 재로드 비교
var ch = UnityEngine.Object.FindFirstObjectByType<Chest>(FindObjectsInactive.Include);
if (ch == null) return "no chest";
SlotContainer m = ViewScanner.BuildFrom(ch.inventoryUI, 30, CsvItemCatalog.Shared);
ch.SaveChest();
SlotContainer back = SaveMapper.FromArray(SaveManager.instance.datas.chestItem, CsvItemCatalog.Shared, 5, 30);
int rt = 0, items = 0;
for (int i = 0; i < m.Capacity; i++) { if (!m[i].IsEmpty) items++; if (m[i].Id != back[i].Id || m[i].Count != back[i].Count) rt++; }
return "chest items=" + items + " roundtrip mismatch=" + rt;
