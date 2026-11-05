using NUnit.Framework;

/// <summary>AUTO-103: 투창 비행·탄창·마도서·CountdownTimer·Cooldown 경계값 고정(현재 동작 기록, 순수 로직만).</summary>
public class BoundaryRulesDTests
{
    const float Eps = 0.0001f;

    // ThrownSpear.FlightTime
    [Test] public void Flight_ZeroDistance_IsZero() { Assert.AreEqual(0f, ThrownSpear.FlightTime(0f, 10f, 5f), Eps); }
    [Test] public void Flight_AtRange_RangeOverSpeed() { Assert.AreEqual(2f, ThrownSpear.FlightTime(10f, 10f, 5f), Eps); }
    [Test] public void Flight_BeyondRange_ClampedToRange() { Assert.AreEqual(2f, ThrownSpear.FlightTime(99f, 10f, 5f), Eps); }
    [Test] public void Flight_BlockedShort_UsesClearDistance() { Assert.AreEqual(0.5f, ThrownSpear.FlightTime(2.5f, 10f, 5f), Eps); }
    [Test] public void Flight_NegativeDistance_ClampedZero() { Assert.AreEqual(0f, ThrownSpear.FlightTime(-3f, 10f, 5f), Eps); }
    [Test] public void Flight_SpeedZeroOrNegative_IsZero()
    {
        Assert.AreEqual(0f, ThrownSpear.FlightTime(5f, 10f, 0f), Eps);
        Assert.AreEqual(0f, ThrownSpear.FlightTime(5f, 10f, -1f), Eps);
    }
    [Test] public void Flight_NegativeRange_IsZero() { Assert.AreEqual(0f, ThrownSpear.FlightTime(5f, -2f, 5f), Eps); }

    // ThrownSpear 상태·함정 경계
    [Test] public void Spear_TrapExactlyDuration_Ends()
    {
        var s = new ThrownSpear();
        s.Throw();
        s.Land();
        s.Tick(ThrownSpear.TrapDuration - 0.5f);
        Assert.IsTrue(s.TrapActive);
        s.Tick(0.5f);
        Assert.IsFalse(s.TrapActive);
    }
    [Test] public void Spear_RetrieveInFlight_Fails()
    {
        var s = new ThrownSpear();
        s.Throw();
        Assert.IsFalse(s.Retrieve());
        Assert.IsFalse(s.Throw());
    }

    // AmmoMagazine
    [Test] public void Ammo_Start_Full()
    {
        var m = new AmmoMagazine();
        Assert.AreEqual(AmmoMagazine.Capacity, m.Ammo);
        Assert.IsTrue(m.CanFire);
    }
    [Test] public void Ammo_LastShot_StartsReload_AndBlocksFire()
    {
        var m = new AmmoMagazine();
        for (int i = 0; i < AmmoMagazine.Capacity; i++)
        {
            Assert.IsTrue(m.TryFire(out _));
        }
        Assert.AreEqual(0, m.Ammo);
        Assert.IsTrue(m.Reloading);
        Assert.IsFalse(m.TryFire(out bool crit));
        Assert.IsFalse(crit);
    }
    [Test] public void Ammo_ReloadExactBoundary_RefillsWithCrit()
    {
        var m = new AmmoMagazine();
        for (int i = 0; i < AmmoMagazine.Capacity; i++) { m.TryFire(out _); }
        m.Tick(AmmoMagazine.ReloadTime - 0.01f);
        Assert.IsTrue(m.Reloading);
        m.Tick(0.01f);
        Assert.IsFalse(m.Reloading);
        Assert.AreEqual(AmmoMagazine.Capacity, m.Ammo);
        Assert.IsTrue(m.TryFire(out bool first));
        Assert.IsTrue(first);
        Assert.IsTrue(m.TryFire(out bool second));
        Assert.IsFalse(second);
    }
    [Test] public void Ammo_Reset_ClearsReloadAndCrit()
    {
        var m = new AmmoMagazine();
        for (int i = 0; i < AmmoMagazine.Capacity; i++) { m.TryFire(out _); }
        m.Reset();
        Assert.AreEqual(AmmoMagazine.Capacity, m.Ammo);
        Assert.IsFalse(m.Reloading);
        m.TryFire(out bool crit);
        Assert.IsFalse(crit);
    }

    // TomePage
    [Test] public void Tome_Switch_Cycles_AndCooldownBlocks()
    {
        var t = new TomePage();
        Assert.IsTrue(t.TrySwitch());
        Assert.AreEqual(1, t.Page);
        Assert.IsFalse(t.TrySwitch());
        Assert.AreEqual(1, t.Page);
    }
    [Test] public void Tome_CooldownJustBeforeAndAtBoundary()
    {
        var t = new TomePage();
        t.TrySwitch();
        t.Tick(TomePage.SwitchCooldown - 0.01f);
        Assert.IsFalse(t.CanSwitch);
        t.Tick(0.01f);
        Assert.IsTrue(t.CanSwitch);
    }
    [Test] public void Tome_FullCycle_ReturnsToFirstPage()
    {
        var t = new TomePage();
        for (int i = 0; i < TomePage.PageCount; i++)
        {
            Assert.IsTrue(t.TrySwitch());
            t.Tick(TomePage.SwitchCooldown);
        }
        Assert.AreEqual(0, t.Page);
    }

    // CountdownTimer
    [Test] public void Timer_NegativeStart_GuardedToZero()
    {
        var c = new CountdownTimer();
        c.Start(-5f);
        Assert.AreEqual(0f, c.Remaining);
        Assert.IsFalse(c.Active);
    }
    [Test] public void Timer_ZeroStart_NotActive()
    {
        var c = new CountdownTimer();
        c.Start(0f);
        Assert.IsFalse(c.Active);
    }
    [Test] public void Timer_TickOverRemaining_ClampsZero()
    {
        var c = new CountdownTimer();
        c.Start(1f);
        c.Tick(5f);
        Assert.AreEqual(0f, c.Remaining);
    }
    [Test] public void Timer_ExtendKeepsLarger()
    {
        var c = new CountdownTimer();
        c.Start(3f);
        c.Extend(1f);
        Assert.AreEqual(3f, c.Remaining, Eps);
        c.Extend(4f);
        Assert.AreEqual(4f, c.Remaining, Eps);
    }
    [Test] public void Timer_NegativeTick_IgnoredAsZero()
    {
        // AUTO-109: 음수 dt 는 0 취급, 남은 시간이 늘지 않는다
        var c = new CountdownTimer();
        c.Start(1f);
        c.Tick(-2f);
        Assert.AreEqual(1f, c.Remaining, Eps);
    }

    // Cooldown
    [Test] public void Cooldown_Default_ReadyAtZero()
    {
        var c = new Cooldown();
        Assert.IsTrue(c.Ready(0f));
    }
    [Test] public void Cooldown_NowEqualsReadyAt_IsReady()
    {
        var c = new Cooldown();
        c.Arm(10f, 2f);
        Assert.AreEqual(12f, c.ReadyAt, Eps);
        Assert.IsFalse(c.Ready(11.99f));
        Assert.IsTrue(c.Ready(12f));
        Assert.IsTrue(c.Ready(12.01f));
    }
    [Test] public void Cooldown_ZeroInterval_ReadyImmediately()
    {
        var c = new Cooldown();
        c.Arm(5f, 0f);
        Assert.IsTrue(c.Ready(5f));
    }
}
