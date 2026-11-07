using NUnit.Framework;

public class TypeMasteryRulesTests
{
    [Test]
    public void Curve_TotalIs152000AndFirstFourMatchLegacy()
    {
        Assert.AreEqual(152000, TypeMasteryRules.TotalExpForLevel(10));
        Assert.AreEqual(WeaponMasteryRules.TotalExpForLevel(5), TypeMasteryRules.TotalExpForLevel(5));
        Assert.AreEqual(12000, TypeMasteryRules.TotalExpForLevel(5));
        Assert.AreEqual(12000, TypeMasteryRules.ExpToNext(5));
        Assert.AreEqual(48000, TypeMasteryRules.ExpToNext(9));
        Assert.AreEqual(0, TypeMasteryRules.ExpToNext(10));
        for (int lv = 1; lv <= 4; lv++)
        {
            Assert.AreEqual(WeaponMasteryRules.ExpToNext(lv), TypeMasteryRules.ExpToNext(lv));
        }
    }

    [Test]
    public void LevelFromExp_Boundaries()
    {
        Assert.AreEqual(1, TypeMasteryRules.LevelFromExp(-1));
        Assert.AreEqual(1, TypeMasteryRules.LevelFromExp(399));
        Assert.AreEqual(2, TypeMasteryRules.LevelFromExp(400));
        Assert.AreEqual(5, TypeMasteryRules.LevelFromExp(12000));
        Assert.AreEqual(5, TypeMasteryRules.LevelFromExp(23999));
        Assert.AreEqual(6, TypeMasteryRules.LevelFromExp(24000));
        Assert.AreEqual(8, TypeMasteryRules.LevelFromExp(103999));
        Assert.AreEqual(9, TypeMasteryRules.LevelFromExp(104000));
        Assert.AreEqual(9, TypeMasteryRules.LevelFromExp(151999));
        Assert.AreEqual(10, TypeMasteryRules.LevelFromExp(152000));
        Assert.AreEqual(10, TypeMasteryRules.LevelFromExp(int.MaxValue));
    }

    [Test]
    public void ExpInLevel_ZeroAtMax()
    {
        Assert.AreEqual(100, TypeMasteryRules.ExpInLevel(500));
        Assert.AreEqual(1000, TypeMasteryRules.ExpInLevel(25000));
        Assert.AreEqual(0, TypeMasteryRules.ExpInLevel(999999));
    }

    [Test]
    public void TitleIndex_FiveStages()
    {
        int[] expected = { 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4 };
        for (int lv = 1; lv <= 10; lv++)
        {
            Assert.AreEqual(expected[lv], TypeMasteryRules.TitleIndex(lv), "lv=" + lv);
        }
        Assert.AreEqual(0, TypeMasteryRules.TitleIndex(0));
        Assert.AreEqual(4, TypeMasteryRules.TitleIndex(99));
    }

    [Test]
    public void EarnedPoints_IsLevelMinusOneClamped()
    {
        Assert.AreEqual(0, TypeMasteryRules.EarnedPoints(1));
        Assert.AreEqual(1, TypeMasteryRules.EarnedPoints(2));
        Assert.AreEqual(9, TypeMasteryRules.EarnedPoints(10));
        Assert.AreEqual(9, TypeMasteryRules.EarnedPoints(50));
        Assert.AreEqual(0, TypeMasteryRules.EarnedPoints(-3));
    }

    [Test]
    public void Data_AddLevelUpRoundTripAndKindFilter()
    {
        var d = new TypeMasteryData();
        Assert.IsFalse(d.Add("Sword", 399));
        Assert.IsTrue(d.Add("Sword", 1));
        Assert.AreEqual(2, d.GetLevel("Sword"));
        Assert.IsFalse(d.Add("Nonsense", 5000));
        Assert.IsFalse(d.Add("None", 5000));
        var d2 = new TypeMasteryData();
        d2.LoadFrom(d.ToSave());
        Assert.AreEqual(400, d2.GetExp("Sword"));
        Assert.AreEqual(1, d2.Count);
        d2.LoadFrom(new TypeMasterySave { kinds = new[] { "Bow", "Bad", null }, exps = new[] { 7, 9, 3 } });
        Assert.AreEqual(1, d2.Count);
        d2.LoadFrom(null);
        Assert.AreEqual(0, d2.Count);
    }

    [Test]
    public void Seed_UsesMaxPerKindNotSum()
    {
        var d = new TypeMasteryData();
        string[] keys = { "SwordA", "SwordB", "BowA", "Unknown" };
        int[] exps = { 12000, 400, 3000, 99999 };
        int unresolved = d.SeedFromWeapons(keys, exps, k => k.StartsWith("Sword") ? WeaponKind.Sword : k.StartsWith("Bow") ? WeaponKind.Bow : WeaponKind.None);
        Assert.AreEqual(1, unresolved);
        Assert.AreEqual(12000, d.GetExp("Sword"));
        Assert.AreEqual(5, d.GetLevel("Sword"));
        Assert.AreEqual(3000, d.GetExp("Bow"));
    }

    [Test]
    public void Seed_SecondWeaponDoesNotLowerOrInflate()
    {
        var d = new TypeMasteryData();
        d.SeedFromWeapons(new[] { "A" }, new[] { 12000 }, k => WeaponKind.Axe);
        d.SeedFromWeapons(new[] { "B" }, new[] { 12000 }, k => WeaponKind.Axe);
        Assert.AreEqual(12000, d.GetExp("Axe"));
    }
}
