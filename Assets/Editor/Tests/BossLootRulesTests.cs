using NUnit.Framework;

public class BossLootRulesTests
{
    [Test]
    public void BelowPity_UsesChance()
    {
        Assert.IsTrue(BossLootRules.ShouldDropUnique(3, 0.05f, 0.08f, 12));
        Assert.IsFalse(BossLootRules.ShouldDropUnique(3, 0.5f, 0.08f, 12));
    }

    [Test]
    public void AtPity_AlwaysDrops()
    {
        Assert.IsTrue(BossLootRules.ShouldDropUnique(12, 0.99f, 0.08f, 12));
    }

    [Test]
    public void ZeroPity_NoGuarantee()
    {
        Assert.IsFalse(BossLootRules.ShouldDropUnique(100, 0.99f, 0.08f, 0));
    }
}
