using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Floor1ExtraTests
{
    private static List<BountyDef> LoadBounties()
    {
        return BountyRules.Parse(Resources.Load<TextAsset>("Floor1Bounties").text);
    }

    [Test]
    public void Flags_ParseOnOff()
    {
        Dictionary<string, bool> f = Floor1Data.ParseFlags("id,enabled\na,1\nb,0\n");
        Assert.IsTrue(f["a"]);
        Assert.IsFalse(f["b"]);
    }

    [Test]
    public void Flags_ShippedTableHasAllFeatures()
    {
        Dictionary<string, bool> f = Floor1Data.ParseFlags(Resources.Load<TextAsset>("Floor1Content").text);
        foreach (string id in new[] { "treasure", "bestiary", "bounty", "chatter" })
        {
            Assert.IsTrue(f.ContainsKey(id), id);
        }
    }

    [Test]
    public void Bounties_ParseAndTargetsExistInMonsterSheet()
    {
        List<BountyDef> defs = LoadBounties();
        Assert.GreaterOrEqual(defs.Count, 12);
        string sheet = Resources.Load<TextAsset>("MonsterSheet").text;
        foreach (BountyDef d in defs)
        {
            Assert.Greater(d.Count, 0, d.Id);
            foreach (string t in d.Targets)
            {
                StringAssert.Contains("\n" + t + ",", sheet.Replace("\r", ""), d.Id + " 목표 몬스터 " + t);
            }
        }
    }

    [Test]
    public void Bounties_OffersAreStableThreeAndRespectLevel()
    {
        List<BountyDef> defs = LoadBounties();
        List<string> a = BountyRules.PickOffers(defs, 5, 8);
        List<string> b = BountyRules.PickOffers(defs, 5, 8);
        CollectionAssert.AreEqual(a, b);
        Assert.AreEqual(3, a.Count);
        foreach (string id in a)
        {
            Assert.LessOrEqual(BountyRules.Find(defs, id).MinLevel, 8, id);
        }
        Assert.AreEqual(2, BountyRules.PickOffers(defs, 1, 1).Count, "레벨 1 은 조건을 채운 2건뿐");
    }

    [Test]
    public void Bounties_AcceptKillReportFlow()
    {
        List<BountyDef> defs = LoadBounties();
        var s = new BountyState();
        BountyRules.NewDay(s, defs, 2, 15);
        string id = s.Offered[0];
        BountyDef d = BountyRules.Find(defs, id);
        Assert.IsFalse(BountyRules.OnKill(s, defs, d.Targets[0]), "수락 전 처치는 세지 않는다");
        Assert.IsTrue(BountyRules.Accept(s, id));
        Assert.IsFalse(BountyRules.Accept(s, s.Offered[1]), "동시에 하나만");
        Assert.IsFalse(BountyRules.OnKill(s, defs, "NotAMonster"));
        for (int i = 0; i < d.Count; i++)
        {
            Assert.IsFalse(BountyRules.IsReady(s, d));
            Assert.IsTrue(BountyRules.OnKill(s, defs, d.Targets[0]));
        }
        Assert.IsTrue(BountyRules.IsReady(s, d));
        Assert.IsFalse(BountyRules.OnKill(s, defs, d.Targets[0]), "목표 초과는 세지 않는다");
        BountyRules.Complete(s, id);
        Assert.AreEqual("완료", BountyRules.StatusOf(s, d));
        Assert.IsFalse(BountyRules.CanAccept(s, id), "같은 날 같은 의뢰는 한 번만");
        Assert.IsTrue(BountyRules.Accept(s, s.Offered[1]));
    }

    [Test]
    public void Bounties_NewDayResetsState()
    {
        List<BountyDef> defs = LoadBounties();
        var s = new BountyState();
        BountyRules.NewDay(s, defs, 1, 15);
        BountyRules.Accept(s, s.Offered[0]);
        BountyRules.NewDay(s, defs, 2, 15);
        Assert.AreEqual("", s.Active);
        Assert.AreEqual(0, s.Done.Count);
        Assert.AreEqual(2, s.Day);
    }

    [Test]
    public void Bounties_RewardsAreModest()
    {
        foreach (BountyDef d in LoadBounties())
        {
            Assert.LessOrEqual(d.Reward.Gold, 500, d.Id);
            Assert.LessOrEqual(d.Reward.Exp, 700, d.Id);
        }
    }

    [Test]
    public void Bestiary_MilestonesClaimInOrder()
    {
        List<BestiaryMilestoneDef> defs = BestiaryMilestoneRules.Parse(Resources.Load<TextAsset>("Floor1BestiaryRewards").text);
        Assert.AreEqual(13, defs.Count); // B01~B07 + 구역 전종 B08~B13 (11-11 이월 2차)
        var claimed = new HashSet<string>();
        Assert.IsNull(BestiaryMilestoneRules.NextClaimable(defs, claimed, 2, 99));
        BestiaryMilestoneDef first = BestiaryMilestoneRules.NextClaimable(defs, claimed, 7, 120);
        Assert.AreEqual("B01", first.Id);
        claimed.Add(first.Id);
        Assert.AreEqual("B02", BestiaryMilestoneRules.NextClaimable(defs, claimed, 7, 120).Id);
        claimed.Add("B02");
        Assert.AreEqual("B05", BestiaryMilestoneRules.NextClaimable(defs, claimed, 7, 120).Id);
    }

    private static HashSet<string> ItemNames()
    {
        var set = new HashSet<string>();
        foreach (string line in Resources.Load<TextAsset>("ItemSheet").text.Replace(((char)13).ToString(), "").Split((char)10))
        {
            string[] c = line.Split(',');
            if (c.Length > 2) { set.Add(c[1].Trim()); }
        }
        return set;
    }

    [Test]
    public void Bestiary_ZoneRows_B08toB13_HaveSpeciesAndRealItems()
    {
        List<BestiaryMilestoneDef> defs = BestiaryMilestoneRules.Parse(Resources.Load<TextAsset>("Floor1BestiaryRewards").text);
        HashSet<string> items = ItemNames();
        for (int n = 8; n <= 13; n++)
        {
            string id = "B" + n.ToString("00");
            BestiaryMilestoneDef d = defs.Find(x => x.Id == id);
            Assert.IsNotNull(d, id);
            Assert.AreEqual("zone", d.Metric, id);
            Assert.GreaterOrEqual(BestiaryZoneRules.SpeciesOf(d.Zone).Count, 4, id + " 구역 종 수 " + d.Zone);
            Assert.IsTrue(items.Contains(d.Reward.Item), id + " 보상 아이템 실존 " + d.Reward.Item);
            Assert.LessOrEqual(d.Reward.Exp, 700, id);
        }
    }

    [Test]
    public void Bestiary_ZoneCleared_NeedsEverySpecies()
    {
        var species = new List<string> { "A", "B", "C" };
        Assert.IsFalse(BestiaryZoneRules.Cleared(species, id => id == "C" ? 0 : 3));
        Assert.IsTrue(BestiaryZoneRules.Cleared(species, id => 1));
        Assert.IsFalse(BestiaryZoneRules.Cleared(new List<string>(), id => 9), "빈 종 목록은 달성 아님");
        List<BestiaryMilestoneDef> defs = BestiaryMilestoneRules.Parse(Resources.Load<TextAsset>("Floor1BestiaryRewards").text);
        var claimed = new HashSet<string> { "B01", "B02", "B03", "B04", "B05", "B06", "B07" };
        Assert.IsNull(BestiaryMilestoneRules.NextClaimable(defs, claimed, 99, 99999), "zone 단계는 판정자 없이 열리지 않는다");
        Assert.AreEqual("B10", BestiaryMilestoneRules.NextClaimable(defs, claimed, 99, 99999, z => z == "Deep Forest").Id);
    }

    [Test]
    public void Bounties_NewMinibossAndEliteTargets_ExistInMonsterSheet()
    {
        var ids = new HashSet<string>();
        foreach (MonsterTable.Row r in MonsterTable.All()) { ids.Add(r.id); }
        List<BountyDef> defs = LoadBounties();
        foreach (string id in new[] { "D18", "D19", "D20", "D21", "D22", "D23" })
        {
            BountyDef d = BountyRules.Find(defs, id);
            Assert.IsNotNull(d, id);
            foreach (string t in d.Targets) { Assert.IsTrue(ids.Contains(t), id + " 대상 " + t); }
        }
        Assert.AreEqual(23, defs.Count);
    }

    [Test]
    public void Treasures_ParseAndMapsAreFloor1()
    {
        List<TreasureDef> defs = TreasureLedger.Parse(Resources.Load<TextAsset>("Floor1Treasures").text);
        Assert.AreEqual(8, defs.Count); // 6구역 + 신규 맵 2장
        var ids = new HashSet<string>();
        foreach (TreasureDef d in defs)
        {
            Assert.IsTrue(ids.Add(d.Id), "중복 id " + d.Id);
            StringAssert.StartsWith("Lv1_", d.Map);
            Assert.Greater(d.Reward.Gold + d.Reward.Exp + d.Reward.Count, 0, d.Id);
        }
    }
}
