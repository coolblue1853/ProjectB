using NUnit.Framework;

/// <summary>자동 플레이 봇 B3 순수 로직: 귀환 판단·장착/판매 규칙·메뉴 키 판단</summary>
public class PlayBotErrandTests
{
    private static BotMenuView Bag(string state, int page, int cell)
    {
        return new BotMenuView { InvOpen = true, State = state, Page = page, Cell = cell, ShopOpen = false };
    }

    [Test]
    public void ShouldReturn_BagOrPotionsAtThreshold()
    {
        Assert.IsTrue(BotErrandRules.ShouldReturn(3, 20, 3, 2));
        Assert.IsTrue(BotErrandRules.ShouldReturn(10, 2, 3, 2));
        Assert.IsFalse(BotErrandRules.ShouldReturn(4, 3, 3, 2));
        Assert.AreEqual(BotErrandRules.Reason.Potions, BotErrandRules.WhyReturn(1, 0, 3, 2));
        Assert.AreEqual(BotErrandRules.Reason.Bag, BotErrandRules.WhyReturn(3, 9, 3, 2));
        Assert.AreEqual(BotErrandRules.Reason.None, BotErrandRules.WhyReturn(9, 9, 3, 2));
    }

    [Test]
    public void Clamp_KeepsHumanSamplesInRange()
    {
        Assert.AreEqual(BotErrandRules.BagFreeMax, BotErrandRules.ClampBagFree(26.6f));
        Assert.AreEqual(BotErrandRules.BagFreeMin, BotErrandRules.ClampBagFree(0.4f));
        Assert.AreEqual(BotErrandRules.PotionsMax, BotErrandRules.ClampPotions(42f));
        Assert.AreEqual(BotErrandRules.PotionsMin, BotErrandRules.ClampPotions(0f));
        Assert.AreEqual(5, BotErrandRules.ClampPotions(5.2f));
    }

    [Test]
    public void IsUpgrade_NeedsKnownHigherScoreOrEmptySlot()
    {
        Assert.IsTrue(BotErrandRules.IsUpgrade(12f, 10f, false));
        Assert.IsFalse(BotErrandRules.IsUpgrade(10f, 10f, false));
        Assert.IsFalse(BotErrandRules.IsUpgrade(-1f, 0f, true)); // 점수 모름
        Assert.IsTrue(BotErrandRules.IsUpgrade(1f, 0f, true));
    }

    [Test]
    public void ShouldSell_RetiredAlways_OtherOnlyIfNotBetter()
    {
        Assert.IsTrue(BotErrandRules.ShouldSell(false, true, false, -1f, 0f, true));
        Assert.IsFalse(BotErrandRules.ShouldSell(true, true, false, -1f, 0f, true)); // 잠금
        Assert.IsTrue(BotErrandRules.ShouldSell(false, false, true, 8f, 10f, false));
        Assert.IsTrue(BotErrandRules.ShouldSell(false, false, true, 10f, 10f, false)); // 같은 점수 중복
        Assert.IsFalse(BotErrandRules.ShouldSell(false, false, true, 11f, 10f, false));
        Assert.IsFalse(BotErrandRules.ShouldSell(false, false, true, 5f, 0f, true)); // 빈 부위는 장착 후보
        Assert.IsFalse(BotErrandRules.ShouldSell(false, false, false, 5f, 10f, false)); // 따로 다루는 부위
        Assert.IsFalse(BotErrandRules.ShouldSell(false, false, true, -1f, 10f, false)); // 점수 모름
    }

    [Test]
    public void IsEquippableArea_OnlyFourSlots()
    {
        Assert.IsTrue(BotErrandRules.IsEquippableArea("Weapon"));
        Assert.IsTrue(BotErrandRules.IsEquippableArea("Chest"));
        Assert.IsFalse(BotErrandRules.IsEquippableArea("Earring"));
        Assert.IsFalse(BotErrandRules.IsEquippableArea("Shoes"));
    }

    [Test]
    public void PageAndCell_FromFlatIndex()
    {
        Assert.AreEqual(1, BotMenuNav.PageOf(25));
        Assert.AreEqual(13, BotMenuNav.CellOf(38));
        Assert.AreEqual(0, BotMenuNav.PageOf(24));
    }

    [Test]
    public void ToItem_WalksCellsThenSelects()
    {
        Assert.AreEqual(BotKey.Right, BotMenuNav.ToItem(Bag("", 0, 0), 0, 7)); // 열 0 → 2
        Assert.AreEqual(BotKey.Left, BotMenuNav.ToItem(Bag("", 0, 9), 0, 7));
        Assert.AreEqual(BotKey.Down, BotMenuNav.ToItem(Bag("", 0, 2), 0, 7)); // 같은 열 → 아래
        Assert.AreEqual(BotKey.Up, BotMenuNav.ToItem(Bag("", 0, 12), 0, 7));
        Assert.AreEqual(BotKey.Select, BotMenuNav.ToItem(Bag("", 0, 7), 0, 7));
    }

    [Test]
    public void ToItem_SelectsThroughDetailAndPopup()
    {
        Assert.AreEqual(BotKey.Select, BotMenuNav.ToItem(Bag("detail", 0, 7), 0, 7));
        BotMenuView v = Bag("Sell", 0, 7);
        v.Popup = true;
        Assert.AreEqual(BotKey.Select, BotMenuNav.ToItem(v, 0, 7));
        Assert.AreEqual(BotKey.None, BotMenuNav.ToItem(Bag("Sell", 0, 7), 0, 7)); // 수량 창이 닫히는 사이는 기다린다
    }

    [Test]
    public void ToItem_OtherPage_GoesLeftThenPicksBoxThenSelect()
    {
        Assert.AreEqual(BotKey.Left, BotMenuNav.ToItem(Bag("", 0, 3), 1, 4)); // 왼쪽 끝으로 → 페이지 고르기
        BotMenuView v = Bag("boxChange", 0, 0);
        v.BoxCursor = 0;
        Assert.AreEqual(BotKey.Down, BotMenuNav.ToItem(v, 1, 4));
        v.BoxCursor = 1;
        Assert.AreEqual(BotKey.Select, BotMenuNav.ToItem(v, 1, 4));
        v.BoxCursor = 2;
        Assert.AreEqual(BotKey.Up, BotMenuNav.ToItem(v, 1, 4));
    }

    [Test]
    public void ToItem_ShopStock_GoesRightIntoBag()
    {
        BotMenuView v = new BotMenuView { InvOpen = true, ShopOpen = true, State = "ShopCusorOn" };
        Assert.AreEqual(BotKey.Right, BotMenuNav.ToItem(v, 0, 3));
        v.State = "Equipment";
        Assert.AreEqual(BotKey.Right, BotMenuNav.ToItem(v, 0, 3));
        v.State = "change"; // 모르는 상태는 한 단계 빠져나온다
        Assert.AreEqual(BotKey.Back, BotMenuNav.ToItem(v, 0, 3));
    }

    [Test]
    public void ToItem_ClosedBag_NoKey()
    {
        BotMenuView v = new BotMenuView { InvOpen = false, State = "" };
        Assert.AreEqual(BotKey.None, BotMenuNav.ToItem(v, 0, 3));
    }

    [Test]
    public void ToNpcShop_MovesCursorToShopRow()
    {
        BotMenuView v = new BotMenuView { NpcMenuOpen = true, NpcCursor = 0, NpcRow = 2 };
        Assert.AreEqual(BotKey.Down, BotMenuNav.ToNpcShop(v));
        v.NpcCursor = 3;
        Assert.AreEqual(BotKey.Up, BotMenuNav.ToNpcShop(v));
        v.NpcCursor = 2;
        Assert.AreEqual(BotKey.Select, BotMenuNav.ToNpcShop(v));
        v.NpcRow = -1;
        Assert.AreEqual(BotKey.Back, BotMenuNav.ToNpcShop(v));
        v.NpcMenuOpen = false;
        Assert.AreEqual(BotKey.None, BotMenuNav.ToNpcShop(v));
    }

    [Test]
    public void PotionBuyAmount_PctOfGold_CappedByStackRoom()
    {
        Assert.AreEqual(0, BotErrandRules.PotionBuyAmount(0, 70, 0, 99, 0.3f));
        Assert.AreEqual(0, BotErrandRules.PotionBuyAmount(100, 70, 0, 99, 0.3f));
        Assert.AreEqual(85, BotErrandRules.PotionBuyAmount(20000, 70, 0, 99, 0.3f));
        Assert.AreEqual(9, BotErrandRules.PotionBuyAmount(20000, 70, 90, 99, 0.3f));
        Assert.AreEqual(0, BotErrandRules.PotionBuyAmount(20000, 70, 99, 99, 0.3f));
        Assert.AreEqual(0, BotErrandRules.PotionBuyAmount(20000, 0, 0, 99, 0.3f));
    }

    [Test]
    public void ToQuantity_StepsTenThenOne()
    {
        Assert.AreEqual(BotKey.Right, BotErrandRules.ToQuantity(1, 30));
        Assert.AreEqual(BotKey.Up, BotErrandRules.ToQuantity(28, 30));
        Assert.AreEqual(BotKey.Down, BotErrandRules.ToQuantity(31, 30));
        Assert.AreEqual(BotKey.Left, BotErrandRules.ToQuantity(50, 30));
        Assert.AreEqual(BotKey.Select, BotErrandRules.ToQuantity(30, 30));
    }

    [Test]
    public void ToStockCell_RowFirstThenColumn()
    {
        Assert.AreEqual(BotKey.Down, BotErrandRules.ToStockCell(0, 3, 1, 0));
        Assert.AreEqual(BotKey.Up, BotErrandRules.ToStockCell(1, 0, 0, 2));
        Assert.AreEqual(BotKey.Right, BotErrandRules.ToStockCell(0, 0, 0, 2));
        Assert.AreEqual(BotKey.Left, BotErrandRules.ToStockCell(0, 3, 0, 1));
        Assert.AreEqual(BotKey.Select, BotErrandRules.ToStockCell(0, 2, 0, 2));
    }
}
