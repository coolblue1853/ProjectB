using NUnit.Framework;

public class WeaponMiscRulesTests
{
    [Test] public void Ammo_ThreeShotsThenReloadThenCrit()
    {
        var m = new AmmoMagazine();
        bool crit;
        for (int i = 0; i < 3; i++)
        {
            Assert.IsTrue(m.TryFire(out crit));
            Assert.IsFalse(crit);
        }
        Assert.IsTrue(m.Reloading);
        Assert.IsFalse(m.TryFire(out crit));
        m.Tick(1.1f);
        Assert.IsFalse(m.CanFire);
        m.Tick(0.2f);
        Assert.AreEqual(3, m.Ammo);
        Assert.IsTrue(m.TryFire(out crit));
        Assert.IsTrue(crit);
        Assert.IsTrue(m.TryFire(out crit));
        Assert.IsFalse(crit);
    }

    [Test] public void Spear_ThrowLandTrapRetrieve()
    {
        var s = new ThrownSpear();
        Assert.IsTrue(s.Throw());
        Assert.IsFalse(s.Throw());
        Assert.IsFalse(s.Retrieve(), "비행 중 회수 불가");
        s.Land();
        Assert.IsTrue(s.TrapActive);
        s.Tick(4.9f);
        Assert.IsTrue(s.TrapActive);
        s.Tick(0.2f);
        Assert.IsFalse(s.TrapActive);
        Assert.IsTrue(s.Retrieve());
        Assert.IsTrue(s.CanThrow);
    }

    [Test] public void Spear_RetrieveClearsTrap()
    {
        var s = new ThrownSpear();
        s.Throw();
        s.Land();
        s.Retrieve();
        Assert.IsFalse(s.TrapActive);
    }

    [Test] public void Tome_CyclesThreePagesWithCooldown()
    {
        var t = new TomePage();
        Assert.AreEqual(0, t.Page);
        Assert.IsTrue(t.TrySwitch());
        Assert.AreEqual(1, t.Page);
        Assert.IsFalse(t.TrySwitch());
        t.Tick(3f);
        Assert.IsTrue(t.TrySwitch());
        t.Tick(3f);
        Assert.IsTrue(t.TrySwitch());
        Assert.AreEqual(0, t.Page);
    }
}
