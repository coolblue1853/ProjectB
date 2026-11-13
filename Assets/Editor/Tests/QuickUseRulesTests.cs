using NUnit.Framework;

public class QuickUseRulesTests
{
    [Test]
    public void Decide_ChestOpenMoves_ShopSells_ElseUseOrEquip()
    {
        Assert.AreEqual(QuickUseRules.Action.Move, QuickUseRules.Decide(true, false, true, "Consum"));
        Assert.AreEqual(QuickUseRules.Action.Move, QuickUseRules.Decide(true, false, false, "Equip"));
        Assert.AreEqual(QuickUseRules.Action.Sell, QuickUseRules.Decide(false, true, true, "Equip"));
        Assert.AreEqual(QuickUseRules.Action.Sell, QuickUseRules.Decide(false, true, true, "Misc"));
        Assert.AreEqual(QuickUseRules.Action.Use, QuickUseRules.Decide(false, false, true, "Consum"));
        Assert.AreEqual(QuickUseRules.Action.Equip, QuickUseRules.Decide(false, false, true, "Equip"));
        Assert.AreEqual(QuickUseRules.Action.None, QuickUseRules.Decide(false, false, true, "Misc"));
    }

    [Test]
    public void Decide_NotInInventory_DoesNothing()
    {
        Assert.AreEqual(QuickUseRules.Action.None, QuickUseRules.Decide(false, false, false, "Equip"));
        Assert.AreEqual(QuickUseRules.Action.None, QuickUseRules.Decide(false, true, false, "Equip"));
    }

    [Test]
    public void StateAllows_ModalStatesBlocked()
    {
        Assert.IsTrue(QuickUseRules.StateAllows(""));
        Assert.IsTrue(QuickUseRules.StateAllows("detail"));
        Assert.IsTrue(QuickUseRules.StateAllows("ShopCusorOn"));
        Assert.IsFalse(QuickUseRules.StateAllows("Sell"));
        Assert.IsFalse(QuickUseRules.StateAllows("divide"));
        Assert.IsFalse(QuickUseRules.StateAllows("drop"));
        Assert.IsFalse(QuickUseRules.StateAllows("change"));
    }

    [Test]
    public void IsModal_QuantityStatesOnly()
    {
        Assert.IsTrue(QuickUseRules.IsModal("Sell"));
        Assert.IsTrue(QuickUseRules.IsModal("drop"));
        Assert.IsFalse(QuickUseRules.IsModal(""));
        Assert.IsFalse(QuickUseRules.IsModal("detail"));
    }

    [Test]
    public void Place_DefaultBelowRight_FlipsAtEdges_ClampsInside()
    {
        float x, y;
        QuickUseRules.Place(100f, 900f, 300f, 200f, 1920f, 1080f, 20f, out x, out y);
        Assert.AreEqual(120f, x);
        Assert.AreEqual(680f, y); // 마우스 아래쪽(위 원점 아님: 아래로 = y 감소)
        QuickUseRules.Place(1800f, 900f, 300f, 200f, 1920f, 1080f, 20f, out x, out y);
        Assert.AreEqual(1480f, x); // 오른쪽이 넘치면 왼쪽으로 뒤집음
        QuickUseRules.Place(100f, 50f, 300f, 200f, 1920f, 1080f, 20f, out x, out y);
        Assert.AreEqual(70f, y); // 아래가 넘치면 위로 뒤집음
        QuickUseRules.Place(100f, 1075f, 300f, 200f, 1920f, 1080f, 20f, out x, out y);
        Assert.LessOrEqual(y + 200f, 1080f);
        QuickUseRules.Place(10f, 500f, 3000f, 200f, 1920f, 1080f, 20f, out x, out y);
        Assert.AreEqual(0f, x);
    }

    [Test]
    public void UnequipWorn_RightOrDoubleClick_OnlyWhenNoChestShopModal()
    {
        Assert.IsTrue(QuickUseRules.ShouldUnequipWorn(true, true, 1, false, false, ""));
        Assert.IsTrue(QuickUseRules.ShouldUnequipWorn(true, false, 2, false, false, "Equipment"));
        Assert.IsFalse(QuickUseRules.ShouldUnequipWorn(true, false, 1, false, false, ""));
        Assert.IsFalse(QuickUseRules.ShouldUnequipWorn(false, true, 1, false, false, ""));
        Assert.IsFalse(QuickUseRules.ShouldUnequipWorn(true, true, 1, true, false, ""));
        Assert.IsFalse(QuickUseRules.ShouldUnequipWorn(true, true, 1, false, true, ""));
        Assert.IsFalse(QuickUseRules.ShouldUnequipWorn(true, true, 1, false, false, "divide"));
    }
}
