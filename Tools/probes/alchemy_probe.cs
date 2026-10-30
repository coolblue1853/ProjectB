// 채집 재료 레시피 검증(MiniGames.md "채집 재료 레시피", 플레이 중, input_bg_on → K 로 자가 제작 창을 연 뒤).
// __ITEM__ 레시피의 재료를 지급 → 그 페이지·칸으로 커서 → 제작 → 결과·재료 수량 보고. 지급량은 레시피 수량 그대로(남는 재료 없음)
CraftingManager cm = null;
foreach (var c in UnityEngine.Object.FindObjectsByType<CraftingManager>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) if (c.isSelfCraft) cm = c;
if (cm == null) return "no self craft";
var box = cm.LV1craftBox[0].transform;
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var pageField = typeof(CraftingManager).GetField("nowPage", BF);
int page = -1, idx = -1;
NeedItem need = null;
for (int p = 0; p < box.childCount && page < 0; p++) { var line = box.GetChild(p).GetChild(0); for (int i = 0; i < line.childCount; i++) if (line.GetChild(i).name == "__ITEM__") { page = p; idx = i; need = line.GetChild(i).GetComponent<NeedItem>(); } }
if (page < 0) return "recipe not found: __ITEM__ pages=" + box.childCount;
var inv = InventoryManager.instance;
var sb = new System.Text.StringBuilder("page " + page + "/" + box.childCount + " idx " + idx + " give:");
for (int k = 0; k < need.needItem.Count; k++) { for (int n = 0; n < need.needItemAmount[k]; n++) inv.CreatItem(need.needItem[k]); sb.Append(" " + need.needItem[k] + "=" + inv.CountItem(need.needItem[k])); }
int now = (int)pageField.GetValue(cm);
box.GetChild(now).gameObject.SetActive(false);
pageField.SetValue(cm, page);
box.GetChild(page).gameObject.SetActive(true);
int before = inv.CountItem("__ITEM__");
cm.MoveCusorByMouse(0, idx);
cm.Craft();
sb.Append(" | __ITEM__ " + before + "->" + inv.CountItem("__ITEM__") + " left:");
for (int k = 0; k < need.needItem.Count; k++) sb.Append(" " + need.needItem[k] + "=" + inv.CountItem(need.needItem[k]));
return sb.ToString();
