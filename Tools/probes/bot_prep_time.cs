// 시험 준비: 낮/밤 고정(런타임만, 세이브 안 건드림). __MODE__ = day | night. 봇 시작 전에만 쓴다.
var tc = TimeChange.instance;
string mode = "__MODE__";
float v;
if (mode == "night")
{
    v = (tc.startNighttime + tc.endNighttime) * 0.5f;
}
else
{
    v = 0.25f;
}
var f = typeof(TimeChange).GetField("_lockedTimeValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
f.SetValue(tc, v);
tc.LockDaytime = true;
return "mode=" + mode + " v=" + v.ToString("F2") + " night=[" + tc.startNighttime + "," + tc.endNighttime + "] lock=" + tc.LockDaytime;
