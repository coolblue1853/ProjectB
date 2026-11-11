// REWORK-12 P6: 툴팁 구획을 HoverTip 으로 화면에 띄운다(세이브 무변경). 스크린샷은 따로
SkillEffectService.AttachIds("Dev:Cap", "EF_DEV_Chain;EF_DEV_Proc;EF_DEV_KillCast", "R");
var tiers = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>> {
    new System.Collections.Generic.KeyValuePair<int, string>(2, "+4 Def"), new System.Collections.Generic.KeyValuePair<int, string>(5, "-50% CoolDown [Haste]") };
var extras = new System.Collections.Generic.List<string> { EffectTooltip.ForSetTier("EF_DEV_Chain", true, WeaponKind.None), EffectTooltip.ForSetTier("EF_FangDagger", false, WeaponKind.None) };
string text = "<b>Gatekeeper's Oath</b>\n120 ~ 180 Dmg\n" + EffectTooltip.ForItem("EF_GatekeepersOath", WeaponKind.None) + "\n" + TooltipBuilder.SetBlock("Leaf", 3, 5, tiers, extras)
    + "\n" + EffectTooltip.ForItem("EF_DEV_Chain;EF_DEV_Proc;EF_DEV_KillCast", WeaponKind.None);
HoverTip.Show(PlayerController.instance, () => text);
return "shown " + HoverTip.IsShowing;
