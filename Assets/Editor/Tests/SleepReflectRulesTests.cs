using NUnit.Framework;

public class SleepReflectRulesTests
{
    [Test] public void Sleep_LongerThanStiff_MinClamp()
    {
        var s = new SleepRules();
        Assert.IsTrue(s.TryApply(1f));
        Assert.AreEqual(SleepRules.MinSeconds, s.Remaining, 1e-4f);
    }

    [Test] public void Sleep_MaxClamp_NoReapply()
    {
        var s = new SleepRules();
        s.TryApply(99f);
        Assert.AreEqual(SleepRules.MaxSeconds, s.Remaining, 1e-4f);
        Assert.IsFalse(s.TryApply(3f));
    }

    [Test] public void Sleep_HitWithinGrace_Ignored_AfterGrace_Wakes()
    {
        var s = new SleepRules();
        s.TryApply(3f);
        s.Tick(0.1f);
        Assert.IsFalse(s.Hit());
        Assert.IsTrue(s.Sleeping);
        s.Tick(0.3f);
        Assert.IsTrue(s.Hit());
        Assert.IsFalse(s.Sleeping);
    }

    [Test] public void Sleep_ExpiresByTime()
    {
        var s = new SleepRules();
        s.TryApply(3f);
        Assert.IsFalse(s.Tick(2.9f));
        Assert.IsTrue(s.Tick(0.2f));
        Assert.IsFalse(s.Sleeping);
    }

    [Test] public void Reflect_OnlyInPosture()
    {
        var r = new MobReflectRules();
        Assert.AreEqual(0, r.Reflect(100));
        r.Begin(2f);
        Assert.AreEqual(30, r.Reflect(100));
        Assert.AreEqual(1, r.Reflect(1));
        r.Tick(2.1f);
        Assert.AreEqual(0, r.Reflect(100));
    }
}
