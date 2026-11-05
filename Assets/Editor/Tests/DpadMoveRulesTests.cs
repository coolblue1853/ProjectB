using NUnit.Framework;

public class DpadMoveRulesTests
{
    [Test]
    public void Off_IgnoresDpad()
    {
        Assert.AreEqual(0f, DpadMoveRules.Resolve(false, 0f, true, false));
        Assert.AreEqual(0f, DpadMoveRules.Resolve(false, 0f, false, true));
        Assert.AreEqual(0.5f, DpadMoveRules.Resolve(false, 0.5f, true, false));
    }

    [Test]
    public void On_DpadLeftRight()
    {
        Assert.AreEqual(-1f, DpadMoveRules.Resolve(true, 0f, true, false));
        Assert.AreEqual(1f, DpadMoveRules.Resolve(true, 0f, false, true));
        Assert.AreEqual(0f, DpadMoveRules.Resolve(true, 0f, false, false));
    }

    [Test]
    public void On_BothDpad_Cancel()
    {
        Assert.AreEqual(0f, DpadMoveRules.Resolve(true, 0f, true, true));
    }

    [Test]
    public void On_StickWins()
    {
        Assert.AreEqual(0.7f, DpadMoveRules.Resolve(true, 0.7f, true, false));
        Assert.AreEqual(-1f, DpadMoveRules.Resolve(true, -1f, false, true));
    }

    [Test]
    public void QuickSlots2And3_BlockedOnlyWhenOn()
    {
        Assert.IsFalse(DpadMoveRules.BlocksQuickSlot(false, 1));
        Assert.IsFalse(DpadMoveRules.BlocksQuickSlot(false, 2));
        Assert.IsTrue(DpadMoveRules.BlocksQuickSlot(true, 1));
        Assert.IsTrue(DpadMoveRules.BlocksQuickSlot(true, 2));
        Assert.IsFalse(DpadMoveRules.BlocksQuickSlot(true, 0));
        Assert.IsFalse(DpadMoveRules.BlocksQuickSlot(true, 3));
    }
}
