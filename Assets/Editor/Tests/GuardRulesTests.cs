using NUnit.Framework;

public class GuardRulesTests
{
    private sealed class FakeSource : IGuardSource
    {
        public bool Called;
        public bool Intercept(ref int damage, out bool parried) { parried = false; Called = true; damage = GuardRules.Resolve(damage, true); return true; }
    }

    [TearDown] public void Cleanup() { PlayerGuard.Source = null; }

    [Test] public void NotHolding_NoChange() { Assert.AreEqual(100, GuardRules.Resolve(100, false)); }
    [Test] public void Holding_Reduces70() { Assert.AreEqual(30, GuardRules.Resolve(100, true)); }
    [Test] public void Holding_MinOne() { Assert.AreEqual(1, GuardRules.Resolve(1, true)); }
    [Test] public void ZeroDamage_Stays() { Assert.AreEqual(0, GuardRules.Resolve(0, true)); }

    [Test] public void Hook_NoSource_NoIntercept()
    {
        int d = 50;
        Assert.IsFalse(PlayerGuard.TryIntercept(ref d));
        Assert.AreEqual(50, d);
    }

    [Test] public void Hook_WithSource_ReducesAndReportsBlocked()
    {
        var s = new FakeSource();
        PlayerGuard.Source = s;
        int d = 100;
        Assert.IsTrue(PlayerGuard.TryIntercept(ref d));
        Assert.IsTrue(s.Called);
        Assert.AreEqual(30, d);
    }
}
