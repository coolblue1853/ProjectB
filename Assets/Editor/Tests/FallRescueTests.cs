using NUnit.Framework;

/// <summary>낙사 복구 규칙(11-02): 맵 최저 지면보다 한참 아래로 떨어졌을 때만 구조한다.</summary>
public class FallRescueTests
{
    [Test]
    public void Rescue_OnlyBelowFloorMinusMargin()
    {
        Assert.IsFalse(FallRescueRules.ShouldRescue(100f, 100f));
        Assert.IsFalse(FallRescueRules.ShouldRescue(100f - FallRescueRules.Margin, 100f)); // 경계는 아직 아니다
        Assert.IsTrue(FallRescueRules.ShouldRescue(100f - FallRescueRules.Margin - 0.01f, 100f));
    }

    [Test]
    public void Rescue_DisabledWithoutGround()
    {
        Assert.IsFalse(FallRescueRules.ShouldRescue(-9999f, float.NegativeInfinity));
    }
}
