// 플레이어 강제 사망 (부활 흐름 검증용)
var phm = PlayerHealthManager.Instance;
phm.HpDamage(phm.nowHp);
return "hp=" + phm.nowHp;
