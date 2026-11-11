using NUnit.Framework;

public class QuantityRulesTests
{
    [Test]
    public void ClampAndStep()
    {
        Assert.AreEqual(1, QuantityRules.Clamp(0, 1, 99));
        Assert.AreEqual(99, QuantityRules.Clamp(500, 1, 99));
        Assert.AreEqual(1, QuantityRules.Clamp(5, 1, 0)); // max < min → min
        Assert.AreEqual(11, QuantityRules.Step(1, 10, 1, 99));
        Assert.AreEqual(99, QuantityRules.Step(95, 10, 1, 99));
        Assert.AreEqual(1, QuantityRules.Step(5, -10, 1, 99));
        Assert.AreEqual(5, QuantityRules.Step(5, int.MaxValue, 1, 5));
    }

    [Test]
    public void Digits_FreshReplacesThenAppends()
    {
        int v = QuantityRules.AppendDigit(37, 4, true, 1, 99);
        Assert.AreEqual(4, v);
        v = QuantityRules.AppendDigit(v, 2, false, 1, 99);
        Assert.AreEqual(42, v);
        v = QuantityRules.AppendDigit(v, 7, false, 1, 99);
        Assert.AreEqual(99, v); // 427 → 최대 99
        Assert.AreEqual(1, QuantityRules.AppendDigit(5, 0, true, 1, 99)); // 0 → min
        Assert.AreEqual(5, QuantityRules.AppendDigit(5, 11, true, 1, 99)); // 숫자 아님 → 그대로
    }

    [Test]
    public void Backspace_DropsLastDigit()
    {
        Assert.AreEqual(4, QuantityRules.Backspace(42, 1, 99));
        Assert.AreEqual(1, QuantityRules.Backspace(4, 1, 99));
    }

    [Test]
    public void HoldMultiplier_Accelerates()
    {
        Assert.AreEqual(1, QuantityRules.HoldMultiplier(0.5f));
        Assert.AreEqual(5, QuantityRules.HoldMultiplier(1.5f));
        Assert.AreEqual(10, QuantityRules.HoldMultiplier(3f));
    }

    [Test]
    public void RepeatTicks_FirstAfterDelay()
    {
        Assert.AreEqual(0, QuantityRules.RepeatTicks(0f, 0.3f));
        Assert.AreEqual(1, QuantityRules.RepeatTicks(0.39f, 0.41f));
        Assert.AreEqual(3, QuantityRules.RepeatTicks(0.5f, 0.5f + 0.06f * 3.01f));
        Assert.AreEqual(0, QuantityRules.RepeatTicks(0.5f, 0.51f));
    }

    [Test]
    public void MaxAffordable_UsesTightestLimit()
    {
        Assert.AreEqual(5, QuantityRules.MaxAffordable(100, 550, 99, 99));
        Assert.AreEqual(3, QuantityRules.MaxAffordable(100, 550, 3, 99));
        Assert.AreEqual(0, QuantityRules.MaxAffordable(100, 50, 99, 99));
        Assert.AreEqual(99, QuantityRules.MaxAffordable(0, 0, 99, 99));
        Assert.AreEqual(int.MaxValue, QuantityRules.Total(int.MaxValue, 5));
    }
}
