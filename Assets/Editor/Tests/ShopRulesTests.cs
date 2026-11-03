using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectB.EditorTools;

/// <summary>상점 규칙(ShopRules·ShopStockTable·되사기 선택) 테스트 — 메이플식 상점 기능, BasicFeatures.md 14절</summary>
public class ShopRulesTests
{
    [Test]
    public void MaxBuyable_LimitedByStackGoldAndBag()
    {
        Assert.AreEqual(99, ShopRules.MaxBuyable(99, 10, 100000, 500)); // 스택 한도
        Assert.AreEqual(7, ShopRules.MaxBuyable(99, 10, 75, 500));      // 골드
        Assert.AreEqual(12, ShopRules.MaxBuyable(99, 10, 100000, 12));  // 가방 여유
        Assert.AreEqual(0, ShopRules.MaxBuyable(99, 10, 9, 500));
        Assert.AreEqual(0, ShopRules.MaxBuyable(99, 10, 100, 0));
        Assert.AreEqual(5, ShopRules.MaxBuyable(99, 0, 0, 5));          // 공짜는 골드 제한 없음(0 나누기 방지)
        Assert.AreEqual(0, ShopRules.MaxBuyable(-3, 10, 100, 5));
    }

    [Test]
    public void CheckBuy_GoldReportedBeforeBag()
    {
        Assert.AreEqual(ShopRules.BuyBlock.None, ShopRules.CheckBuy(10, 10, 1));
        Assert.AreEqual(ShopRules.BuyBlock.NoGold, ShopRules.CheckBuy(10, 9, 5));
        Assert.AreEqual(ShopRules.BuyBlock.NoGold, ShopRules.CheckBuy(10, 9, 0));
        Assert.AreEqual(ShopRules.BuyBlock.BagFull, ShopRules.CheckBuy(10, 100, 0));
    }

    [Test]
    public void RefillAmount_FillsToTargetWithinGoldAndBag()
    {
        Assert.AreEqual(0, ShopRules.RefillAmount(99, 99, 25, 10000, 50)); // 이미 가득
        Assert.AreEqual(0, ShopRules.RefillAmount(120, 99, 25, 10000, 50));
        Assert.AreEqual(89, ShopRules.RefillAmount(10, 99, 25, 100000, 500));
        Assert.AreEqual(8, ShopRules.RefillAmount(10, 99, 25, 200, 500));  // 골드가 모자라면 살 수 있는 만큼
        Assert.AreEqual(3, ShopRules.RefillAmount(10, 99, 25, 100000, 3)); // 가방이 모자라면 들어가는 만큼
        Assert.AreEqual(99, ShopRules.RefillTarget(99));
        Assert.AreEqual(1, ShopRules.RefillTarget(0));
    }

    [Test]
    public void SellFilter_MatchesTypeAndCycles()
    {
        Assert.IsTrue(ShopRules.FilterMatches(SellFilter.All, "Equip"));
        Assert.IsTrue(ShopRules.FilterMatches(SellFilter.Equip, "Equip"));
        Assert.IsFalse(ShopRules.FilterMatches(SellFilter.Equip, "Consum"));
        Assert.IsTrue(ShopRules.FilterMatches(SellFilter.Consum, "Consum"));
        Assert.IsTrue(ShopRules.FilterMatches(SellFilter.Misc, "Misc"));
        Assert.IsFalse(ShopRules.FilterMatches(SellFilter.Misc, "Equip"));
        var f = SellFilter.All;
        var seen = new List<SellFilter>();
        for (int i = 0; i < 4; i++)
        {
            f = ShopRules.NextFilter(f);
            seen.Add(f);
        }
        CollectionAssert.AreEqual(new[] { SellFilter.Equip, SellFilter.Consum, SellFilter.Misc, SellFilter.All }, seen);
    }

    [Test]
    public void SaleTotal_NoOverflow_AndBuyPriceMatchesOldFormula()
    {
        Assert.AreEqual(3000000000L, ShopRules.SaleTotal(1500000000, 2));
        Assert.AreEqual(0L, ShopRules.SaleTotal(-5, 3));
        Assert.AreEqual(25, ShopRules.BuyPrice(17)); // 17 x 1.5 = 25.5 -> 25
        Assert.AreEqual(0, ShopRules.BuyPrice(0));
    }

    private const string Csv = "Page,Row,ItemId,MinFloor,PageName\r\n"
        + "0,0,A,1,보급\r\n0,0,B,1,보급\r\n0,1,C,2,보급\r\n1,0,D,1,장비\r\n1,1,E,3,장비\r\n"
        + "bad,line\r\n,,,\r\n2,0,F,4,고급\r\n";

    [Test]
    public void StockTable_FiltersByFloor_AndSkipsBadLines()
    {
        var t = ShopStockTable.Parse(Csv);
        Assert.AreEqual(6, t.Entries.Count);
        var layout = t.Layout(1, null, out var names);
        Assert.AreEqual(2, layout.Count);              // 2페이지(F 는 4층계부터)
        CollectionAssert.AreEqual(new[] { "A", "B" }, layout[0][0]);
        Assert.AreEqual(1, layout[0].Count);           // C 는 2층계부터 -> 줄 1개
        CollectionAssert.AreEqual(new[] { "보급", "장비" }, names);

        layout = t.Layout(2, null, out names);
        Assert.AreEqual(2, layout[0].Count);
        CollectionAssert.AreEqual(new[] { "C" }, layout[0][1]);

        layout = t.Layout(4, null, out names);
        Assert.AreEqual(3, layout.Count);
        Assert.AreEqual(2, layout[1].Count);           // E(3층계) 합류
        Assert.AreEqual("고급", names[2]);
    }

    [Test]
    public void StockTable_MapOnly_ShownOnlyInThatMap_NoColumnIsGlobal()
    {
        string csv = "Page,Row,ItemId,MinFloor,PageName,MapOnly\n0,0,A,1,x\n2,0,S,1,fort,Lv1_BanditFortress\n";
        var t = ShopStockTable.Parse(csv);
        Assert.AreEqual(1, t.Layout(1, null, out _, "Lv1_Town").Count);
        Assert.AreEqual(1, t.Layout(1, null, out _, null).Count);
        var fort = t.Layout(1, null, out var names, "Lv1_BanditFortress");
        Assert.AreEqual(2, fort.Count);
        CollectionAssert.AreEqual(new[] { "S" }, fort[1][0]);
        Assert.AreEqual("fort", names[1]);
        Assert.AreEqual(2, ShopStockTable.Parse(Csv).Layout(1, null, out _, "Lv1_BanditFortress").Count); // 열 없는 CSV 호환
        Assert.IsTrue(ShopStockTable.MapAllows("", "any"));
    }

    [Test]
    public void StockTable_ExistsFilterDropsUnknown_AndEmptyPagesCollapse()
    {
        var t = ShopStockTable.Parse(Csv);
        var layout = t.Layout(9, id => id != "D" && id != "E", out var names);
        Assert.AreEqual(2, layout.Count);              // 장비 페이지가 통째로 사라지고 고급 페이지가 앞으로
        Assert.AreEqual("고급", names[1]);
        Assert.AreEqual(0, ShopStockTable.Parse("").Layout(1, null, out names).Count);
    }

    [Test]
    public void ShopStockCsv_EveryItemExistsInItemSheet_AndFloor1HasPotions()
    {
        string text = File.ReadAllText("Assets/Resources/ShopStock.csv");
        var table = ShopStockTable.Parse(text);
        Assert.Greater(table.Entries.Count, 10);
        var names = new HashSet<string>(ItemTable.Load().Rows.Select(r => r.Name));
        foreach (ShopStockTable.Entry e in table.Entries)
        {
            Assert.IsTrue(names.Contains(e.Id), "ItemSheet 에 없는 진열 품목: " + e.Id);
        }
        var layout = table.Layout(1, null, out _);
        Assert.AreEqual(2, layout.Count);
        CollectionAssert.Contains(layout[0][0], "Small HP Potion");
    }

    [Test]
    public void Buyback_TakeAt_PicksAnyEntry_AndKeepsOrder()
    {
        var m = new BuybackModel();
        m.Record(new BuybackModel.Sold { Id = "A", Count = 1, Price = 1 });
        m.Record(new BuybackModel.Sold { Id = "B", Count = 2, Price = 2 });
        m.Record(new BuybackModel.Sold { Id = "C", Count = 3, Price = 3 });
        int v = m.Version;
        Assert.IsTrue(m.TryTakeAt(1, out var s)); // 최근부터 1번째 = B
        Assert.AreEqual("B", s.Id);
        Assert.AreEqual(2, m.Count);
        Assert.AreEqual("C", m.PeekAt(0).Id);
        Assert.AreEqual("A", m.PeekAt(1).Id);
        Assert.Greater(m.Version, v);
        Assert.IsFalse(m.TryTakeAt(2, out s));
        Assert.IsFalse(m.TryTakeAt(-1, out s));
        Assert.IsNull(s.Id);
        Assert.AreEqual(2, m.Count);
    }
}
