// 물약 연속 사용: t=0 성공, t=1.0 거절, t=2.1 성공 (플레이 중)
var shop = UnityEngine.Object.FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
shop.CloseShop(); InventoryManager.instance.state = "";
var inv = InventoryManager.instance;
inv.CreatItem("Small HP Potion"); inv.CreatItem("Small HP Potion"); inv.CreatItem("Small HP Potion");
var phm = PlayerHealthManager.Instance;
phm.nowHp = 10;
int n0 = inv.CountItem("Small HP Potion");
bool a = inv.ConsumeFirst("Small HP Potion");
int hp1 = phm.nowHp;
DG.Tweening.DOVirtual.DelayedCall(1.0f, () => { bool b = inv.ConsumeFirst("Small HP Potion"); Debug.Log("[probe-pot] t=1.0 used=" + b + " hp=" + phm.nowHp); }, true);
DG.Tweening.DOVirtual.DelayedCall(2.1f, () => { phm.nowHp = 10; bool c = inv.ConsumeFirst("Small HP Potion"); Debug.Log("[probe-pot] t=2.1 used=" + c + " hp=" + phm.nowHp + " count=" + inv.CountItem("Small HP Potion") + " full=" + phm.fullHP); }, true);
return "n0=" + n0 + " t=0 used=" + a + " hp 10->" + hp1;
