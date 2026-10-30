// 되사기 목록 패널·패드 검증(BasicFeatures.md 13절, 플레이 중): 상점 열기 → StoneCap(+3)·LeafStaff 판매 → 패널 본문 → 가상 패드 RT 로 맨 위 되사기
var inv = InventoryManager.instance;
var BF = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var shop = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
if (shop == null) return "no ShopManager";
if (!inv.ShopGameObject.activeSelf) shop.OpenShop();
string[] ids = { "StoneCap", "LeafStaff" };
foreach (string id in ids) {
    inv.CreatItem(id);
    ItemCheck it = null;
    foreach (ItemCheck c in inv.EnumerateInventoryItems()) { if (c.name == id) it = c; }
    if (id == "StoneCap") inv.Service.SetUpgrade(it, 3);
    inv.detail = it;
    inv.SellItemUI();
    typeof(InventoryManager).GetMethod("SellItem", BF).Invoke(inv, null);
}
inv.state = "";
int m1 = DatabaseManager.money;
var pad = UnityEngine.InputSystem.Gamepad.current;
if (pad == null) { pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>(); }
pad.MakeCurrent();
DG.Tweening.DOVirtual.DelayedCall(0.6f, () => { Debug.Log("[SIDELIST] shop panel showing=" + BuybackPanel.Instance.IsShowing + " canUse=" + Buyback.CanUse() + " body=" + BuybackPanel.Instance.BodyText.Replace("\n", " / ")); 
    var st = new UnityEngine.InputSystem.LowLevel.GamepadState(); st.rightTrigger = 1f; UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, st); // RT 는 축(버튼 비트 아님)
    DG.Tweening.DOVirtual.DelayedCall(0.5f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState()), true); }, true);
DG.Tweening.DOVirtual.DelayedCall(4.0f, () => Debug.Log("[SIDELIST] after RT money=" + DatabaseManager.money + " list=" + Buyback.Model.Count + " staff=" + inv.CountItem("LeafStaff") + " body=" + BuybackPanel.Instance.BodyText.Replace("\n", " / ")), true);
return "sold money=" + m1 + " list=" + Buyback.Model.Count + " state=" + inv.state;
