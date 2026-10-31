// 귀환 시전 시작(바 표시 확인용). recall_setup.cs 후 사냥터에서 실행
bool a = InventoryManager.instance.ConsumeFirst("Recall Scroll");
return "used=" + a + " casting=" + RecallCast.IsCasting + " bar=" + CastBar.IsVisible;
