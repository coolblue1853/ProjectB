using NUnit.Framework;

/// <summary>AUTO-71: 레벨·골드드롭·사냥 공식 경계값 고정(현재 동작 기록).</summary>
public class BoundaryRulesATests
{
    [Test]
    public void ExpToNext_ZeroAndNegative_TreatedAsLevel1()
    {
        Assert.AreEqual(500, LevelRules.ExpToNext(1));
        Assert.AreEqual(500, LevelRules.ExpToNext(0));
        Assert.AreEqual(500, LevelRules.ExpToNext(-10));
    }

    [Test]
    public void ExpToNext_BeforeAndAtMaxLevel()
    {
        Assert.Greater(LevelRules.ExpToNext(LevelRules.MaxLevel - 1), 0);
        Assert.AreEqual(0, LevelRules.ExpToNext(LevelRules.MaxLevel));
        Assert.AreEqual(0, LevelRules.ExpToNext(LevelRules.MaxLevel + 1));
        Assert.AreEqual(0, LevelRules.ExpToNext(int.MaxValue));
    }

    [Test]
    public void AddExp_ExactNeed_LevelsUpWithZeroExp()
    {
        int level = 1, exp = 0;
        Assert.AreEqual(1, LevelRules.AddExp(ref level, ref exp, 500));
        Assert.AreEqual(2, level);
        Assert.AreEqual(0, exp);
    }

    [Test]
    public void AddExp_OneShortOfNeed_NoLevelUp()
    {
        int level = 1, exp = 0;
        Assert.AreEqual(0, LevelRules.AddExp(ref level, ref exp, 499));
        Assert.AreEqual(1, level);
        Assert.AreEqual(499, exp);
    }

    [Test]
    public void AddExp_NegativeGain_KeepsExp()
    {
        int level = 5, exp = 100;
        Assert.AreEqual(0, LevelRules.AddExp(ref level, ref exp, -999));
        Assert.AreEqual(5, level);
        Assert.AreEqual(100, exp);
    }

    [Test]
    public void AddExp_LevelBelowOne_ClampedToOne()
    {
        int level = 0, exp = 0;
        LevelRules.AddExp(ref level, ref exp, 0);
        Assert.AreEqual(1, level);
    }

    [Test]
    public void AddExp_HugeGain_StopsAtMaxLevelWithZeroExp()
    {
        int level = LevelRules.MaxLevel - 1, exp = 0;
        Assert.AreEqual(1, LevelRules.AddExp(ref level, ref exp, long.MaxValue / 2));
        Assert.AreEqual(LevelRules.MaxLevel, level);
        Assert.AreEqual(0, exp);
    }

    [Test]
    public void AddExp_AtMaxLevel_NoChange()
    {
        int level = LevelRules.MaxLevel, exp = 77;
        Assert.AreEqual(0, LevelRules.AddExp(ref level, ref exp, 1000));
        Assert.AreEqual(0, exp);
    }

    [Test]
    public void KillExp_ZeroAndNegativeBase_Zero()
    {
        Assert.AreEqual(0, LevelRules.KillExp(0, true, false));
        Assert.AreEqual(0, LevelRules.KillExp(-5, false, false));
    }

    [Test]
    public void KillExp_EliteTimes4_BossNoEliteBonus()
    {
        Assert.AreEqual(40, LevelRules.KillExp(10, true, false));
        Assert.AreEqual(10, LevelRules.KillExp(10, true, true));
    }

    [Test]
    public void DeathLoss_BoundaryAtLevel10And11()
    {
        Assert.AreEqual(0, LevelRules.DeathLossPercent(LevelRules.DeathFreeMaxLevel, false));
        Assert.AreEqual(5, LevelRules.DeathLossPercent(LevelRules.DeathFreeMaxLevel + 1, false));
    }

    [Test]
    public void ExpAfterDeath_ZeroNegativeAndRounding()
    {
        Assert.AreEqual(0, LevelRules.ExpAfterDeath(0, 20));
        Assert.AreEqual(0, LevelRules.ExpAfterDeath(-50, 20));
        Assert.AreEqual(100, LevelRules.ExpAfterDeath(100, 10));
        Assert.AreEqual(95, LevelRules.ExpAfterDeath(100, 11));
        Assert.AreEqual(19, LevelRules.ExpAfterDeath(19, 11)); // 19*5/100 = 0 (내림)
    }

    [Test]
    public void Bonuses_ClampedToLevelRange()
    {
        Assert.AreEqual(0, LevelRules.HpBonus(0));
        Assert.AreEqual(0, LevelRules.HpBonus(1));
        Assert.AreEqual(69 * LevelRules.HpPerLevel, LevelRules.HpBonus(LevelRules.MaxLevel));
        Assert.AreEqual(LevelRules.HpBonus(LevelRules.MaxLevel), LevelRules.HpBonus(LevelRules.MaxLevel + 5));
        Assert.AreEqual(0, LevelRules.AtkBonus(2));
        Assert.AreEqual(1, LevelRules.AtkBonus(3));
        Assert.AreEqual(LevelRules.AtkBonus(5), LevelRules.DefBonus(5));
    }

    [Test]
    public void GoldTotal_ZeroAndNegativeLevel_Zero()
    {
        Assert.AreEqual(0, GoldDropRules.Total(0, true, true, 0.5f));
        Assert.AreEqual(0, GoldDropRules.Total(-3, false, false, 0.5f));
    }

    [Test]
    public void GoldTotal_RollClampedAndMultipliers()
    {
        Assert.AreEqual(5, GoldDropRules.Total(1, false, false, 0f));
        Assert.AreEqual(7, GoldDropRules.Total(1, false, false, 1f));
        Assert.AreEqual(5, GoldDropRules.Total(1, false, false, -1f));
        Assert.AreEqual(7, GoldDropRules.Total(1, false, false, 2f));
        Assert.AreEqual(15, GoldDropRules.Total(1, true, false, 0f));
        Assert.AreEqual(50, GoldDropRules.Total(1, true, true, 0f)); // 보스 배율 우선
    }

    [Test]
    public void ShouldDrop_ChanceBoundaries()
    {
        Assert.IsTrue(GoldDropRules.ShouldDrop(false, false, 0f));
        Assert.IsFalse(GoldDropRules.ShouldDrop(false, false, GoldDropRules.NormalChance)); // 경계는 미드롭
        Assert.IsFalse(GoldDropRules.ShouldDrop(false, false, 1f));
        Assert.IsTrue(GoldDropRules.ShouldDrop(true, false, 1f));
        Assert.IsTrue(GoldDropRules.ShouldDrop(false, true, 1f));
    }

    [Test]
    public void Share_PreservesTotalAndHandlesEdges()
    {
        Assert.AreEqual(0, GoldDropRules.Share(0, 3, 1));
        Assert.AreEqual(9, GoldDropRules.Share(9, 0, 0));
        Assert.AreEqual(0, GoldDropRules.Share(1, 3, 1));
        int sum = GoldDropRules.Share(10, 3, 0) + GoldDropRules.Share(10, 3, 1) + GoldDropRules.Share(10, 3, 2);
        Assert.AreEqual(10, sum);
        Assert.AreEqual(4, GoldDropRules.Share(10, 3, 0));
    }

    [Test]
    public void Tier_BoundariesAt19_20_59_60()
    {
        Assert.AreEqual(0, GoldDropRules.Tier(-1));
        Assert.AreEqual(0, GoldDropRules.Tier(19));
        Assert.AreEqual(1, GoldDropRules.Tier(20));
        Assert.AreEqual(1, GoldDropRules.Tier(59));
        Assert.AreEqual(2, GoldDropRules.Tier(60));
    }

    [Test]
    public void GoldPerKill_NonPositiveLevelAndEliteChanceClamp()
    {
        Assert.AreEqual(0d, HuntingRules.GoldPerKill(0, 0.5f), 1e-9);
        Assert.AreEqual(0d, HuntingRules.GoldPerKill(-1, 0.5f), 1e-9);
        Assert.AreEqual(HuntingRules.GoldPerKill(1, 0f), HuntingRules.GoldPerKill(1, -3f), 1e-9);
        Assert.AreEqual(HuntingRules.GoldPerKill(1, 1f), HuntingRules.GoldPerKill(1, 9f), 1e-9);
        Assert.AreEqual(6d * GoldDropRules.NormalChance, HuntingRules.GoldPerKill(1, 0f), 1e-6);
    }

    [Test]
    public void ObservedDamageFactor_ZeroDenominatorsFallBackToOne()
    {
        Assert.AreEqual(1d, HuntingRules.ObservedDamageFactor(0, 0, 100, 5), 1e-9);
        Assert.AreEqual(1d, HuntingRules.ObservedDamageFactor(10, 0, 100, 5), 1e-9);
        Assert.AreEqual(1d, HuntingRules.ObservedDamageFactor(10, 5, 0, 5), 1e-9);
        Assert.AreEqual(1d, HuntingRules.ObservedDamageFactor(10, 5, 100, 0), 1e-9);
        Assert.AreEqual(1d, HuntingRules.ObservedDamageFactor(-1, 5, 100, 5), 1e-9);
    }

    [Test]
    public void PotionCount_ZeroGuards()
    {
        Assert.AreEqual(0d, HuntingRules.PotionCount(0, 50, 100), 1e-9);
        Assert.AreEqual(0d, HuntingRules.PotionCount(100, 0, 100), 1e-9);
        Assert.AreEqual(0d, HuntingRules.PotionCount(100, 50, 0), 1e-9);
        Assert.AreEqual(2d, HuntingRules.PotionCount(100, 500, 50), 1e-9); // 회복량은 최대 HP 로 제한
    }

    [Test]
    public void ExpToReach_Level1AndBelowZero_AndFirstStep()
    {
        Assert.AreEqual(0, HuntingRules.ExpToReach(1));
        Assert.AreEqual(0, HuntingRules.ExpToReach(0));
        Assert.AreEqual(500, HuntingRules.ExpToReach(2));
    }

    [Test]
    public void RefillAfter_FirstTickAndCycleBoundary()
    {
        Assert.AreEqual(11d, HuntingRules.RefillAfter(0, 10), 1e-9);
        Assert.AreEqual(11d, HuntingRules.RefillAfter(10, 10), 1e-9);
        Assert.AreEqual(21d, HuntingRules.RefillAfter(11, 10), 1e-9);
    }
}
