using NUnit.Framework;

public class MobDebuffRulesTests
{
    [Test] public void Slow_ExpiresAfterDuration()
    {
        var d = new MobDebuffRules();
        Assert.AreEqual(1f, d.SlowMul);
        d.ApplySlow(0.5f, 2f);
        Assert.AreEqual(0.5f, d.SlowMul, 1e-4f);
        d.Tick(1.9f);
        Assert.IsTrue(d.Slowed);
        d.Tick(0.2f);
        Assert.AreEqual(1f, d.SlowMul);
    }

    [Test] public void Slow_KeepsStrongestAndLongest()
    {
        var d = new MobDebuffRules();
        d.ApplySlow(0.5f, 2f);
        d.ApplySlow(0.8f, 5f);
        Assert.AreEqual(0.5f, d.SlowMul, 1e-4f);
        d.Tick(3f);
        Assert.IsTrue(d.Slowed);
    }

    [Test] public void Petrify_ChainLimit()
    {
        var d = new MobDebuffRules();
        Assert.AreEqual(2f, d.TryPetrify(2f, 0f, 0f), 1e-4f);
        Assert.AreEqual(0f, d.TryPetrify(2f, 0f, 1f));   // 경직 중
        Assert.AreEqual(0f, d.TryPetrify(2f, 0f, 4.9f)); // 면역(끝 2 + 3 = 5)
        Assert.AreEqual(2f, d.TryPetrify(2f, 0f, 5.1f), 1e-4f);
    }

    [Test] public void Petrify_ResistShortens_FullResistBlocks()
    {
        var d = new MobDebuffRules();
        Assert.AreEqual(1.2f, d.TryPetrify(2f, 0.4f, 0f), 1e-4f);
        Assert.AreEqual(0f, new MobDebuffRules().TryPetrify(2f, 1f, 0f));
    }

    [Test] public void Burn_DamageMinOne()
    {
        Assert.AreEqual(1, MobDebuffRules.BurnDamage(2));
        Assert.AreEqual(15, MobDebuffRules.BurnDamage(100));
    }

    [Test] public void Clear_ResetsAll()
    {
        var d = new MobDebuffRules();
        d.ApplySlow(0.5f, 5f);
        d.TryPetrify(2f, 0f, 0f);
        d.Clear();
        Assert.AreEqual(1f, d.SlowMul);
        Assert.AreEqual(2f, d.TryPetrify(2f, 0f, 0.1f), 1e-4f);
    }
}
