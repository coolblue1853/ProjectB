using NUnit.Framework;

public class PadChordRulesTests
{
    [Test]
    public void RtAlone_IgnoredWhileLt()
    {
        Assert.IsTrue(PadChordRules.RtAlone(false, true));
        Assert.IsFalse(PadChordRules.RtAlone(true, true));
        Assert.IsFalse(PadChordRules.RtAlone(false, false));
    }

    [Test]
    public void FilterChord_NeedsLtAndRt()
    {
        Assert.IsTrue(PadChordRules.FilterChord(true, true));
        Assert.IsFalse(PadChordRules.FilterChord(false, true));
        Assert.IsFalse(PadChordRules.FilterChord(true, false));
    }

    [Test]
    public void LtRt_FiresFilterOnly_NeverRtAlone()
    {
        Assert.IsTrue(PadChordRules.FilterChord(true, true) && !PadChordRules.RtAlone(true, true));
    }

    [Test]
    public void LtR3_IsLockOnly_FilterNotTriggered()
    {
        // LT+R3 는 잠금 한 곳만: 필터는 RT 를 읽으므로 R3 만 눌린 프레임에 발동하지 않는다
        Assert.IsTrue(PadChordRules.LockChord(true, true));
        Assert.IsFalse(PadChordRules.FilterChord(true, false));
    }
}
