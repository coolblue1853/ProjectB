// 플레이어 피격(빨강)·회복(초록) 숫자 확인용
var ph = PlayerHealthManager.Instance; ph.HpDown(37); DG.Tweening.DOVirtual.DelayedCall(0.25f, () => ph.HpHeal(25), false); return "hp=" + ph.nowHp;
