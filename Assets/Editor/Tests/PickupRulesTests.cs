using NUnit.Framework;

public class PickupRulesTests
{
    [Test]
    public void FilterAll_PicksEverything()
    {
        Assert.IsTrue(PickupRules.ShouldPickUp("Misc", "C", PickupFilter.All));
        Assert.IsTrue(PickupRules.ShouldPickUp("Equip", "U", PickupFilter.All));
    }

    [Test]
    public void SkipCommonMaterials_OnlySkipsCommonMisc()
    {
        Assert.IsFalse(PickupRules.ShouldPickUp("Misc", "C", PickupFilter.SkipCommonMaterials));
        Assert.IsFalse(PickupRules.ShouldPickUp("Misc", "", PickupFilter.SkipCommonMaterials));
        Assert.IsTrue(PickupRules.ShouldPickUp("Misc", "R", PickupFilter.SkipCommonMaterials));
        Assert.IsTrue(PickupRules.ShouldPickUp("Equip", "C", PickupFilter.SkipCommonMaterials));
        Assert.IsTrue(PickupRules.ShouldPickUp("Consum", "C", PickupFilter.SkipCommonMaterials));
    }

    [Test]
    public void Magnet_StartsInsideRadius_ReleasesFarAway()
    {
        float inside = PickupRules.MagnetRadius - 0.1f;
        float between = (PickupRules.MagnetRadius + PickupRules.MagnetReleaseRadius) * 0.5f;
        float far = PickupRules.MagnetReleaseRadius + 0.1f;
        Assert.IsTrue(PickupRules.UpdateMagnet(false, inside * inside));
        Assert.IsFalse(PickupRules.UpdateMagnet(false, between * between));
        Assert.IsTrue(PickupRules.UpdateMagnet(true, between * between)); // 히스테리시스
        Assert.IsFalse(PickupRules.UpdateMagnet(true, far * far));
    }

    [Test]
    public void Accelerate_ClampsToMax()
    {
        Assert.AreEqual(PickupRules.MagnetAccel * 0.1f, PickupRules.Accelerate(0f, 0.1f), 1e-4f);
        Assert.AreEqual(PickupRules.MagnetMaxSpeed, PickupRules.Accelerate(PickupRules.MagnetMaxSpeed, 1f), 1e-4f);
    }

    [Test]
    public void Next_Cycles()
    {
        Assert.AreEqual(PickupFilter.SkipCommonMaterials, PickupRules.Next(PickupFilter.All));
        Assert.AreEqual(PickupFilter.All, PickupRules.Next(PickupFilter.SkipCommonMaterials));
    }
}
