// REWORK-12 P7: 세트 단계 효과 Query 덤프(스킬별) vs 설계 §4.1 표
EffectTable.ResetCache(); EffectTable.Load(); SkillEffectService.ClearAll();
var sb = new System.Text.StringBuilder();
var map = new (string, string, string[])[] {
 ("Leaf5","EF_Set_Leaf5",new[]{"Scattering_Leaves"}), ("Wolf4","EF_Set_Wolf4",new[]{"Howl_of_the_Pack"}), ("Wolf5","EF_Set_Wolf5",new[]{"Howl_of_the_Pack","Boar_Charge"}),
 ("Wisp4","EF_Set_Wisp4",new[]{"Wisp_Bolt"}), ("Wisp5","EF_Set_Wisp5",new[]{"Wisp_Bolt"}), ("Grove4","EF_Set_Grove4",new[]{"Ancient_Roots"}), ("Grove5","EF_Set_Grove5",new[]{"Ancient_Roots"}),
 ("Bog4","EF_Set_Bog4",new[]{"Tongue_Lash"}), ("Bog5","EF_Set_Bog5",new[]{"Tongue_Lash"}), ("Stone4","EF_Set_Stone4",new[]{"Boulder_Toss"}), ("Stone5","EF_Set_Stone5",new[]{"Boulder_Toss","Quake_Slam"}) };
foreach (var m in map) {
  SkillEffectService.Detach("Dev:P7"); SkillEffectService.AttachIds("Dev:P7", m.Item2, "U");
  foreach (var sk in m.Item3) { var q = SkillEffectService.Query(sk, WeaponKind.None);
    var rules = new System.Collections.Generic.List<string>(); if (q.rules != null) foreach (var r in q.rules) rules.Add(r.rule.ToString());
    sb.Append(m.Item1 + " " + sk + ": dmg" + q.damagePct + " rng" + q.rangePct + " dur" + q.durationPct + " proj" + q.projectileAdd + " hit" + q.hitCountAdd + " rules[" + string.Join(",", rules.ToArray()) + "] score=" + EffectScore.OfIds(m.Item2).ToString("0.#") + "\n"); }
}
SkillEffectService.Detach("Dev:P7");
System.IO.File.WriteAllText("Docs/Playtest/EquipEffectAudit.md", EquipEffectAudit.Build(), new System.Text.UTF8Encoding(true));
return sb.ToString();
