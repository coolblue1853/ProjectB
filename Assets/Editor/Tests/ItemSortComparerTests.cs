using System.Collections.Generic;
using NUnit.Framework;

/// <summary>InventoryRefactorDesign 3.2 정렬(D4) — ItemSortComparer · SlotContainer.SortRange</summary>
public class ItemSortComparerTests
{
    class FakeCatalog : IItemCatalog
    {
        readonly Dictionary<string, ItemDef> defs = new Dictionary<string, ItemDef>
        {
            { "Wood", new ItemDef { Id = "Wood", Type = "Misc", MaxStack = 10, DisplayName = "Wood" } },
            { "Potion", new ItemDef { Id = "Potion", Type = "Consum", MaxStack = 10, DisplayName = "Potion" } },
            { "Helm", new ItemDef { Id = "Helm", Type = "Equip", EquipArea = "Head", Rarity = "C", DisplayName = "Helm" } },
            { "Blade", new ItemDef { Id = "Blade", Type = "Equip", EquipArea = "Weapon", Rarity = "C", DisplayName = "Blade" } },
            { "Saber", new ItemDef { Id = "Saber", Type = "Equip", EquipArea = "Weapon", Rarity = "U", DisplayName = "Saber" } },
        };
        public ItemDef Get(string id) => id != null && defs.TryGetValue(id, out var d) ? d : null;
        public bool Exists(string id) => Get(id) != null;
    }

    static SlotContainer New(int capacity, int unlocked) => new SlotContainer(new FakeCatalog(), capacity, unlocked);

    [Test]
    public void Sort_TypeThenAreaThenRarity()
    {
        var c = New(6, 6);
        c.TryAdd(new ItemStack("Wood", 3));
        c.TryAdd(new ItemStack("Helm", 1));
        c.TryAdd(new ItemStack("Potion", 2));
        c.TryAdd(new ItemStack("Blade", 1));
        c.TryAdd(new ItemStack("Saber", 1));
        c.Sort(new ItemSortComparer(new FakeCatalog()));
        Assert.AreEqual(new[] { "Saber", "Blade", "Helm", "Potion", "Wood" }, new[] { c[0].Id, c[1].Id, c[2].Id, c[3].Id, c[4].Id });
        Assert.IsTrue(c[5].IsEmpty);
    }

    [Test]
    public void Sort_HigherUpgradeFirst_AndMergesStacks()
    {
        var c = New(6, 6);
        c.TryAdd(new ItemStack("Blade", 1, 0, 1));
        c.TryAdd(new ItemStack("Blade", 1, 0, 5));
        c.TryAdd(new ItemStack("Wood", 4));
        c.TryAdd(new ItemStack("Wood", 4));
        c.TryAdd(new ItemStack("Wood", 4));  // 4+4+2 / 2 → 정렬 시 10 + 2
        c.Sort(new ItemSortComparer(new FakeCatalog()));
        Assert.AreEqual(5, c[0].Upgrade);
        Assert.AreEqual(1, c[1].Upgrade);
        Assert.AreEqual(10, c[2].Count);
        Assert.AreEqual(2, c[3].Count);
        Assert.AreEqual(12, c.Count("Wood"));
    }

    [Test]
    public void SortRange_OnlyTouchesThatPage()
    {
        var c = New(4, 4);
        c.TryAdd(new ItemStack("Wood", 10));
        c.TryAdd(new ItemStack("Potion", 1));
        c.TryAdd(new ItemStack("Wood", 10));
        c.TryAdd(new ItemStack("Blade", 1));
        c.SortRange(2, 2, new ItemSortComparer(new FakeCatalog()));
        Assert.AreEqual("Wood", c[0].Id);
        Assert.AreEqual("Potion", c[1].Id);
        Assert.AreEqual("Blade", c[2].Id);
        Assert.AreEqual("Wood", c[3].Id);
    }

    [Test]
    public void SortRange_IgnoresLockedSlots()
    {
        var c = New(4, 2);
        c.TryAdd(new ItemStack("Wood", 1));
        c.SortRange(0, 4, new ItemSortComparer(new FakeCatalog()));
        Assert.AreEqual(1, c.Count("Wood"));
        Assert.AreEqual("Wood", c[0].Id);
    }
}
