// 자가 제작 페이지 검증(BasicFeatures.md 10절, 플레이 중, K 로 제작 창을 연 뒤). __PHASE__:
//  pages = 페이지·레시피 목록 보고 / give = 숲붕어 2·나무 1 지급 / craft = 2페이지의 숲붕어 구이로 커서 → 제작 → 수량 보고
string phase = "__PHASE__";
CraftingManager cm = null;
foreach (var c in UnityEngine.Object.FindObjectsByType<CraftingManager>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) if (c.isSelfCraft) cm = c;
if (cm == null) return "no self craft";
var box = cm.LV1craftBox[0].transform;
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
if (phase == "pages")
{
    string r = "pages=" + box.childCount + " nowPage=" + typeof(CraftingManager).GetField("nowPage", BF).GetValue(cm) + " cur=" + (cm.nowCraftItem != null ? cm.nowCraftItem.name : "null") + " |";
    for (int p = 0; p < box.childCount; p++) { var line = box.GetChild(p).GetChild(0); r += " P" + p + "(" + box.GetChild(p).gameObject.activeSelf + "):"; for (int i = 0; i < line.childCount; i++) { var n = line.GetChild(i).GetComponent<NeedItem>(); r += " " + line.GetChild(i).name + "[" + string.Join("+", n.needItem.ToArray()) + "]"; } }
    return r;
}
var inv = InventoryManager.instance;
if (phase == "give") { inv.CreatItem("Forest Carp"); inv.CreatItem("Forest Carp"); inv.CreatItem("Wood"); return "carp=" + inv.CountItem("Forest Carp") + " wood=" + inv.CountItem("Wood"); }
int before = inv.CountItem("Grilled Forest Carp");
cm.MoveCusorByMouse(0, 1);
cm.Craft();
return "cur=" + cm.nowCraftItem.name + " grilled " + before + "->" + inv.CountItem("Grilled Forest Carp") + " carp=" + inv.CountItem("Forest Carp");
