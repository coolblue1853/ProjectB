using System.Globalization;
using NUnit.Framework;
using UnityEngine;

/// <summary>경제 불변식 고정 (EconomyAudit.md 4-1 결정). 데이터가 의도적으로 바뀌면 테스트도 함께 고친다.</summary>
public class EconomyInvariantTests
{
    static BagExpansionRules LoadBag()
    {
        TextAsset csv = Resources.Load<TextAsset>("BagExpansion");
        Assert.IsNotNull(csv, "BagExpansion.csv");
        return BagExpansionRules.Parse(csv.text);
    }

    static BagExpansionRules.Step[] Steps()
    {
        BagExpansionRules r = LoadBag();
        var list = new System.Collections.Generic.List<BagExpansionRules.Step>();
        int cur = 0;
        while (r.TryNext(cur, out BagExpansionRules.Step s))
        {
            list.Add(s);
            cur = s.Slots;
        }
        return list.ToArray();
    }

    [Test]
    public void Bag_PriceStrictlyIncreasing()
    {
        BagExpansionRules.Step[] s = Steps();
        Assert.AreEqual(17, s.Length);
        for (int i = 1; i < s.Length; i++)
        {
            Assert.Greater(s[i].Price, s[i - 1].Price, $"단계 {i + 1} 가격은 이전보다 커야 한다");
        }
    }

    [Test]
    public void Bag_Steps1to5_Price650to2200()
    {
        BagExpansionRules.Step[] s = Steps();
        Assert.AreEqual(650, s[0].Price);
        Assert.AreEqual(2200, s[4].Price);
        for (int i = 0; i < 5; i++)
        {
            Assert.That(s[i].Price, Is.InRange(650, 2200));
        }
    }

    [Test]
    public void Bag_Last5Steps_MatchTable()
    {
        BagExpansionRules.Step[] s = Steps();
        int[] expect = { 10500, 12500, 15000, 18000, 22000 };
        for (int i = 0; i < 5; i++)
        {
            Assert.AreEqual(expect[i], s[12 + i].Price, $"단계 {13 + i}");
        }
    }

    [Test]
    public void MonsterGold_WithinFormulaRange()
    {
        TextAsset csv = Resources.Load<TextAsset>("MonsterSheet");
        Assert.IsNotNull(csv, "MonsterSheet.csv");
        string[] lines = csv.text.Split('\n');
        int checkedCount = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Trim().Split(',');
            if (c.Length < 13 || !int.TryParse(c[12], NumberStyles.Integer, CultureInfo.InvariantCulture, out int lv) || lv <= 0)
            {
                continue;
            }
            checkedCount++;
            int lo = lv * 3 + 2, hi = lv * 5 + 2;
            Assert.AreEqual(lo, GoldDropRules.Total(lv, false, false, 0f), c[0]);
            Assert.AreEqual(hi, GoldDropRules.Total(lv, false, false, 1f), c[0]);
            Assert.AreEqual(lo * 3, GoldDropRules.Total(lv, true, false, 0f), c[0]);
            Assert.AreEqual(hi * 10, GoldDropRules.Total(lv, false, true, 1f), c[0]);
        }
        Assert.Greater(checkedCount, 10, "레벨 있는 몹 행");
    }

    [Test]
    public void Enhance_TopGoldCosts_Kept()
    {
        Assert.AreEqual(1800, EnhanceRules.Gold(8));
        Assert.AreEqual(2500, EnhanceRules.Gold(9));
    }
}
