using NUnit.Framework;
using System;

/// <summary>전투 입력 재설계 CI-P0·P1: 공격 중 이동 모드·Lunge 곡선·캔슬 비율·방향 버퍼·스틱 디지털·타입 표</summary>
public class AttackMoveRulesTests
{
    const float Eps = 0.001f;

    [Test]
    public void LungeTime_ClampsBetweenMinAndMax()
    {
        Assert.AreEqual(0.12f, AttackMoveRules.LungeTime(0f), Eps);
        Assert.AreEqual(0.19f, AttackMoveRules.LungeTime(0.07f), Eps);
        Assert.AreEqual(0.30f, AttackMoveRules.LungeTime(0.5f), Eps);
    }

    [Test]
    public void LungeVelocity_ZeroWhenNotHeldOrWrongModeOrNoDist()
    {
        Assert.AreEqual(0f, AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, 0.9f, 0.05f, 0.07f, false));
        Assert.AreEqual(0f, AttackMoveRules.LungeVelocity(AttackMoveMode.InPlace, 0.9f, 0.05f, 0.07f, true));
        Assert.AreEqual(0f, AttackMoveRules.LungeVelocity(AttackMoveMode.Free, 0.9f, 0.05f, 0.07f, true));
        Assert.AreEqual(0f, AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, 0f, 0.05f, 0.07f, true));
    }

    [Test]
    public void LungeVelocity_EndsAtLungeTimeAndNeverNegative()
    {
        float time = AttackMoveRules.LungeTime(0.07f);
        Assert.AreEqual(0f, AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, 0.9f, time, 0.07f, true));
        Assert.AreEqual(0f, AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, 0.9f, time + 1f, 0.07f, true));
        Assert.AreEqual(0f, AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, 0.9f, -0.1f, 0.07f, true));
        Assert.Greater(AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, 0.9f, time - 0.001f, 0.07f, true), 0f);
    }

    [Test]
    public void LungeVelocity_ConstantFirst60PercentThenDecelerates()
    {
        float hit = 0.18f;
        float time = AttackMoveRules.LungeTime(hit);
        float peak = AttackMoveRules.LungePeakSpeed(0.9f, time);
        Assert.AreEqual(peak, AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, 0.9f, 0f, hit, true), Eps);
        Assert.AreEqual(peak, AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, 0.9f, time * 0.6f, hit, true), Eps);
        Assert.AreEqual(peak * 0.5f, AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, 0.9f, time * 0.8f, hit, true), Eps);
    }

    [Test]
    public void LungeVelocity_IntegralEqualsDistance()
    {
        float[] dists = { 0.6f, 0.9f, 1.3f, 1.5f };
        float[] hits = { 0f, 0.07f, 0.18f, 0.4f };
        foreach (float d in dists)
        {
            foreach (float hit in hits)
            {
                float sum = 0f, dt = 1f / 600f;
                for (float t = 0f; t < 1f; t += dt)
                {
                    sum += AttackMoveRules.LungeVelocity(AttackMoveMode.LungeOnForward, d, t, hit, true) * dt;
                }
                Assert.AreEqual(d, sum, d * 0.02f, "dist " + d + " hit " + hit);
            }
        }
    }

    [Test]
    public void MoveCancelFraction_ByClass()
    {
        Assert.AreEqual(0.30f, AttackMoveRules.MoveCancelFraction(MoveWeightClass.Light), Eps);
        Assert.AreEqual(0.40f, AttackMoveRules.MoveCancelFraction(MoveWeightClass.Standard), Eps);
        Assert.AreEqual(0.55f, AttackMoveRules.MoveCancelFraction(MoveWeightClass.Heavy), Eps);
        Assert.AreEqual(0.50f, AttackMoveRules.MoveCancelFraction(MoveWeightClass.Cast), Eps);
    }

    [Test]
    public void CanMoveCancel_StandardFloor1_MatchesLegacy()
    {
        // 표준 + 1층계(0.4) = 현행 CombatRules 와 같은 시점(후딜 1.0초 = 0.4초부터)
        Assert.IsFalse(AttackMoveRules.CanMoveCancel(MoveWeightClass.Standard, 0.4f, 0.39f, 1f, 0.05f, false));
        Assert.IsTrue(AttackMoveRules.CanMoveCancel(MoveWeightClass.Standard, 0.4f, 0.41f, 1f, 0.05f, false));
        Assert.AreEqual(CombatRules.CanMoveCancel(0.41f, 1f, 0.05f, false), AttackMoveRules.CanMoveCancel(MoveWeightClass.Standard, 0.4f, 0.41f, 1f, 0.05f, false));
    }

    [Test]
    public void CanMoveCancel_HeavyOpensAt55Percent_LightAt30()
    {
        Assert.IsFalse(AttackMoveRules.CanMoveCancel(MoveWeightClass.Heavy, 0.4f, 0.54f, 1f, 0.05f, false));
        Assert.IsTrue(AttackMoveRules.CanMoveCancel(MoveWeightClass.Heavy, 0.4f, 0.56f, 1f, 0.05f, false));
        Assert.IsFalse(AttackMoveRules.CanMoveCancel(MoveWeightClass.Light, 0.4f, 0.29f, 1f, 0.05f, false));
        Assert.IsTrue(AttackMoveRules.CanMoveCancel(MoveWeightClass.Light, 0.4f, 0.31f, 1f, 0.05f, false));
        Assert.IsFalse(AttackMoveRules.CanMoveCancel(MoveWeightClass.Heavy, 0.4f, 1f, 1f, 0.05f, false)); // 후딜 끝난 뒤는 불필요
    }

    [Test]
    public void CanJumpCancel_HeavyMargin010()
    {
        Assert.IsTrue(AttackMoveRules.CanJumpCancel(MoveWeightClass.Standard, 0.26f, 0.6f, 0.2f));
        Assert.IsFalse(AttackMoveRules.CanJumpCancel(MoveWeightClass.Heavy, 0.26f, 0.6f, 0.2f));
        Assert.IsTrue(AttackMoveRules.CanJumpCancel(MoveWeightClass.Heavy, 0.31f, 0.6f, 0.2f));
        Assert.IsFalse(AttackMoveRules.CanJumpCancel(MoveWeightClass.Standard, 0.6f, 0.6f, 0.2f));
    }

    [Test]
    public void ComboConsumeFraction_ByClass()
    {
        Assert.AreEqual(0.75f, AttackMoveRules.ComboConsumeFraction(MoveWeightClass.Light), Eps);
        Assert.AreEqual(0.90f, AttackMoveRules.ComboConsumeFraction(MoveWeightClass.Standard), Eps);
        Assert.AreEqual(1f, AttackMoveRules.ComboConsumeFraction(MoveWeightClass.Heavy), Eps);
        Assert.AreEqual(1f, AttackMoveRules.ComboConsumeFraction(MoveWeightClass.Cast), Eps);
    }

    [Test]
    public void AirAttackVx_InPlaceUses03_OthersUse07()
    {
        Assert.AreEqual(5f * 0.3f, AttackMoveRules.AirAttackVx(AttackMoveMode.InPlace, 0f, 1f, 5f, 0.016f), Eps);
        Assert.AreEqual(5f * 0.7f, AttackMoveRules.AirAttackVx(AttackMoveMode.LungeOnForward, 0f, 1f, 5f, 0.016f), Eps);
        Assert.AreEqual(CombatRules.AirAttackVx(2f, 0f, 5f, 0.016f), AttackMoveRules.AirAttackVx(AttackMoveMode.InPlace, 2f, 0f, 5f, 0.016f), Eps);
    }

    // ── DirectionBuffer ──
    [Test]
    public void DirectionBuffer_CurrentInputWins()
    {
        var b = new DirectionBuffer();
        b.Record(1f, -1f);
        Assert.AreEqual(1f, b.Resolve(1.01f, 1f));
    }

    [Test]
    public void DirectionBuffer_WithinWindowReturnsLast_AfterWindowZero()
    {
        var b = new DirectionBuffer();
        b.Record(1f, 0.8f);
        b.Record(1.02f, 0f); // 중립은 기록하지 않는다
        Assert.AreEqual(1f, b.Resolve(1.05f, 0f)); // 0.05초 뒤
        Assert.AreEqual(1f, b.Resolve(1.07f, 0f)); // 경계 근처
        Assert.AreEqual(0f, b.Resolve(1.12f, 0f)); // 0.12초 뒤
    }

    [Test]
    public void DirectionBuffer_EmptyAndCleared()
    {
        var b = new DirectionBuffer();
        Assert.AreEqual(0f, b.Resolve(0f, 0f));
        b.Record(1f, -0.3f);
        Assert.AreEqual(-1f, b.Resolve(1.01f, 0f));
        b.Clear();
        Assert.AreEqual(0f, b.Resolve(1.02f, 0f));
    }

    // ── StickDigital ──
    [Test]
    public void StickDigital_HysteresisEnter05Exit035()
    {
        var s = new StickDigital();
        Assert.AreEqual(0, s.Update(0.4f));  // 진입 전
        Assert.AreEqual(1, s.Update(0.5f));  // 진입
        Assert.AreEqual(1, s.Update(0.4f));  // 해제 임계 위: 유지
        Assert.AreEqual(0, s.Update(0.3f));  // 0.35 미만: 해제
        Assert.AreEqual(0, s.Update(0.45f)); // 재진입 전
    }

    [Test]
    public void StickDigital_DeadZoneAndOppositeDirection()
    {
        var s = new StickDigital();
        Assert.AreEqual(0, s.Update(0.2f));
        Assert.AreEqual(-1, s.Update(-0.9f));
        Assert.AreEqual(1, s.Update(0.9f));   // 반대로 진입 임계 초과: 즉시 전환
        Assert.AreEqual(0, s.Update(-0.4f));  // 반대 약기울임: 해제만
        Assert.AreEqual(0, s.Update(0f));
    }

    [Test]
    public void StickDigital_StatelessDigital()
    {
        Assert.AreEqual(0, StickDigital.Digital(0.3f));
        Assert.AreEqual(0, StickDigital.Digital(0.49f));
        Assert.AreEqual(1, StickDigital.Digital(0.5f));
        Assert.AreEqual(-1, StickDigital.Digital(-1f));
    }

    // ── 프로파일 표 (CI-P1) ──
    [Test]
    public void Profiles_EveryWeaponKindHasRow()
    {
        foreach (WeaponKind k in Enum.GetValues(typeof(WeaponKind)))
        {
            if (k == WeaponKind.None)
            {
                continue;
            }
            Assert.IsNotNull(WeaponMoveProfiles.ProfileOf(k, null), "표 누락: " + k);
        }
    }

    [Test]
    public void Profiles_ValuesInRange()
    {
        foreach (WeaponKind k in Enum.GetValues(typeof(WeaponKind)))
        {
            WeaponMoveProfile p = WeaponMoveProfiles.ProfileOf(k, null);
            if (p == null)
            {
                continue;
            }
            for (int step = 1; step <= 6; step++)
            {
                float d = p.LungeDistAt(step);
                Assert.IsTrue(d >= 0f && d <= 2f, k + " step " + step + " dist " + d);
                if (p.ModeAt(step) != AttackMoveMode.LungeOnForward)
                {
                    Assert.AreEqual(0f, d, k + " 비 Lunge 스텝은 거리 0");
                }
                else
                {
                    Assert.Greater(d, 0f, k + " Lunge 스텝은 거리 > 0");
                }
            }
        }
    }

    [Test]
    public void Profiles_StepValuesMatchDesignTable()
    {
        WeaponMoveProfile sword = WeaponMoveProfiles.ProfileOf(WeaponKind.Sword, null);
        Assert.AreEqual(0.9f, sword.LungeDistAt(1), Eps);
        Assert.AreEqual(0.9f, sword.LungeDistAt(2), Eps);
        Assert.AreEqual(1.3f, sword.LungeDistAt(3), Eps);
        Assert.AreEqual(1.3f, sword.LungeDistAt(9), Eps); // 배열보다 길면 마지막 값
        Assert.AreEqual(MoveWeightClass.Standard, sword.Class);
        WeaponMoveProfile shield = WeaponMoveProfiles.ProfileOf(WeaponKind.Sword, "RustedSwordandShield");
        Assert.AreEqual(0.6f, shield.LungeDistAt(1), Eps);
        Assert.AreEqual(0.9f, shield.LungeDistAt(3), Eps);
        WeaponMoveProfile dual = WeaponMoveProfiles.ProfileOf(WeaponKind.Dual, null);
        Assert.AreEqual(0.5f, dual.LungeDistAt(3), Eps);
        Assert.AreEqual(0.7f, dual.LungeDistAt(4), Eps);
        WeaponMoveProfile axe = WeaponMoveProfiles.ProfileOf(WeaponKind.Axe, null);
        Assert.AreEqual(AttackMoveMode.InPlace, axe.ModeAt(1));
        Assert.AreEqual(AttackMoveMode.LungeOnForward, axe.ModeAt(2));
        Assert.AreEqual(AttackMoveMode.InPlace, WeaponMoveProfiles.ProfileOf(WeaponKind.Greatsword, null).ModeAt(1));
        Assert.AreEqual(AttackMoveMode.Free, WeaponMoveProfiles.ProfileOf(WeaponKind.Staff, null).ModeAt(1));
    }

    [Test]
    public void ActiveProfile_OnlyWhitelistedKinds()
    {
        Assert.IsNotNull(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Sword, null));
        Assert.IsNotNull(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Greatsword, null));
        Assert.IsNotNull(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Spear, null)); // CI-P4 이후 화이트리스트
        Assert.IsNotNull(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Staff, null));
        Assert.IsNull(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.None, null));
    }

    [Test]
    public void LocksMove_FreeKindsDoNotLock_OthersLockPerStep()
    {
        Assert.IsFalse(WeaponMoveProfiles.LocksMove(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Staff, null), 1));
        Assert.IsFalse(WeaponMoveProfiles.LocksMove(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Adae, null), 2));
        Assert.IsFalse(WeaponMoveProfiles.LocksMove(null, 1)); // 화이트리스트 밖 = 프리팹 값만
        Assert.IsTrue(WeaponMoveProfiles.LocksMove(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Spear, null), 1));
        Assert.IsTrue(WeaponMoveProfiles.LocksMove(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Axe, null), 1)); // InPlace
        Assert.IsTrue(WeaponMoveProfiles.LocksMove(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Axe, null), 2)); // Lunge
    }

    [Test]
    public void EveryTableKind_IsWhitelisted()
    {
        foreach (WeaponKind k in System.Enum.GetValues(typeof(WeaponKind)))
        {
            if (WeaponMoveProfiles.ProfileOf(k, null) != null)
            {
                Assert.IsTrue(WeaponMoveProfiles.EnabledKinds.Contains(k), k + " 표에는 있으나 화이트리스트에 없다");
            }
        }
    }

    [Test]
    public void ActiveProfile_RollbackByWhitelistRemoval()
    {
        Assert.IsTrue(WeaponMoveProfiles.EnabledKinds.Remove(WeaponKind.Dagger));
        try
        {
            Assert.IsNull(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Dagger, null));
        }
        finally
        {
            WeaponMoveProfiles.EnabledKinds.Add(WeaponKind.Dagger);
        }
        Assert.IsNotNull(WeaponMoveProfiles.ActiveProfileOf(WeaponKind.Dagger, null));
    }

    // ── CI-P5·P6 ──
    [Test]
    public void HoldRepeat_OnlyMidCombo()
    {
        Assert.IsFalse(AttackMoveRules.HoldRepeatAllowed(0, 3)); // 콤보 밖
        Assert.IsTrue(AttackMoveRules.HoldRepeatAllowed(1, 3));
        Assert.IsTrue(AttackMoveRules.HoldRepeatAllowed(2, 3));
        Assert.IsFalse(AttackMoveRules.HoldRepeatAllowed(3, 3)); // 마지막 타 뒤: 재누름 필요
        Assert.IsFalse(AttackMoveRules.HoldRepeatAllowed(1, 1)); // 단타 무기
    }

    [Test]
    public void Buffers_SkillAndJumpAndSimulWindow()
    {
        Assert.AreEqual(0.15f, CombatRules.SkillBufferTime, Eps);
        Assert.AreEqual(0.15f, CombatRules.JumpBufferTime, Eps);
        Assert.AreEqual(2f / 60f, AttackMoveRules.JumpAttackSimulWindow, Eps);
        Assert.AreEqual(0.08f, DirectionBuffer.DefaultWindow, Eps);
    }
}
