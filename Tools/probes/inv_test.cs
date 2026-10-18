// 인벤토리 로직 검증: 스택·수량·일괄 삭제·가득 참 처리. (세이브를 건드리므로 플레이 모드에서만, 끝나면 원복 시도)
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
int before = inv.CountItem("Wood");
for (int i = 0; i < 25; i++) inv.CreatItem("Wood");
sb.AppendLine("Wood +25: " + before + " -> " + inv.CountItem("Wood"));
inv.DeletItemByName("Wood", 18);
sb.AppendLine("Wood -18 (일괄): -> " + inv.CountItem("Wood"));
inv.RebuildItemStackCache();
int cache; DatabaseManager.inventoryItemStack.TryGetValue("Wood", out cache);
sb.AppendLine("cache Wood = " + cache + " (count " + inv.CountItem("Wood") + ")");
// 가득 채우기: maxStack 1 장비로 빈칸 소진
int created = 0, rejected = 0;
for (int i = 0; i < 40; i++) { if (inv.CreatItem("Cap")) created++; else rejected++; }
sb.AppendLine("Cap x40: created=" + created + " rejected=" + rejected + " hasRoom(Cap)=" + inv.HasRoomFor("Cap"));
// 정리
inv.DeletItemByName("Cap", created);
inv.DeletItemByName("Wood", inv.CountItem("Wood") - before);
sb.AppendLine("cleanup: Cap=" + inv.CountItem("Cap") + " Wood=" + inv.CountItem("Wood"));
return sb.ToString();
