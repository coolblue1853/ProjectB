using System.Collections.Generic;
using NUnit.Framework;

public sealed class ChestTransferTests
{
    private sealed class Catalog : IItemCatalog
    {
        private readonly Dictionary<string, ItemDef> _items = new Dictionary<string, ItemDef>
        {
            { "Wood", new ItemDef { Id = "Wood", Type = "Misc", MaxStack = 10, Rarity = "C", DisplayName = "나무" } },
            { "Ore", new ItemDef { Id = "Ore", Type = "Misc", MaxStack = 10, Rarity = "R", DisplayName = "광석" } },
            { "Potion", new ItemDef { Id = "Potion", Type = "Consum", MaxStack = 10, Rarity = "C", DisplayName = "물약" } },
            { "Sword", new ItemDef { Id = "Sword", Type = "Equip", MaxStack = 1, Rarity = "R", DisplayName = "검" } },
            { "Armor", new ItemDef { Id = "Armor", Type = "Equip", MaxStack = 1, Rarity = "U", DisplayName = "갑옷" } }
        };

        public ItemDef Get(string id)
        {
            return id != null && _items.TryGetValue(id, out ItemDef item) ? item : null;
        }

        public bool Exists(string id)
        {
            return Get(id) != null;
        }
    }

    private readonly Catalog _catalog = new Catalog();

    private SlotContainer Container(int capacity)
    {
        return new SlotContainer(_catalog, capacity, capacity);
    }

    [Test]
    public void FullInventory_OnlyMergeRoomMoves_ConservesBothSides()
    {
        SlotContainer inventory = Container(1), chest = Container(1);
        inventory.TryAdd(new ItemStack("Wood", 8));
        chest.TryAdd(new ItemStack("Wood", 7));
        Assert.AreEqual(2, SlotContainer.Transfer(chest, 0, inventory, 7));
        Assert.AreEqual(10, inventory.Count("Wood"));
        Assert.AreEqual(5, chest.Count("Wood"));
        Assert.AreEqual(0, SlotContainer.Transfer(chest, 0, inventory, 7));
        Assert.AreEqual(15, inventory.Count("Wood") + chest.Count("Wood"));
    }

    [Test]
    public void FullChest_NoMutationOrEvents()
    {
        SlotContainer inventory = Container(1), chest = Container(1);
        inventory.TryAdd(new ItemStack("Wood", 7));
        chest.TryAdd(new ItemStack("Sword", 1, 2, 8));
        int events = 0;
        inventory.OnSlotChanged += _ => events++;
        chest.OnSlotChanged += _ => events++;
        Assert.AreEqual(0, ChestTransferRules.Deposit(inventory, chest, _catalog));
        Assert.AreEqual(0, events);
        Assert.AreEqual(7, inventory.Count("Wood"));
        Assert.AreEqual(8, chest[0].Upgrade);
    }

    [Test]
    public void DepositSameKind_AllStacksAcrossPages_LeavesOtherKinds()
    {
        SlotContainer inventory = Container(60), chest = Container(60);
        inventory.PutAt(0, new ItemStack("Wood", 6));
        inventory.PutAt(35, new ItemStack("Wood", 9));
        inventory.PutAt(36, new ItemStack("Potion", 4));
        chest.PutAt(31, new ItemStack("Wood", 7));
        Assert.AreEqual(15, ChestTransferRules.Deposit(inventory, chest, _catalog, "Wood"));
        Assert.AreEqual(0, inventory.Count("Wood"));
        Assert.AreEqual(22, chest.Count("Wood"));
        Assert.AreEqual(10, chest[31].Count);
        Assert.AreEqual(4, inventory.Count("Potion"));
    }

    [Test]
    public void DepositMaterials_ExcludesConsumablesAndEquipment()
    {
        SlotContainer inventory = Container(6), chest = Container(6);
        inventory.TryAdd(new ItemStack("Wood", 8));
        inventory.TryAdd(new ItemStack("Ore", 3));
        inventory.TryAdd(new ItemStack("Potion", 6));
        inventory.TryAdd(new ItemStack("Sword", 1, 2, 7));
        Assert.AreEqual(11, ChestTransferRules.Deposit(inventory, chest, _catalog));
        Assert.AreEqual(6, inventory.Count("Potion"));
        Assert.AreEqual(1, inventory.Count("Sword"));
        Assert.AreEqual(0, chest.Count("Sword"));
    }

    [Test]
    public void DepositConsumables_MergesSameId_PreservesVariants()
    {
        SlotContainer inventory = Container(4), chest = Container(4);
        inventory.TryAdd(new ItemStack("Potion", 8, 1, 2));
        inventory.TryAdd(new ItemStack("Potion", 7, 0, 0));
        chest.TryAdd(new ItemStack("Potion", 2, 1, 2));
        Assert.AreEqual(15, ChestTransferRules.Deposit(inventory, chest, _catalog, "Potion"));
        Assert.AreEqual(10, chest[0].Count);
        Assert.AreEqual(1, chest[0].Tier);
        Assert.AreEqual(2, chest[0].Upgrade);
        Assert.AreEqual(7, chest[1].Count);
        Assert.AreEqual(0, chest[1].Tier);
    }

    [Test]
    public void Equipment_RoundTripPreservesTierAndUpgrade()
    {
        SlotContainer inventory = Container(1), chest = Container(1);
        inventory.TryAdd(new ItemStack("Sword", 1, 3, 10));
        Assert.AreEqual(1, SlotContainer.Transfer(inventory, 0, chest, 1));
        Assert.AreEqual(1, SlotContainer.Transfer(chest, 0, inventory, 1));
        Assert.AreEqual(3, inventory[0].Tier);
        Assert.AreEqual(10, inventory[0].Upgrade);
        Assert.IsTrue(chest[0].IsEmpty);
    }

    [Test]
    public void WithdrawQuantity_MovesOnlyRequestedAmount_AndCanEmptyStack()
    {
        SlotContainer inventory = Container(2), chest = Container(1);
        chest.TryAdd(new ItemStack("Potion", 9));
        Assert.AreEqual(3, SlotContainer.Transfer(chest, 0, inventory, 3));
        Assert.AreEqual(6, chest[0].Count);
        Assert.AreEqual(6, SlotContainer.Transfer(chest, 0, inventory, 6));
        Assert.IsTrue(chest[0].IsEmpty);
        Assert.AreEqual(9, inventory[0].Count);
    }

    [Test]
    public void Sort_MergesThenOrdersTypeRarityName_PreservesVariantsAndTotals()
    {
        SlotContainer chest = Container(10);
        chest.PutAt(0, new ItemStack("Wood", 4));
        chest.PutAt(1, new ItemStack("Potion", 2));
        chest.PutAt(2, new ItemStack("Sword", 1, 3, 8));
        chest.PutAt(3, new ItemStack("Armor", 1));
        chest.PutAt(4, new ItemStack("Ore", 1));
        chest.PutAt(5, new ItemStack("Wood", 6));
        chest.PutAt(6, new ItemStack("Wood", 3, 1, 0));
        chest.Sort(new ChestItemComparer(_catalog));
        Assert.AreEqual("Armor", chest[0].Id);
        Assert.AreEqual("Sword", chest[1].Id);
        Assert.AreEqual(3, chest[1].Tier);
        Assert.AreEqual(8, chest[1].Upgrade);
        Assert.AreEqual("Potion", chest[2].Id);
        Assert.AreEqual("Ore", chest[3].Id);
        Assert.AreEqual(13, chest.Count("Wood"));
        Assert.AreEqual(1, chest[4].Tier);
        Assert.AreEqual(10, chest[5].Count);
        Assert.IsTrue(chest[6].IsEmpty);
    }

    [Test]
    public void TransferToSelfOrInvalidIndex_DoesNotDuplicate()
    {
        SlotContainer chest = Container(2);
        chest.TryAdd(new ItemStack("Wood", 8));
        Assert.AreEqual(0, SlotContainer.Transfer(chest, 0, chest, 4));
        Assert.AreEqual(0, SlotContainer.Transfer(chest, -1, Container(2), 4));
        Assert.AreEqual(8, chest.Count("Wood"));
    }

    [Test]
    public void SaveRoundTrip_BothSidesAfterPartialTransfer_PreservesTotalAndAttributes()
    {
        SlotContainer inventory = Container(1), chest = Container(1);
        inventory.TryAdd(new ItemStack("Wood", 8, 2, 3));
        chest.TryAdd(new ItemStack("Wood", 7, 2, 3));
        SlotContainer.Transfer(chest, 0, inventory, 7);
        SlotContainer loadedInventory = SaveMapper.FromArray(SaveMapper.ToArray(inventory, 1, 1), _catalog, 1, 1);
        SlotContainer loadedChest = SaveMapper.FromArray(SaveMapper.ToArray(chest, 1, 1), _catalog, 1, 1);
        Assert.AreEqual(15, loadedInventory.Count("Wood") + loadedChest.Count("Wood"));
        Assert.AreEqual(2, loadedChest[0].Tier);
        Assert.AreEqual(3, loadedInventory[0].Upgrade);
    }
}
