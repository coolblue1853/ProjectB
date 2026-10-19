// 인벤토리 현재 페이지 정렬(D4): 섞어 넣고 정렬 → 순서·총량 확인. __KEY__ = run | open (open = 인벤 열고 스크린샷용). 세이브를 바꾸므로 테스트 뒤 백업 복원
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder();
if ("__KEY__" == "run")
{
    inv.CreatItem("Wood"); inv.CreatItem("Cap"); inv.CreatItem("Wood"); inv.CreatItem("GreenHurb");
    for (int i = 0; i < 3; i++) inv.CreatItem("Wood");
    inv.CreatItem("PickAxe"); inv.CreatItem("Potion");
}
int wood = inv.CountItem("Wood"), cap = inv.CountItem("Cap");
inv.nowBox = 0;
inv.SortCurrentPage();
var model = inv.Service.Model;
for (int i = 0; i < 15; i++) { var s = model[i]; if (!s.IsEmpty) sb.Append(i + ":" + s + " "); }
sb.Append("| wood " + wood + "->" + inv.CountItem("Wood") + " cap " + cap + "->" + inv.CountItem("Cap"));
return sb.ToString();
