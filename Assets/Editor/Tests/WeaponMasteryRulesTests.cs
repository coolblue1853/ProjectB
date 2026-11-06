using NUnit.Framework;

public class WeaponMasteryRulesTests
{
    [Test]
    public void Curve_MatchesDesign()
    {
        Assert.AreEqual(400, WeaponMasteryRules.ExpToNext(1));
        Assert.AreEqual(1100, WeaponMasteryRules.ExpToNext(2));
        Assert.AreEqual(3000, WeaponMasteryRules.ExpToNext(3));
        Assert.AreEqual(7500, WeaponMasteryRules.ExpToNext(4));
        Assert.AreEqual(0, WeaponMasteryRules.ExpToNext(5));
        Assert.AreEqual(12000, WeaponMasteryRules.TotalExpForLevel(5));
        Assert.AreEqual(1500, WeaponMasteryRules.TotalExpForLevel(3));
    }

    [Test]
    public void ExpToNext_AppliesScale()
    {
        Assert.AreEqual(1000, WeaponMasteryRules.ExpToNext(1, 2.5f));
    }

    [Test]
    public void LevelFromExp_Boundaries()
    {
        Assert.AreEqual(1, WeaponMasteryRules.LevelFromExp(-5));
        Assert.AreEqual(1, WeaponMasteryRules.LevelFromExp(399));
        Assert.AreEqual(2, WeaponMasteryRules.LevelFromExp(400));
        Assert.AreEqual(2, WeaponMasteryRules.LevelFromExp(1499));
        Assert.AreEqual(3, WeaponMasteryRules.LevelFromExp(1500));
        Assert.AreEqual(4, WeaponMasteryRules.LevelFromExp(4500));
        Assert.AreEqual(5, WeaponMasteryRules.LevelFromExp(12000));
        Assert.AreEqual(5, WeaponMasteryRules.LevelFromExp(int.MaxValue));
        Assert.AreEqual(5, WeaponMasteryRules.LevelFromExp(WeaponMasteryRules.LegacyStartExp));
    }

    [Test]
    public void ExpInLevel_IsRemainder()
    {
        Assert.AreEqual(100, WeaponMasteryRules.ExpInLevel(500));
        Assert.AreEqual(0, WeaponMasteryRules.ExpInLevel(99999));
    }

    [Test]
    public void UnlockTable_PerSkillCount()
    {
        int[][] expected =
        {
            new[] { 1 },
            new[] { 1, 3 },
            new[] { 1, 2, 4 },
            new[] { 1, 2, 3, 4 },
        };
        for (int n = 1; n <= 4; n++)
        {
            for (int rank = 0; rank < n; rank++)
            {
                Assert.AreEqual(expected[n - 1][rank], WeaponMasteryRules.UnlockLevel(rank, n), "n=" + n + " rank=" + rank);
            }
        }
    }

    [Test]
    public void FirstSkill_AlwaysOpenAtLv1()
    {
        for (int n = 1; n <= 4; n++)
        {
            Assert.IsTrue(WeaponMasteryRules.IsUnlocked(0, n, 1));
        }
        Assert.IsFalse(WeaponMasteryRules.IsUnlocked(1, 2, 2));
        Assert.IsTrue(WeaponMasteryRules.IsUnlocked(1, 2, 3));
    }

    [Test]
    public void MaxLevel_UnlocksEverything()
    {
        for (int n = 1; n <= 4; n++)
        {
            for (int rank = 0; rank < n; rank++)
            {
                Assert.IsTrue(WeaponMasteryRules.IsUnlocked(rank, n, WeaponMasteryRules.MaxLevel));
            }
        }
    }

    [Test]
    public void Bonus_OnlyAtMax()
    {
        Assert.AreEqual(1f, WeaponMasteryRules.Bonus(4));
        Assert.AreEqual(1.05f, WeaponMasteryRules.Bonus(5), 1e-6f);
    }

    [Test]
    public void KeyFromName_StripsClone()
    {
        Assert.AreEqual("RustedSword", WeaponMasteryRules.KeyFromName("RustedSword(Clone)"));
        Assert.AreEqual("RustedSword", WeaponMasteryRules.KeyFromName("RustedSword"));
        Assert.AreEqual(string.Empty, WeaponMasteryRules.KeyFromName(null));
    }

    [Test]
    public void Data_AddReportsLevelUpAndRoundTrips()
    {
        var d = new WeaponMasteryData();
        Assert.IsFalse(d.Add("A", 399));
        Assert.IsTrue(d.Add("A", 1));
        Assert.AreEqual(2, d.GetLevel("A"));
        Assert.IsFalse(d.Add("A", 0));
        var d2 = new WeaponMasteryData();
        d2.LoadFrom(d.ToSave());
        Assert.AreEqual(400, d2.GetExp("A"));
        Assert.AreEqual(0, d2.GetExp("B"));
    }

    [Test]
    public void Data_RaiseToNeverLowers()
    {
        var d = new WeaponMasteryData();
        d.Add("A", 20000);
        d.RaiseTo("A", 12000);
        Assert.AreEqual(20000, d.GetExp("A"));
        d.RaiseTo("B", 12000);
        Assert.AreEqual(5, d.GetLevel("B"));
    }

    [Test]
    public void Data_LoadIgnoresBrokenSave()
    {
        var d = new WeaponMasteryData();
        d.LoadFrom(null);
        d.LoadFrom(new WeaponMasterySave { keys = new[] { "A", null, "C" }, exps = new[] { 5, 9 } });
        Assert.AreEqual(5, d.GetExp("A"));
        Assert.AreEqual(1, d.Count);
    }

    [Test]
    public void Migration_CollectsOnlyWeaponsFromAllContainers()
    {
        var inven = new string[1, 3, 4];
        inven[0, 0, 0] = "RustedSword";
        inven[0, 1, 0] = "non";
        inven[0, 2, 0] = "HealPotion";
        var chest = new string[1, 1, 4];
        chest[0, 0, 0] = "BanditShortbow";
        var gear = new string[2, 3];
        gear[0, 0] = "RustedSword";
        System.Collections.Generic.List<string> keys = WeaponMasteryData.CollectWeaponKeys(inven, chest, gear, n => n != "HealPotion");
        CollectionAssert.AreEquivalent(new[] { "RustedSword", "BanditShortbow" }, keys);
        var d = new WeaponMasteryData();
        foreach (string k in keys)
        {
            d.RaiseTo(k, WeaponMasteryRules.LegacyStartExp);
        }
        Assert.AreEqual(5, d.GetLevel("RustedSword"));
        Assert.AreEqual(1, d.GetLevel("HealPotion"));
    }
}
