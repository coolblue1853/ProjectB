using NUnit.Framework;

public class BreathRulesTests
{
    static BreathRules Charged()
    {
        var b = new BreathRules();
        Assert.IsTrue(b.TryStart());
        return b;
    }

    [Test]
    public void Charging_NoDamage_ThenBreathing()
    {
        BreathRules b = Charged();
        Assert.AreEqual(0, b.Step(BreathRules.ChargeSeconds - 0.01f));
        Assert.AreEqual(BreathRules.Phase.Charging, b.Current);
        Assert.IsFalse(b.Dealing);
        b.Step(0.01f);
        Assert.AreEqual(BreathRules.Phase.Breathing, b.Current);
        Assert.IsTrue(b.Dealing);
    }

    [Test]
    public void TotalTicks_MatchGroundZone()
    {
        BreathRules b = Charged();
        int total = b.Step(BreathRules.ChargeSeconds + BreathRules.ActiveSeconds + 0.001f);
        var z = new GroundZoneRules(BreathRules.ActiveSeconds, BreathRules.TickInterval);
        int expect = z.Step(BreathRules.ActiveSeconds);
        Assert.Greater(expect, 0);
        Assert.AreEqual(expect, total);
        Assert.AreEqual(BreathRules.Phase.Cooldown, b.Current);
    }

    [Test]
    public void TickSplitAcrossSteps_SameTotal()
    {
        BreathRules b = Charged();
        int total = 0;
        for (int i = 0; i < 100; i++)
        {
            total += b.Step(0.05f);
        }
        var z = new GroundZoneRules(BreathRules.ActiveSeconds, BreathRules.TickInterval);
        Assert.AreEqual(z.Step(BreathRules.ActiveSeconds), total);
    }

    [Test]
    public void Cooldown_Boundary_ThenReady()
    {
        BreathRules b = Charged();
        b.Step(BreathRules.ChargeSeconds + BreathRules.ActiveSeconds + 0.001f);
        Assert.IsFalse(b.TryStart());
        b.Step(BreathRules.CooldownSeconds - 0.1f);
        Assert.IsFalse(b.CanStart);
        b.Step(0.1f);
        Assert.IsTrue(b.CanStart);
        Assert.IsTrue(b.TryStart());
    }

    [Test]
    public void NoStartWhileBusy()
    {
        BreathRules b = Charged();
        Assert.IsFalse(b.TryStart());
    }

    [Test]
    public void Guard_ReducesTickDamage_MinOne()
    {
        Assert.AreEqual(100, BreathRules.TickDamage(100, false));
        Assert.AreEqual(GuardRules.Resolve(100, true), BreathRules.TickDamage(100, true));
        Assert.Less(BreathRules.TickDamage(100, true), 100);
        Assert.AreEqual(1, BreathRules.TickDamage(1, true));
    }
}
