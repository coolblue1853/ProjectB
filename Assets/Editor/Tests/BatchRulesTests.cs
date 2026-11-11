using System.Collections.Generic;
using NUnit.Framework;

/// <summary>REWORK-11 일괄 요리·제작 순수 규칙</summary>
public class BatchRulesTests
{
    private static Dictionary<string, int> Bag(params object[] kv)
    {
        var d = new Dictionary<string, int>();
        for (int i = 0; i < kv.Length; i += 2)
        {
            d[(string)kv[i]] = (int)kv[i + 1];
        }
        return d;
    }

    private static int Max(Dictionary<string, int> needs, Dictionary<string, int> have, int gold, int money, int cap)
    {
        return BatchRules.MaxCount(needs, id => have.TryGetValue(id, out int n) ? n : 0, gold, money, cap);
    }

    [Test]
    public void MaxCount_LimitedByScarcestIngredient()
    {
        Assert.AreEqual(3, Max(Bag("a", 2, "b", 1), Bag("a", 7, "b", 9), 0, 0, 99));
    }

    [Test]
    public void MaxCount_CapApplies()
    {
        Assert.AreEqual(10, Max(Bag("a", 1), Bag("a", 500), 0, 0, BatchRules.CookCap));
    }

    [Test]
    public void MaxCount_MissingIngredientIsZero()
    {
        Assert.AreEqual(0, Max(Bag("a", 1, "b", 1), Bag("a", 5), 0, 0, 99));
    }

    [Test]
    public void MaxCount_GoldLimits()
    {
        Assert.AreEqual(4, Max(Bag("a", 1), Bag("a", 50), 100, 450, 99));
        Assert.AreEqual(0, Max(Bag("a", 1), Bag("a", 50), 100, 99, 99));
    }

    [Test]
    public void MaxCount_ZeroGoldIgnoresMoney()
    {
        Assert.AreEqual(5, Max(Bag("a", 1), Bag("a", 5), 0, 0, 99));
    }

    [Test]
    public void MaxCount_NonPositiveNeedIgnoredAndNegativeCapIsZero()
    {
        Assert.AreEqual(7, Max(Bag("a", 0, "b", 1), Bag("b", 7), 0, 0, 99));
        Assert.AreEqual(0, Max(Bag("a", 1), Bag("a", 5), 0, 0, -3));
    }

    [Test]
    public void Cooking_ResultCountScalesWithBatch()
    {
        Assert.AreEqual(10, CookingRules.ResultCount(CookGrade.Perfect, 5));
        Assert.AreEqual(5, CookingRules.ResultCount(CookGrade.Good, 5));
        Assert.AreEqual(5, CookingRules.ResultCount(CookGrade.Fail, 5));
        Assert.AreEqual(2, CookingRules.ResultCount(CookGrade.Perfect, 0)); // 0 이하는 1회분
    }

    [Test]
    public void Cooking_MaxCountUsesRecipe()
    {
        CookRecipe r = CookingRules.Recipes[0]; // Rabbit Meat 2, GreenHurb 1, Wood 1
        var have = Bag("Rabbit Meat", 9, "GreenHurb", 3, "Wood", 50);
        Assert.AreEqual(3, CookingRules.MaxCount(r, id => have.TryGetValue(id, out int n) ? n : 0));
        have["Rabbit Meat"] = 1;
        Assert.AreEqual(0, CookingRules.MaxCount(r, id => have.TryGetValue(id, out int n) ? n : 0));
    }

    [Test]
    public void Session_ConfirmStoresCount()
    {
        var s = new CookingSession(new List<CookRecipe> { CookingRules.Recipes[0] }, () => 0.5f);
        Assert.AreEqual(1, s.Count);
        s.Confirm(4);
        Assert.AreEqual(4, s.Count);
        Assert.AreEqual(CookPhase.Play, s.Phase);
    }

    [Test]
    public void Session_ConfirmClampsCountToOne()
    {
        var s = new CookingSession(new List<CookRecipe> { CookingRules.Recipes[0] }, () => 0.5f);
        s.Confirm(-2);
        Assert.AreEqual(1, s.Count);
    }

    [Test]
    public void Cooking_PlaySecondsIsIndependentOfBatch()
    {
        Assert.AreEqual(12f, CookingRules.PlaySeconds(CookMethod.Grill));
        Assert.AreEqual(18f, CookingRules.PlaySeconds(CookMethod.Boil));
    }
}
