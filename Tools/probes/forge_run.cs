// 고급 강화 +10→+11 검증(플레이 중 실행). 세이브 백업 필수.
var sb = new System.Text.StringBuilder();
var inv = InventoryManager.instance;
inv.CreatItem("CliffBlade");
for (int i = 0; i < 30; i++) inv.CreatItem("Enhance Stone");
DatabaseManager.money += 20000;
ItemCheck blade = null;
foreach (var c in inv.EnumerateInventoryItems()) if (c.itemData.name == "CliffBlade") blade = c;
inv.Service.SetUpgrade(blade, 10);
ForgeState.UnlockMask = 0; ForgeState.Pity = 0;
sb.Append("lv=" + blade.itemData.upgrade + " 비트없음 CanTry=" + EnhanceService.CanTry(blade) + " CanEnhance=" + EnhanceService.CanEnhance(10) + "\n");
EnhanceRules.Result r; int lv;
sb.Append("비트없음 TryEnhance=" + EnhanceService.TryEnhance(blade, out r, out lv) + "\n");
ForgeState.UnlockMask = ForgeState.BitLv12;
sb.Append("비트1 CanTry=" + EnhanceService.CanTry(blade) + " 13가능=" + ForgeState.IsUnlocked(12) + "\n");
// 단조 3타 완벽 시뮬
var fs = new ForgeSession(() => 0.5f);
float t = 0;
while (!fs.Done && t < 60f) { if (!fs.Waiting && System.Math.Abs(fs.Cursor - fs.Target) < 1.5f) fs.Hit(); fs.Tick(0.01f); t += 0.01f; }
sb.Append("단조점수=" + fs.Total + " (" + fs.ScoreAt(0) + "," + fs.ScoreAt(1) + "," + fs.ScoreAt(2) + ")\n");
int stone0 = inv.CountItem("Enhance Stone"); long g0 = DatabaseManager.money;
EnhanceService.Rng = () => 0.99f; // 실패 고정
bool ok = EnhanceService.TryEnhance(blade, out r, out lv, fs.Total);
sb.Append("실패시도 ok=" + ok + " " + r + " lv=" + lv + " rate=" + EnhanceService.LastRate + " 환급=" + EnhanceService.LastRefund + " 결정 " + stone0 + "→" + inv.CountItem("Enhance Stone") + " 골드 " + g0 + "→" + DatabaseManager.money + " pity=" + ForgeState.Pity + " streak(기존)=" + EnhanceService.FailStreak + "\n");
stone0 = inv.CountItem("Enhance Stone"); g0 = DatabaseManager.money;
ok = EnhanceService.TryEnhance(blade, out r, out lv, 0); // 단조 없이 실패 → 환급 없음
sb.Append("단조0 실패 " + r + " rate=" + EnhanceService.LastRate + " 환급=" + EnhanceService.LastRefund + " 결정 " + stone0 + "→" + inv.CountItem("Enhance Stone") + " pity=" + ForgeState.Pity + "\n");
EnhanceService.Rng = () => 0f; // 성공 고정
stone0 = inv.CountItem("Enhance Stone"); g0 = DatabaseManager.money;
ok = EnhanceService.TryEnhance(blade, out r, out lv, fs.Total);
sb.Append("성공시도 " + r + " lv=" + lv + " item.upgrade=" + blade.itemData.upgrade + " rate=" + EnhanceService.LastRate + " 결정 " + stone0 + "→" + inv.CountItem("Enhance Stone") + " 골드 " + g0 + "→" + DatabaseManager.money + " pity=" + ForgeState.Pity + "\n");
sb.Append("+11 CanTry(+12 시도, 비트1)=" + EnhanceService.CanTry(blade) + " 무기배율=" + EnhanceRules.WeaponMultiplier(blade.itemData.upgrade) + "\n");
EnhanceService.Rng = () => UnityEngine.Random.value;
return sb.ToString();
