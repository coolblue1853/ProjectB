AscentLoad.GrantRank(2); AscentLoad.SetLoad(45); int d0 = AscentLoad.Deficit; float o0 = AscentLoad.OutgoingMul();
ExplorerSeal.Grant(); ExplorerSeal.Grant(); ExplorerSeal.Grant(); int d1 = AscentLoad.Deficit;
AscentLoad.SetGearResist(5); int d2 = AscentLoad.Deficit; AscentLoad.SetGearResist(0);
bool blocked = RecallRules.BlockedByBoss(10f);
var t = RecallCast.Begin("x"); // 보스 없음 → 시전 시작 허용
RecallCast.Cancel("test");
return "rank=" + AscentLoad.Rank + " 인장 전 부족=" + d0 + " 후=" + d1 + " 장비+5=" + d2 + " seals=" + ExplorerSeal.Count + " 저항=" + AscentLoad.Resistance + " es3=" + ES3.Load<int>("explorer.seals") + " begin=" + t;
