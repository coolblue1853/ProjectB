// 콤보 HUD: Kill 메시지 6회 → 콤보·드롭률 보너스 확인, 0.3초 뒤 정지(스크린샷). 재개 후 3.5초 뒤 보너스 회수 확인은 호출측에서
int base0 = DatabaseManager.playerDropRate;
for (int i = 0; i < 6; i++) PixelCrushers.MessageSystem.SendMessage(null, "Kill", "Rabbit_BD");
DG.Tweening.DOVirtual.DelayedCall(0.3f, () => Time.timeScale = 0f, false);
return "combo=" + KillCombo.Current + " dropRate " + base0 + "→" + DatabaseManager.playerDropRate;
