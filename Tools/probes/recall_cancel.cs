// 귀환 취소: 피격(HpDamage) → 취소·소모 없음, 이동(위치 이동) → 취소·소모 없음. 사냥터에서 실행
var inv = InventoryManager.instance; var pc = PlayerController.instance; var phm = PlayerHealthManager.Instance;
int n0 = inv.CountItem("Recall Scroll");
bool a = inv.ConsumeFirst("Recall Scroll");
Debug.Log("[probe-rc] B start used=" + a + " casting=" + RecallCast.IsCasting);
DG.Tweening.DOVirtual.DelayedCall(0.4f, () => phm.HpDamage(5), true);
DG.Tweening.DOVirtual.DelayedCall(0.8f, () => Debug.Log("[probe-rc] B after hit casting=" + RecallCast.IsCasting + " n=" + inv.CountItem("Recall Scroll") + "/" + n0), true);
DG.Tweening.DOVirtual.DelayedCall(2.0f, () => { bool b = inv.ConsumeFirst("Recall Scroll"); Debug.Log("[probe-rc] C start used=" + b + " casting=" + RecallCast.IsCasting); }, true);
DG.Tweening.DOVirtual.DelayedCall(2.4f, () => pc.transform.position += new Vector3(1f, 0f, 0f), true);
DG.Tweening.DOVirtual.DelayedCall(3.0f, () => Debug.Log("[probe-rc] C after move casting=" + RecallCast.IsCasting + " n=" + inv.CountItem("Recall Scroll") + "/" + n0), true);
return "started n0=" + n0;
