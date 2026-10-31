using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>프로브에서 호출하는 사냥 모델. 플레이 데이터는 읽기만 하고, 결과는 Logs/playtest에 쓴다.</summary>
public static class HuntingAudit
{
    private static readonly string[] Maps =
    {
        "Lv1_NearForest_SideRoad", "Lv1_NearForest", "Lv1_NormalForest", "Lv1_DeepForest",
        "Lv1_Swamp", "Lv1_BanditFortress", "Lv1_TowerOutskirts",
    };

    public static string WeaponName(int level)
    {
        return level >= 13 ? "PebbleGauntlet" : level >= 11 ? "CutthroatDagger" : level >= 9 ? "CrocJawAxe"
            : level >= 6 ? "BearClaw" : level >= 3 ? "FangDagger" : "RustedSword";
    }

    public static double KillSeconds(MonsterTable.Row row, int level, int load, bool elite)
    {
        Weapon weapon = Resources.Load<GameObject>("Weapon/" + WeaponName(level)).GetComponent<Weapon>();
        int[] damage = { weapon.minDmg, weapon.maxDmg, weapon.critPer, weapon.critDmg, weapon.incDmg,
            weapon.ignDef, weapon.skillDmg, weapon.finDmg, weapon.addDmg, 0 };
        var offense = new DamageCalculator.PlayerOffense { addBasicDmg = LevelRules.AtkBonus(level) };
        int def = elite ? Mathf.RoundToInt(row.enemyDef.Value * EnemyHealth.EliteDefMul) : row.enemyDef.Value;
        double total = 0, seconds = 0;
        for (int step = 0; step < weapon.attckSpeed.Length; step++)
        {
            seconds += weapon.attckSpeed[step] + (weapon.isHasDelayTime && step < weapon.delayTime.Length ? weapon.delayTime[step] : 0);
            DamageObject hit = weapon.attackPrefab[step].GetComponent<DamageObject>();
            total += HuntingRules.ExpectedHit(damage, offense, def, hit.dmgRatio,
                DamageCalculator.LevelGapMul(row.level.Value, level), AscentLoad.OutgoingMul(load)) * hit.hitCount;
        }
        return row.maxHP.Value * (elite ? EnemyHealth.EliteHpMul : 1) * seconds / total;
    }

    private static MapInfo Map(string name)
    {
        return UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .First(m => m.name == name);
    }

    private static double Sale(EnemySpawner spawner)
    {
        double gold = 0;
        foreach (var drop in spawner.GetComponent<DropManager>().AllDrops())
        {
            int index = DatabaseManager.instance.FindItemDataIndex(drop.name);
            if (index >= 0)
            {
                gold += drop.chance * DatabaseManager.instance.LoadItemData(index).price;
            }
        }
        return gold; // 보장 장비·추가 드롭·퀘스트 재료 판매는 제외한 기본 드롭표 판매 기대치
    }

    public static HuntingRules.Result Estimate(string mapName, bool day, int level, bool before, double damageFactor = 1)
    {
        MapInfo map = Map(mapName);
        var old = File.ReadAllLines("Docs/Balance/Floor1HuntingBaseline.csv").Skip(1)
            .Select(line => line.Split(',')).Where(c => c[0] == mapName).ToDictionary(c => c[1]);
        var camps = new List<HuntingRules.Camp>();
        foreach (EnemySpawner sp in map.GetComponentsInChildren<EnemySpawner>(true).OrderBy(s => s.name, StringComparer.Ordinal))
        {
            if (sp.isDaytimeSpowner != day || sp.EnemyPrefab == null)
            {
                continue;
            }
            MonsterTable.Row original = MonsterTable.Get(sp.EnemyPrefab.name);
            HuntingSpawnTable.Row setting = HuntingSpawnTable.Get(mapName, sp.name);
            if (original == null || original.isBoss == true || setting == null)
            {
                continue;
            }
            MonsterTable.Row row = before || string.IsNullOrEmpty(setting.Profile) ? original : MonsterTable.Get(setting.Profile);
            if (before && sp.EnemyPrefab.name == "CreepingTerror_BD")
            {
                EnemyHealth prefab = sp.EnemyPrefab.GetComponent<EnemyHealth>();
                row = new MonsterTable.Row
                {
                    maxHP = prefab.maxHP, enemyDef = prefab.enemyDef, level = 0, exp = 0, contactDamage = 0,
                };
            }
            double p = double.Parse(old[sp.name][6], CultureInfo.InvariantCulture);
            double normal = KillSeconds(row, level, map.ascentLoad, false);
            double elite = KillSeconds(row, level, map.ascentLoad, true);
            double incoming = DamageCalculator.IncomingToPlayer(row.contactDamage.Value, 1 + LevelRules.DefBonus(level), AscentLoad.IncomingMul(map.ascentLoad));
            double eliteIncoming = DamageCalculator.IncomingToPlayer(Mathf.RoundToInt(row.contactDamage.Value * 1.5f), 1 + LevelRules.DefBonus(level), AscentLoad.IncomingMul(map.ascentLoad));
            camps.Add(new HuntingRules.Camp
            {
                Count = before ? int.Parse(old[sp.name][4]) : setting.Count,
                Cycle = before ? float.Parse(old[sp.name][5], CultureInfo.InvariantCulture) : setting.Cycle,
                KillSeconds = (normal * (1 - p) + elite * p) / damageFactor,
                Exp = (before ? int.Parse(old[sp.name][13]) : row.exp.Value) * (1 + 3 * p),
                Gold = before && sp.EnemyPrefab.name == "CreepingTerror_BD" ? 0 : HuntingRules.GoldPerKill(row.level.Value, (float)p),
                Sale = Sale(sp),
                HpLoss = (1 - p) * Math.Min(1, normal / 6) * incoming + p * Math.Min(1, elite / 6) * eliteIncoming,
            });
        }
        return HuntingRules.Simulate(camps, 600, 3);
    }

    private static string MapFor(int level, bool bandits)
    {
        return level >= 13 ? Maps[6] : level >= 11 && bandits ? Maps[5] : level >= 9 ? Maps[4]
            : level >= 6 ? Maps[3] : level >= 3 ? Maps[2] : Maps[0];
    }

    private static double Rate(int level, bool bandits, bool before, Dictionary<string, double> factors = null)
    {
        string name = MapFor(level, bandits);
        double factor = factors != null ? factors[name] : 1;
        return (Estimate(name, true, level, before, factor).Exp + Estimate(name, false, level, before).Exp) / 20;
    }

    private static Dictionary<string, double> MeasuredFactors()
    {
        var samples = File.ReadAllLines("Docs/Balance/Floor1HuntingMeleeSample.csv").Skip(1)
            .Select(line => line.Split(',')).ToDictionary(c => c[0]);
        string[] monsters = { "Rabbit_BD", "Rabbit_BD", "ForestWolf_BD", "ForestBear_BD", "SwampFrog_BD", "BanditCutthroat_BD", "GolemSmall_BD" };
        var factors = new Dictionary<string, double>();
        var csv = new StringBuilder("map,modelDps,observedDps,damageFactor\n");
        foreach (var pair in Maps.Select((map, index) => new { map, index }))
        {
            string[] sample = samples[pair.map];
            int level = int.Parse(sample[7]);
            double damage = double.Parse(sample[3], CultureInfo.InvariantCulture);
            double seconds = double.Parse(sample[1], CultureInfo.InvariantCulture);
            MonsterTable.Row row = MonsterTable.Get(monsters[pair.index]);
            double ttk = KillSeconds(row, level, Map(pair.map).ascentLoad, false);
            factors[pair.map] = HuntingRules.ObservedDamageFactor(damage, seconds, row.maxHP.Value, ttk);
            csv.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0},{1:F3},{2:F3},{3:F3}",
                pair.map, row.maxHP.Value / ttk, damage / seconds, factors[pair.map]));
        }
        File.WriteAllText("Logs/playtest/hunt_calibration.csv", csv.ToString());
        return factors;
    }

    public static string Run()
    {
        var csv = new StringBuilder("map,time,level,weapon,kpmBefore,kpmAfter,expMinBefore,expMinAfter,goldMinBefore,goldMinAfter,saleMinAfter,potions10min,potionCostPercent\n");
        int[] levels = { 2, 2, 4, 7, 10, 12, 14 };
        for (int i = 0; i < Maps.Length; i++)
        {
            for (int dn = 0; dn < 2; dn++)
            {
                var before = Estimate(Maps[i], dn == 0, levels[i], true);
                var after = Estimate(Maps[i], dn == 0, levels[i], false);
                int heal = levels[i] < 3 ? 120 : 220, cost = heal == 120 ? 25 : 45;
                double potions = HuntingRules.PotionCount(after.HpLoss, heal, 200 + LevelRules.HpBonus(levels[i]));
                csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3},{4:F2},{5:F2},{6:F2},{7:F2},{8:F2},{9:F2},{10:F2},{11:F2},{12:F2}",
                    Maps[i], dn == 0 ? "day" : "night", levels[i], WeaponName(levels[i]), before.Kills / 10d, after.Kills / 10d,
                    before.Exp / 10, after.Exp / 10, before.Gold / 10, after.Gold / 10, after.Sale / 10, potions,
                    after.Gold > 0 ? potions * cost / after.Gold * 100 : 0));
            }
        }
        File.WriteAllText("Logs/playtest/hunt_estimates.csv", csv.ToString());
        var summary = new StringBuilder();
        summary.AppendLine("Lv1→15 EXP=" + HuntingRules.ExpToReach(15));
        Dictionary<string, double> factors = MeasuredFactors();
        foreach (bool bandits in new[] { false, true })
        {
            double before = HuntingRules.LevelingMinutes(15, NpcQuestCatalog.All, lv => Rate(lv, bandits, true), bandits);
            double after = HuntingRules.LevelingMinutes(15, NpcQuestCatalog.All, lv => Rate(lv, bandits, false), bandits);
            double bosses = KillSeconds(MonsterTable.Get("AncientEnt_BD"), 7, 20, false)
                + KillSeconds(MonsterTable.Get("GiantSlime_BD"), 11, 30, false);
            if (bandits)
            {
                bosses += KillSeconds(MonsterTable.Get("BanditChief_BD"), 13, 30, false);
            }
            double overhead = 48 + (bandits ? 8 : 0) + bosses / 60;
            double day = HuntingRules.LevelingMinutes(15, NpcQuestCatalog.All,
                lv => Estimate(MapFor(lv, bandits), true, lv, false).Exp / 10, bandits);
            double night = HuntingRules.LevelingMinutes(15, NpcQuestCatalog.All,
                lv => Estimate(MapFor(lv, bandits), false, lv, false).Exp / 10, bandits);
            summary.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "bandits={0}: hunt {1:F2}→{2:F2} min, quest/travel/boss {3:F2} min, total {4:F2}→{5:F2} min",
                bandits, before, after, overhead, before + overhead, after + overhead));
            summary.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "day only {0:F2} min, night only {1:F2} min (including overhead)", day + overhead, night + overhead));
            double measuredMixed = HuntingRules.LevelingMinutes(15, NpcQuestCatalog.All,
                lv => Rate(lv, bandits, false, factors), bandits);
            double measuredDay = HuntingRules.LevelingMinutes(15, NpcQuestCatalog.All,
                lv => Estimate(MapFor(lv, bandits), true, lv, false, factors[MapFor(lv, bandits)]).Exp / 10, bandits);
            summary.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "observed HP-DPS calibration: mixed {0:F2} min, day {1:F2} min (including overhead; night uncalibrated)",
                measuredMixed + overhead, measuredDay + overhead));
        }
        File.WriteAllText("Logs/playtest/hunt_progression.txt", summary.ToString());
        return summary.ToString();
    }
}
