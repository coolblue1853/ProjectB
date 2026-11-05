using NUnit.Framework;

public class CounterBuffRulesTests
{
    [Test] public void Idle_NoBonus()
    {
        var r = new CounterBuffRules();
        Assert.IsFalse(r.Active);
        Assert.AreEqual(1f, r.DamageMultiplier);
    }

    [Test] public void Arm_GivesBonusFor3Seconds()
    {
        var r = new CounterBuffRules();
        r.Arm();
        Assert.AreEqual(1.2f, r.DamageMultiplier, 1e-4f);
        r.Tick(2.9f);
        Assert.IsTrue(r.Active);
        r.Tick(0.2f);
        Assert.IsFalse(r.Active);
        Assert.AreEqual(1f, r.DamageMultiplier);
    }

    [Test] public void ReArm_RefreshesWithoutStacking()
    {
        var r = new CounterBuffRules();
        r.Arm();
        r.Tick(2f);
        r.Arm();
        Assert.AreEqual(CounterBuffRules.Duration, r.Remaining, 1e-4f);
        Assert.AreEqual(1.2f, r.DamageMultiplier, 1e-4f);
    }

    [Test] public void Clear_Removes()
    {
        var r = new CounterBuffRules();
        r.Arm();
        r.Clear();
        Assert.IsFalse(r.Active);
    }
}
