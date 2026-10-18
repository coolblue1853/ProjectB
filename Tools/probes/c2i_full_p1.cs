// 인벤토리를 가득 채운 상태에서 창고→인벤 이동 시 창고 수량이 보존되는지 (B-008), 나누기 실패 시 원본 보존 (B-009)
var inv = InventoryManager.instance;
var chest = UnityEngine.Object.FindFirstObjectByType<Chest>(FindObjectsInactive.Include);
if (chest == null) return "no chest";
var sb = new System.Text.StringBuilder();
int capCreated = 0; for (int i = 0; i < 40; i++) if (inv.CreatItem("Cap")) capCreated++;   // 빈칸 소진
sb.AppendLine("filled inventory with Cap x" + capCreated + " hasRoom(Wood)=" + inv.HasRoomFor("Wood"));
// 창고 0번 페이지 첫 칸에 Wood 10개
chest.nowBox = 0;
bool made = chest.CreatItemSelected("Wood", 0, 10);
var f = typeof(Chest).GetField("inventoryUI", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
var ui = (GameObject[])f.GetValue(chest);
int slot = -1; ItemCheck woodInChest = null;
for (int i = 0; i < ui[0].transform.childCount; i++) { var b = ui[0].transform.GetChild(i); if (b.childCount > 0) { var ic = b.GetChild(0).GetComponent<ItemCheck>(); if (ic.itemData.name == "Wood") { slot = i; woodInChest = ic; } } }
sb.AppendLine("chest Wood made=" + made + " slot=" + slot + " stack=" + (woodInChest ? woodInChest.nowStack : -1));
var cf = typeof(Chest).GetField("cusorCount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic); ((int[])cf.GetValue(chest))[0] = slot;
inv.CheckNowChest(chest);
typeof(Chest).GetMethod("C2IMove", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(chest, null);
sb.AppendLine("after C2I (inventory full): chest Wood=" + (woodInChest ? woodInChest.nowStack : -1) + " inv Wood=" + inv.CountItem("Wood"));
inv.DeletItemByName("Cap", capCreated);
return sb.ToString();
