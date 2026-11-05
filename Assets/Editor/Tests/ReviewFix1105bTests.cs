using NUnit.Framework;

public class ReviewFix1105bTests
{
    [Test] public void BlockText_NotShownWhenGuarded()
    {
        Assert.IsFalse(GuardState.ShowBlockText(true, 0));
        Assert.IsTrue(GuardState.ShowBlockText(false, 0));
        Assert.IsFalse(GuardState.ShowBlockText(false, 5));
    }
}
