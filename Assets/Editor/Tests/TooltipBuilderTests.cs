using System.Collections.Generic;
using NUnit.Framework;

/// <summary>InventoryRefactorDesign 4절 Phase 7 — TooltipBuilder (D3). 문구 번역에 의존하지 않게 숫자·색·기호만 본다</summary>
public class TooltipBuilderTests
{
    static TooltipBuilder.Stat S(string key, float v, string unit = "") => new TooltipBuilder.Stat(key, key, v, unit);

    [Test]
    public void Signed_NegativeHasSingleMinus()
    {
        Assert.AreEqual("-5", TooltipBuilder.Signed(-5));
        Assert.AreEqual("+5", TooltipBuilder.Signed(5));
        Assert.AreEqual("0", TooltipBuilder.Signed(0));
    }

    [Test]
    public void Name_UsesRarityColorAndUpgradePrefix()
    {
        string n = TooltipBuilder.Name("Blade", "U", 3);
        StringAssert.Contains(TooltipBuilder.ColorUnique, n);
        StringAssert.Contains("+3 Blade", n);
        StringAssert.Contains(TooltipBuilder.ColorRare, TooltipBuilder.Name("Blade", "R", 0));
    }

    [Test]
    public void StatLines_NoCompare_NoArrows()
    {
        string t = TooltipBuilder.StatLines(new List<TooltipBuilder.Stat> { S("Def", 5) }, null);
        Assert.AreEqual("+5 Def\n", t);
    }

    [Test]
    public void StatLines_CompareShowsUpAndDownWithColors()
    {
        var mine = new List<TooltipBuilder.Stat> { S("Def", 8), S("Hp", 10) };
        var worn = new List<TooltipBuilder.Stat> { S("Def", 5), S("Hp", 12) };
        string t = TooltipBuilder.StatLines(mine, worn);
        StringAssert.Contains("<color=" + TooltipBuilder.ColorUp + ">▲+3", t);
        StringAssert.Contains("<color=" + TooltipBuilder.ColorDown + ">▼-2", t);
    }

    [Test]
    public void StatLines_StatOnlyOnWornItem_AddsRedLossLine()
    {
        var mine = new List<TooltipBuilder.Stat> { S("Def", 5) };
        var worn = new List<TooltipBuilder.Stat> { S("Def", 5), S("Critical Rate", 4, "%") };
        string t = TooltipBuilder.StatLines(mine, worn);
        StringAssert.Contains("<color=" + TooltipBuilder.ColorDown + ">-4% Critical Rate</color>", t);
        StringAssert.DoesNotContain("▲", t); // 같은 값은 화살표 없음
    }

    [Test]
    public void SetBlock_ShowsProgressAndTierColors()
    {
        var tiers = new List<KeyValuePair<int, string>>
        {
            new KeyValuePair<int, string>(2, "+10 Def"),
            new KeyValuePair<int, string>(5, "+100 Hp"),
        };
        string t = TooltipBuilder.SetBlock("Rabbit", 2, 6, tiers);
        StringAssert.Contains("(2/6)", t);
        StringAssert.Contains("<color=" + TooltipBuilder.ColorSet + ">(2) +10 Def", t);
        StringAssert.Contains("<color=" + TooltipBuilder.ColorOff + ">(5) +100 Hp", t);
    }

    [Test]
    public void WeaponDamageLine_ComparesAverage()
    {
        string t = TooltipBuilder.WeaponDamageLine(10, 20, 8, 12, true); // 평균 15 vs 10
        StringAssert.Contains("10 ~ 20", t);
        StringAssert.Contains("▲+5", t);
    }
}
