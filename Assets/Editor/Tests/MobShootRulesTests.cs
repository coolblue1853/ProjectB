using NUnit.Framework;

public class MobShootRulesTests
{
    [Test]
    public void TryGetProfile_Shoot_Rune_None()
    {
        Assert.IsTrue(MobShootRules.TryGetProfile("shoot", out var p));
        Assert.AreEqual(MobShootRules.Shoot.MaxRange, p.MaxRange);
        Assert.IsTrue(MobShootRules.TryGetProfile("slow|rune", out p));
        Assert.AreEqual(MobShootRules.Rune.Windup, p.Windup);
        Assert.IsFalse(MobShootRules.TryGetProfile("dash|slow", out _));
        Assert.IsFalse(MobShootRules.TryGetProfile("", out _));
        Assert.IsFalse(MobShootRules.TryGetProfile(null, out _));
    }

    [Test]
    public void CanStart_RangeBoundariesInclusive()
    {
        var p = MobShootRules.Shoot;
        Assert.IsTrue(MobShootRules.CanStart(p, p.MinRange, 0f, true));
        Assert.IsTrue(MobShootRules.CanStart(p, -p.MaxRange, 0f, true));
        Assert.IsFalse(MobShootRules.CanStart(p, p.MinRange - 0.01f, 0f, true));
        Assert.IsFalse(MobShootRules.CanStart(p, p.MaxRange + 0.01f, 0f, true));
    }

    [Test]
    public void CanStart_VerticalGapAndCooldown()
    {
        var p = MobShootRules.Shoot;
        Assert.IsTrue(MobShootRules.CanStart(p, 5f, RangedAttackRules.MaxVerticalGap, true));
        Assert.IsFalse(MobShootRules.CanStart(p, 5f, RangedAttackRules.MaxVerticalGap + 0.1f, true));
        Assert.IsFalse(MobShootRules.CanStart(p, 5f, 0f, false));
    }

    [Test]
    public void ShotDamage_ScalesFromContact()
    {
        Assert.AreEqual(10, MobShootRules.ShotDamage(20, MobShootRules.Shoot.DamageMul));
        Assert.AreEqual(17, MobShootRules.ShotDamage(28, MobShootRules.Rune.DamageMul));
        Assert.AreEqual(1, MobShootRules.ShotDamage(1, 0.1f));
        Assert.AreEqual(0, MobShootRules.ShotDamage(0, 0.5f));
    }

    [Test]
    public void Profiles_EarlyMobStaysModest()
    {
        // 초반 숲 몹: 몸박보다 약하고 쿨이 길다
        Assert.LessOrEqual(MobShootRules.Shoot.DamageMul, 0.6f);
        Assert.GreaterOrEqual(MobShootRules.Shoot.Cooldown, 3f);
        Assert.Greater(MobShootRules.Rune.Cooldown, MobShootRules.Shoot.Cooldown);
    }

    [Test]
    public void Csv_NewMobsCarryShootTraits()
    {
        string csv = UnityEngine.Resources.Load<UnityEngine.TextAsset>("MonsterSheet").text;
        Assert.IsTrue(csv.Contains("ForestHunter_BD,5E7F3A,shoot,"));
        Assert.IsTrue(csv.Contains("RuinWarden_BD,7F8FB0,rune,"));
    }
}
