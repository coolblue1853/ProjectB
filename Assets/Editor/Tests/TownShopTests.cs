using System.Collections.Generic;
using NUnit.Framework;
using ProjectB.EditorTools;

public class TownShopTests
{
    [Test]
    public void 진열은_유효한_품목만_판매하고_되팔기_차익이_없다()
    {
        var seen = new HashSet<string>();
        foreach (string[] page in new[] { ShopStockExtender.ExtraItems, ShopStockExtender.Supplies, ShopStockExtender.Weapons, ShopStockExtender.Armor })
        {
            foreach (string id in page)
            {
                ItemRow row = ItemTable.Load().Rows.Find(r => r.Name == id);
                Assert.IsNotNull(row, id);
                Assert.IsTrue(seen.Add(id), "중복 진열: " + id);
                Assert.Greater((int)(row.Price * 1.5f), row.Price, id);
                Assert.AreEqual("C", row.Rarity, "보급 상점에는 일반품만: " + id);
            }
        }
    }

    [Test]
    public void 퀘스트_계보무기는_상점에서_건너뛰지_않는다()
    {
        foreach (string id in new[] { "LeafAdae", "FangDagger", "BearClaw", "RootStaff", "CrocJawAxe", "HighArmor", "Potion" })
        {
            CollectionAssert.DoesNotContain(ShopStockExtender.Weapons, id);
            CollectionAssert.DoesNotContain(ShopStockExtender.Armor, id);
            CollectionAssert.DoesNotContain(ShopStockExtender.ExtraItems, id);
        }
    }

    [Test]
    public void 기본_장비_한벌은_첫시간_골드_예산을_넘지_않는다()
    {
        int cost = 0;
        foreach (string id in ShopStockExtender.Armor)
        {
            ItemRow row = ItemTable.Load().Rows.Find(r => r.Name == id);
            Assert.AreEqual("Equip", row.Type);
            cost += (int)(row.Price * 1.5f);
        }
        // 기본 방어구 + 검 + 중물약 20개 + 귀환 2장은 6,000G 안에서 선택 가능하다.
        cost += 450 + 45 * 20 + 120 * 2;
        Assert.LessOrEqual(cost, 6000);
    }
}
