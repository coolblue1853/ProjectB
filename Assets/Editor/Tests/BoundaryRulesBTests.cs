using NUnit.Framework;

/// <summary>AUTO-72: 상점·가방 확장·강화·방어 경계값 고정(현재 동작 기록).</summary>
public class BoundaryRulesBTests
{
    private const string Csv = "step,slots,price,minFloor\n1,30,200,1\n2,125,500,4\n3,130,999,4\n";

    [Test]
    public void MaxBuyable_GoldExactlyOnePrice()
    {
        Assert.AreEqual(1, ShopRules.MaxBuyable(99, 100, 100, 99));
        Assert.AreEqual(0, ShopRules.MaxBuyable(99, 100, 99, 99));
    }

    [Test]
    public void MaxBuyable_ZeroPriceIgnoresGold_AndNegativeSpace()
    {
        Assert.AreEqual(5, ShopRules.MaxBuyable(5, 0, 0, 99));
        Assert.AreEqual(0, ShopRules.MaxBuyable(5, 10, 1000, -3));
        Assert.AreEqual(0, ShopRules.MaxBuyable(-5, 10, 1000, 3));
    }

    [Test]
    public void CheckBuy_GoldBeforeBagAndExactBoundary()
    {
        Assert.AreEqual(ShopRules.BuyBlock.None, ShopRules.CheckBuy(100, 100, 1));
        Assert.AreEqual(ShopRules.BuyBlock.NoGold, ShopRules.CheckBuy(100, 99, 1));
        Assert.AreEqual(ShopRules.BuyBlock.NoGold, ShopRules.CheckBuy(100, 99, 0));
        Assert.AreEqual(ShopRules.BuyBlock.BagFull, ShopRules.CheckBuy(0, 0, 0));
    }

    [Test]
    public void BuyPrice_FloorsAfterTimes1_5()
    {
        Assert.AreEqual(0, ShopRules.BuyPrice(0));
        Assert.AreEqual(1, ShopRules.BuyPrice(1));
        Assert.AreEqual(4, ShopRules.BuyPrice(3));
    }

    [Test]
    public void SaleTotal_NoOverflowAndNegativeIsZero()
    {
        Assert.AreEqual((long)int.MaxValue * int.MaxValue, ShopRules.SaleTotal(int.MaxValue, int.MaxValue));
        Assert.AreEqual(0L, ShopRules.SaleTotal(-5, 10));
        Assert.AreEqual(0L, ShopRules.SaleTotal(5, -10));
    }

    [Test]
    public void Refill_AtOrAboveTarget_ZeroAndTargetFloor()
    {
        Assert.AreEqual(0, ShopRules.RefillAmount(10, 10, 5, 1000, 99));
        Assert.AreEqual(0, ShopRules.RefillAmount(11, 10, 5, 1000, 99));
        Assert.AreEqual(1, ShopRules.RefillAmount(9, 10, 5, 5, 99));
        Assert.AreEqual(0, ShopRules.RefillAmount(9, 10, 5, 4, 99));
        Assert.AreEqual(1, ShopRules.RefillTarget(0));
        Assert.AreEqual(1, ShopRules.RefillTarget(-5));
    }

    [Test]
    public void NextFilter_WrapsAround()
    {
        Assert.AreEqual(SellFilter.Equip, ShopRules.NextFilter(SellFilter.All));
        Assert.AreEqual(SellFilter.All, ShopRules.NextFilter(SellFilter.Misc));
    }

    [Test]
    public void BagClamp_Boundaries()
    {
        Assert.AreEqual(25, BagExpansionRules.Clamp(int.MinValue));
        Assert.AreEqual(25, BagExpansionRules.Clamp(0));
        Assert.AreEqual(25, BagExpansionRules.Clamp(24));
        Assert.AreEqual(125, BagExpansionRules.Clamp(125));
        Assert.AreEqual(125, BagExpansionRules.Clamp(126));
        Assert.AreEqual(125, BagExpansionRules.Clamp(int.MaxValue));
    }

    [Test]
    public void BagParse_NullOrEmpty_NoSteps_MaxBlock()
    {
        BagExpansionRules r = BagExpansionRules.Parse(null);
        Assert.AreEqual(0, r.StepCount);
        Assert.AreEqual(BagExpansionRules.Block.Max, r.Check(25, 4, 99999, out _));
        Assert.AreEqual(25, r.FloorCap(99));
    }

    [Test]
    public void BagCheck_FloorBeforeGold_ExactGold()
    {
        BagExpansionRules r = BagExpansionRules.Parse(Csv);
        Assert.AreEqual(BagExpansionRules.Block.NeedFloor, r.Check(25, 0, 0, out _));
        Assert.AreEqual(BagExpansionRules.Block.NoGold, r.Check(25, 1, 199, out _));
        Assert.AreEqual(BagExpansionRules.Block.None, r.Check(25, 1, 200, out BagExpansionRules.Step s));
        Assert.AreEqual(30, s.Slots);
    }

    [Test]
    public void BagCheck_JustAfterMaxExpansion_IsMax()
    {
        BagExpansionRules r = BagExpansionRules.Parse(Csv);
        Assert.AreEqual(BagExpansionRules.Block.None, r.Check(30, 4, 500, out BagExpansionRules.Step s));
        Assert.AreEqual(125, s.Slots);
        Assert.AreEqual(BagExpansionRules.Block.Max, r.Check(125, 4, 99999, out _));
        Assert.AreEqual(BagExpansionRules.Block.Max, r.Check(500, 4, 99999, out _)); // 130 단계는 MaxSlots 초과라 무시
    }

    [Test]
    public void BagFloorCap_AndTotalCost_Boundaries()
    {
        BagExpansionRules r = BagExpansionRules.Parse(Csv);
        Assert.AreEqual(25, r.FloorCap(0));
        Assert.AreEqual(30, r.FloorCap(1));
        Assert.AreEqual(130, r.FloorCap(4));
        Assert.AreEqual(700, r.TotalCost(25, 125));
        Assert.AreEqual(500, r.TotalCost(30, 125)); // from 은 제외
        Assert.AreEqual(0, r.TotalCost(125, 125));
    }

    [Test]
    public void Enhance_MaxLevel_NoEnhance()
    {
        Assert.IsFalse(EnhanceRules.CanEnhance(EnhanceRules.MaxLevel));
        Assert.IsFalse(EnhanceRules.CanEnhance(-1));
        Assert.AreEqual(EnhanceRules.Result.MaxLevel, EnhanceRules.Roll(EnhanceRules.MaxLevel, 0f, 99, out int lv));
        Assert.AreEqual(EnhanceRules.MaxLevel, lv);
        Assert.AreEqual(0f, EnhanceRules.Rate(EnhanceRules.MaxLevel));
        Assert.AreEqual(0, EnhanceRules.Stones(-1));
        Assert.AreEqual(0, EnhanceRules.Gold(EnhanceRules.MaxLevel));
    }

    [Test]
    public void Enhance_LastStep_SuccessToMax()
    {
        int last = EnhanceRules.MaxLevel - 1;
        Assert.AreEqual(EnhanceRules.Result.Success, EnhanceRules.Roll(last, 0f, 0, out int lv));
        Assert.AreEqual(EnhanceRules.MaxLevel, lv);
    }

    [Test]
    public void Enhance_RollBoundary_RateExactIsFail()
    {
        float rate = EnhanceRules.Rate(2);
        Assert.AreEqual(EnhanceRules.Result.Success, EnhanceRules.Roll(2, rate - 0.001f, 0, out _));
        Assert.AreEqual(EnhanceRules.Result.Fail, EnhanceRules.Roll(2, rate, 0, out int lv));
        Assert.AreEqual(2, lv);
    }

    [Test]
    public void Enhance_PityAndDropBoundary()
    {
        Assert.AreEqual(EnhanceRules.Result.Success, EnhanceRules.Roll(9, 1f, EnhanceRules.PityFails, out _));
        Assert.AreEqual(EnhanceRules.Result.FailDown, EnhanceRules.Roll(9, 1f, EnhanceRules.PityFails - 1, out _));
        Assert.AreEqual(EnhanceRules.Result.Fail, EnhanceRules.Roll(4, 1f, 0, out _));
        Assert.AreEqual(EnhanceRules.Result.FailDown, EnhanceRules.Roll(5, 1f, 0, out int down));
        Assert.AreEqual(4, down);
    }

    [Test]
    public void Enhance_Multipliers_AtThresholdsAndBeyond()
    {
        Assert.AreEqual(1f, EnhanceRules.WeaponMultiplier(-3), 1e-5f);
        Assert.AreEqual(1f, EnhanceRules.WeaponMultiplier(0), 1e-5f);
        Assert.AreEqual(1.3f, EnhanceRules.WeaponMultiplier(5), 1e-5f);
        Assert.AreEqual(1.6f, EnhanceRules.WeaponMultiplier(10), 1e-5f);
        Assert.AreEqual(1.9f, EnhanceRules.WeaponMultiplier(15), 1e-5f);
        Assert.AreEqual(1.9f, EnhanceRules.WeaponMultiplier(99), 1e-5f);
        Assert.AreEqual(1.95f, EnhanceRules.GearMultiplier(15), 1e-5f);
        Assert.AreEqual(1.95f, EnhanceRules.GearMultiplier(99), 1e-5f);
    }

    [Test]
    public void Guard_NotHoldingOrNonPositive_PassThrough()
    {
        Assert.AreEqual(10, GuardRules.Resolve(10, false));
        Assert.AreEqual(0, GuardRules.Resolve(0, true));
        Assert.AreEqual(-5, GuardRules.Resolve(-5, true));
    }

    [Test]
    public void Guard_MinimumOneDamageWhenHolding()
    {
        Assert.AreEqual(1, GuardRules.Resolve(1, true));
        Assert.AreEqual(1, GuardRules.Resolve(2, true));
        Assert.AreEqual(1, GuardRules.Resolve(100, true, 1f));
        Assert.AreEqual(1, GuardRules.Resolve(100, true, 5f)); // 비율은 0~1 로 제한
    }

    [Test]
    public void Guard_RatioZeroBlocksNothing_NegativeClamped()
    {
        Assert.AreEqual(10, GuardRules.Resolve(10, true, 0f));
        Assert.AreEqual(10, GuardRules.Resolve(10, true, -2f));
        Assert.AreEqual(30, GuardRules.Resolve(100, true));
    }
}
