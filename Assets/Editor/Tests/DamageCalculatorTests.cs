using NUnit.Framework;

/// <summary>데미지 공식(CombatSpec 4절) 회귀 테스트</summary>
public class DamageCalculatorTests
{
    // [min, max, 치명%, 치피%, 증뎀%, 방무%, 스킬뎀%, 최종뎀%, 추가타%, armorAtp]
    private static int[] Weapon(int min = 10, int max = 10, int crit = 0, int critDmg = 0, int inc = 0,
                                int ign = 0, int skill = 0, int fin = 0, int add = 0, int atp = 0)
        => new[] { min, max, crit, critDmg, inc, ign, skill, fin, add, atp };

    private static DamageCalculator.Result Calc(int[] w, int def = 0, bool bleed = false, bool skill = false,
                                                float baseRoll = 0f, float critRoll = 99.9f,
                                                DamageCalculator.PlayerOffense p = default)
        => DamageCalculator.Calculate(w, p, def, bleed, skill, 1f, baseRoll, critRoll);

    [Test] public void 기본_피해는_최소값()
        => Assert.AreEqual(10, Calc(Weapon()).damage);

    [Test] public void 최대_피해가_나올_수_있다()
        => Assert.AreEqual(20, Calc(Weapon(min: 10, max: 20), baseRoll: 1f).damage);

    [Test] public void 치명타_기본_치피_20퍼센트()
    {
        var r = Calc(Weapon(min: 100, max: 100, crit: 100), critRoll: 0f);
        Assert.IsTrue(r.isCrit);
        Assert.AreEqual(120, r.damage);
    }

    [Test] public void 퍼센트_보정이_100미만이어도_적용된다()
    {
        Assert.AreEqual(150, Calc(Weapon(min: 100, max: 100, inc: 50)).damage, "증뎀 50%");
        Assert.AreEqual(130, Calc(Weapon(min: 100, max: 100, skill: 30), skill: true).damage, "스킬뎀 30%");
        Assert.AreEqual(110, Calc(Weapon(min: 100, max: 100, fin: 10)).damage, "최종뎀 10%");
    }

    [Test] public void 장비_증뎀은_비치명에도_적용된다()
    {
        var p = new DamageCalculator.PlayerOffense { incDmg = 20 };
        Assert.AreEqual(120, Calc(Weapon(min: 100, max: 100), p: p).damage);
    }

    [Test] public void 방어_공식과_방무()
    {
        // 100² / (100 + 100) = 50
        Assert.AreEqual(50, Calc(Weapon(min: 100, max: 100), def: 100).damage);
        // 방무 50% → 실효 방어 50 → 100² / 150 = 66.7 → 67
        Assert.AreEqual(67, Calc(Weapon(min: 100, max: 100, ign: 50), def: 100).damage);
        // 방무 100% → 방어 무시
        Assert.AreEqual(100, Calc(Weapon(min: 100, max: 100, ign: 100), def: 100).damage);
    }

    [Test] public void 출혈_대상_증뎀()
    {
        var p = new DamageCalculator.PlayerOffense { bleedingAddDmg = 25 };
        Assert.AreEqual(100, Calc(Weapon(min: 100, max: 100), p: p).damage, "출혈 아님");
        Assert.AreEqual(125, Calc(Weapon(min: 100, max: 100), bleed: true, p: p).damage, "출혈");
    }

    [Test] public void 추가타()
        => Assert.AreEqual(30, Calc(Weapon(min: 100, max: 100, add: 30)).extraDamage);

    [Test] public void 피해는_최소_1()
        => Assert.AreEqual(1, Calc(Weapon(min: 1, max: 1), def: 1000).damage);

    [Test] public void 받는_피해_공식()
    {
        Assert.AreEqual(10, DamageCalculator.IncomingToPlayer(20, 20, 1f));   // 400/40
        Assert.AreEqual(13, DamageCalculator.IncomingToPlayer(20, 20, 1.2f)); // 24²/44 = 13.09
    }
}
