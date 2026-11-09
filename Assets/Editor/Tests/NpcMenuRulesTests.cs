using NUnit.Framework;

public class NpcMenuRulesTests
{
    [Test]
    public void Build_OnlyOwnedServices_CloseLast()
    {
        var smith = NpcMenuRules.Build(true, false, true, true, true, false);
        CollectionAssert.AreEqual(new[] { NpcMenuRules.Kind.Quest, NpcMenuRules.Kind.Craft, NpcMenuRules.Kind.Enhance, NpcMenuRules.Kind.Trial, NpcMenuRules.Kind.Close }, smith);
        var merchant = NpcMenuRules.Build(false, true, false, false, false, false);
        CollectionAssert.AreEqual(new[] { NpcMenuRules.Kind.Shop, NpcMenuRules.Kind.Close }, merchant);
    }

    [Test]
    public void NeedsMenu_SingleService_RunsDirectly()
    {
        Assert.IsFalse(NpcMenuRules.NeedsMenu(NpcMenuRules.Build(false, true, false, false, false, false)));
        Assert.IsFalse(NpcMenuRules.NeedsMenu(NpcMenuRules.Build(false, false, false, false, false, false)));
        Assert.IsTrue(NpcMenuRules.NeedsMenu(NpcMenuRules.Build(true, true, false, false, false, false)));
    }

    [Test]
    public void Step_Wraps()
    {
        Assert.AreEqual(2, NpcMenuRules.Step(0, 3, -1));
        Assert.AreEqual(0, NpcMenuRules.Step(2, 3, 1));
        Assert.AreEqual(1, NpcMenuRules.Step(0, 3, 1));
        Assert.AreEqual(0, NpcMenuRules.Step(5, 0, 1));
    }

    [Test]
    public void Labels_Korean()
    {
        Assert.AreEqual("퀘스트 이야기", NpcMenuRules.Label(NpcMenuRules.Kind.Quest));
        Assert.AreEqual("닫기", NpcMenuRules.Label(NpcMenuRules.Kind.Close));
    }
}
