var db = DatabaseManager.instance; var sb = new System.Text.StringBuilder();
foreach (var k in new[] { "LeafArmor_5", "WolfArmor_4", "StoneArmor_5", "LeafArmor_2" }) { var d = db.LoadSetsData(db.FindSetsDataIndex(k)); sb.Append(k + " ids=[" + (d == null ? "null" : d.effectIds) + "]\n"); }
SetEffectApplier.Apply("(+)2 skillHitCount Rising_Roots/(+)50 coolDown Haste", true, "Set:LeafArmor_5", "EF_Set_Leaf5");
var q = SkillEffectService.Query("Scattering_Leaves", WeaponKind.None); sb.Append("after apply rules=" + (q.rules == null ? 0 : q.rules.Length) + " active=" + SkillEffectService.ActiveCount + "\n");
SetEffectApplier.Apply("(+)2 skillHitCount Rising_Roots/(+)50 coolDown Haste", false, "Set:LeafArmor_5", "EF_Set_Leaf5");
q = SkillEffectService.Query("Scattering_Leaves", WeaponKind.None); sb.Append("after remove rules=" + (q.rules == null ? 0 : q.rules.Length) + " active=" + SkillEffectService.ActiveCount);
return sb.ToString();
