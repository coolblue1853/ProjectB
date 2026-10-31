// 귀환 시나리오 A(2초 후 마을 도착) — 시나리오 B(시전 중 피격 취소) 순서. 플레이 중, 사냥터에서 실행
var inv = InventoryManager.instance;
var pc = PlayerController.instance;
int n0 = inv.CountItem("Recall Scroll");
bool a = inv.ConsumeFirst("Recall Scroll");
Debug.Log("[probe-rc] A start used=" + a + " casting=" + RecallCast.IsCasting + " n=" + n0);
DG.Tweening.DOVirtual.DelayedCall(1.0f, () => Debug.Log("[probe-rc] A t=1 casting=" + RecallCast.IsCasting + " prog=" + RecallCast.Progress.ToString("F2")), true);
DG.Tweening.DOVirtual.DelayedCall(2.6f, () => {
    var town = GameObject.Find("Map").transform.Find("Town");
    Debug.Log("[probe-rc] A done casting=" + RecallCast.IsCasting + " townActive=" + town.gameObject.activeInHierarchy + " n=" + inv.CountItem("Recall Scroll") + " pos=" + pc.transform.position);
}, true);
return "started";
