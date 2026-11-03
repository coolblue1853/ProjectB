var __u=1;
UnityEditor.AssetDatabase.Refresh();
typeof(MonsterTable).GetField("rows", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, null);
typeof(HuntingSpawnTable).GetField("s_rows", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, null);
var sb = new System.Text.StringBuilder();
string mapName = "Lv1_NearForest_SideRoad";
var map = UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None).First(m => m.name == mapName);
var old = System.IO.File.ReadAllLines("Docs/Balance/Floor1HuntingBaseline.csv").Skip(1).Select(l => l.Split(',')).Where(c => c[0] == mapName).ToDictionary(c => c[1]);
foreach (int level in new[] { 1, 2, 3 })
foreach (bool day in new[] { true, false })
foreach (float cm in new[] { 1f, 0.5f })
{
    var camps = new System.Collections.Generic.List<HuntingRules.Camp>();
    foreach (var sp in map.GetComponentsInChildren<EnemySpawner>(true).OrderBy(s => s.name, System.StringComparer.Ordinal))
    {
        if (sp.isDaytimeSpowner != day || sp.EnemyPrefab == null) continue;
        var orig = MonsterTable.Get(sp.EnemyPrefab.name); var set = HuntingSpawnTable.Get(mapName, sp.name);
        if (orig == null || orig.isBoss == true || set == null) continue;
        var row = string.IsNullOrEmpty(set.Profile) ? orig : MonsterTable.Get(set.Profile);
        double p = double.Parse(old[sp.name][6], System.Globalization.CultureInfo.InvariantCulture);
        double n = HuntingAudit.KillSeconds(row, level, map.ascentLoad, false), e = HuntingAudit.KillSeconds(row, level, map.ascentLoad, true);
        int cn = cm < 1 ? ContactRules.ScaledDamage(row.contactDamage.Value, false, false) : row.contactDamage.Value;
        double inN = DamageCalculator.IncomingToPlayer(cn, 1 + LevelRules.DefBonus(level), AscentLoad.IncomingMul(map.ascentLoad));
        double inE = DamageCalculator.IncomingToPlayer(Mathf.RoundToInt(row.contactDamage.Value * 1.5f), 1 + LevelRules.DefBonus(level), AscentLoad.IncomingMul(map.ascentLoad));
        camps.Add(new HuntingRules.Camp { Count = set.Count, Cycle = set.Cycle, KillSeconds = n * (1 - p) + e * p, Exp = row.exp.Value * (1 + 3 * p),
            Gold = HuntingRules.GoldPerKill(row.level.Value, (float)p), Sale = 0, HpLoss = (1 - p) * System.Math.Min(1, n / 6) * inN + p * System.Math.Min(1, e / 6) * inE });
        if (level == 2 && cm == 1f && day) sb.AppendLine($"  {sp.name} n={set.Count} ttk={n:F1}s contact={row.contactDamage.Value} hp={row.maxHP.Value}");
    }
    var r = HuntingRules.Simulate(camps, 600, 3);
    int heal = level < 3 ? 120 : 220, cost = heal == 120 ? 25 : 45;
    double pot = HuntingRules.PotionCount(r.HpLoss, heal, 200 + LevelRules.HpBonus(level)) * 6;
    sb.AppendLine($"Lv{level} {(day ? "day" : "night")} contactMul={cm}: kills/min={r.Kills / 10.0:F1} exp/min={r.Exp / 10:F1} gold/min={r.Gold / 10:F1} hpLoss/min={r.HpLoss / 10:F1} potions/h={pot:F1} potionCost/h={pot * cost:F0}G goldHr={r.Gold * 6:F0} ratio={(r.Gold > 0 ? pot * cost / (r.Gold * 6) * 100 : 0):F1}%");
}
return sb.ToString();
