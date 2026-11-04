var o = new System.Text.StringBuilder();
int l0 = LevelSystem.Level, e0 = LevelSystem.Exp;
o.Append("orig Lv" + l0 + " exp" + e0);
foreach (var c in new[]{ new[]{1,50,0}, new[]{10,2000,0}, new[]{11,2000,0}, new[]{11,2000,1}, new[]{20,3000,0}, new[]{20,3000,1} }) {
  LevelSystem.SetState(c[0], c[1]);
  int cap = LevelRules.ExpToNext(LevelSystem.Level);
  int b = LevelSystem.Exp;
  LevelSystem.ApplyDeathPenalty(c[2]==1);
  o.Append("\nLv" + c[0] + " boss=" + c[2] + " " + b + "→" + LevelSystem.Exp + " loss=" + (b-LevelSystem.Exp) + " (" + (b>0? (100.0*(b-LevelSystem.Exp)/b).ToString("F1"):"0") + "%) lv=" + LevelSystem.Level);
}
o.Append("\nBossNearPlayer=" + BgmDirector.BossNearPlayer());
LevelSystem.SetState(l0, e0);
o.Append("\nrestored Lv" + LevelSystem.Level + " exp" + LevelSystem.Exp);
return o.ToString();
