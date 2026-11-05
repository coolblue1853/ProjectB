using NUnit.Framework;

public class GroundZoneRulesTests
{
    [Test] public void NoTickBeforeFirstDelay()
    {
        var z = new GroundZoneRules(3f, 0.5f);
        Assert.AreEqual(0, z.Step(0.1f));
        Assert.AreEqual(1, z.Step(0.15f)); // 0.25 >= 0.2
    }

    [Test] public void TickCount_OverFullDuration()
    {
        var z = new GroundZoneRules(3f, 0.5f);
        int total = 0;
        for (int i = 0; i < 310; i++) { total += z.Step(0.01f); }
        Assert.IsTrue(z.Expired);
        Assert.AreEqual(6, total); // 0.2,0.7,1.2,1.7,2.2,2.7
    }

    [Test] public void BigStep_ReturnsMultipleTicks()
    {
        var z = new GroundZoneRules(3f, 0.5f);
        Assert.AreEqual(3, z.Step(1.3f)); // 0.2,0.7,1.2
    }

    [Test] public void Expired_NoMoreTicks()
    {
        var z = new GroundZoneRules(1f, 0.5f);
        z.Step(5f);
        Assert.IsTrue(z.Expired);
        Assert.AreEqual(0, z.Step(1f));
        Assert.AreEqual(0f, z.RemainingRatio);
    }
}
