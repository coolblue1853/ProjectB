// 열린 칸 밖(슬롯 박스 없음) 세이브 아이템이 저장 후에도 남는지. 빈 페이지 끝 칸에 넣고 로드→저장→확인→정리
var inv = InventoryManager.instance;
int per = inv.maxHor * inv.maxVer;
int page = -1;
for (int p = inv.inventoryUI.Length - 1; p >= 0; p--) if (inv.inventoryUI[p].transform.childCount < per) { page = p; break; }
if (page < 0) return "no locked slot (all pages full of boxes)";
int slot = per - 1;
var arr = SaveManager.instance.datas.invenItem;
SaveMapper.WriteCell(arr, page, slot, new ItemStack("Wood", 3));
inv.Service.LoadFrom(arr);
inv.SaveInventory();
ItemStack kept = SaveMapper.ReadCell(SaveManager.instance.datas.invenItem, page, slot);
// 정리
var arr2 = SaveManager.instance.datas.invenItem;
SaveMapper.WriteCell(arr2, page, slot, ItemStack.Empty);
inv.Service.LoadFrom(arr2);
inv.SaveInventory();
return "page=" + page + " boxes=" + inv.inventoryUI[page].transform.childCount + " kept=" + kept + " afterCleanup=" + SaveMapper.ReadCell(SaveManager.instance.datas.invenItem, page, slot);
