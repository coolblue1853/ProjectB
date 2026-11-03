using NUnit.Framework;

public class CameraClampRulesTests
{
    [Test]
    public void Inside_Unchanged()
    {
        Assert.AreEqual(0f, CameraClampRules.Clamp(0f, 5f, true, -100f, true, 100f), 1e-5f);
    }

    [Test]
    public void OutsideLeft_PushedToMinPlusHalf()
    {
        Assert.AreEqual(-95f, CameraClampRules.Clamp(-120f, 5f, true, -100f, true, 100f), 1e-5f);
    }

    [Test]
    public void OutsideRight_PushedToMaxMinusHalf()
    {
        Assert.AreEqual(95f, CameraClampRules.Clamp(120f, 5f, true, -100f, true, 100f), 1e-5f);
    }

    [Test]
    public void UnusedBoundary_Ignored()
    {
        Assert.AreEqual(-500f, CameraClampRules.Clamp(-500f, 5f, false, -100f, false, 100f), 1e-5f);
    }

    [Test]
    public void NarrowerThanScreen_MaxWins()
    {
        Assert.AreEqual(-2f, CameraClampRules.Clamp(0f, 10f, true, -10f, true, 8f), 1e-5f);
    }
}
