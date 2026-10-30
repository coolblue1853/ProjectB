using NUnit.Framework;

public class NestHuntRulesTests
{
    [Test]
    public void Hint_DirectionAndFiveMeterRounding()
    {
        NestHuntRules.Hint(-300f, -355f, out int dir, out int m);
        Assert.AreEqual(-1, dir);
        Assert.AreEqual(55, m);
        NestHuntRules.Hint(-300f, -267.4f, out dir, out m);
        Assert.AreEqual(1, dir);
        Assert.AreEqual(35, m);
        NestHuntRules.Hint(-300f, -293.1f, out dir, out m);
        Assert.AreEqual(1, dir);
        Assert.AreEqual(5, m); // 최소 5m
    }

    [Test]
    public void Hint_WithinHereRangeIsZero()
    {
        NestHuntRules.Hint(-300f, -302f, out int dir, out int m);
        Assert.AreEqual(0, dir);
        Assert.AreEqual(0, m);
    }

    [Test]
    public void State_OnlyNextMarkInOrder()
    {
        var s = new NestHuntState();
        Assert.IsTrue(s.CanInvestigate(0));
        Assert.IsFalse(s.CanInvestigate(1));
        Assert.IsFalse(s.Investigate(2));
        Assert.IsFalse(s.Investigate(3));
        Assert.AreEqual(NestPhase.Idle, s.Phase);
        Assert.IsTrue(s.Investigate(0));
        Assert.AreEqual(NestPhase.Hunting, s.Phase);
        Assert.IsFalse(s.Investigate(0)); // 같은 표식 두 번 불가
        Assert.IsFalse(s.Investigate(2));
        Assert.IsTrue(s.Investigate(1));
        Assert.IsTrue(s.Investigate(2));
        Assert.AreEqual(NestPhase.NestRevealed, s.Phase);
        Assert.IsTrue(s.Investigate(3));
        Assert.AreEqual(NestPhase.Success, s.Phase);
        Assert.IsTrue(s.Finished);
        Assert.IsFalse(s.CanInvestigate(0));
    }

    [Test]
    public void State_ClockStartsAtFirstTrail()
    {
        var s = new NestHuntState();
        s.Tick(100f);
        Assert.AreEqual(NestPhase.Idle, s.Phase);
        Assert.AreEqual(0f, s.Elapsed);
        s.Investigate(0);
        s.Tick(NestHuntRules.TimeLimit - 1f);
        Assert.AreEqual(NestPhase.Hunting, s.Phase);
        Assert.AreEqual(1f, s.Remaining, 0.001f);
    }

    [Test]
    public void State_TimeoutBlocksFurtherProgress()
    {
        var s = new NestHuntState();
        s.Investigate(0);
        s.Investigate(1);
        s.Tick(NestHuntRules.TimeLimit);
        Assert.AreEqual(NestPhase.TimedOut, s.Phase);
        Assert.IsFalse(s.Investigate(2));
        Assert.AreEqual(0f, s.Remaining);
    }

    [Test]
    public void State_TimeoutAlsoWhileNestRevealed()
    {
        var s = new NestHuntState();
        s.Investigate(0);
        s.Investigate(1);
        s.Investigate(2);
        s.Tick(NestHuntRules.TimeLimit + 0.1f);
        Assert.AreEqual(NestPhase.TimedOut, s.Phase);
        Assert.IsFalse(s.Investigate(3));
    }

    [Test]
    public void State_CancelOnlyWhileRunning()
    {
        var s = new NestHuntState();
        s.Cancel();
        Assert.AreEqual(NestPhase.Idle, s.Phase);
        s.Investigate(0);
        s.Cancel();
        Assert.AreEqual(NestPhase.Cancelled, s.Phase);
        Assert.IsFalse(s.CanInvestigate(1));
    }

    [Test]
    public void Numbers_MatchSpec()
    {
        Assert.AreEqual(3, NestHuntRules.Trails);
        Assert.That(NestHuntRules.TimeLimit, Is.InRange(40f, 60f));
        Assert.AreEqual(600f, NestHuntRules.SuccessCooldown);
        Assert.AreEqual(180f, NestHuntRules.FailCooldown);
    }
}
