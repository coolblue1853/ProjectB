// 비교 패널 검증(BasicFeatures.md 6절, 플레이 중). __PHASE__:
//  open  = 인벤을 연 상태에서 착용 부위와 같은 장비를 인벤에 만들고 상세 창 띄움 (다음: Shift 누른 채 check)
//  check = 패널 표시 여부·내용 보고
string phase = "__PHASE__";
var inv = InventoryManager.instance;
if (phase == "open")
{
    ItemCheck worn = inv.EquippedFor("Weapon");
    if (worn == null) return "no weapon equipped";
    if (!inv.inventory.activeSelf) return "inventory closed — press I first";
    inv.CreatItem(worn.name);
    ItemCheck target = null;
    foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.itemData != null && c.itemData.equipArea == "Weapon") { target = c; break; } }
    if (target == null) return "no weapon in inventory";
    inv.ChangeCusor(target.transform.parent.gameObject);
    inv.state = "";
    inv.BoxContentChecker();
    return "worn=" + worn.name + " detail=" + (inv.detail != null ? inv.detail.name : "null") + " state=" + inv.state + " equipDetail=" + inv.equipDetail.activeSelf;
}
var go = UnityEngine.GameObject.Find("[ComparePanel]");
var kb = UnityEngine.InputSystem.Keyboard.current;
return "shiftHeld=" + (kb != null && kb.shiftKey.isPressed) + " showing=" + ComparePanel.IsShowing + " texts=" + (go != null ? string.Join(" | ", System.Array.ConvertAll(go.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true), t => t.text.Replace("\n", " / "))) : "none");
