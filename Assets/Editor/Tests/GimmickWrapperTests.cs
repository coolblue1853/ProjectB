using NUnit.Framework;
using UnityEngine;

public class GimmickWrapperTests
{
    private GameObject _go;
    [TearDown] public void Cleanup() { if (_go != null) { Object.DestroyImmediate(_go); } }

    [Test] public void Ammo_FireThreeThenReloadText()
    {
        _go = new GameObject("t");
        var g = _go.AddComponent<AmmoGimmick>();
        for (int i = 0; i < 3; i++) { g.OnAttackStart(null, 1); }
        Assert.AreEqual(0, g.Magazine.Ammo);
        StringAssert.Contains("재장전", g.HudText);
    }

    [Test] public void Spear_ThrowThenRetrieveNeedsLand()
    {
        _go = new GameObject("t");
        var g = _go.AddComponent<ThrownSpearGimmick>();
        g.OnAttackStart(null, 1);
        Assert.IsFalse(g.Retrieve());          // 비행 중 회수 불가
        g.Spear.Land();
        Assert.IsTrue(g.Retrieve());
        Assert.IsTrue(g.Spear.CanThrow);
    }

    [Test] public void Tome_SwitchCycleAndCooldown()
    {
        _go = new GameObject("t");
        var g = _go.AddComponent<TomePageGimmick>();
        Assert.IsTrue(g.Tome.TrySwitch());
        Assert.AreEqual(1, g.Tome.Page);
        Assert.IsFalse(g.Tome.TrySwitch());
        StringAssert.Contains("냉기", g.HudText);
    }
}
