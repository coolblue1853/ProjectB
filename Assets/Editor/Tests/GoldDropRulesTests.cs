using NUnit.Framework;

/// <summary>몹 골드 드롭 규칙 (ClassicMapleDesign.md 14절)</summary>
public class GoldDropRulesTests
{
    [TestCase(1, 0f, 5)]
    [TestCase(1, 1f, 7)]
    [TestCase(3, 0.5f, 14)]
    [TestCase(10, 1f, 52)]
    public void Total_Normal(int lv, float t, int expect)
    {
        Assert.AreEqual(expect, GoldDropRules.Total(lv, false, false, t));
    }

    [Test]
    public void Total_EliteAndBossMultiply()
    {
        Assert.AreEqual(14 * 3, GoldDropRules.Total(3, true, false, 0.5f));
        Assert.AreEqual(14 * 10, GoldDropRules.Total(3, false, true, 0.5f));
        Assert.AreEqual(14 * 10, GoldDropRules.Total(3, true, true, 0.5f)); // 보스는 정예 배율 없음
        Assert.AreEqual(0, GoldDropRules.Total(0, false, false, 0.5f));
    }

    [Test]
    public void ShouldDrop_Chance()
    {
        Assert.IsTrue(GoldDropRules.ShouldDrop(false, false, 0.59f));
        Assert.IsFalse(GoldDropRules.ShouldDrop(false, false, 0.6f));
        Assert.IsTrue(GoldDropRules.ShouldDrop(true, false, 0.99f));
        Assert.IsTrue(GoldDropRules.ShouldDrop(false, true, 0.99f));
    }

    [TestCase(100, 3)]
    [TestCase(101, 3)]
    [TestCase(7, 2)]
    [TestCase(5, 1)]
    public void Share_PreservesTotal(int total, int piles)
    {
        int sum = 0;
        for (int i = 0; i < piles; i++)
        {
            sum += GoldDropRules.Share(total, piles, i);
        }
        Assert.AreEqual(total, sum);
    }

    [TestCase(1, 0)]
    [TestCase(19, 0)]
    [TestCase(20, 1)]
    [TestCase(59, 1)]
    [TestCase(60, 2)]
    public void Tier_Thresholds(int amount, int tier)
    {
        Assert.AreEqual(tier, GoldDropRules.Tier(amount));
    }

    [Test]
    public void Sprites_ThreeTiers()
    {
        for (int t = 0; t < 3; t++)
        {
            Assert.IsNotNull(GoldSprites.Get(t));
        }
    }
}
