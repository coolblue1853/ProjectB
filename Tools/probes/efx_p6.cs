// REWORK-12 P6: 툴팁 특수 효과 구획 문구(한국어) + 상태 문구 + 세트 단계 줄. 플레이 모드에서 실행
var sb = new System.Text.StringBuilder();
sb.Append(EffectTooltip.ForItem("EF_GatekeepersOath", WeaponKind.None)).Append("\n---\n");
sb.Append(EffectTooltip.ForItem("EF_FangDagger;EF_LeafStaff", WeaponKind.Dagger)).Append("\n---\n");
sb.Append(EffectTooltip.ForSetTier("EF_DEV_Chain;EF_DEV_Proc", true, WeaponKind.None)).Append("\n");
sb.Append(EffectTooltip.ForSetTier("EF_DEV_Chain", false, WeaponKind.None)).Append("\n---\n");
SkillEffectService.AttachIds("Dev:Cap", "EF_DEV_Chain;EF_DEV_Proc;EF_DEV_KillCast", "R"); // Chain/Proc/OnKillCast 같은 슬롯(최대 2) → 하나 비활성
sb.Append(EffectTooltip.ForItem("EF_DEV_Chain;EF_DEV_Proc;EF_DEV_KillCast", WeaponKind.None));
SkillEffectService.Detach("Dev:Cap");
return sb.ToString();
