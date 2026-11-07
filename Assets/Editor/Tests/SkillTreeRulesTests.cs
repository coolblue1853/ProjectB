using NUnit.Framework;

public class SkillTreeRulesTests
{
    const string Sw = "Sword";

    [Test]
    public void Capacity_TypeIs32CommonIs60()
    {
        Assert.AreEqual(32, SkillTreeRules.Capacity(false));
        Assert.AreEqual(60, SkillTreeRules.Capacity(true));
    }

    [Test]
    public void NodeIds_AreUnique()
    {
        var seen = new System.Collections.Generic.HashSet<string>();
        foreach (SkillNode n in SkillTreeRules.Nodes)
        {
            Assert.IsTrue(seen.Add(n.Id), n.Id);
        }
    }

    [Test]
    public void CommonEarned_IsLevelMinusOne()
    {
        Assert.AreEqual(0, SkillTreeRules.CommonEarnedPoints(1));
        Assert.AreEqual(14, SkillTreeRules.CommonEarnedPoints(15));
        Assert.AreEqual(69, SkillTreeRules.CommonEarnedPoints(70));
    }

    [Test]
    public void Invest_PrereqAndMaxAndPoints()
    {
        var d = new SkillTreeData();
        Assert.AreEqual(InvestResult.PrereqUnmet, d.Invest(Sw, "P2", 9));
        for (int i = 0; i < 5; i++)
        {
            Assert.AreEqual(InvestResult.Ok, d.Invest(Sw, "P1", 9));
        }
        Assert.AreEqual(InvestResult.MaxLevel, d.Invest(Sw, "P1", 9));
        Assert.AreEqual(InvestResult.Ok, d.Invest(Sw, "P2", 9));
        Assert.AreEqual(6, d.Spent(Sw));
        Assert.AreEqual(InvestResult.Ok, d.Invest(Sw, "P2", 9));
        Assert.AreEqual(InvestResult.NoPoints, d.Invest(Sw, "P3", 9)); // P2 Lv2 선행은 충족, 남은 2 < 비용 3
    }

    [Test]
    public void Invest_UltimateCostsThreeAndNeedsP2Lv2()
    {
        var d = new SkillTreeData();
        for (int i = 0; i < 3; i++)
        {
            d.Invest(Sw, "P1", 20);
        }
        d.Invest(Sw, "P2", 20);
        Assert.AreEqual(InvestResult.PrereqUnmet, d.Invest(Sw, "P3", 20));
        d.Invest(Sw, "P2", 20);
        Assert.AreEqual(5, d.Spent(Sw));
        Assert.AreEqual(InvestResult.NoPoints, d.Invest(Sw, "P3", 7)); // 남은 2 < 비용 3
        Assert.AreEqual(InvestResult.Ok, d.Invest(Sw, "P3", 8));
        Assert.AreEqual(8, d.Spent(Sw));
    }

    [Test]
    public void Invest_RejectsWrongTreeAndUnknown()
    {
        var d = new SkillTreeData();
        Assert.AreEqual(InvestResult.UnknownNode, d.Invest(SkillTreeRules.CommonTree, "P1", 5));
        Assert.AreEqual(InvestResult.UnknownNode, d.Invest(Sw, "C1", 5));
        Assert.AreEqual(InvestResult.UnknownNode, d.Invest(Sw, "ZZ", 5));
        Assert.AreEqual(InvestResult.UnknownNode, d.Invest("None", "P1", 5));
        Assert.AreEqual(InvestResult.UnknownNode, d.Invest("Nope", "P1", 5));
        Assert.AreEqual(InvestResult.NoPoints, d.Invest(SkillTreeRules.CommonTree, "C1", 0));
    }

    [Test]
    public void Respec_FirstFreeThenCostScalesAndCaps()
    {
        Assert.AreEqual(0, SkillTreeRules.RespecCost(10, 0));
        Assert.AreEqual(1500, SkillTreeRules.RespecCost(10, 1));
        Assert.AreEqual(3000, SkillTreeRules.RespecCost(10, 2));
        Assert.AreEqual(7500, SkillTreeRules.RespecCost(10, 5));
        Assert.AreEqual(7500, SkillTreeRules.RespecCost(10, 99));
        Assert.AreEqual(0, SkillTreeRules.RespecCost(0, 3));
    }

    [Test]
    public void Respec_RefundsAllAndKeepsOtherTrees()
    {
        var d = new SkillTreeData();
        d.Invest(Sw, "P1", 9);
        d.Invest(Sw, "A1", 9);
        d.Invest("Bow", "P1", 9);
        Assert.AreEqual(0, d.RespecCostNow(Sw));
        Assert.AreEqual(2, d.ClearForRespec(Sw));
        Assert.AreEqual(0, d.Spent(Sw));
        Assert.AreEqual(1, d.Spent("Bow"));
        Assert.AreEqual(1, d.RespecCount(Sw));
        d.Invest(Sw, "P1", 9);
        Assert.AreEqual(150, d.RespecCostNow(Sw));
        Assert.AreEqual(0, d.ClearForRespec("Axe")); // 투자 없음 = 횟수 증가 없음
        Assert.AreEqual(0, d.RespecCount("Axe"));
    }

    [Test]
    public void FixOverspent_ClearsOnlyBadTreesFree()
    {
        var d = new SkillTreeData();
        d.Invest(Sw, "P1", 9);
        d.Invest(Sw, "P1", 9);
        d.Invest("Bow", "P1", 9);
        int fixedCount = d.FixOverspent(t => t == Sw ? 1 : 9);
        Assert.AreEqual(1, fixedCount);
        Assert.AreEqual(0, d.Spent(Sw));
        Assert.AreEqual(1, d.Spent("Bow"));
        Assert.AreEqual(0, d.RespecCount(Sw));
    }

    [Test]
    public void Mod_TypeOnlyForGivenKindAndCommonAlways()
    {
        var d = new SkillTreeData();
        for (int i = 0; i < 3; i++)
        {
            d.Invest(Sw, "P1", 20);
        }
        d.Invest(SkillTreeRules.CommonTree, "C1", 20);
        d.Invest("Bow", "A1", 20);
        Assert.AreEqual(6f, d.Mod(TreeStat.AttackDamage, Sw), 1e-4f);
        Assert.AreEqual(0f, d.Mod(TreeStat.AttackDamage, "Bow"), 1e-4f);
        Assert.AreEqual(0f, d.Mod(TreeStat.AttackDamage, null), 1e-4f);
        Assert.AreEqual(0.5f, d.Mod(TreeStat.CritChance, Sw), 1e-4f);
        Assert.AreEqual(1.0f, d.Mod(TreeStat.CritChance, "Bow"), 1e-4f);
        Assert.AreEqual(0.5f, d.Mod(TreeStat.CritChance, null), 1e-4f);
    }

    [Test]
    public void Mod_ClampsToCap()
    {
        var d = new SkillTreeData();
        for (int i = 0; i < 10; i++)
        {
            d.Invest(SkillTreeRules.CommonTree, "C1", 99); // +5%p
        }
        for (int i = 0; i < 5; i++)
        {
            d.Invest(Sw, "A1", 99); // +2.5%p
        }
        Assert.AreEqual(7.5f, d.Mod(TreeStat.CritChance, Sw), 1e-4f);
        Assert.AreEqual(10f, SkillTreeRules.Cap(TreeStat.CritChance));
        Assert.AreEqual(10f, SkillTreeRules.Clamp(TreeStat.CritChance, 99f));
        Assert.AreEqual(0f, SkillTreeRules.Clamp(TreeStat.CritChance, -3f));
        float a = 20f;
        float s = 20f;
        SkillTreeRules.ClampDamagePair(ref a, ref s);
        Assert.AreEqual(15f, a, 1e-4f);
        Assert.AreEqual(15f, s, 1e-4f);
        float a2 = 10f;
        float s2 = 9f;
        SkillTreeRules.ClampDamagePair(ref a2, ref s2);
        Assert.AreEqual(10f, a2);
    }

    [Test]
    public void Mod_UltimateOnlyFromTypeTree()
    {
        var d = new SkillTreeData();
        for (int i = 0; i < 3; i++)
        {
            d.Invest(Sw, "P1", 99);
        }
        d.Invest(Sw, "P2", 99);
        d.Invest(Sw, "P2", 99);
        d.Invest(Sw, "P3", 99);
        Assert.AreEqual(1f, d.Mod(TreeStat.Ultimate, Sw));
        Assert.AreEqual(0f, d.Mod(TreeStat.Ultimate, "Bow"));
        Assert.AreEqual(0f, d.Mod(TreeStat.Ultimate, null));
    }

    [Test]
    public void Save_RoundTripAndUnknownNodesDropped()
    {
        var d = new SkillTreeData();
        d.Invest(Sw, "P1", 9);
        d.Invest(SkillTreeRules.CommonTree, "C3", 9);
        d.ClearForRespec(Sw);
        d.Invest(Sw, "A1", 9);
        var d2 = new SkillTreeData();
        d2.LoadFrom(d.ToSave());
        Assert.AreEqual(1, d2.GetLevel(Sw, "A1"));
        Assert.AreEqual(0, d2.GetLevel(Sw, "P1"));
        Assert.AreEqual(1, d2.GetLevel(SkillTreeRules.CommonTree, "C3"));
        Assert.AreEqual(1, d2.RespecCount(Sw));
        var d3 = new SkillTreeData();
        d3.LoadFrom(new SkillTreeSave
        {
            nodeIds = new[] { "Sword.P1", "Sword.GONE", "Ghost.P1", "C1", "Sword.C1", "Sword.P1x" },
            levels = new[] { 99, 3, 3, 4, 2, 2 },
        });
        Assert.AreEqual(5, d3.GetLevel(Sw, "P1")); // 최대치로 자름
        Assert.AreEqual(4, d3.GetLevel(SkillTreeRules.CommonTree, "C1"));
        Assert.AreEqual(5, d3.Spent(Sw));
        d3.LoadFrom(null);
        Assert.AreEqual(0, d3.Spent(Sw));
    }
}
