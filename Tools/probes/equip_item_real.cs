// 플레이 중: 인벤토리의 실제 장착 경로(시작 무기 지급과 같음 — 아이템 생성 → UseEquipment)로 __ITEM__ 장착. 장비 칸 아이콘까지 들어간다(CelEquipSync 검증용)
var inv = InventoryManager.instance;
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
var prefab = typeof(InventoryManager).GetField("itemPrefab", flags).GetValue(inv) as GameObject;
var go = UnityEngine.Object.Instantiate(prefab, inv.transform.position, Quaternion.identity, inv.transform);
var item = go.GetComponent<ItemCheck>();
item.SetItem("__ITEM__");
if (item.itemData == null) { UnityEngine.Object.Destroy(go); return "no item __ITEM__"; }
typeof(InventoryManager).GetMethod("UseEquipment", flags).Invoke(inv, new object[] { item });
var sb = new System.Text.StringBuilder("equipped __ITEM__ area=" + item.itemData.equipArea + " boxes:");
foreach (int b in new[] { 2, 5 }) sb.Append(" " + b + "=" + (inv.equipBox[b].transform.childCount > 0 ? inv.equipBox[b].transform.GetChild(0).GetComponent<ItemCheck>().itemData.name : "-"));
return sb.ToString();
