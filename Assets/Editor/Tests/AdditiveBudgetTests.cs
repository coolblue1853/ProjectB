using NUnit.Framework;

public class AdditiveBudgetTests
{
    [Test]
    public void Factor_FullUpToSoftLimit()
    {
        Assert.AreEqual(1f, AdditiveBudget.FactorFor(1));
        Assert.AreEqual(1f, AdditiveBudget.FactorFor(AdditiveBudget.SoftLayers));
    }

    [Test]
    public void Factor_SumBrightnessNeverExceedsSoftLimit()
    {
        for (int n = 1; n <= AdditiveBudget.HardLayers; n++)
        {
            Assert.LessOrEqual(n * AdditiveBudget.FactorFor(n), AdditiveBudget.SoftLayers + 0.0001f, "n=" + n);
        }
    }

    [Test]
    public void Factor_ZeroBeyondHardLimit()
    {
        Assert.AreEqual(0f, AdditiveBudget.FactorFor(AdditiveBudget.HardLayers + 1));
        Assert.Greater(AdditiveBudget.FactorFor(AdditiveBudget.HardLayers), 0f);
    }
}
