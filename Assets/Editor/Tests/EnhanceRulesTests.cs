using NUnit.Framework;

public class EnhanceRulesTests
{
    [Test]
    public void FirstStepAlwaysSucceeds()
    {
        Assert.AreEqual(EnhanceRules.Result.Success, EnhanceRules.Roll(0, 0.999f, 0, out int lv));
        Assert.AreEqual(1, lv);
    }

    [Test]
    public void LowLevelFail_KeepsLevel()
    {
        Assert.AreEqual(EnhanceRules.Result.Fail, EnhanceRules.Roll(3, 0.95f, 0, out int lv));
        Assert.AreEqual(3, lv);
    }

    [Test]
    public void HighLevelFail_DropsOne()
    {
        Assert.AreEqual(EnhanceRules.Result.FailDown, EnhanceRules.Roll(7, 0.9f, 0, out int lv));
        Assert.AreEqual(6, lv);
    }

    [Test]
    public void PityAfterTwoFails_Succeeds()
    {
        Assert.AreEqual(EnhanceRules.Result.Success, EnhanceRules.Roll(9, 0.99f, EnhanceRules.PityFails, out int lv));
        Assert.AreEqual(10, lv);
    }

    [Test]
    public void MaxLevel_CannotEnhance()
    {
        Assert.AreEqual(EnhanceRules.Result.MaxLevel, EnhanceRules.Roll(10, 0f, 0, out int lv));
        Assert.AreEqual(10, lv);
        Assert.AreEqual(0, EnhanceRules.Stones(10));
    }

    [Test]
    public void Multipliers_MilestonesAtFiveAndTen()
    {
        Assert.AreEqual(1f, EnhanceRules.WeaponMultiplier(0), 1e-5f);
        Assert.AreEqual(1.16f, EnhanceRules.WeaponMultiplier(4), 1e-5f);
        Assert.AreEqual(1.3f, EnhanceRules.WeaponMultiplier(5), 1e-5f);
        Assert.AreEqual(1.6f, EnhanceRules.WeaponMultiplier(10), 1e-5f);
        Assert.AreEqual(1.7f, EnhanceRules.GearMultiplier(10), 1e-5f);
    }
}
