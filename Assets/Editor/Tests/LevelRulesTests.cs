using NUnit.Framework;

/// <summary>레벨·EXP 규칙 (ClassicMapleDesign.md 3.1·8절) + MonsterSheet 새 컬럼</summary>
public class LevelRulesTests
{
    [Test]
    public void ExpTable_Lv1To15_MatchesDesign()
    {
        int[] expect = { 500, 650, 800, 1200, 1450, 2900, 3500, 4200, 5000, 6000, 11500, 14000, 17000, 20500, 24500 };
        for (int i = 0; i < expect.Length; i++)
        {
            Assert.AreEqual(expect[i], LevelRules.ExpToNext(i + 1), "Lv" + (i + 1));
        }
        int cumulative = 0;
        for (int lv = 1; lv <= 15; lv++)
        {
            cumulative += LevelRules.ExpToNext(lv);
        }
        Assert.AreEqual(113700, cumulative); // Lv16 누적
    }

    [Test]
    public void ExpTable_Extrapolation_IncreasesAndFitsInt()
    {
        for (int lv = 15; lv < 69; lv++)
        {
            Assert.Greater(LevelRules.ExpToNext(lv + 1), LevelRules.ExpToNext(lv), "Lv" + (lv + 1));
        }
        Assert.AreEqual(0, LevelRules.ExpToNext(70));
        Assert.Greater(LevelRules.ExpToNext(69), 0);
    }

    [Test]
    public void AddExp_MultipleLevelsAtOnce()
    {
        int level = 1, exp = 0;
        int ups = LevelRules.AddExp(ref level, ref exp, 500 + 650 + 800 + 100);
        Assert.AreEqual(3, ups);
        Assert.AreEqual(4, level);
        Assert.AreEqual(100, exp);
    }

    [Test]
    public void AddExp_CarriesRemainder_AndBelowThresholdNoLevel()
    {
        int level = 1, exp = 400;
        Assert.AreEqual(0, LevelRules.AddExp(ref level, ref exp, 99));
        Assert.AreEqual(499, exp);
        Assert.AreEqual(1, LevelRules.AddExp(ref level, ref exp, 2));
        Assert.AreEqual(2, level);
        Assert.AreEqual(1, exp);
    }

    [Test]
    public void AddExp_CapsAtMaxLevel()
    {
        int level = 69, exp = 0;
        int ups = LevelRules.AddExp(ref level, ref exp, long.MaxValue / 4);
        Assert.AreEqual(1, ups);
        Assert.AreEqual(70, level);
        Assert.AreEqual(0, exp);
        Assert.AreEqual(0, LevelRules.AddExp(ref level, ref exp, 999999));
        Assert.AreEqual(70, level);
        Assert.AreEqual(0, exp);
    }

    [Test]
    public void DeathPenalty_FivePercent_NoLevelDown()
    {
        Assert.AreEqual(950, LevelRules.ExpAfterDeath(1000));
        Assert.AreEqual(0, LevelRules.ExpAfterDeath(0));
        Assert.AreEqual(19, LevelRules.ExpAfterDeath(19)); // 5% 내림 → 0 손실
        Assert.AreEqual(0, LevelRules.ExpAfterDeath(-5));
    }

    [Test]
    public void KillExp_EliteX4_BossFixed()
    {
        Assert.AreEqual(43, LevelRules.KillExp(43, false, false));
        Assert.AreEqual(172, LevelRules.KillExp(43, true, false));
        Assert.AreEqual(950, LevelRules.KillExp(950, true, true)); // 보스는 정예 승격 없음
        Assert.AreEqual(0, LevelRules.KillExp(0, true, false));
    }

    [Test]
    public void Growth_PerLevel()
    {
        Assert.AreEqual(0, LevelRules.HpBonus(1));
        Assert.AreEqual(8, LevelRules.HpBonus(2));
        Assert.AreEqual(552, LevelRules.HpBonus(70)); // 200 + 552 = 752
        Assert.AreEqual(0, LevelRules.AtkBonus(2));
        Assert.AreEqual(1, LevelRules.AtkBonus(3));
        Assert.AreEqual(34, LevelRules.AtkBonus(70));
        Assert.AreEqual(LevelRules.AtkBonus(41), LevelRules.DefBonus(41));
    }

    [Test]
    public void MonsterSheet_NewColumns_Parsed()
    {
        MonsterTable.Row r = MonsterTable.Get("AncientEnt_BD");
        Assert.IsNotNull(r);
        Assert.AreEqual(9, r.level);
        Assert.AreEqual(950, r.exp);
        Assert.AreEqual(43, r.contactDamage);
        foreach (MonsterTable.Row row in MonsterTable.All())
        {
            Assert.IsTrue(row.level.HasValue && row.level.Value >= 1 && row.level.Value <= 70, row.id + " level");
            Assert.IsTrue(row.exp.HasValue && row.exp.Value > 0, row.id + " exp");
            Assert.IsTrue(row.contactDamage.HasValue && row.contactDamage.Value > 0, row.id + " contactDamage");
        }
    }
}
