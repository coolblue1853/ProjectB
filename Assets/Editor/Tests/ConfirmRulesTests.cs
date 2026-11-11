using NUnit.Framework;

public class ConfirmRulesTests
{
    [Test]
    public void 기본_선택은_아니오()
    {
        Assert.AreEqual(ConfirmRules.No, ConfirmRules.Initial(false));
        Assert.AreEqual(ConfirmRules.Yes, ConfirmRules.Initial(true));
    }

    [Test]
    public void 좌우_이동은_끝에서_멈춘다()
    {
        Assert.AreEqual(ConfirmRules.Yes, ConfirmRules.Move(ConfirmRules.No, -1));
        Assert.AreEqual(ConfirmRules.Yes, ConfirmRules.Move(ConfirmRules.Yes, -1));
        Assert.AreEqual(ConfirmRules.No, ConfirmRules.Move(ConfirmRules.Yes, 1));
        Assert.AreEqual(ConfirmRules.No, ConfirmRules.Move(ConfirmRules.No, 1));
        Assert.AreEqual(ConfirmRules.Yes, ConfirmRules.Move(ConfirmRules.Yes, 0));
    }
}
