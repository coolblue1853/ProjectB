using System.Collections.Generic;
using NUnit.Framework;

/// <summary>REWORK-12 P6 세트 창 — SetOverview 순수 규칙(착용 세트 목록·켜진 단계·다음 단계)</summary>
public class SetOverviewTests
{
    static SetTierTable Table()
    {
        var t = new SetTierTable();
        t.Define("Rabbit", 2, "a");
        t.Define("Rabbit", 4, "b");
        t.Define("Rat", 3, "c");
        t.Define("Rat", 5, "d");
        return t;
    }

    static Dictionary<string, int> Counts(params (string, int)[] kv)
    {
        var d = new Dictionary<string, int>();
        foreach (var (k, v) in kv)
        {
            d[k] = v;
        }
        return d;
    }

    [Test]
    public void Empty_NoEntries()
    {
        CollectionAssert.IsEmpty(SetOverview.Build(Counts(), Table()));
    }

    [Test]
    public void ZeroOrUndefinedSet_Skipped()
    {
        var list = SetOverview.Build(Counts(("Rabbit", 0), ("Unknown", 3), ("", 2)), Table());
        CollectionAssert.IsEmpty(list);
    }

    [Test]
    public void OnePiece_ListedWithNoActiveTier_NextIs2()
    {
        var e = SetOverview.Build(Counts(("Rabbit", 1)), Table())[0];
        Assert.AreEqual("Rabbit", e.SetKey);
        Assert.AreEqual(1, e.Worn);
        CollectionAssert.IsEmpty(e.ActiveTiers);
        Assert.AreEqual(2, e.NextTier);
        Assert.AreEqual(1, e.NeedForNext);
        CollectionAssert.AreEqual(new[] { 2, 4 }, e.AllTiers);
    }

    [Test]
    public void ThreePieces_Tier2On_Next4()
    {
        var e = SetOverview.Build(Counts(("Rabbit", 3)), Table())[0];
        CollectionAssert.AreEqual(new[] { 2 }, e.ActiveTiers);
        Assert.IsTrue(e.IsOn(2));
        Assert.IsFalse(e.IsOn(4));
        Assert.AreEqual(4, e.NextTier);
        Assert.AreEqual(1, e.NeedForNext);
    }

    [Test]
    public void AllTiersOn_NextZero()
    {
        var e = SetOverview.Build(Counts(("Rat", 5)), Table())[0];
        CollectionAssert.AreEqual(new[] { 3, 5 }, e.ActiveTiers);
        Assert.AreEqual(0, e.NextTier);
        Assert.AreEqual(0, e.NeedForNext);
    }

    [Test]
    public void Order_MoreActiveTiersFirst_ThenWorn_ThenKey()
    {
        var list = SetOverview.Build(Counts(("Rat", 2), ("Rabbit", 4), ("Zed", 1)), Zed());
        Assert.AreEqual("Rabbit", list[0].SetKey); // 단계 2개 켜짐
        Assert.AreEqual("Rat", list[1].SetKey);    // 0개, 착용 2
        Assert.AreEqual("Zed", list[2].SetKey);    // 0개, 착용 1
    }

    static SetTierTable Zed()
    {
        var t = Table();
        t.Define("Zed", 2, "z");
        return t;
    }

    [Test]
    public void TotalActiveTiers_SumsAllSets()
    {
        var list = SetOverview.Build(Counts(("Rat", 3), ("Rabbit", 4)), Table());
        Assert.AreEqual(3, SetOverview.TotalActiveTiers(list));
    }
}
