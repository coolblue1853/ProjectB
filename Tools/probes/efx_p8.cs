// REWORK-12 P8: 장비 쿨감(FangDagger Howl -15) x 트리 쿨감(리플렉션으로 15) 곱 결합 확인. 기대 27.75
EffectTable.ResetCache(); EffectTable.Load(); SkillEffectService.ClearAll();
var f = typeof(SkillTreeEffects).GetField("_cd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
float old = (float)f.GetValue(null);
var sb = new System.Text.StringBuilder();
sb.Append("tree0 equip0: " + SkillEffectService.Query("Howl_of_the_Pack", WeaponKind.None).cooldownPct + "\n");
SkillEffectService.AttachIds("Dev:P8", "EF_FangDagger", "R");
sb.Append("equip only: " + SkillEffectService.Query("Howl_of_the_Pack", WeaponKind.None).cooldownPct + "\n");
f.SetValue(null, 15f);
var q = SkillEffectService.Query("Howl_of_the_Pack", WeaponKind.None);
sb.Append("equip15 x tree15: " + q.cooldownPct + " -> 20s becomes " + CooldownMath.Apply(20f, q.cooldownPct) + "\n");
f.SetValue(null, old); SkillEffectService.Detach("Dev:P8");
return sb.ToString();
