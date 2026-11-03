using System.Collections.Generic;
using NUnit.Framework;

public sealed class InventoryConvenienceTests
{
    private sealed class Catalog : IItemCatalog
    {
        private readonly Dictionary<string, ItemDef> _items = new Dictionary<string, ItemDef>
        {
            { "Wood", new ItemDef { Id = "Wood", Type = "Misc", Price = 7, MaxStack = 99, Category = "Junk" } },
            { "Ore", new ItemDef { Id = "Ore", Type = "Misc", Price = 50, MaxStack = 99, Category = "Material" } },
            { "Fang", new ItemDef { Id = "Fang", Type = "Misc", Price = 50, MaxStack = 99 } },
            { "Potion", new ItemDef { Id = "Potion", Type = "Consum", Price = 10, MaxStack = 99 } },
            { "Blade", new ItemDef { Id = "Blade", Type = "Equip", EquipArea = "Weapon", Rarity = "C" } },
            { "Helm", new ItemDef { Id = "Helm", Type = "Equip", EquipArea = "Head", Rarity = "U" } },
            { "BroochResonance", new ItemDef { Id = "BroochResonance", Type = "Misc", Price = 99 } }
        };
        public ItemDef Get(string id) { return id != null && _items.TryGetValue(id, out ItemDef result) ? result : null; }
        public bool Exists(string id) { return Get(id) != null; }
    }

    private readonly Catalog _catalog = new Catalog();
    private SlotContainer Bag() { return new SlotContainer(_catalog, 10, 10); }

    [Test]
    public void LockedStacksNeverMergeWithUnlocked_AfterSort()
    {
        SlotContainer bag = Bag();
        bag.TryAdd(new ItemStack("Wood", 3, 0, 0, true));
        bag.TryAdd(new ItemStack("Wood", 4));
        bag.Sort(new ItemSortComparer(_catalog));
        Assert.AreEqual(7, bag.Count("Wood"));
        Assert.AreNotEqual(bag[0].IsLocked, bag[1].IsLocked);
    }

    [Test]
    public void FullBagWithOnlyLockedStackRejectsUnlockedAcquisitionWithoutMutation()
    {
        var bag = new SlotContainer(_catalog, 1, 1);
        bag.TryAdd(new ItemStack("Wood", 3, 0, 0, true));
        Assert.IsFalse(bag.TryAddAll(new ItemStack("Wood", 1, 0, 0, false, true)));
        Assert.AreEqual(3, bag[0].Count);
        Assert.IsTrue(bag[0].IsLocked);
    }

    [Test]
    public void NewAcquisitionMergesIntoSeenStack_WithoutNeedingEmptySlot()
    {
        var bag = new SlotContainer(_catalog, 1, 1);
        bag.TryAdd(new ItemStack("Wood", 3));
        Assert.IsTrue(bag.TryAddAll(new ItemStack("Wood", 4, 0, 0, false, true)));
        Assert.AreEqual(7, bag[0].Count);
        Assert.IsTrue(bag[0].IsNew);
    }

    [Test]
    public void SplitMoveAndChestTransferPreserveFlags()
    {
        SlotContainer bag = Bag(), chest = Bag();
        bag.TryAdd(new ItemStack("Wood", 8, 2, 3, true, true));
        Assert.IsTrue(bag.Split(0, 3));
        Assert.IsTrue(bag.Move(1, 5));
        Assert.AreEqual(3, SlotContainer.Transfer(bag, 5, chest, 3));
        Assert.AreEqual(3, SlotContainer.Transfer(chest, 0, bag, 3));
        Assert.AreEqual(8, bag[0].Count);
        Assert.IsTrue(bag[0].IsLocked && bag[0].IsNew);
    }

    [Test]
    public void SaveRoundtripAddsFlagsWithoutChangingLegacyArrayShape()
    {
        SlotContainer bag = Bag();
        bag.TryAdd(new ItemStack("Wood", 8, 2, 3, true, true));
        string[,,] items = SaveMapper.ToArray(bag, 1, 10);
        Assert.AreEqual(4, items.GetLength(2));
        SlotContainer loaded = SaveMapper.FromArray(items, _catalog, 1, 10, ItemFlagSave.ToArray(bag, 1, 10));
        Assert.IsTrue(loaded[0].IsLocked && loaded[0].IsNew);
        SlotContainer legacy = SaveMapper.FromArray(items, _catalog, 1, 10);
        Assert.IsFalse(legacy[0].IsLocked || legacy[0].IsNew);
        Assert.AreEqual(8, legacy[0].Count);
        Assert.AreEqual(0, ItemFlagSave.At(new int[0, 0], 1, 2));
    }

    [Test]
    public void BulkSaleExcludesLockedEquipmentConsumablesUnknownAndQuestItem()
    {
        SlotContainer bag = Bag();
        bag.PutAt(0, new ItemStack("Wood", 3));
        bag.PutAt(1, new ItemStack("Wood", 4, 0, 0, true));
        bag.PutAt(2, new ItemStack("Blade", 1));
        bag.PutAt(3, new ItemStack("Potion", 5));
        bag.PutAt(4, new ItemStack("Unknown", 1));
        bag.PutAt(5, new ItemStack("BroochResonance", 1));
        bag.PutAt(6, new ItemStack("Ore", 9));
        bag.PutAt(7, new ItemStack("Fang", 9));
        BulkSaleRules.Plan plan = BulkSaleRules.Build(bag, _catalog);
        Assert.AreEqual(3, plan.Count);
        Assert.AreEqual(21L, plan.Price);
        CollectionAssert.AreEqual(new[] { 0 }, plan.Indices);
    }

    [Test]
    public void BulkSaleRequiresNewConfirmationAfterLockOrCountChanges()
    {
        SlotContainer bag = Bag();
        bag.TryAdd(new ItemStack("Wood", 3));
        BulkSaleRules.Plan before = BulkSaleRules.Build(bag, _catalog);
        Assert.IsTrue(BulkSaleRules.Matches(before, BulkSaleRules.Build(bag, _catalog)));
        bag.ReplaceAt(0, bag[0].WithFlags(true, false));
        Assert.IsFalse(BulkSaleRules.Matches(before, BulkSaleRules.Build(bag, _catalog)));
        bag.ReplaceAt(0, bag[0].WithFlags(false, false));
        bag.TryAdd(new ItemStack("Wood", 1));
        Assert.IsFalse(BulkSaleRules.Matches(before, BulkSaleRules.Build(bag, _catalog)));
    }

    [Test]
    public void BulkSaleUsesWidePriceForOverflowRejection()
    {
        var catalog = new HugePriceCatalog();
        var bag = new SlotContainer(catalog, 1, 1);
        bag.PutAt(0, new ItemStack("Expensive", 99));
        Assert.AreEqual(99L * int.MaxValue, BulkSaleRules.Build(bag, catalog).Price);
    }

    private sealed class HugePriceCatalog : IItemCatalog
    {
        public ItemDef Get(string id) { return new ItemDef { Type = "Misc", Price = int.MaxValue, Category = "Junk" }; }
        public bool Exists(string id) { return true; }
    }

    [Test]
    public void SortPrefersRarityThenTierBeforeEquipmentArea()
    {
        var comparer = new ItemSortComparer(_catalog);
        Assert.Less(comparer.Compare(new ItemStack("Helm", 1), new ItemStack("Blade", 1, 9)), 0);
        Assert.Less(comparer.Compare(new ItemStack("Blade", 1, 2), new ItemStack("Blade", 1, 1, 10)), 0);
    }

    [Test]
    public void SwapPreviewShowsLostAndGainedCumulativeTiersWithoutMutatingCounts()
    {
        var table = new SetTierTable();
        table.Define("Old", 2, "armor"); table.Define("Old", 5, "hp"); table.Define("New", 2, "damage");
        var before = new Dictionary<string, int> { { "Old", 5 }, { "New", 1 } };
        SetBonusDiff diff = SetSwapPreview.Calculate(before, "Old", "New", table);
        CollectionAssert.AreEqual(new[] { ("Old", 5) }, diff.Remove);
        CollectionAssert.AreEqual(new[] { ("New", 2) }, diff.Apply);
        Assert.AreEqual(5, before["Old"]);
        Assert.IsTrue(SetSwapPreview.Calculate(before, "Old", "Old", table).IsEmpty);
    }
}
