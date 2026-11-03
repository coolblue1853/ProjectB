using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Floor1Extra2Tests
{
    [Test]
    public void Flags_ShippedTableHasSecondBatch()
    {
        Dictionary<string, bool> f = Floor1Data.ParseFlags(Resources.Load<TextAsset>("Floor1Content").text);
        foreach (string id in new[] { "rare", "raid", "bossretry", "roadlink" })
        {
            Assert.IsTrue(f.ContainsKey(id), id);
        }
    }

    [Test]
    public void RareNodes_EveryZoneHasTwoCandidates()
    {
        List<RareNodeDef> defs = RareColonyRules.Parse(Resources.Load<TextAsset>("Floor1RareNodes").text);
        var perMap = new Dictionary<string, int>();
        foreach (RareNodeDef d in defs)
        {
            perMap[d.Map] = perMap.TryGetValue(d.Map, out int n) ? n + 1 : 1;
        }
        Assert.AreEqual(8, perMap.Count); // 6구역 + 신규 맵 2장
        foreach (KeyValuePair<string, int> kv in perMap)
        {
            Assert.GreaterOrEqual(kv.Value, 2, kv.Key);
        }
    }

    [Test]
    public void RarePick_RespectsChanceAndRange()
    {
        Assert.AreEqual(-1, RareColonyRules.Pick(0, 0f, 0f));
        Assert.AreEqual(-1, RareColonyRules.Pick(2, 0.2f, 0f)); // 확률 경계: 20% 미만만 출현
        Assert.AreEqual(0, RareColonyRules.Pick(2, 0.19f, 0.2f));
        Assert.AreEqual(1, RareColonyRules.Pick(2, 0f, 0.99f));
        Assert.AreEqual(1, RareColonyRules.Pick(2, 0f, 1f)); // 1.0 이어도 범위 밖으로 나가지 않는다
    }

    [Test]
    public void RareRollCooldown()
    {
        Assert.IsFalse(RareColonyRules.CanRoll(10f, 100f));
        Assert.IsTrue(RareColonyRules.CanRoll(100f, 100f));
    }

    [Test]
    public void RareTables_AreRicherThanNormal()
    {
        Assert.Greater(UncommonShare(GatherRules.RareSpecimenFloor1), UncommonShare(GatherRules.SpecimenFloor1) * 3f);
        Assert.Greater(UncommonShare(GatherRules.RareOreFloor1), UncommonShare(GatherRules.OreFloor1) * 3f);
        Assert.AreSame(GatherRules.RareOreFloor1, GatherRules.TableFor(GatherKind.Ore, 1, true));
        Assert.AreSame(GatherRules.SpecimenFloor1, GatherRules.TableFor(GatherKind.Specimen, 1));
    }

    private static float UncommonShare(GatherDef[] table)
    {
        float total = 0f, rare = 0f;
        foreach (GatherDef d in table)
        {
            total += d.BaseWeight;
            if (d.Rarity != ItemRarity.C)
            {
                rare += d.BaseWeight;
            }
        }
        return rare / total;
    }

    [Test]
    public void Raids_ParseAndPickByLevel()
    {
        List<NightRaidDef> defs = NightRaidRules.Parse(Resources.Load<TextAsset>("Floor1Raids").text);
        Assert.AreEqual(6, defs.Count);
        foreach (NightRaidDef d in defs)
        {
            Assert.GreaterOrEqual(d.Waves, 2, d.Map);
            Assert.LessOrEqual(d.TotalMobs, 20, d.Map); // 초반 소박: 한 번에 20마리를 넘기지 않는다
        }
        for (int night = 1; night <= 30; night++)
        {
            NightRaidDef p = NightRaidRules.Pick(defs, night, 1);
            Assert.IsNotNull(p);
            Assert.AreEqual("Lv1_NearForest_SideRoad", p.Map); // Lv1 에선 샛길만
            Assert.AreSame(p, NightRaidRules.Pick(defs, night, 1)); // 같은 밤은 같은 곳
        }
        var seen = new HashSet<string>();
        for (int night = 1; night <= 60; night++)
        {
            seen.Add(NightRaidRules.Pick(defs, night, 15).Map);
        }
        Assert.AreEqual(6, seen.Count);
        Assert.IsNull(NightRaidRules.Pick(new List<NightRaidDef>(), 1, 15));
    }

    [Test]
    public void BossRetry_MultiplierSteps()
    {
        Assert.AreEqual(1f, BossRetryRules.Multiplier(0));
        Assert.AreEqual(0.5f, BossRetryRules.Multiplier(1));
        Assert.AreEqual(0.25f, BossRetryRules.Multiplier(2));
        Assert.AreEqual(0.25f, BossRetryRules.Multiplier(9));
        Assert.AreEqual(5, BossRetryRules.Stones(5, 1f));
        Assert.AreEqual(3, BossRetryRules.Stones(5, 0.5f));
        Assert.AreEqual(2, BossRetryRules.Stones(5, 0.25f));
        Assert.AreEqual(1, BossRetryRules.Stones(1, 0.25f)); // 최소 1개
        Assert.IsTrue(BossRetryRules.GivesSetPiece(1f, 0.99f));
        Assert.IsTrue(BossRetryRules.GivesSetPiece(0.5f, 0.49f));
        Assert.IsFalse(BossRetryRules.GivesSetPiece(0.5f, 0.5f));
        Assert.AreEqual(0.02f, BossRetryRules.UniqueChance(0.08f, 0.25f), 1e-6f);
        Assert.IsTrue(BossRetryRules.CountsForPity(0.5f));
        Assert.IsFalse(BossRetryRules.CountsForPity(0.25f));
    }

    [Test]
    public void BountyTracker_LinesFollowState()
    {
        var def = new BountyDef { Id = "X1", Title = "테스트 의뢰", Count = 5, Targets = new[] { "A" } };
        var state = new BountyState { Active = "X1", Progress = 2 };
        string running = BountyRules.TrackerLines(state, def);
        StringAssert.Contains("테스트 의뢰", running);
        StringAssert.Contains("2/5", running);
        state.Progress = 5;
        StringAssert.Contains("보고", BountyRules.TrackerLines(state, def));
        Assert.AreEqual("", BountyRules.TrackerLines(new BountyState(), def));
        Assert.AreEqual("", BountyRules.TrackerLines(state, null));
    }

    [Test]
    public void Treasures_PositionsAreDistinctPerMap()
    {
        List<TreasureDef> defs = TreasureLedger.Parse(Resources.Load<TextAsset>("Floor1Treasures").text);
        Assert.AreEqual(8, defs.Count); // 6구역 + 신규 맵 2장
        var ids = new HashSet<string>();
        foreach (TreasureDef d in defs)
        {
            Assert.IsTrue(ids.Add(d.Id), d.Id);
        }
    }
}
