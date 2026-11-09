using NUnit.Framework;
using UnityEngine;

public class BagExpansionRulesTests
{
    private static BagExpansionRules Load()
    {
        TextAsset csv = Resources.Load<TextAsset>("BagExpansion");
        Assert.IsNotNull(csv, "BagExpansion.csv");
        return BagExpansionRules.Parse(csv.text);
    }

    [Test]
    public void Clamp_OldSaveAndBounds()
    {
        Assert.AreEqual(40, BagExpansionRules.Clamp(0)); // 구 세이브(키 없음)
        Assert.AreEqual(40, BagExpansionRules.Clamp(15));
        Assert.AreEqual(40, BagExpansionRules.Clamp(25)); // 구 시작값
        Assert.AreEqual(40, BagExpansionRules.Clamp(35));
        Assert.AreEqual(60, BagExpansionRules.Clamp(60));
        Assert.AreEqual(125, BagExpansionRules.Clamp(999));
    }

    [Test]
    public void PriceTable_MatchesDesign()
    {
        BagExpansionRules r = Load();
        Assert.AreEqual(17, r.StepCount);
        Assert.AreEqual(1550, r.TotalCost(40, 50));
        Assert.AreEqual(11500, r.TotalCost(50, 75));
        Assert.AreEqual(32300, r.TotalCost(75, 100));
        Assert.AreEqual(78000, r.TotalCost(100, 125));
    }

    [Test]
    public void Check_FloorCapGoldAndMax()
    {
        BagExpansionRules r = Load();
        Assert.AreEqual(BagExpansionRules.Block.NoGold, r.Check(40, 1, 100, out BagExpansionRules.Step s));
        Assert.AreEqual(45, s.Slots);
        Assert.AreEqual(650, s.Price);
        Assert.AreEqual(BagExpansionRules.Block.None, r.Check(0, 1, 650, out s)); // 0 = 시작 40
        Assert.AreEqual(BagExpansionRules.Block.NeedFloor, r.Check(50, 1, 99999, out s));
        Assert.AreEqual(55, s.Slots);
        Assert.AreEqual(BagExpansionRules.Block.None, r.Check(50, 2, 99999, out s));
        Assert.AreEqual(BagExpansionRules.Block.Max, r.Check(125, 9, 999999, out s));
    }

    [Test]
    public void FloorCap_PerFloor()
    {
        BagExpansionRules r = Load();
        Assert.AreEqual(50, r.FloorCap(1));
        Assert.AreEqual(75, r.FloorCap(2));
        Assert.AreEqual(100, r.FloorCap(3));
        Assert.AreEqual(125, r.FloorCap(4));
        Assert.AreEqual(125, r.FloorCap(7));
    }

    [Test]
    public void Parse_IgnoresBadRows()
    {
        BagExpansionRules r = BagExpansionRules.Parse("step,slots,price,minFloor\n1,30,200,1\nbad\n2,x,1,1\n");
        Assert.AreEqual(1, r.StepCount);
        Assert.AreEqual(0, BagExpansionRules.Parse(null).StepCount);
    }
}
