using System.Collections.Generic;
using NUnit.Framework;

/// <summary>InventoryRefactorDesign 2.7 세이브 매핑 (Phase 2)</summary>
public class SaveMapperTests
{
    class FakeCatalog : IItemCatalog
    {
        readonly Dictionary<string, ItemDef> defs = new Dictionary<string, ItemDef>
        {
            { "Wood", new ItemDef { Id = "Wood", MaxStack = 10 } },
            { "Sword", new ItemDef { Id = "Sword", MaxStack = 1 } },
        };
        public ItemDef Get(string id) => id != null && defs.TryGetValue(id, out var d) ? d : null;
        public bool Exists(string id) => Get(id) != null;
    }

    static void Cell(string[,,] a, int p, int s, string id, string n, string t, string u)
    {
        a[p, s, 0] = id;
        a[p, s, 1] = n;
        a[p, s, 2] = t;
        a[p, s, 3] = u;
    }

    [Test]
    public void Roundtrip_PreservesSlotsTierAndUpgrade()
    {
        var c = new SlotContainer(new FakeCatalog(), 50, 50);
        c.PutAt(0, new ItemStack("Wood", 7));
        c.PutAt(26, new ItemStack("Sword", 1, 2, 5));
        string[,,] arr = SaveMapper.ToArray(c, 2, 25);
        Assert.AreEqual("Sword", arr[1, 1, 0]);
        Assert.AreEqual(SaveMapper.EmptyMark, arr[0, 1, 0]);
        SlotContainer back = SaveMapper.FromArray(arr, new FakeCatalog(), 2, 25);
        Assert.AreEqual(7, back[0].Count);
        Assert.AreEqual(2, back[26].Tier);
        Assert.AreEqual(5, back[26].Upgrade);
        Assert.IsTrue(back[1].IsEmpty);
    }

    [Test]
    public void Read_BadNumbers_AreSafe()
    {
        var a = new string[1, 4, 4];
        Cell(a, 0, 0, "Wood", "abc", "1", "1");   // 수량 파싱 실패 → 빈칸
        Cell(a, 0, 1, "Wood", "0", "0", "0");     // 수량 0 → 빈칸
        Cell(a, 0, 2, "Sword", "1", "x", null);   // tier/upgrade 실패 → 0
        Cell(a, 0, 3, "non", "1", "0", "0");
        Assert.IsTrue(SaveMapper.ReadCell(a, 0, 0).IsEmpty);
        Assert.IsTrue(SaveMapper.ReadCell(a, 0, 1).IsEmpty);
        ItemStack s = SaveMapper.ReadCell(a, 0, 2);
        Assert.AreEqual(1, s.Count);
        Assert.AreEqual(0, s.Tier);
        Assert.AreEqual(0, s.Upgrade);
        Assert.IsTrue(SaveMapper.ReadCell(a, 0, 3).IsEmpty);
    }

    [Test]
    public void UnknownId_IsPreserved()
    {
        var a = new string[1, 2, 4];
        Cell(a, 0, 1, "RemovedItem", "3", "0", "0");
        SlotContainer c = SaveMapper.FromArray(a, new FakeCatalog(), 1, 2);
        Assert.AreEqual("RemovedItem", c[1].Id);
        Assert.AreEqual("RemovedItem", SaveMapper.ToArray(c, 1, 2)[0, 1, 0]);
    }

    [Test]
    public void Read_OutOfRangeOrNull_IsEmpty()
    {
        Assert.IsTrue(SaveMapper.ReadCell(null, 0, 0).IsEmpty);
        Assert.IsTrue(SaveMapper.ReadCell(new string[1, 1, 4], 3, 0).IsEmpty);
    }
}
