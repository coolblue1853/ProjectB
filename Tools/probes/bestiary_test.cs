// 도감: 처치 기록 주입(토끼 1, 고블린 10, 버섯 50) → 창 열고 커서 __KEY__ 로 이동 → 정지(스크린샷용)
if (BestiaryRecord.Kills("SporeShroom_BD") < 50) {
  PixelCrushers.MessageSystem.SendMessage(null, "Kill", "Rabbit_BD");
  for (int i = 0; i < 10; i++) PixelCrushers.MessageSystem.SendMessage(null, "Kill", "CliffGoblin_BD");
  for (int i = 0; i < 50; i++) PixelCrushers.MessageSystem.SendMessage(null, "Kill", "SporeShroom_BD");
}
var w = UnityEngine.Object.FindFirstObjectByType<BestiaryWindow>();
var f = typeof(BestiaryWindow).GetField("open", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
if (!(bool)f.GetValue(w)) BestiaryWindow.Toggle();
typeof(BestiaryWindow).GetField("cursor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(w, __KEY__);
typeof(BestiaryWindow).GetMethod("Redraw", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(w, null);
return "kills rabbit=" + BestiaryRecord.Kills("Rabbit_BD") + " goblin=" + BestiaryRecord.Kills("CliffGoblin_BD") + " shroom=" + BestiaryRecord.Kills("SporeShroom_BD");
