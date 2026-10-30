// 방어구 대장장이 추가 페이지 검증(BasicFeatures.md 15절, 플레이 중). __PHASE__:
//  pages = 페이지·레시피 보고 / give = Death Sigil 8·T1_Ore 10 지급 / craft = 마지막 페이지의 브링어 인장 반지로 커서 → 제작 → 수량 보고
string phase = "__PHASE__";
CraftingManager cm = null;
foreach (var c in UnityEngine.Object.FindObjectsByType<CraftingManager>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) if (!c.isSelfCraft && !c.isCraftWeapon) cm = c;
if (cm == null) return "no armor craft";
var box = cm.LV1craftBox[0].transform;
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
if (phase == "pages")
{
    string r = "pages=" + box.childCount + " nowPage=" + typeof(CraftingManager).GetField("nowPage", BF).GetValue(cm) + " cur=" + (cm.nowCraftItem != null ? cm.nowCraftItem.name : "null") + " |";
    for (int p = 0; p < box.childCount; p++) { var pg = box.GetChild(p); r += " P" + p + "(" + pg.gameObject.activeSelf + "):"; var line = pg.GetChild(pg.childCount - 1); for (int i = 0; i < line.childCount; i++) { var n = line.GetChild(i).GetComponent<NeedItem>(); r += " " + line.GetChild(i).name + "[" + (n != null ? string.Join("+", n.needItem.ToArray()) : "-") + "]"; } }
    return r;
}
var inv = InventoryManager.instance;
string ring = "Bringer's Sigil Ring";
if (phase == "give") { for (int i = 0; i < 8; i++) inv.CreatItem("Death Sigil"); for (int i = 0; i < 10; i++) inv.CreatItem("T1_Ore"); return "sigil=" + inv.CountItem("Death Sigil") + " ore=" + inv.CountItem("T1_Ore"); }
int before = inv.CountItem(ring);
int last = box.childCount - 1; typeof(CraftingManager).GetField("nowPage", BF).SetValue(cm, last); for (int p = 0; p < box.childCount; p++) box.GetChild(p).gameObject.SetActive(p == last); cm.CraftUI.SetActive(true);
cm.MoveCusorByMouse(0, 0);
cm.Craft();
return "cur=" + cm.nowCraftItem.name + " ring " + before + "->" + inv.CountItem(ring) + " sigil=" + inv.CountItem("Death Sigil") + " ore=" + inv.CountItem("T1_Ore");
