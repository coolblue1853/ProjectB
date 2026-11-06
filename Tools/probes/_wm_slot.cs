// 숙련 잠금 검증: slot 1 상태·쿨·스태미나 스냅샷
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var w = am.equipWeapon;
var s = w.skill[1];
return "mLv=" + WeaponMasteryService.LevelOf(w.MasteryKey) + " block=" + s.BlockReason(1) + " cool1=" + SkillCooldown.instance.isCooldown[1] + " stam=" + PlayerHealthManager.Instance.nowStemina + " canAct=" + s.CanActivate(1);
