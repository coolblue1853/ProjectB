// 강화 테스트: 세이브 백업 후 실행. CliffBlade + 강화석 30 + 골드 지급 → 강화 창 열기 → 5회 강화 → 장착해 데미지 배율 확인
var inv = InventoryManager.instance;
inv.CreatItem("CliffBlade");
for (int i = 0; i < 30; i++) inv.CreatItem("Enhance Stone");
DatabaseManager.money += 20000;
EnhanceWindow.Open();
var sb = new System.Text.StringBuilder();
ItemCheck blade = null;
foreach (var c in inv.EnumerateInventoryItems()) if (c.itemData.name == "CliffBlade") blade = c;
for (int i = 0; i < 5; i++) {
  EnhanceRules.Result r; int lv;
  EnhanceService.TryEnhance(blade, out r, out lv);
  sb.Append(r + "→+" + lv + " ");
}
sb.Append("| stones=" + inv.CountItem("Enhance Stone") + " gold=" + DatabaseManager.money + " streak=" + EnhanceService.FailStreak);
return sb.ToString();
