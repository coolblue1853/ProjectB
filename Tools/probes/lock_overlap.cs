// B-114 Phase 3 검증: 평타 → 토큰 확인 → 대기 후 해제 / 무기 비활성화 시 토큰 해제
var w = UnityEngine.Object.FindFirstObjectByType<Weapon>();
if (w == null) return "no weapon equipped";
w.MeleeAttack();
string afterAttack = PlayerStatus.Dump().Replace("\n", " | ");
DG.Tweening.DOVirtual.DelayedCall(1.5f, () => Debug.Log("[probe] 1.5s after attack: " + PlayerStatus.Dump().Replace("\n", " | ")), false);
DG.Tweening.DOVirtual.DelayedCall(2.0f, () => { w.MeleeAttack(); w.gameObject.SetActive(false); Debug.Log("[probe] attack+disable: " + PlayerStatus.Dump().Replace("\n", " | ")); w.gameObject.SetActive(true); }, false);
return w.name + " stopMove=" + w.isWeaponStopMove + " after attack: " + afterAttack;
