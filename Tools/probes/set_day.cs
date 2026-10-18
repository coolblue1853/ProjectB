// 낮으로 전환: 현재 진행도가 밤 구간이 끝난 직후가 되도록 시간 추가 (set_night.cs 의 반대)
var tc = TimeChange.instance;
float target = (tc.endNighttime + 0.05f) * tc.totalTime;
float now = tc.value * tc.totalTime;
float add = target - now; if (add < 0) add += tc.totalTime;
tc.AddTime(add);
return "value " + tc.value.ToString("F2") + " +" + add.ToString("F0") + "s day=" + tc.isDaytime;
