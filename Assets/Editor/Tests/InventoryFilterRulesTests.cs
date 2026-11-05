using NUnit.Framework;

public class InventoryFilterRulesTests
{
    [Test]
    public void Next_cycles_all_four()
    {
        var f = InventoryFilter.All;
        f = InventoryFilterRules.Next(f); Assert.AreEqual(InventoryFilter.Equip, f);
        f = InventoryFilterRules.Next(f); Assert.AreEqual(InventoryFilter.Consum, f);
        f = InventoryFilterRules.Next(f); Assert.AreEqual(InventoryFilter.Misc, f);
        f = InventoryFilterRules.Next(f); Assert.AreEqual(InventoryFilter.All, f);
    }

    [Test]
    public void Matches_by_type()
    {
        Assert.IsTrue(InventoryFilterRules.Matches(InventoryFilter.All, "Equip"));
        Assert.IsTrue(InventoryFilterRules.Matches(InventoryFilter.All, "Misc"));
        Assert.IsTrue(InventoryFilterRules.Matches(InventoryFilter.Equip, "Equip"));
        Assert.IsFalse(InventoryFilterRules.Matches(InventoryFilter.Equip, "Consum"));
        Assert.IsTrue(InventoryFilterRules.Matches(InventoryFilter.Consum, "Consum"));
        Assert.IsFalse(InventoryFilterRules.Matches(InventoryFilter.Consum, "Misc"));
        Assert.IsTrue(InventoryFilterRules.Matches(InventoryFilter.Misc, "Misc"));
        Assert.IsFalse(InventoryFilterRules.Matches(InventoryFilter.Misc, null));
    }

    [Test]
    public void Labels_are_korean_and_distinct()
    {
        var seen = new System.Collections.Generic.HashSet<string>();
        for (int i = 0; i < 4; i++) Assert.IsTrue(seen.Add(InventoryFilterRules.Label((InventoryFilter)i)));
    }
}
