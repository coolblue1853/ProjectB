using NUnit.Framework;

public class CountdownTimerTests
{
    [Test] public void Idle_NotActive()
    {
        var t = new CountdownTimer();
        Assert.IsFalse(t.Active);
        Assert.AreEqual(0f, t.Remaining);
    }

    [Test] public void Start_Tick_ExpiresAtZero_NoNegative()
    {
        var t = new CountdownTimer();
        t.Start(1f);
        t.Tick(0.9f);
        Assert.IsTrue(t.Active);
        t.Tick(0.2f);
        Assert.IsFalse(t.Active);
        Assert.AreEqual(0f, t.Remaining);
        t.Tick(5f);
        Assert.AreEqual(0f, t.Remaining);
    }

    [Test] public void Tick_ExactBoundary_Inactive()
    {
        var t = new CountdownTimer();
        t.Start(1f);
        t.Tick(1f);
        Assert.IsFalse(t.Active);
    }

    [Test] public void Extend_KeepsLarger()
    {
        var t = new CountdownTimer();
        t.Start(3f);
        t.Extend(1f);
        Assert.AreEqual(3f, t.Remaining, 1e-4f);
        t.Extend(5f);
        Assert.AreEqual(5f, t.Remaining, 1e-4f);
    }

    [Test] public void Start_Overwrites_EvenIfSmaller()
    {
        var t = new CountdownTimer();
        t.Start(3f);
        t.Start(1f);
        Assert.AreEqual(1f, t.Remaining, 1e-4f);
    }

    [Test] public void Clear_Stops()
    {
        var t = new CountdownTimer();
        t.Start(2f);
        t.Clear();
        Assert.IsFalse(t.Active);
    }

    [Test] public void Start_NegativeOrZero_RemainingNotNegative()
    {
        var t = new CountdownTimer();
        t.Start(-2f);
        Assert.AreEqual(0f, t.Remaining, 1e-4f);
        Assert.IsFalse(t.Active);
        t.Start(0f);
        Assert.AreEqual(0f, t.Remaining, 1e-4f);
        t.Start(2f);
        Assert.AreEqual(2f, t.Remaining, 1e-4f);
    }
}
