using NUnit.Framework;
using UnityEngine;

/// <summary>던파식 전투 규칙(CombatSpec 9절): 캔슬·경직 연쇄/저항·공중 콤보·다운·스킬 막힘 이유·히트 카운터</summary>
public class CombatRulesTests
{
    [Test]
    public void Cancel_BeforeHalfOrHit_Blocked()
    {
        // 후딜 0.4초: 절반(0.2)이 지나야. 판정 0.19초 + 0.05 = 0.24 가 더 늦으면 그때부터
        Assert.IsFalse(CombatRules.CanCancelMeleeRecovery(0.1f, 0.4f, 0.05f));
        Assert.IsFalse(CombatRules.CanCancelMeleeRecovery(0.21f, 0.4f, 0.19f)); // 판정 뒤 0.05초 안
        Assert.IsTrue(CombatRules.CanCancelMeleeRecovery(0.25f, 0.4f, 0.19f));
        Assert.IsTrue(CombatRules.CanCancelMeleeRecovery(0.2f, 0.4f, 0.05f));
    }

    [Test]
    public void Cancel_AfterRecoveryEnds_NotNeeded()
    {
        Assert.IsFalse(CombatRules.CanCancelMeleeRecovery(0.4f, 0.4f, 0.1f));
        Assert.IsFalse(CombatRules.CanCancelMeleeRecovery(0.3f, 0f, 0f));
    }

    [Test]
    public void SkillBlock_PriorityAndStrictStamina()
    {
        Assert.AreEqual(SkillBlock.Cooldown, CombatRules.GetSkillBlock(true, 0, 5, false));
        Assert.AreEqual(SkillBlock.Stamina, CombatRules.GetSkillBlock(false, 5, 5, true)); // 같으면 부족(기존 '초과' 규칙)
        Assert.AreEqual(SkillBlock.Busy, CombatRules.GetSkillBlock(false, 6, 5, false));
        Assert.AreEqual(SkillBlock.None, CombatRules.GetSkillBlock(false, 6, 5, true));
        Assert.AreEqual(SkillBlock.Stamina, CombatRules.GetSkillBlock(false, 0, 0, true)); // 소모 0 스킬도 스태미나 0 이면 막힘(기존과 같음)
    }

    [Test]
    public void StaggerChain_ResetsAfterWindow()
    {
        Assert.AreEqual(1, CombatRules.NextChain(0, 99f));
        Assert.AreEqual(2, CombatRules.NextChain(1, 0.5f));
        Assert.AreEqual(5, CombatRules.NextChain(4, 1.5f));
        Assert.AreEqual(1, CombatRules.NextChain(4, 1.6f));
    }

    [Test]
    public void StiffnessScale_ChainAndResist()
    {
        Assert.AreEqual(1f, CombatRules.StiffnessScale(1, 0f), 1e-4f);
        Assert.AreEqual(0.75f, CombatRules.StiffnessScale(2, 0f), 1e-4f);
        Assert.AreEqual(0.35f, CombatRules.StiffnessScale(9, 0f), 1e-4f); // 하한
        Assert.AreEqual(0.5f, CombatRules.StiffnessScale(1, 0.5f), 1e-4f);
        Assert.AreEqual(0.35f * 0.4f, CombatRules.StiffnessScale(4, 0.6f), 1e-4f);
        Assert.AreEqual(0f, CombatRules.StiffnessScale(1, 5f), 1e-4f); // 저항은 1 로 제한
    }

    [Test]
    public void StaggerResist_TakesLargest()
    {
        Assert.AreEqual(0f, CombatRules.StaggerResist(false, false, false));
        Assert.AreEqual(0.3f, CombatRules.StaggerResist(false, false, true), 1e-4f);
        Assert.AreEqual(0.5f, CombatRules.StaggerResist(false, true, true), 1e-4f);
        Assert.AreEqual(0.6f, CombatRules.StaggerResist(true, true, true), 1e-4f);
    }

    [Test]
    public void AirGravity_RisesPerHitAndClamps()
    {
        Assert.AreEqual(0.35f, CombatRules.AirGravityMul(1), 1e-4f);
        Assert.AreEqual(0.45f, CombatRules.AirGravityMul(2), 1e-4f);
        Assert.AreEqual(1f, CombatRules.AirGravityMul(20), 1e-4f);
        Assert.AreEqual(0.35f, CombatRules.AirGravityMul(0), 1e-4f);
    }

    [Test]
    public void JuggleLift_ShrinksToZero()
    {
        Assert.AreEqual(4f, CombatRules.JuggleLift(1), 1e-4f);
        Assert.AreEqual(3.3f, CombatRules.JuggleLift(2), 1e-4f);
        Assert.AreEqual(0f, CombatRules.JuggleLift(12), 1e-4f);
    }

    [Test]
    public void DownDamage_OnlyWhenDownAndHoldingDown()
    {
        Assert.AreEqual(1f, CombatRules.DownDamageMul(false, true));
        Assert.AreEqual(1f, CombatRules.DownDamageMul(true, false));
        Assert.AreEqual(1.3f, CombatRules.DownDamageMul(true, true), 1e-4f);
    }

    [Test]
    public void HitCounter_StacksWithinWindowAndResets()
    {
        var c = new HitComboCounter();
        c.Hit(1f, 10);
        c.Hit(2.5f, 20);
        Assert.AreEqual(2, c.Count);
        Assert.AreEqual(30, c.TotalDamage);
        Assert.IsTrue(c.Active(4.5f));
        Assert.IsFalse(c.Active(4.6f));
        c.Hit(10f, 5); // 창을 넘기면 새로 시작
        Assert.AreEqual(1, c.Count);
        Assert.AreEqual(5, c.TotalDamage);
        Assert.AreEqual(2, c.Best);
        c.Reset();
        Assert.AreEqual(0, c.Count);
        Assert.IsFalse(c.Active(10f));
    }

    [Test]
    public void HitCounter_RemainingGauge()
    {
        var c = new HitComboCounter();
        Assert.AreEqual(0f, c.Remaining01(0f));
        c.Hit(1f, 1);
        Assert.AreEqual(1f, c.Remaining01(1f), 1e-4f);
        Assert.AreEqual(0.5f, c.Remaining01(2f), 1e-4f);
        Assert.AreEqual(0f, c.Remaining01(5f), 1e-4f);
    }

    [Test]
    public void AreaBurstKnock_IsHorizontalOnly_NoLaunch()
    {
        Vector2 dir = CombatRules.AreaBurstKnockDir();
        Assert.AreEqual(0f, dir.y);
        Assert.AreNotEqual(0f, dir.x);
        Assert.IsFalse(CombatRules.IsLaunch(dir, 8f));
        Assert.IsTrue(CombatRules.IsLaunch(Vector2.up, 8f)); // 예전 값은 띄움이었다
    }

    [Test]
    public void CooldownLabel_Format()
    {
        Assert.AreEqual(string.Empty, SkillCooldown.FormatRemaining(0f));
        Assert.AreEqual("3.5", SkillCooldown.FormatRemaining(3.5f));
        Assert.AreEqual("0.1", SkillCooldown.FormatRemaining(0.1f));
        Assert.AreEqual("10", SkillCooldown.FormatRemaining(10f));
        Assert.AreEqual("13", SkillCooldown.FormatRemaining(12.4f));
    }

    [Test]
    public void Drift_FinisherHalved_PreHitScaled()
    {
        Assert.AreEqual(0.5f, CombatRules.DriftMul(0.5f, 1, 3, 0.3f, 0.2f), 1e-4f);
        Assert.AreEqual(0.5f, CombatRules.DriftMul(0.5f, 2, 3, 0.3f, 0.2f), 1e-4f);
        Assert.AreEqual(0.25f, CombatRules.DriftMul(0.5f, 3, 3, 0.3f, 0.2f), 1e-4f);
        Assert.AreEqual(0.35f, CombatRules.DriftMul(0.5f, 1, 3, 0.1f, 0.2f), 1e-4f); // 선딜 x0.7
        Assert.AreEqual(0.5f, CombatRules.DriftMul(0.5f, 2, 2, 0.3f, 0.2f), 1e-4f); // 2단 콤보는 마무리 취급 안 함
    }

    [Test]
    public void Drift_OnlyForwardInput()
    {
        Assert.IsTrue(CombatRules.CanDrift(1f, 1f));
        Assert.IsTrue(CombatRules.CanDrift(-1f, -1f));
        Assert.IsFalse(CombatRules.CanDrift(-1f, 1f));
        Assert.IsFalse(CombatRules.CanDrift(0f, 1f));
    }

    [Test]
    public void JumpCancel_OnlyAfterHit()
    {
        Assert.IsFalse(CombatRules.CanJumpCancel(0.1f, 0.5f, 0.2f));
        Assert.IsFalse(CombatRules.CanJumpCancel(0.24f, 0.5f, 0.2f));
        Assert.IsTrue(CombatRules.CanJumpCancel(0.26f, 0.5f, 0.2f));
        Assert.IsFalse(CombatRules.CanJumpCancel(0.5f, 0.5f, 0.2f)); // 후딜 끝
        Assert.IsFalse(CombatRules.CanJumpCancel(0.3f, 0f, 0f));
    }

    [Test]
    public void MoveCancel_FortyPercent_FinisherSixty()
    {
        Assert.IsFalse(CombatRules.CanMoveCancel(0.15f, 0.5f, 0.1f, false)); // 40% = 0.2
        Assert.IsTrue(CombatRules.CanMoveCancel(0.2f, 0.5f, 0.1f, false));
        Assert.IsFalse(CombatRules.CanMoveCancel(0.25f, 0.5f, 0.1f, true)); // 60% = 0.3
        Assert.IsTrue(CombatRules.CanMoveCancel(0.3f, 0.5f, 0.1f, true));
        Assert.IsFalse(CombatRules.CanMoveCancel(0.2f, 0.5f, 0.18f, false)); // 판정 + 0.05 가 더 늦다
        Assert.IsFalse(CombatRules.CanMoveCancel(0.5f, 0.5f, 0.1f, false));
    }

    [Test]
    public void Turn_AfterHit_NotFinisher()
    {
        Assert.IsFalse(CombatRules.CanTurnInRecovery(0.1f, 0.5f, 0.2f, false));
        Assert.IsTrue(CombatRules.CanTurnInRecovery(0.3f, 0.5f, 0.2f, false));
        Assert.IsFalse(CombatRules.CanTurnInRecovery(0.3f, 0.5f, 0.2f, true));
    }
}
