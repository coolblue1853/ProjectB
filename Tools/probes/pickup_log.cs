// 획득 로그 표시 테스트: 일반 병합(x3)·R·U 강조·가득 참 경고
PickupLog.Report("Wood", 1, "C");
PickupLog.Report("Wood", 2, "C");
PickupLog.Report("Rabbit Hide", 1, "C");
PickupLog.Report("LeafCap", 1, "R");
PickupLog.Report("RatCheifDagger", 1, "U");
PickupLog.ReportFull();
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => Time.timeScale = 0f, false);
return "reported";
