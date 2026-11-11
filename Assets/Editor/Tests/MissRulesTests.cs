using NUnit.Framework;

/// <summary>미스(회피) 규칙 (CombatSpec.md 4-1절, REWORK-13)</summary>
public class MissRulesTests
{
    [TestCase(13, 10, 0)]   // +3: 미스 없음
    [TestCase(14, 10, 6)]   // +4: 6%
    [TestCase(15, 10, 12)]
    [TestCase(23, 10, 60)]  // +13: 상한 60
    [TestCase(60, 10, 60)]
    [TestCase(5, 10, 0)]    // 몹이 낮으면 내 공격은 안 빗나감
    [TestCase(0, 10, 0)]    // 레벨 미지정
    public void 내_공격_미스율(int mob, int player, int expected)
        => Assert.AreEqual(expected, MissRules.PlayerAttackMissPercent(mob, player, false, 0));

    [TestCase(10, 13, 0)]
    [TestCase(10, 14, 6)]
    [TestCase(10, 30, 50)]  // 상한 50
    [TestCase(13, 10, 0)]   // 몹이 더 높으면 안 빗나감
    public void 몹_공격_미스율(int mob, int player, int expected)
        => Assert.AreEqual(expected, MissRules.MobAttackMissPercent(mob, player, false, 0));

    [Test] public void 보스는_양방향_미스_없음()
    {
        Assert.AreEqual(0, MissRules.PlayerAttackMissPercent(40, 10, true, 0));
        Assert.AreEqual(0, MissRules.MobAttackMissPercent(10, 40, true, 20));
    }

    [Test] public void 명중은_내_미스율을_깎고_회피는_몹_미스율을_올린다()
    {
        Assert.AreEqual(6, MissRules.PlayerAttackMissPercent(15, 10, false, 6));  // 12-6
        Assert.AreEqual(0, MissRules.PlayerAttackMissPercent(15, 10, false, 50)); // 하한 0
        Assert.AreEqual(10, MissRules.MobAttackMissPercent(10, 10, false, 10));   // 같은 레벨도 회피 10%
        Assert.AreEqual(0, MissRules.MobAttackMissPercent(0, 40, false, 10));     // 몹 Lv 미지정
    }

    [TestCase(6, 5.9f, true)]
    [TestCase(6, 6f, false)]
    [TestCase(0, 0f, false)]
    public void 난수_판정(int percent, float roll, bool miss)
        => Assert.AreEqual(miss, MissRules.IsMiss(percent, roll));
}
