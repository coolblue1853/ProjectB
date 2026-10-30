using System.Collections.Generic;
using NUnit.Framework;

/// <summary>InventoryRefactorDesign 4절 Phase 1 테스트 목록 (SlotContainerTests · TransferTests)</summary>
public class SlotContainerTests
{
    class FakeCatalog : IItemCatalog
    {
        readonly Dictionary<string, ItemDef> defs = new Dictionary<string, ItemDef>
        {
            { "Wood", new ItemDef { Id = "Wood", MaxStack = 10 } },
            { "Ore", new ItemDef { Id = "Ore", MaxStack = 10 } },
            { "Sword", new ItemDef { Id = "Sword", MaxStack = 1, Type = "Equip" } },
        };
        public ItemDef Get(string id) => id != null && defs.TryGetValue(id, out var d) ? d : null;
        public bool Exists(string id) => Get(id) != null;
    }

    static SlotContainer New(int capacity = 6, int unlocked = 6) => new SlotContainer(new FakeCatalog(), capacity, unlocked);
    static ItemStack Wood(int n) => new ItemStack("Wood", n);

    [Test]
    public void Add_EmptyContainer_GoesToFirstSlot()
    {
        var c = New();
        Assert.AreEqual(3, c.TryAdd(Wood(3)));
        Assert.AreEqual(3, c[0].Count);
    }

    [Test]
    public void Add_PrefersPartialStackOverEarlierEmptySlot()
    {
        var c = New();
        c.TryAdd(new ItemStack("Ore", 1));
        c.TryAdd(Wood(4));          // slot1
        c.TryRemove("Ore", 1);      // slot0 비움
        c.TryAdd(Wood(3));
        Assert.IsTrue(c[0].IsEmpty);
        Assert.AreEqual(7, c[1].Count);
    }

    [Test]
    public void Add_OverflowsMaxStack()
    {
        var c = New();
        Assert.AreEqual(25, c.TryAdd(Wood(25)));
        Assert.AreEqual(10, c[0].Count);
        Assert.AreEqual(10, c[1].Count);
        Assert.AreEqual(5, c[2].Count);
    }

    [Test]
    public void Add_Full_ReturnsZeroAndNoChange()
    {
        var c = New(2, 2);
        c.TryAdd(new ItemStack("Sword", 1));
        c.TryAdd(new ItemStack("Sword", 1));
        int events = 0;
        c.OnSlotChanged += _ => events++;
        Assert.AreEqual(0, c.TryAdd(Wood(1)));
        Assert.AreEqual(0, events);
    }

    [Test]
    public void AddAll_PartialFit_AddsNothing()
    {
        var c = New(2, 2);
        Assert.IsFalse(c.TryAddAll(Wood(25)));
        Assert.AreEqual(0, c.Count("Wood"));
    }

    [Test]
    public void DifferentUpgrade_DoesNotStack()
    {
        var c = New();
        c.TryAdd(new ItemStack("Wood", 1, 0, 0));
        c.TryAdd(new ItemStack("Wood", 1, 0, 3));
        Assert.AreEqual(1, c[0].Count);
        Assert.AreEqual(3, c[1].Upgrade);
    }

    [Test]
    public void LockedSlots_NotUsed()
    {
        var c = New(6, 2);
        Assert.AreEqual(20, c.TryAdd(Wood(25)));
        Assert.IsTrue(c[2].IsEmpty);
    }

    [Test]
    public void Remove_SpansSlotsAndClears()
    {
        var c = New();
        c.TryAdd(Wood(15));
        Assert.IsTrue(c.TryRemove("Wood", 12));
        Assert.IsTrue(c[0].IsEmpty);
        Assert.AreEqual(3, c[1].Count);
    }

    [Test]
    public void Remove_NotEnough_FailsNoChange()
    {
        var c = New();
        c.TryAdd(Wood(5));
        Assert.IsFalse(c.TryRemove("Wood", 6));
        Assert.AreEqual(5, c.Count("Wood"));
    }

    [Test]
    public void Move_ToEmpty_Moves()
    {
        var c = New();
        c.TryAdd(Wood(5));
        Assert.IsTrue(c.Move(0, 3));
        Assert.IsTrue(c[0].IsEmpty);
        Assert.AreEqual(5, c[3].Count);
    }

    [Test]
    public void Move_DifferentItems_Swaps()
    {
        var c = New();
        c.TryAdd(Wood(5));
        c.TryAdd(new ItemStack("Ore", 2));
        Assert.IsTrue(c.Move(0, 1));
        Assert.AreEqual("Ore", c[0].Id);
        Assert.AreEqual("Wood", c[1].Id);
    }

    [Test]
    public void Move_SameItem_MergesWithOverflowLeftBehind()
    {
        var c = New();
        c.TryAdd(Wood(10));
        c.TryAdd(Wood(8));          // slot1 = 8
        c.TryRemove("Wood", 4);     // slot0 = 6, slot1 = 8
        Assert.IsTrue(c.Move(0, 1));
        Assert.AreEqual(10, c[1].Count);
        Assert.AreEqual(4, c[0].Count);
    }

    [Test]
    public void Move_FullStacks_Swap()
    {
        var c = New();
        c.TryAdd(new ItemStack("Wood", 10, 1, 0));
        c.TryAdd(new ItemStack("Wood", 10, 1, 0));
        Assert.IsTrue(c.Move(0, 1));
        Assert.AreEqual(10, c[0].Count);
        Assert.AreEqual(10, c[1].Count);
    }

    [Test]
    public void Split_IntoEmptySlot()
    {
        var c = New();
        c.TryAdd(Wood(8));
        Assert.IsTrue(c.Split(0, 3));
        Assert.AreEqual(5, c[0].Count);
        Assert.AreEqual(3, c[1].Count);
    }

    [Test]
    public void Split_NoEmptySlot_FailsAndPreservesTotal()
    {
        var c = New(2, 2);
        c.TryAdd(Wood(8));
        c.TryAdd(new ItemStack("Ore", 1));
        Assert.IsFalse(c.Split(0, 3));
        Assert.AreEqual(8, c.Count("Wood"));
    }

    [Test]
    public void Split_PrefersGivenPage_KeepsTierAndUpgrade()
    {
        var c = New(6, 6); // 3칸 × 2페이지
        Assert.IsTrue(c.PutAt(0, new ItemStack("Wood", 8, 2, 3)));
        Assert.IsTrue(c.Split(0, 3, 1, 3));
        Assert.AreEqual(5, c[0].Count);
        Assert.IsTrue(c[1].IsEmpty, "같은 페이지 빈칸보다 지정 페이지가 먼저");
        Assert.AreEqual(3, c[3].Count);
        Assert.AreEqual(2, c[3].Tier);
        Assert.AreEqual(3, c[3].Upgrade);
    }

    [Test]
    public void Split_GivenPageFull_FallsBackToAnyEmptySlot()
    {
        var c = New(4, 4); // 2칸 × 2페이지, 1페이지 꽉 참
        c.PutAt(0, Wood(8));
        c.PutAt(2, new ItemStack("Ore", 1));
        c.PutAt(3, new ItemStack("Ore", 1));
        Assert.IsTrue(c.Split(0, 3, 1, 2));
        Assert.AreEqual(3, c[1].Count);
        Assert.AreEqual(8, c.Count("Wood"));
    }

    [Test]
    public void ReplaceAt_ChangesUpgradeInPlace_RejectsOtherIdOrEmpty()
    {
        var c = New();
        c.PutAt(2, new ItemStack("Sword", 1, 1, 0));
        int events = 0;
        c.OnSlotChanged += i => events++;
        Assert.IsTrue(c.ReplaceAt(2, new ItemStack("Sword", 1, 1, 5)));
        Assert.AreEqual(5, c[2].Upgrade);
        Assert.AreEqual(1, c[2].Tier);
        Assert.AreEqual(1, events);
        Assert.IsTrue(c.ReplaceAt(2, new ItemStack("Sword", 1, 1, 5)), "같은 값(강화 실패 유지)은 성공·이벤트 없음");
        Assert.AreEqual(1, events);
        Assert.IsFalse(c.ReplaceAt(2, new ItemStack("Sword", 2, 1, 5)), "수량 변경은 거절");
        Assert.IsFalse(c.ReplaceAt(2, new ItemStack("Wood", 1)));
        Assert.IsFalse(c.ReplaceAt(0, new ItemStack("Sword", 1)));
        Assert.AreEqual(5, c[2].Upgrade);
    }

    [Test]
    public void Split_AmountMustBeBetweenOneAndCount()
    {
        var c = New();
        c.TryAdd(Wood(5));
        Assert.IsFalse(c.Split(0, 0));
        Assert.IsFalse(c.Split(0, 5));
    }

    [Test]
    public void Events_OnlyForChangedSlots()
    {
        var c = New();
        var changed = new List<int>();
        c.OnSlotChanged += changed.Add;
        c.TryAdd(Wood(15));
        CollectionAssert.AreEqual(new[] { 0, 1 }, changed);
    }

    [Test]
    public void Transfer_All()
    {
        var inv = New();
        var chest = New();
        inv.TryAdd(Wood(7));
        Assert.AreEqual(7, SlotContainer.Transfer(inv, 0, chest, 7));
        Assert.IsTrue(inv[0].IsEmpty);
        Assert.AreEqual(7, chest.Count("Wood"));
    }

    [Test]
    public void Transfer_DestinationFull_DeductsOnlyMoved()
    {
        var inv = New();
        var chest = New(1, 1);
        chest.TryAdd(Wood(8));
        inv.TryAdd(Wood(7));
        Assert.AreEqual(2, SlotContainer.Transfer(inv, 0, chest, 7));
        Assert.AreEqual(5, inv[0].Count);
        Assert.AreEqual(10, chest[0].Count);
    }

    [Test]
    public void Transfer_PreservesTierAndUpgrade()
    {
        var inv = New();
        var chest = New();
        inv.TryAdd(new ItemStack("Sword", 1, 2, 7));
        SlotContainer.Transfer(inv, 0, chest, 1);
        Assert.AreEqual(2, chest[0].Tier);
        Assert.AreEqual(7, chest[0].Upgrade);
    }

    [Test]
    public void Sort_MergesThenOrders()
    {
        var c = New();
        c.TryAdd(new ItemStack("Ore", 3));
        c.TryAdd(Wood(4));
        c.Move(1, 4);
        c.TryAdd(Wood(10));  // slot1 에 4 → 아니, 덜 찬 스택(4번 칸)부터 채움
        c.Sort(Comparer<ItemStack>.Create((a, b) => string.CompareOrdinal(b.Id, a.Id)));
        Assert.AreEqual("Wood", c[0].Id);
        Assert.AreEqual(10, c[0].Count);
        Assert.AreEqual(4, c[1].Count);
        Assert.AreEqual("Ore", c[2].Id);
        Assert.IsTrue(c[3].IsEmpty);
    }
}
