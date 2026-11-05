using NUnit.Framework;

public class CooldownTests
{
    [Test]
    public void Default_ReadyFromTimeZero()
    {
        Cooldown c = default;
        Assert.IsTrue(c.Ready(0f));
        Assert.IsTrue(c.Ready(5f));
    }

    [Test]
    public void Arm_NotReadyBeforeInterval()
    {
        Cooldown c = default;
        c.Arm(10f, 2f);
        Assert.IsFalse(c.Ready(10f));
        Assert.IsFalse(c.Ready(11.99f));
    }

    [Test]
    public void Ready_AtExactBoundary()
    {
        Cooldown c = default;
        c.Arm(10f, 2f);
        Assert.IsTrue(c.Ready(12f));
        Assert.IsTrue(c.Ready(12.01f));
    }

    [Test]
    public void Arm_RearmsFromNow()
    {
        Cooldown c = default;
        c.Arm(0f, 1f);
        c.Arm(5f, 3f);
        Assert.AreEqual(8f, c.ReadyAt, 1e-4f);
        Assert.IsFalse(c.Ready(7.9f));
    }

    [Test]
    public void NightRaid_TimedOut_NotBeforeActive()
    {
        Cooldown c = default;
        Assert.IsFalse(NightRaidRules.TimedOut(false, c, 100f));
        Assert.IsTrue(NightRaidRules.TimedOut(true, c, 100f));
        c.Arm(10f, 40f);
        Assert.IsFalse(NightRaidRules.TimedOut(true, c, 49f));
        Assert.IsTrue(NightRaidRules.TimedOut(true, c, 50f));
    }
}
