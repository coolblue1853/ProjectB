// 상점 골드 부족 표시 검증: 돈 0 → 상점 열기 → 상점 쪽 커서(← 분기 직접 호출) → Z(상세) → Z(구매 시도) → "골드 부족" 경고 확인. 먼저 input_bg_on, 세이브는 백업·복원할 것
var shop = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
if (shop == null) return "no ShopManager";
int prevMoney = DatabaseManager.money;
DatabaseManager.money = 0;
shop.OpenShop();
var kb = UnityEngine.InputSystem.Keyboard.current;
DG.Tweening.TweenCallback left = () => { // ← 키와 같은 경로(InventoryManager 상점 접촉 분기)
    shop.leftKeyCheck = true;
    InventoryManager.instance.cusor.SetActive(false);
    InventoryManager.instance.state = "ShopCusorOn";
    shop.ChangeCusor();
};
DG.Tweening.TweenCallback press = () => {
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Z));
    DG.Tweening.DOVirtual.DelayedCall(0.1f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()), true);
};
DG.Tweening.DOVirtual.DelayedCall(0.5f, left, true);
DG.Tweening.DOVirtual.DelayedCall(1.0f, press, true);
DG.Tweening.DOVirtual.DelayedCall(1.5f, press, true);
DG.Tweening.DOVirtual.DelayedCall(1.9f, () => {
    var log = UnityEngine.Object.FindFirstObjectByType<PickupLog>();
    string warn = "none";
    if (log != null)
    {
        foreach (var t in log.GetComponentsInChildren<TMPro.TextMeshProUGUI>(false))
        {
            if (t.gameObject.name.Contains("Full")) { warn = t.text + " alpha=" + t.alpha; }
        }
    }
    Debug.Log("[probe-shop] state=" + InventoryManager.instance.state + " isBuyDetail=" + shop.isBuyDetail + " money=" + DatabaseManager.money + " warn=" + warn);
}, true);
DG.Tweening.DOVirtual.DelayedCall(4.0f, () => { DatabaseManager.money = prevMoney; }, true);
return "shop opened, money " + prevMoney + " -> 0";
