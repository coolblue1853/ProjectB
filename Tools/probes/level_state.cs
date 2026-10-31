var p = PlayerHealthManager.Instance;
return "Lv" + LevelSystem.Level + " exp=" + LevelSystem.Exp + "/" + LevelRules.ExpToNext(LevelSystem.Level) + " fullHP=" + p.fullHP + " nowHp=" + p.nowHp + " def=" + DatabaseManager.playerDef + " basicDmg=" + DatabaseManager.addbasicDmg + " started=" + p.Started;
