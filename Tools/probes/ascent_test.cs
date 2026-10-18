// 상승 부하 확인: 현재 부하·저항·배율, 그 뒤 2층계(마법 절벽)로 이동 후 다시 확인 → 정지(HUD 스크린샷)
var sb = new System.Text.StringBuilder("before: load=" + AscentLoad.CurrentLoad + " rank=" + AscentLoad.Rank + " out=" + AscentLoad.OutgoingMul().ToString("0.00") + " in=" + AscentLoad.IncomingMul().ToString("0.00") + "; ");
return sb.ToString();
