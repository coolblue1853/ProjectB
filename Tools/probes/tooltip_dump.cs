// 장비 툴팁(D3) 덤프: Cap 장착 → CliffCap 상세(착용품 비교·세트 진행도). 세이브를 바꾸므로 테스트 뒤 백업 복원
var inv = InventoryManager.instance;
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
inv.DeletItemByName("Cap", System.Math.Max(0, inv.CountItem("Cap") - 1)); inv.CreatItem("Cap"); inv.CreatItem("CliffCap");
UiLock.Set("InvGroup", true); inv.inventory.SetActive(true); inv.cusor.SetActive(true); inv.nowBox = 0;
System.Func<string, ItemCheck> find = id => {
    for (int pg = 0; pg < inv.inventoryUI.Length; pg++) foreach (Transform box in inv.inventoryUI[pg].transform) if (box.childCount > 0) { var c = box.GetChild(0).GetComponent<ItemCheck>(); if (c != null && c.itemData.name == id) { inv.nowBox = pg; return c; } }
    return null; };
var cap = find("Cap");
inv.state = ""; inv.cusorCount[inv.nowBox] = cap.transform.parent.GetSiblingIndex();
inv.BoxContentChecker(cap.gameObject);
inv.GetType().GetMethod("UseEquipment", flags).Invoke(inv, new object[] { null });
inv.GetType().GetMethod("CloseCheck", flags).Invoke(inv, null);
var cliff = find("CliffCap");
inv.state = ""; inv.cusorCount[inv.nowBox] = cliff.transform.parent.GetSiblingIndex();
inv.BoxContentChecker(cliff.gameObject);
var e = inv.equipDetail.transform;
var sb = new System.Text.StringBuilder("state=" + inv.state + "\n");
foreach (int i in new[] { 1, 6, 7, 8 }) sb.Append("[" + i + "] " + e.GetChild(i).GetComponent<TMPro.TextMeshProUGUI>().text + "\n");
return sb.ToString();
