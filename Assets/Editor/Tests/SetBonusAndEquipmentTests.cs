using System.Collections.Generic;
using NUnit.Framework;

/// <summary>InventoryRefactorDesign Phase 1 — SetBonusEvaluatorTests · EquipmentModelTests</summary>
public class SetBonusAndEquipmentTests
{
    class FakeCatalog : IItemCatalog
    {
        public readonly Dictionary<string, ItemDef> Defs = new Dictionary<string, ItemDef>();
        public FakeCatalog()
        {
            foreach (var area in new[] { "Head", "Chest", "Leg", "Hand", "Shoes", "Weapon" })
            {
                Defs["Rabbit" + area] = new ItemDef { Id = "Rabbit" + area, Type = "Equip", EquipArea = area, SetKey = "RabbitArmor", MaxStack = 1 };
            }
            Defs["Necklace1"] = new ItemDef { Id = "Necklace1", Type = "Equip", EquipArea = "Necklace", MaxStack = 1 };
            Defs["Rat1"] = new ItemDef { Id = "Rat1", Type = "Equip", EquipArea = "Head", SetKey = "RatArmor", MaxStack = 1 };
            Defs["Plain"] = new ItemDef { Id = "Plain", Type = "Equip", EquipArea = "Head", SetKey = "", MaxStack = 1 };
            Defs["Wood"] = new ItemDef { Id = "Wood", Type = "Misc", MaxStack = 99 };
        }
        public ItemDef Get(string id) => id != null && Defs.TryGetValue(id, out var d) ? d : null;
        public bool Exists(string id) => Get(id) != null;
    }

    static SetTierTable Table()
    {
        var t = new SetTierTable();
        t.Define("RabbitArmor", 2, "(+)5 Def");
        t.Define("RabbitArmor", 5, "(+)20 Def");
        return t;
    }

    static Dictionary<string, int> Counts(params (string, int)[] kv)
    {
        var d = new Dictionary<string, int>();
        foreach (var (k, v) in kv) d[k] = v;
        return d;
    }

    [Test] public void OnePiece_Inactive() => CollectionAssert.IsEmpty(SetBonusEvaluator.ActiveTiers("RabbitArmor", 1, Table()));
    [Test] public void TwoPieces_TierTwo() => CollectionAssert.AreEqual(new[] { 2 }, SetBonusEvaluator.ActiveTiers("RabbitArmor", 2, Table()));
    [Test] public void FivePieces_Cumulative() => CollectionAssert.AreEqual(new[] { 2, 5 }, SetBonusEvaluator.ActiveTiers("RabbitArmor", 5, Table()));
    [Test] public void SixPieces_UpToFive() => CollectionAssert.AreEqual(new[] { 2, 5 }, SetBonusEvaluator.ActiveTiers("RabbitArmor", 6, Table()));

    [Test]
    public void FiveToFour_RemovesOnlyFive()
    {
        var d = SetBonusEvaluator.Diff(Counts(("RabbitArmor", 5)), Counts(("RabbitArmor", 4)), Table());
        CollectionAssert.IsEmpty(d.Apply);
        CollectionAssert.AreEqual(new[] { ("RabbitArmor", 5) }, d.Remove);
    }

    [Test]
    public void EmptySetKey_Ignored()
    {
        var cat = new FakeCatalog();
        var counts = SetBonusEvaluator.CountPieces(new[] { cat.Get("Plain"), cat.Get("RabbitHead") });
        Assert.IsFalse(counts.ContainsKey(""));
        Assert.AreEqual(1, counts["RabbitArmor"]);
    }

    [Test]
    public void UndefinedSet_ProgressOnly()
    {
        var cat = new FakeCatalog();
        var counts = SetBonusEvaluator.CountPieces(new[] { cat.Get("Rat1") });
        Assert.AreEqual(1, counts["RatArmor"]);
        CollectionAssert.IsEmpty(SetBonusEvaluator.ActiveTiers("RatArmor", 5, Table()));
    }

    [Test]
    public void Diff_ApplyAndRemoveLists()
    {
        var d = SetBonusEvaluator.Diff(Counts(("RabbitArmor", 1)), Counts(("RabbitArmor", 5)), Table());
        CollectionAssert.AreEqual(new[] { ("RabbitArmor", 2), ("RabbitArmor", 5) }, d.Apply);
        CollectionAssert.IsEmpty(d.Remove);
    }

    [Test]
    public void SameState_EmptyDiff()
    {
        Assert.IsTrue(SetBonusEvaluator.Diff(Counts(("RabbitArmor", 5)), Counts(("RabbitArmor", 5)), Table()).IsEmpty);
    }

    [Test]
    public void Equip_EmptySlot()
    {
        var cat = new FakeCatalog();
        var inv = new SlotContainer(cat, 4, 4);
        var eq = new EquipmentModel(cat);
        inv.TryAdd(new ItemStack("RabbitHead", 1, 0, 3));
        Assert.IsTrue(eq.Equip(inv, 0));
        Assert.AreEqual(3, eq[EquipSlot.Head].Upgrade);
        Assert.IsTrue(inv[0].IsEmpty);
    }

    [Test]
    public void Equip_Replace_ReturnsOldToSameSlot()
    {
        var cat = new FakeCatalog();
        var inv = new SlotContainer(cat, 4, 4);
        var eq = new EquipmentModel(cat);
        inv.TryAdd(new ItemStack("RabbitHead", 1));
        eq.Equip(inv, 0);
        inv.TryAdd(new ItemStack("Rat1", 1));
        Assert.IsTrue(eq.Equip(inv, 0));
        Assert.AreEqual("Rat1", eq[EquipSlot.Head].Id);
        Assert.AreEqual("RabbitHead", inv[0].Id);
    }

    [Test]
    public void Equip_NonEquipment_Rejected()
    {
        var cat = new FakeCatalog();
        var inv = new SlotContainer(cat, 4, 4);
        inv.TryAdd(new ItemStack("Wood", 3));
        Assert.IsFalse(new EquipmentModel(cat).Equip(inv, 0));
        Assert.AreEqual(3, inv[0].Count);
    }

    [Test]
    public void Necklace_MapsToSlotThree()
    {
        Assert.IsTrue(EquipmentModel.TryGetSlot(new FakeCatalog().Get("Necklace1"), out EquipSlot s));
        Assert.AreEqual(3, (int)s);
    }

    [Test]
    public void Unequip_InventoryFull_Rejected()
    {
        var cat = new FakeCatalog();
        var inv = new SlotContainer(cat, 1, 1);
        var eq = new EquipmentModel(cat);
        inv.TryAdd(new ItemStack("RabbitHead", 1));
        eq.Equip(inv, 0);
        inv.TryAdd(new ItemStack("Wood", 1));
        Assert.IsFalse(eq.Unequip(EquipSlot.Head, inv));
        Assert.AreEqual("RabbitHead", eq[EquipSlot.Head].Id);
    }
}
