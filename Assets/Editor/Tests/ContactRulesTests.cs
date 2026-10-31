using NUnit.Framework;

/// <summary>몸박·레벨 차 피해 감소 규칙 (ClassicMapleDesign.md 3.3)</summary>
public class ContactRulesTests
{
    [TestCase(10, 10, 1f)]
    [TestCase(13, 10, 1f)]      // +3 = 0%
    [TestCase(14, 10, 0.97f)]   // +4 = -3%
    [TestCase(15, 10, 0.94f)]   // +5 = -6%
    [TestCase(23, 10, 0.70f)]   // +13 = -30% 상한
    [TestCase(40, 10, 0.70f)]   // 상한 유지
    [TestCase(5, 10, 1f)]       // 몹이 낮으면 감소 없음
    [TestCase(0, 10, 1f)]       // 레벨 미지정 몹
    public void 레벨_차_감소(int mob, int player, float expected)
        => Assert.AreEqual(expected, DamageCalculator.LevelGapMul(mob, player), 0.0001f);

    [Test] public void 레벨_차_감소는_피해에_곱해진다()
    {
        var w = new[] { 100, 100, 0, 0, 0, 0, 0, 0, 0, 0 };
        var r = DamageCalculator.Calculate(w, default, 0, false, false, 1f, 0f, 99.9f, DamageCalculator.LevelGapMul(23, 10));
        Assert.AreEqual(70, r.damage);
    }

    [Test] public void 레벨_차_감소가_있어도_최소_1()
    {
        var w = new[] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        var r = DamageCalculator.Calculate(w, default, 0, false, false, 1f, 0f, 99.9f, 0.7f);
        Assert.AreEqual(1, r.damage);
    }

    [Test] public void 넉백_방향은_몹_반대쪽()
    {
        Assert.AreEqual(1f, ContactRules.KnockSign(5f, 3f));
        Assert.AreEqual(-1f, ContactRules.KnockSign(1f, 3f));
    }

    [Test] public void 몸박_피해는_음수가_없다()
        => Assert.AreEqual(0, ContactRules.RawDamage(-5));

    [Test] public void 피격_무적은_0점9초()
        => Assert.AreEqual(0.9f, ContactRules.InvincibleTime, 0.0001f);
}
