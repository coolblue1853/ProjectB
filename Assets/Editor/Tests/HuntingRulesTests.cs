using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class HuntingRulesTests
{
    [TestCase("SporeShroom_BD", "ForestSpore_BD", 5)]
    [TestCase("SporeShroom_BD", "DeepSpore_BD", 7)]
    [TestCase("SporeShroom_BD", "SwampSpore_BD", 9)]
    [TestCase("CliffGoblin_BD", "DeepGoblin_BD", 7)]
    public void SpawnProfile_PreservesExistingKillMessageKey(string prefabId, string profile, int level)
    {
        var go = new GameObject(prefabId + "(Clone)");
        go.SetActive(false);
        try
        {
            var enemy = go.AddComponent<EnemyHealth>();
            enemy.ApplySpawnProfile(profile);
            Assert.AreEqual(profile, enemy.MonsterId);
            Assert.AreEqual(level, enemy.Level);
            Assert.AreEqual(prefabId, enemy.KillMessageId);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void Goal15_DoesNotIncludeLevel15To16()
    {
        Assert.AreEqual(89200, HuntingRules.ExpToReach(15));
        Assert.AreEqual(113700, HuntingRules.ExpToReach(16));
    }

    [Test]
    public void QuestBudget_ExcludesRewardAvailableAtGoal()
    {
        int budget = NpcQuestCatalog.All.Where(q => q.MinLevel < 15).Sum(q => q.Exp);
        Assert.AreEqual(26425, budget);
        Assert.AreEqual(23014, NpcQuestCatalog.All.Where(q => q.MinLevel < 15 && !q.Id.StartsWith("Q5")).Sum(q => q.Exp));
        var quests = new[] { new NpcQuestDef { Id = "Q604", MinLevel = 15, Exp = int.MaxValue } };
        Assert.AreEqual(892, HuntingRules.LevelingMinutes(15, quests, lv => 100, false), 0.0001);
    }

    [Test]
    public void QuestGating_RequiresLevelAndPrerequisite()
    {
        var quests = new[]
        {
            new NpcQuestDef { Id = "late", MinLevel = 2, Requires = "first", Exp = 650 },
            new NpcQuestDef { Id = "first", MinLevel = 1, Exp = 500 },
        };
        Assert.AreEqual(0, HuntingRules.LevelingMinutes(3, quests, lv => 100, false));
        quests[1].MinLevel = 3;
        Assert.AreEqual(11.5, HuntingRules.LevelingMinutes(3, quests, lv => 100, false), 0.0001);
    }

    [Test]
    public void QuestRewards_CarryOverflowAndSkipOptionalBranch()
    {
        var quests = new[] { new NpcQuestDef { Id = "Q501", MinLevel = 1, Exp = 600 } };
        Assert.AreEqual(5.5, HuntingRules.LevelingMinutes(3, quests, lv => 100, true), 0.0001);
        Assert.AreEqual(11.5, HuntingRules.LevelingMinutes(3, quests, lv => 100, false), 0.0001);
    }

    [Test]
    public void ExpectedHit_IntegratesCritAndSeparateExtraDamage()
    {
        int[] damage = { 10, 10, 20, 0, 0, 0, 0, 0, 10, 0 };
        Assert.AreEqual(11.4, HuntingRules.ExpectedHit(damage, default, 0, 1, 1, 1), 0.00001);
        // 주 피해에만 부하 50%, 별도 추가타 1은 그대로(EnemyHealth 실제 경로).
        Assert.AreEqual(6.2, HuntingRules.ExpectedHit(damage, default, 0, 1, 1, 0.5f), 0.00001);
    }

    [TestCase(1, 0f, 3.6)]
    [TestCase(1, 0.04f, 4.176)]
    [TestCase(15, 0.04f, 43.152)]
    public void Gold_UsesDropChanceAndEliteMultiplier(int level, float elite, double expected)
    {
        Assert.AreEqual(expected, HuntingRules.GoldPerKill(level, elite), 0.00001);
        Assert.AreEqual(0, HuntingRules.GoldPerKill(0, elite));
    }

    [Test]
    public void Refill_UsesSpawnerTickRatherThanDeathPlusCycle()
    {
        Assert.AreEqual(11, HuntingRules.RefillAfter(2, 10));
        Assert.AreEqual(11, HuntingRules.RefillAfter(10.9, 10));
        Assert.AreEqual(21, HuntingRules.RefillAfter(11, 10));
    }

    [Test]
    public void TenMinutes_AccountsForEmptySlotsAndInitialDelay()
    {
        var camp = new HuntingRules.Camp { Count = 1, Cycle = 10, KillSeconds = 1, Exp = 12, Gold = 3.6 };
        var result = HuntingRules.Simulate(new[] { camp }, 600, 0);
        Assert.AreEqual(60, result.Kills);
        Assert.AreEqual(720, result.Exp);
        Assert.AreEqual(216, result.Gold, 0.00001);
    }

    [TestCase(599, 1)]
    [TestCase(600, 0)]
    public void TenMinutes_ExcludesKillsAfterEnd(double killSeconds, int expected)
    {
        var camp = new HuntingRules.Camp { Count = 1, Cycle = 10, KillSeconds = killSeconds };
        Assert.AreEqual(expected, HuntingRules.Simulate(new[] { camp }, 600, 0).Kills);
    }

    [Test]
    public void Simulation_RejectsZeroCombatTime()
    {
        Assert.Throws<ArgumentException>(() => HuntingRules.Simulate(new[] { new HuntingRules.Camp { Count = 1, Cycle = 10 } }, 600, 0));
    }

    [Test]
    public void Potion_ClampsHealingToPlayerMaxHp()
    {
        Assert.AreEqual(10, HuntingRules.PotionCount(2000, 320, 200));
        Assert.AreEqual(5, HuntingRules.PotionCount(1100, 220, 312));
        Assert.AreEqual(0, HuntingRules.PotionCount(0, 220, 312));
    }

    [Test]
    public void ObservedDamageFactor_CalibratesCleaveAndMisses()
    {
        Assert.AreEqual(2, HuntingRules.ObservedDamageFactor(200, 10, 100, 10));
        Assert.AreEqual(0.5, HuntingRules.ObservedDamageFactor(50, 10, 100, 10));
    }

    [Test]
    public void ObservedDamageFactor_InvalidSampleKeepsModel()
    {
        Assert.AreEqual(1, HuntingRules.ObservedDamageFactor(0, 10, 100, 10));
        Assert.AreEqual(1, HuntingRules.ObservedDamageFactor(100, 0, 100, 10));
        Assert.AreEqual(1, HuntingRules.ObservedDamageFactor(double.NaN, 10, 100, 10));
    }

    [Test]
    public void SpawnWeights_DecoupleSlotAndAnchorCounts()
    {
        CollectionAssert.AreEqual(new[] { 0, 0, 1, 2, 2, 2 }, HuntingSpawnTable.PositionIndices(6, 3, new[] { 2, 1, 3 }));
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 0, 1 }, HuntingSpawnTable.PositionIndices(5, 3, null));
    }

    [Test]
    public void ScreenDensity_IncludesJitterAndCameraHeight()
    {
        Assert.AreEqual(1, HuntingRules.VisibleChance(Vector2.zero, Vector2.zero));
        Assert.AreEqual(0.5, HuntingRules.VisibleChance(new Vector2(10, 0), Vector2.zero));
        Assert.AreEqual(0, HuntingRules.VisibleChance(new Vector2(11, 0), Vector2.zero));
        Assert.AreEqual(0, HuntingRules.VisibleChance(new Vector2(0, 8), Vector2.zero));
    }

    [Test]
    public void InvalidCycle_DoesNotCreateInfiniteSimulation()
    {
        Assert.Throws<ArgumentException>(() => HuntingRules.Simulate(new[]
        {
            new HuntingRules.Camp { Count = 1, Cycle = float.NaN, KillSeconds = 1 },
        }, 600, 0));
    }

    [Test]
    public void Floor1SpawnData_ProfilesAndWeightsAreValid()
    {
        var rows = HuntingSpawnTable.Parse(Resources.Load<TextAsset>("HuntingSpawnsFloor1").text);
        Assert.AreEqual(92, rows.Count);
        foreach (var pair in rows)
        {
            Assert.IsTrue(pair.Key.StartsWith("Lv1_"), pair.Key);
            Assert.That(pair.Value.Cycle, Is.InRange(8f, 12f), pair.Key);
            Assert.NotNull(pair.Value.PointWeights, pair.Key);
            Assert.AreEqual(pair.Value.Count, pair.Value.PointWeights.Sum(), pair.Key);
            Assert.IsTrue(pair.Value.PointWeights.All(w => w > 0), pair.Key);
            if (!string.IsNullOrEmpty(pair.Value.Profile))
            {
                var monster = MonsterTable.Get(pair.Value.Profile);
                Assert.NotNull(monster, pair.Key);
                Assert.AreEqual(1, monster.floor, pair.Key);
                Assert.That(monster.level.Value, Is.InRange(1, 15), pair.Key);
            }
        }
        Assert.IsNull(HuntingSpawnTable.Get("Lv2_MagicCliffs", "Rabbit"));
        Assert.IsNull(HuntingSpawnTable.Get("Lv1_Swamp", "BossSpawoner"));
    }

    [Test]
    public void ReusedArt_DoesNotChangeFloor2Stats()
    {
        Assert.AreEqual(16, MonsterTable.Get("SporeShroom_BD").level);
        Assert.AreEqual(240, MonsterTable.Get("SporeShroom_BD").exp);
        Assert.AreEqual(16, MonsterTable.Get("CliffGoblin_BD").level);
        Assert.AreEqual(240, MonsterTable.Get("CliffGoblin_BD").exp);
        Assert.AreEqual(5, MonsterTable.Get("ForestSpore_BD").level);
        Assert.AreEqual(7, MonsterTable.Get("DeepGoblin_BD").level);
        Assert.AreEqual(9, MonsterTable.Get("SwampSpore_BD").level);
    }
}
