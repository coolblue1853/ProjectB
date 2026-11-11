// WSKILL-V2 P3·P4 검증: 타입 트리 효과(데미지 배율·치명·타입 불일치 0·상한)와 공용 트리 효과(공속·이동·방어·HP). 플레이 모드, 세이브는 호출자가 백업/복원
var sb = new System.Text.StringBuilder();
TypeMasteryService.Data.RaiseTo("Sword", TypeMasteryRules.TotalExpForLevel(10));
SkillTreeService.Respec("Sword"); SkillTreeService.Respec(SkillTreeRules.CommonTree);
DatabaseManager.money = 1000000;
SkillTreeService.Respec("Sword");
int crit0 = DatabaseManager.playerCritRate, cd0 = DatabaseManager.playerCritDmgRate;
SkillTreeEffects.Refresh(WeaponKind.Sword);
sb.AppendLine("[기준] 평타%=" + SkillTreeEffects.AttackDamagePct + " 치명=" + crit0 + " 공속=" + DatabaseManager.attackSpeedBuff);
for (int i = 0; i < 3; i++) { SkillTreeService.Invest("Sword", "P1"); }
SkillTreeService.Invest("Sword", "P2");
SkillTreeEffects.Refresh(WeaponKind.Sword);
sb.AppendLine("[P1x3 P2x1] 평타%=" + SkillTreeEffects.AttackDamagePct + " (기대 6) 스킬%=" + SkillTreeEffects.SkillDamagePct + " (기대 3) 배율 평타=" + SkillTreeEffects.DamageMul(false) + " (기대 1.06) 스킬=" + SkillTreeEffects.DamageMul(true) + " (기대 1.03)");
SkillTreeEffects.Refresh(WeaponKind.Bow);
sb.AppendLine("[타입 불일치 Bow] 평타%=" + SkillTreeEffects.AttackDamagePct + " (기대 0) 배율=" + SkillTreeEffects.DamageMul(false));
SkillTreeEffects.Refresh(WeaponKind.Sword);
for (int i = 0; i < 4; i++) { SkillTreeService.Invest("Sword", "A1"); }
SkillTreeEffects.Refresh(WeaponKind.Sword);
sb.AppendLine("[A1x4] 치명 전=" + crit0 + " 후=" + DatabaseManager.playerCritRate + " (기대 +2) Bow일 때=" );
SkillTreeEffects.Refresh(WeaponKind.Bow);
sb.AppendLine("  Bow 치명=" + DatabaseManager.playerCritRate + " (기대 " + crit0 + ")");
SkillTreeEffects.Refresh(WeaponKind.Sword);
// 공용: 포인트 우회 투자(Data 직접)
var d = SkillTreeService.Data;
int hp0 = PlayerHealthManager.Instance != null ? PlayerHealthManager.Instance.fullHP : -1;
int sp0 = DatabaseManager.SpeedBuff, as0 = DatabaseManager.attackSpeedBuff, def0 = DatabaseManager.playerDef;
for (int i = 0; i < 10; i++) { d.Invest("Common", "C3", 999); d.Invest("Common", "C5", 999); d.Invest("Common", "C1", 999); }
for (int i = 0; i < 5; i++) { d.Invest("Common", "C4", 999); }
for (int i = 0; i < 10; i++) { d.Invest("Common", "C6", 999); d.Invest("Common", "C2", 999); }
SkillTreeEffects.Refresh(WeaponKind.Sword);
SkillTreeEffects.SyncMaxHp();
sb.AppendLine("[공용] 공속 " + as0 + "->" + DatabaseManager.attackSpeedBuff + " (C3x10=+10, Sword M1 없음) 이동 " + sp0 + "->" + DatabaseManager.SpeedBuff + " (+5) 방어 " + def0 + "->" + DatabaseManager.playerDef + " 치명 " + crit0 + "->" + DatabaseManager.playerCritRate + " (공용 5+A1 2=7 → +7) 치명피해 " + cd0 + "->" + DatabaseManager.playerCritDmgRate + " (+20) HP " + hp0 + "->" + (PlayerHealthManager.Instance != null ? PlayerHealthManager.Instance.fullHP : -1) + " (C5x10=+20%)");
sb.AppendLine("[상한] CritChance Mod=" + SkillTreeService.Mod(TreeStat.CritChance, WeaponKind.Sword) + " (상한 10) Stats=" + PlayerStats.Dump());
// 리스펙으로 제거 확인
d.ClearForRespec("Common"); SkillTreeService.Respec("Sword");
SkillTreeEffects.Refresh(WeaponKind.Sword);
SkillTreeEffects.SyncMaxHp();
sb.AppendLine("[원복] 공속=" + DatabaseManager.attackSpeedBuff + " 치명=" + DatabaseManager.playerCritRate + " HP=" + (PlayerHealthManager.Instance != null ? PlayerHealthManager.Instance.fullHP : -1) + " 쿨(Sword)=" + SkillTreeEffects.CooldownPct(WeaponKind.Sword));
return sb.ToString();
