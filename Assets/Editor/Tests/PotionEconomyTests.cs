using NUnit.Framework;
using ProjectB.EditorTools;
using UnityEngine;

/// <summary>물약 경제·귀환 주문서 규칙 (ClassicMapleDesign.md 3.3 / 구현 3단계)</summary>
public class PotionEconomyTests
{
    [Test] public void 물약_공유_쿨다운은_2초()
    {
        Assert.AreEqual(2f, ConsumeCooldown.PotionSeconds, 1e-4f);
        var cd = new ConsumeCooldown();
        cd.Start(ConsumeCooldown.PotionGroup, 5f);
        Assert.IsFalse(cd.IsReady(ConsumeCooldown.PotionGroup, 6.99f));
        Assert.IsTrue(cd.IsReady(ConsumeCooldown.PotionGroup, 7f));
    }

    [TestCase("Small HP Potion", 120, 25)]
    [TestCase("Medium HP Potion", 220, 45)]
    [TestCase("Large HP Potion", 320, 70)]
    public void 물약_회복량과_구매가(string id, int heal, int buy)
    {
        ItemRow d = Find(id);
        Assert.IsNotNull(d);
        Assert.AreEqual("hp +", d.EffectOb);
        Assert.AreEqual(heal.ToString(), d.EffectPow);
        Assert.AreEqual(ConsumeCooldown.PotionGroup, ConsumeCooldown.GroupOf(d.EffectOb));
        Assert.AreEqual(buy, (int)(d.Price * 1.5f)); // ItemCheck.buyprice
    }

    [Test] public void 귀환_주문서_행과_구매가()
    {
        ItemRow d = Find("Recall Scroll");
        Assert.IsNotNull(d);
        Assert.AreEqual("Consum", d.Type);
        Assert.IsTrue(d.EffectOb.StartsWith("recall"));
        Assert.AreEqual(120, (int)(d.Price * 1.5f));
    }

    [Test] public void 상점_추가_목록은_모두_시트에_있다()
    {
        foreach (string id in ShopStockExtender.ExtraItems)
        {
            Assert.IsNotNull(Find(id), id);
        }
    }

    [Test] public void 귀환_시전은_2초_후_완료()
    {
        Assert.IsFalse(RecallRules.Completed(1.99f));
        Assert.IsTrue(RecallRules.Completed(2f));
    }

    [Test] public void 귀환_이동하면_취소()
    {
        Assert.IsFalse(RecallRules.Moved(Vector2.zero, new Vector2(0.1f, 0f)));
        Assert.IsTrue(RecallRules.Moved(Vector2.zero, new Vector2(0.25f, 0f)));
        Assert.IsTrue(RecallRules.Moved(Vector2.zero, new Vector2(0f, 0.3f)));
    }

    [Test] public void 귀환_피격_이동_사망이면_취소()
    {
        Assert.IsFalse(RecallRules.ShouldCancel(false, false, false));
        Assert.IsTrue(RecallRules.ShouldCancel(true, false, false));
        Assert.IsTrue(RecallRules.ShouldCancel(false, true, false));
        Assert.IsTrue(RecallRules.ShouldCancel(false, false, true));
    }

    private static ItemRow Find(string id)
    {
        return ItemTable.Load().Rows.Find(r => r.Name == id);
    }
}
