// 3c 준비(그림자 모델) 검증(플레이 모드): 서비스 명령만 쓰면 OutsideChanges 가 늘지 않고, GameObject 를 직접 옮기면 다음 명령 때 감지되는지.
// 세이브는 프로브 밖에서 백업·복원한다.
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
Transform page0 = inv.inventoryUI[0].transform;
inv.Service.SyncModelFromViews();
int base0 = inv.Service.OutsideChanges;

// 1) 명령만: 획득·나누기·이동·차감
for (int i = 0; i < 3; i++) inv.CreatItem("Potion");
ItemCheck pc = null; foreach (var c in inv.EnumerateInventoryItems()) if (c.name == "Potion") pc = c;
inv.Service.SplitAt(pc, 1, 0);
int empty = -1; for (int s = 0; s < page0.childCount; s++) if (page0.GetChild(s).childCount == 0) { empty = s; break; }
inv.MoveInInventory(pc.transform.parent, page0.GetChild(empty));
inv.DeletItemByName("Potion", 1);
inv.Service.SyncModelFromViews();
int afterCmd = inv.Service.OutsideChanges - base0;
sb.AppendLine("commands only: outside=" + afterCmd + " (expect 0)");

// 2) 직접 옮기기(옛 경로 흉내): 아이템 하나를 빈칸으로 SetParent → 다음 동기화에서 2칸(빠진 칸·채워진 칸) 감지
ItemCheck any = null; foreach (var c in inv.EnumerateInventoryItems()) { any = c; break; }
empty = -1; for (int s = 0; s < page0.childCount; s++) if (page0.GetChild(s).childCount == 0) { empty = s; break; }
Transform from = any.transform.parent;
any.transform.SetParent(page0.GetChild(empty), false);
int before = inv.Service.OutsideChanges;
inv.Service.SyncModelFromViews();
sb.AppendLine("direct move: outside +" + (inv.Service.OutsideChanges - before) + " (expect 2) last=" + inv.Service.LastOutsideChange);
any.transform.SetParent(from, false);
inv.Service.SyncModelFromViews();

inv.DeletItemByName("Potion", inv.CountItem("Potion"));
return sb.ToString();
