// EXP 2000 주입 (Lv1 기준 3레벨 업)
LevelSystem.GainExp(2000, "probe");
var p = PlayerHealthManager.Instance;
return "Lv" + LevelSystem.Level + " exp=" + LevelSystem.Exp + " fullHP=" + p.fullHP + " nowHp=" + p.nowHp + " def=" + DatabaseManager.playerDef + " basicDmg=" + DatabaseManager.addbasicDmg;
