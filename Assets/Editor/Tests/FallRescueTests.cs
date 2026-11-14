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

    [Test]
    public void Lost_BelowMapBottomMinusKillMargin()
    {
        Assert.IsFalse(FallRecovery.IsLost(-10f, -10f));
        Assert.IsFalse(FallRecovery.IsLost(-10f - FallRecovery.KillMargin, -10f)); // 경계는 아직 아니다
        Assert.IsTrue(FallRecovery.IsLost(-10f - FallRecovery.KillMargin - 0.01f, -10f));
        Assert.IsTrue(FallRecovery.IsLost(-74312f, -188f)); // 11-15 R5: 허공에서 저장된 y
        Assert.IsFalse(FallRecovery.IsLost(-179f, -188f)); // 2층계 맵은 하한이 낮아 정상 위치
    }
}
