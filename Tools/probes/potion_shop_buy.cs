// 상점 확장 줄 확인 + 키보드(Z)로 소형 물약 구매: 골드 감소·수량 증가 (플레이 중, input_bg_on 먼저)
var shop = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
if (shop == null) return "no ShopManager";
var page = shop.LV1craftBox[0].transform.GetChild(0);
var sb = new System.Text.StringBuilder();
for (int l = 0; l < page.childCount; l++) {
    var line = page.GetChild(l);
    for (int c = 0; c < line.childCount; c++) {
        var ic = line.GetChild(c).GetComponent<ItemCheck>();
        sb.Append(l + ":" + line.GetChild(c).name + "(buy=" + (ic != null ? ic.buyprice : -1) + ") ");
    }
}
DatabaseManager.money = 500;
InventoryManager.instance.CreatItem("Rock"); // no-op safety
int before = InventoryManager.instance.CountItem("Small HP Potion");
shop.OpenShop();
var kb = UnityEngine.InputSystem.Keyboard.current;
DG.Tweening.TweenCallback left = () => {
    shop.leftKeyCheck = true;
    InventoryManager.instance.cusor.SetActive(false);
    InventoryManager.instance.state = "ShopCusorOn";
    shop.ChangeCusor();
    shop.MoveCusorByMouse(1, 0);
};
DG.Tweening.TweenCallback press = () => {
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Z));
    DG.Tweening.DOVirtual.DelayedCall(0.1f, () => UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()), true);
};
DG.Tweening.DOVirtual.DelayedCall(0.5f, left, true);
DG.Tweening.DOVirtual.DelayedCall(1.0f, press, true);
DG.Tweening.DOVirtual.DelayedCall(1.5f, press, true);
DG.Tweening.DOVirtual.DelayedCall(2.0f, press, true);
DG.Tweening.DOVirtual.DelayedCall(2.8f, () => {
    Debug.Log("[probe-shop] money=" + DatabaseManager.money + " potions " + before + "->" + InventoryManager.instance.CountItem("Small HP Potion") + " state=" + InventoryManager.instance.state);
}, true);
return sb.ToString();
