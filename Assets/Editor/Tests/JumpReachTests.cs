using NUnit.Framework;

/// <summary>점프 도달 높이: 1층계 지형(계단 1칸, 발판 4~6칸+사다리)과 가변 점프(CF-7)의 정합</summary>
public class JumpReachTests
{
    private const float JumpForce = 13f; // PlayerController.jumpForce

    [Test]
    public void FullJump_ReachesAbout2_9()
    {
        Assert.AreEqual(2.87f, JumpReach.FullHeight(JumpForce), 0.02f);
    }

    [Test]
    public void TapJump_StillClimbsOneUnitStairs()
    {
        // 1층계 샛길 계단은 1칸씩. 탭 점프(JumpCutMul 0.7 → 약 1.89)도 모서리 여유 0.2 를 두고 넘어야 한다 (결정 11-05)
        Assert.AreEqual(1.89f, JumpReach.TapHeight(JumpForce), 0.05f);
        Assert.IsTrue(JumpReach.CanStepWithTap(JumpForce, 1f));
        Assert.IsTrue(JumpReach.CanStepWithTap(JumpForce, 1.6f));
        Assert.IsFalse(JumpReach.CanStepWithTap(JumpForce, 2.2f));
        Assert.Less(JumpReach.TapHeight(JumpForce), JumpReach.FullHeight(JumpForce));
    }

    [Test]
    public void Floor1_SingleJump_CannotClearPlatformRise()
    {
        // 1층계는 더블점프 미해금(MovementUnlockRules). 발판 4칸 이상은 사다리가 있어야 한다
        Assert.Less(JumpReach.FullHeight(JumpForce), 4f);
    }
}
