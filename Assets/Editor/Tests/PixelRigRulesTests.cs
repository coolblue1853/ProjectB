using NUnit.Framework;

/// <summary>픽셀 리그 주인공·기본 공격 이펙트 규칙(PixelRigDesign 2절, GameFeelPlan 10.7) 회귀 테스트</summary>
public class PixelRigRulesTests
{
    [TestCase("ShieldUp", "Guard")]
    [TestCase("Spining", "Spin")]
    [TestCase("StaffAttack", "Cast")]
    [TestCase("SpeadBuff", "Cast")]
    [TestCase("RightHandstabbing_OneHand", "Stab")]
    [TestCase("StrongStab", "Stab")]
    [TestCase("RightHandVashUp_OneHand", "SwingUp")]
    [TestCase("upperCut", "SwingUp")]
    [TestCase("GreatSideAttack", "SwingSide")]
    [TestCase("WideVesh", "SwingSide")]
    [TestCase("RightHandVashDown_OneHand", "SwingDown")]
    [TestCase("GreatVashDown", "SwingDown")]
    [TestCase("RopeStay", "RopeStay")]
    [TestCase("RopeDown", "RopeUp")]
    public void MapName_SkillAndAttackNames(string anim, string clip)
    {
        Assert.AreEqual(clip, PixelRig.MapName(anim));
    }

    [Test]
    public void WeaponFx_MeleeSwingPicksTrail()
    {
        Assert.IsTrue(WeaponFx.TryMelee("RightHandstabbing_OneHand", VfxId.SlashWhite, out VfxId id, out bool flip));
        Assert.AreEqual(VfxId.WThrust, id);
        Assert.IsFalse(flip);
        Assert.IsTrue(WeaponFx.TryMelee("RightHandVashUp_OneHand", VfxId.SlashRed, out id, out flip));
        Assert.AreEqual(VfxId.WSlashDown, id);
        Assert.IsTrue(flip, "올려베기는 세로 초승달 상하 반전");
    }

    [Test]
    public void WeaponFx_StaffKeepsOwnEffect()
    {
        Assert.IsFalse(WeaponFx.TryMelee("StaffAttack", VfxId.BurstBlue, out _, out _));
        Assert.IsFalse(WeaponFx.TryMelee("StaffAttack", VfxId.None, out _, out _));
    }

    [Test]
    public void PixelRamp_FromSwingColor()
    {
        Assert.AreEqual(PixelRampId.Red, PixelRamp.FromSwing(VfxId.SlashRed));
        Assert.AreEqual(PixelRampId.Gold, PixelRamp.FromSwing(VfxId.SlashGoldWide));
        Assert.AreEqual(PixelRampId.White, PixelRamp.FromSwing(VfxId.SlashWhite));
        Assert.AreEqual(5, PixelRamp.Get(PixelRampId.Blue).Length);
    }

    [Test]
    public void Vfx_LegacySlashUpgrades()
    {
        Assert.IsTrue(Vfx.TryUpgradeLegacy(VfxId.SlashGold, out VfxId to, out PixelRampId ramp, out float mul, out bool flip));
        Assert.AreEqual(VfxId.WSlashDown, to);
        Assert.AreEqual(PixelRampId.Gold, ramp);
        Assert.IsTrue(Vfx.TryUpgradeLegacy(VfxId.CritCross, out to, out ramp, out mul, out flip));
        Assert.AreEqual(VfxId.WHitStar, to);
        Assert.AreEqual(2f, mul);
        Assert.IsFalse(Vfx.TryUpgradeLegacy(VfxId.BurstBlue, out _, out _, out _, out _), "폭발·빛기둥 등은 대체하지 않는다");
    }
}
