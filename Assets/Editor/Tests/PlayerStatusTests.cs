using NUnit.Framework;

/// <summary>PlayerStatus 토큰 동작 (PlayerStateDesign Phase 1 검증 항목)</summary>
public class PlayerStatusTests
{
    float now;

    [SetUp]
    public void SetUp()
    {
        PlayerStatus.ResetStatics();
        now = 0f;
        PlayerStatus.Clock = () => now;
    }

    [TearDown]
    public void TearDown() => PlayerStatus.ResetStatics();

    [Test]
    public void OneOfTwoHandlesReleased_StillOn()
    {
        var a = PlayerStatus.Acquire(StatusFlag.Invincible, "A");
        var b = PlayerStatus.Acquire(StatusFlag.Invincible, "B");
        a.Dispose();
        Assert.IsTrue(PlayerStatus.Has(StatusFlag.Invincible));
        b.Dispose();
        Assert.IsFalse(PlayerStatus.Has(StatusFlag.Invincible));
    }

    [Test]
    public void DoubleDispose_DecrementsOnce()
    {
        var a = PlayerStatus.Acquire(StatusFlag.MoveLock, "A");
        PlayerStatus.Acquire(StatusFlag.MoveLock, "B");
        a.Dispose();
        a.Dispose();
        Assert.AreEqual(1, PlayerStatus.Count(StatusFlag.MoveLock));
    }

    [Test]
    public void LegacyFalse_DoesNotClearOtherOwners()
    {
        PlayerStatus.Acquire(StatusFlag.SuperArmor, "Buff");
        PlayerStatus.SetLegacy(StatusFlag.SuperArmor, true);
        PlayerStatus.SetLegacy(StatusFlag.SuperArmor, false);
        Assert.IsTrue(PlayerStatus.Has(StatusFlag.SuperArmor));
    }

    [Test]
    public void LegacySetTwice_IsSingleToken()
    {
        PlayerStatus.SetLegacy(StatusFlag.InputLock, true);
        PlayerStatus.SetLegacy(StatusFlag.InputLock, true);
        Assert.AreEqual(1, PlayerStatus.Count(StatusFlag.InputLock));
        PlayerStatus.SetLegacy(StatusFlag.InputLock, false);
        Assert.IsFalse(PlayerStatus.Has(StatusFlag.InputLock));
    }

    [Test]
    public void AcquireFor_ExpiresOnTick()
    {
        PlayerStatus.AcquireFor(StatusFlag.Invincible, 0.2f, "Dash");
        now = 0.1f;
        PlayerStatus.Tick();
        Assert.IsTrue(PlayerStatus.Has(StatusFlag.Invincible));
        now = 0.25f;
        PlayerStatus.Tick();
        Assert.IsFalse(PlayerStatus.Has(StatusFlag.Invincible));
    }

    [Test]
    public void SkillDashThenDash_InvincibilityKeptUntilLongerEnds()
    {
        // C1: 스킬 대시(0.5s) 도중 일반 대시(0.2s) → 대시 끝나도 스킬 무적 유지
        PlayerStatus.AcquireFor(StatusFlag.Invincible, 0.5f, "SkillDash");
        now = 0.1f;
        PlayerStatus.AcquireFor(StatusFlag.Invincible, 0.2f, "Dash");
        now = 0.35f;
        PlayerStatus.Tick();
        Assert.IsTrue(PlayerStatus.Has(StatusFlag.Invincible));
        now = 0.55f;
        PlayerStatus.Tick();
        Assert.IsFalse(PlayerStatus.Has(StatusFlag.Invincible));
    }

    [Test]
    public void ForceClear_ClearsEverything()
    {
        PlayerStatus.Acquire(StatusFlag.MoveLock, "A");
        PlayerStatus.SetLegacy(StatusFlag.Invincible, true);
        PlayerStatus.ForceClear("test");
        Assert.IsFalse(PlayerStatus.Has(StatusFlag.MoveLock));
        Assert.IsFalse(PlayerStatus.Has(StatusFlag.Invincible));
    }

    [Test]
    public void Changed_FiresOnlyOnEdges()
    {
        int on = 0, off = 0;
        PlayerStatus.Changed += (f, v) => { if (v) on++; else off++; };
        var a = PlayerStatus.Acquire(StatusFlag.DashLock, "A");
        var b = PlayerStatus.Acquire(StatusFlag.DashLock, "B");
        a.Dispose();
        b.Dispose();
        Assert.AreEqual(1, on);
        Assert.AreEqual(1, off);
    }
}
