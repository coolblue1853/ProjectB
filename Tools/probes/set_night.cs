// 밤으로 전환: 현재 진행도가 밤 구간(startNighttime~endNighttime)에 들어가도록 시간 추가
var tc = TimeChange.instance;
tc.LockDaytime = false; // 낮 고정(기본 켜짐)을 풀어야 밤으로 넘어간다
float target = (tc.startNighttime + 0.05f) * tc.totalTime;
float now = tc.value * tc.totalTime;
float add = target - now; if (add < 0) add += tc.totalTime;
tc.AddTime(add);
return "value " + tc.value.ToString("F2") + " +" + add.ToString("F0") + "s night=[" + tc.startNighttime + "," + tc.endNighttime + "]";
