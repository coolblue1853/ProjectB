// REWORK-12 P2: 효과 id 부여 아이템 효과 수치 확인(장착=Attach, 해제=Detach)
var sb = new System.Text.StringBuilder();
var db = UnityEngine.Object.FindFirstObjectByType<ItemDatabase>();
foreach (var it in db.items) { if (!string.IsNullOrEmpty(it.effectIds)) sb.AppendLine("item " + it.name + " rarity=" + it.rarity + " effectIds=" + it.effectIds); }
SkillMods m0 = SkillEffectService.Query("Scattering_Leaves", WeaponKind.Staff);
sb.AppendLine("before dmg=" + m0.damagePct + " range=" + m0.rangePct + " stamina(10)=" + SkillModApplier.Stamina(10, SkillEffectService.StaminaPct("Scattering_Leaves", WeaponKind.Staff)));
SkillEffectService.AttachIds("Item:Weapon", "EF_LeafStaff", "R");
SkillEffectService.AttachIds("Dev:Test", "EF_DEV_Test");
SkillMods m1 = SkillEffectService.Query("Scattering_Leaves", WeaponKind.Staff);
sb.AppendLine("equip dmg=" + m1.damagePct + " range=" + m1.rangePct + " proj=" + m1.projSpeedPct + " dur=" + m1.durationPct + " stamina(10)=" + SkillModApplier.Stamina(10, SkillEffectService.StaminaPct("Scattering_Leaves", WeaponKind.Staff)));
var go = new GameObject("efx_tmp");
var d = go.AddComponent<DamageObject>();
d.holdingTime = 1f; d.launchForce = 10f; d.desiredSpeed = 5f; d.isDestroyByTime = true;
int[] src = new int[] { 100, 50 };
SkillModApplier.Apply(d, src, ref m1);
sb.AppendLine("applied scale=" + go.transform.localScale.x + " hold=" + d.holdingTime + " launch=" + d.launchForce + " speed=" + d.desiredSpeed + " dmg=" + d.damageArr[0] + "," + d.damageArr[1] + " src=" + src[0]);
UnityEngine.Object.DestroyImmediate(go);
SkillEffectService.Detach("Item:Weapon"); SkillEffectService.Detach("Dev:Test");
SkillMods m2 = SkillEffectService.Query("Scattering_Leaves", WeaponKind.Staff);
sb.AppendLine("after dmg=" + m2.damagePct + " range=" + m2.rangePct + " stamina(10)=" + SkillModApplier.Stamina(10, SkillEffectService.StaminaPct("Scattering_Leaves", WeaponKind.Staff)) + " active=" + SkillEffectService.ActiveCount);
return sb.ToString();
