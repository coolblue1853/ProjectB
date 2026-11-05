using NUnit.Framework;

public class GuardStateTests
{
    [Test] public void NotHeld_PassThrough()
    {
        var g = new GuardState();
        var o = g.Resolve(100);
        Assert.IsFalse(o.Blocked);
        Assert.AreEqual(100, o.Taken);
    }

    [Test] public void ParryWindow_Boundary()
    {
        var g = new GuardState();
        g.Press();
        g.Tick(GuardState.ParryWindow - 0.01f);
        Assert.IsTrue(g.Resolve(100).Parried);
        g.Tick(0.02f);
        var o = g.Resolve(100);
        Assert.IsFalse(o.Parried);
        Assert.AreEqual(30, o.Taken);
    }

    [Test] public void Parry_ZeroDamage()
    {
        var g = new GuardState();
        g.Press();
        Assert.AreEqual(0, g.Resolve(100).Taken);
    }

    [Test] public void Reflect_RatioOfBlocked()
    {
        var g = new GuardState(0.7f, 0.5f);
        g.Press();
        g.Tick(0.3f);
        var o = g.Resolve(100); // 막은 70 의 50%
        Assert.AreEqual(35, o.Reflected);
        Assert.AreEqual(30, o.Taken);
    }

    [Test] public void NoReflect_ByDefault()
    {
        var g = new GuardState();
        g.Press(); g.Tick(0.3f);
        Assert.AreEqual(0, g.Resolve(100).Reflected);
    }

    [Test] public void Barrier_BreaksAfterDuration_ReleaseRestores()
    {
        var g = new GuardState();
        g.Press();
        g.Tick(GuardState.BarrierDuration + 0.1f);
        Assert.IsFalse(g.Holding);
        Assert.IsFalse(g.Resolve(100).Blocked);
        g.Release();
        g.Press();
        Assert.IsTrue(g.Holding);
    }
}
