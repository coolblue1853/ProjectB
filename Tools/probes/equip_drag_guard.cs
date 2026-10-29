// B-012/013 회귀(플레이 모드): 장비칸(모자)에서 끌어 ① 차 있는 인벤 칸 ② 페이지 탭 ③ 빈 인벤 칸에 놓기.
// ①②(+아무 곳도 아님) 은 장비칸으로 되돌아오고 세이브 equipGear 유지, ③ 은 해제(기존 동작). 세이브를 바꾸므로 전후 백업·복원할 것
var sb = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
inv.inventory.SetActive(true);
Transform head = inv.equipBox[2].transform;
if (head.childCount == 0)
{
    var go = UnityEngine.Object.Instantiate(inv.itemPrefab, inv.transform);
    var ic0 = go.GetComponent<ItemCheck>();
    ic0.SetItem("Cap");
    typeof(InventoryManager).GetMethod("UseEquipment", flags).Invoke(inv, new object[] { ic0 });
}
var cap = head.GetChild(0).GetComponent<ItemCheck>();
var drag = cap.GetComponent<ItemDrag>();
if (drag.dragCanvus == null) { drag.dragCanvus = GameObject.FindWithTag("dragCanvas"); } // 창을 방금 열어 Start 전
sb.Append("equipped=" + cap.itemData.name + " gear=" + SaveManager.instance.datas.equipGear[2, 0]);

// 인벤 첫 페이지에서 차 있는 칸·빈 칸 찾기
inv.CreatItem("Wood");
Transform page = inv.inventoryUI[inv.nowBox].transform;
Transform full = null, empty = null;
for (int i = 0; i < page.childCount; i++)
{
    Transform b = page.GetChild(i);
    if (!b.CompareTag("Box")) { continue; }
    if (b.childCount > 0 && full == null) { full = b; }
    if (b.childCount == 0 && empty == null) { empty = b; }
}
Transform tab = null;
foreach (var t in inv.GetComponentsInChildren<Transform>(true))
{
    if (t.CompareTag("UpperBox")) { tab = t; break; }
}
sb.Append(" | full=" + (full != null) + " empty=" + (empty != null) + " tab=" + (tab != null));

var fChange = typeof(ItemDrag).GetField("changeParent", flags);
var fTab = typeof(ItemDrag).GetField("changeBox", flags);
var ev = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
System.Func<string> state = () => "parent=" + cap.transform.parent.name + " gear=" + SaveManager.instance.datas.equipGear[2, 0];

// ① 차 있는 칸
((UnityEngine.EventSystems.IBeginDragHandler)drag).OnBeginDrag(ev);
fChange.SetValue(drag, full); fTab.SetValue(drag, null); drag.equipBox = null;
string fullItem = full.GetChild(0).GetComponent<ItemCheck>().itemData.name;
((UnityEngine.EventSystems.IEndDragHandler)drag).OnEndDrag(ev);
sb.Append(" | 1.full " + state() + " fullStill=" + (full.childCount > 0 ? full.GetChild(0).GetComponent<ItemCheck>().itemData.name : "-") + "(was " + fullItem + ") headItem=" + head.GetChild(0).GetComponent<ItemCheck>().itemData.name);

// ② 페이지 탭
((UnityEngine.EventSystems.IBeginDragHandler)drag).OnBeginDrag(ev);
fChange.SetValue(drag, null); fTab.SetValue(drag, tab); drag.equipBox = null;
((UnityEngine.EventSystems.IEndDragHandler)drag).OnEndDrag(ev);
sb.Append(" | 2.tab " + state() + " alive=" + (cap != null));

// ②' 아무 칸도 아닌 곳 → 되돌림(드래그 캔버스에 떠 있지 않게)
((UnityEngine.EventSystems.IBeginDragHandler)drag).OnBeginDrag(ev);
fChange.SetValue(drag, null); fTab.SetValue(drag, null); drag.equipBox = null;
((UnityEngine.EventSystems.IEndDragHandler)drag).OnEndDrag(ev);
sb.Append(" | 2b.nowhere " + state());

// ③ 빈 칸 → 해제
((UnityEngine.EventSystems.IBeginDragHandler)drag).OnBeginDrag(ev);
fChange.SetValue(drag, empty); fTab.SetValue(drag, null); drag.equipBox = null;
((UnityEngine.EventSystems.IEndDragHandler)drag).OnEndDrag(ev);
sb.Append(" | 3.empty " + state() + " headChildren=" + head.childCount);
fChange.SetValue(drag, null);
return sb.ToString();
