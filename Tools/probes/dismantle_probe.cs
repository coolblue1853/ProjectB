// 분해 검증(BasicFeatures.md 8절, 플레이 중, 인벤토리를 연 뒤). __PHASE__:
//  setup = StoneCap(R) 지급·+4 강화·커서 이동, 재료 수 기록 (다음: press_key D 두 번)
//  check = 장비 사라짐·광석/층계 결정 증가 보고
string phase = "__PHASE__";
var inv = InventoryManager.instance;
if (!inv.inventory.activeSelf) return "inventory closed";
if (phase == "setup")
{
    inv.CreatItem("StoneCap");
    ItemCheck cap = null;
    foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == "StoneCap") { cap = c; } }
    if (cap == null) return "no cap";
    inv.Service.SetUpgrade(cap, 4);
    inv.ChangeCusor(cap.transform.parent.gameObject);
    inv.state = "";
    UnityEngine.PlayerPrefs.SetString("pb_probe_dis", inv.CountItem("StoneCap") + "," + inv.CountItem("T0_Ore") + "," + inv.CountItem("T1_Ore") + "," + inv.CountItem("Enhance Stone"));
    return "setup cursor=" + (inv.CursorItem() != null ? inv.CursorItem().name + " +" + inv.CursorItem().itemData.upgrade + " T" + inv.CursorItem().itemData.tear : "null") + " before=" + UnityEngine.PlayerPrefs.GetString("pb_probe_dis");
}
string before = UnityEngine.PlayerPrefs.GetString("pb_probe_dis");
UnityEngine.PlayerPrefs.DeleteKey("pb_probe_dis");
return "before(cap,T0,T1,stone)=" + before + " after=" + inv.CountItem("StoneCap") + "," + inv.CountItem("T0_Ore") + "," + inv.CountItem("T1_Ore") + "," + inv.CountItem("Enhance Stone");
