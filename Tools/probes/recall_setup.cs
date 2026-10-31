// 사냥터로 이동 + 귀환 주문서 지급
var map = GameObject.Find("Map");
var sb = new System.Text.StringBuilder();
foreach (Transform m in map.transform) sb.Append(m.name + (m.gameObject.activeSelf ? "*" : "") + " ");
InventoryManager.instance.CreatItem("Recall Scroll"); InventoryManager.instance.CreatItem("Recall Scroll");
bool ok = MapInfo.GoToMap("Lv1_NearForest", PlayerController.instance.gameObject);
return sb + " go=" + ok;
