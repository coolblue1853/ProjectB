using System.Collections.Generic;
using NUnit.Framework;

/// <summary>SLOT-1: 신규 장비 슬롯 모델·직렬화 (EquipSlotExpansion.md 4~5절)</summary>
public class EquipSlotExpansionTests
{
    class Cat : IItemCatalog
    {
        readonly Dictionary<string, ItemDef> defs = new Dictionary<string, ItemDef>
        {
            { "Crown", new ItemDef { Id = "Crown", Type = "Equip", EquipArea = "Title", MaxStack = 1 } },
            { "Halo", new ItemDef { Id = "Halo", Type = "Equip", EquipArea = "Aura", MaxStack = 1 } },
            { "Sword", new ItemDef { Id = "Sword", Type = "Equip", EquipArea = "Weapon", MaxStack = 1 } },
        };
        public ItemDef Get(string id) => id != null && defs.TryGetValue(id, out var d) ? d : null;
        public bool Exists(string id) => Get(id) != null;
    }

    [Test]
    public void FloorToSlot_MapsOneToFour()
    {
        Assert.IsFalse(SlotRules.TrySlotForFloor(0, out _));
        Assert.IsTrue(SlotRules.TrySlotForFloor(1, out EquipSlot a));
        Assert.AreEqual(EquipSlot.Title, a);
        Assert.IsTrue(SlotRules.TrySlotForFloor(4, out EquipSlot b));
        Assert.AreEqual(EquipSlot.Cape, b);
        Assert.IsFalse(SlotRules.TrySlotForFloor(5, out _));
    }

    [Test]
    public void Enum_ExistingValuesUnchanged_NewValues()
    {
        Assert.AreEqual(1, (int)EquipSlot.Ring);
        Assert.AreEqual(9, (int)EquipSlot.Shoes);
        Assert.AreEqual(10, (int)EquipSlot.Title);
        Assert.AreEqual(11, (int)EquipSlot.Aura);
        Assert.AreEqual(12, (int)EquipSlot.Charm);
        Assert.AreEqual(13, (int)EquipSlot.Cape);
    }

    [Test]
    public void SlotRules_BitsAndDefaults()
    {
        Assert.AreEqual(1, SlotRules.Bit(EquipSlot.Title));
        Assert.AreEqual(2, SlotRules.Bit(EquipSlot.Aura));
        Assert.AreEqual(4, SlotRules.Bit(EquipSlot.Charm));
        Assert.AreEqual(8, SlotRules.Bit(EquipSlot.Cape));
        Assert.AreEqual(0, SlotRules.Bit(EquipSlot.Ring));
        Assert.IsTrue(SlotRules.IsUnlocked(0, EquipSlot.Weapon));
        Assert.IsFalse(SlotRules.IsUnlocked(0, EquipSlot.Title));
        Assert.IsTrue(SlotRules.IsUnlocked(SlotRules.Unlock(0, EquipSlot.Aura), EquipSlot.Aura));
    }

    [Test]
    public void LockedSlot_RejectsEquip_UnlockAllows()
    {
        var cat = new Cat();
        var inv = new SlotContainer(cat, 4, 4);
        var eq = new EquipmentModel(cat);
        inv.TryAdd(new ItemStack("Crown", 1));
        Assert.IsFalse(eq.Equip(inv, 0));
        Assert.IsFalse(inv[0].IsEmpty);
        Assert.IsTrue(eq.Unlock(EquipSlot.Title));
        Assert.IsFalse(eq.Unlock(EquipSlot.Title));
        Assert.IsFalse(eq.Unlock(EquipSlot.Weapon));
        Assert.IsTrue(eq.Equip(inv, 0));
        Assert.AreEqual("Crown", eq[EquipSlot.Title].Id);
    }

    [Test]
    public void BaseSlots_StillWorkWithoutUnlock()
    {
        var cat = new Cat();
        var inv = new SlotContainer(cat, 4, 4);
        var eq = new EquipmentModel(cat);
        inv.TryAdd(new ItemStack("Sword", 1));
        Assert.IsTrue(eq.Equip(inv, 0));
    }

    [Test]
    public void Roundtrip_PreservesItemsUpgradeAndMask()
    {
        var cat = new Cat();
        var inv = new SlotContainer(cat, 4, 4);
        var eq = new EquipmentModel(cat);
        eq.Unlock(EquipSlot.Title);
        eq.Unlock(EquipSlot.Aura);
        inv.TryAdd(new ItemStack("Halo", 1, 2, 3));
        Assert.IsTrue(eq.Equip(inv, 0));
        string[] arr = eq.ToExtraArray();
        Assert.AreEqual(4, arr.Length);
        Assert.AreEqual("", arr[0]);
        Assert.AreEqual("Halo|2|3", arr[1]);

        var back = new EquipmentModel(cat);
        Assert.AreEqual(0, back.LoadExtra(arr, eq.ExtraUnlockMask, new SlotContainer(cat, 4, 4)));
        Assert.AreEqual(3, back.ExtraUnlockMask);
        Assert.AreEqual("Halo", back[EquipSlot.Aura].Id);
        Assert.AreEqual(2, back[EquipSlot.Aura].Tier);
        Assert.AreEqual(3, back[EquipSlot.Aura].Upgrade);
    }

    [Test]
    public void OldSave_NullOrShort_AllLockedEmpty()
    {
        var cat = new Cat();
        var eq = new EquipmentModel(cat);
        Assert.AreEqual(0, eq.LoadExtra(null, 0, new SlotContainer(cat, 4, 4)));
        Assert.AreEqual(0, eq.ExtraUnlockMask);
        foreach (EquipSlot s in SlotRules.Extras)
        {
            Assert.IsTrue(eq[s].IsEmpty);
            Assert.IsFalse(eq.IsUnlocked(s));
        }
        eq.LoadExtra(new[] { "Crown" }, 1, null);
        Assert.AreEqual("Crown", eq[EquipSlot.Title].Id);
    }

    [Test]
    public void LockedSlotItem_ReturnsToInventory_OrStaysIfFull()
    {
        var cat = new Cat();
        var inv = new SlotContainer(cat, 4, 4);
        var eq = new EquipmentModel(cat);
        Assert.AreEqual(1, eq.LoadExtra(new[] { "", "Halo", "", "" }, 0, inv));
        Assert.IsTrue(eq[EquipSlot.Aura].IsEmpty);
        Assert.AreEqual(1, inv.Count("Halo"));

        var full = new SlotContainer(cat, 1, 1);
        full.TryAdd(new ItemStack("Sword", 1));
        Assert.AreEqual(0, eq.LoadExtra(new[] { "", "Halo", "", "" }, 0, full));
        Assert.AreEqual("Halo", eq[EquipSlot.Aura].Id);
    }

    [Test]
    public void ExtraSlot_CountsInEquippedDefs()
    {
        var cat = new Cat();
        var inv = new SlotContainer(cat, 4, 4);
        var eq = new EquipmentModel(cat);
        eq.Unlock(EquipSlot.Aura);
        inv.TryAdd(new ItemStack("Halo", 1));
        eq.Equip(inv, 0);
        var ids = new List<string>();
        foreach (ItemDef d in eq.EquippedDefs())
        {
            ids.Add(d.Id);
        }
        CollectionAssert.AreEqual(new[] { "Halo" }, ids);
    }

    [Test]
    public void AuraRing_PointsOnEllipse_PulseGrows()
    {
        var p = AuraRingRules.Points(8, 2f, 1f, 0f);
        Assert.AreEqual(8, p.Length);
        Assert.AreEqual(2f, p[0].x, 0.001f);
        Assert.AreEqual(1f, p[2].y, 0.001f);
        Assert.Greater(AuraRingRules.Points(8, 2f, 1f, 1f)[0].x, 2.1f);
    }
}
