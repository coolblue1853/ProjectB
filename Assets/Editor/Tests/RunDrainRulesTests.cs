using NUnit.Framework;

public class RunDrainRulesTests
{
    [Test]
    public void Locked_NoDrain()
    {
        Assert.IsFalse(RunDrainRules.ShouldDrain(true, false, 1f));
        Assert.IsFalse(RunDrainRules.ShouldDrain(true, true, 1f));
    }

    [Test]
    public void NoInput_NoDrain()
    {
        Assert.IsFalse(RunDrainRules.ShouldDrain(false, false, 0f));
    }

    [Test]
    public void BelowThreshold_NoDrain()
    {
        Assert.IsFalse(RunDrainRules.ShouldDrain(false, false, 0.05f));
        Assert.IsTrue(RunDrainRules.ShouldDrain(false, false, RunDrainRules.MoveThreshold));
    }

    [Test]
    public void Moving_Drains()
    {
        Assert.IsTrue(RunDrainRules.ShouldDrain(false, false, -1f));
        Assert.IsTrue(RunDrainRules.ShouldDrain(false, true, 0f));
    }
}
