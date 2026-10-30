using System.Collections.Generic;
using NUnit.Framework;

public class CraftPinModelTests
{
    [Test]
    public void Set_MergesDuplicateMaterials()
    {
        var m = new CraftPinModel();
        m.Set("Ring", new[] { "Wood", "Rock", "Wood" }, new[] { 2, 1, 3 });
        Assert.AreEqual("Ring", m.Result);
        Assert.AreEqual(2, m.Count);
        Assert.AreEqual("Wood", m.IdAt(0));
        Assert.AreEqual(5, m.NeedAt(0));
    }

    [Test]
    public void Set_NoMaterials_Empty()
    {
        var m = new CraftPinModel();
        m.Set("Ring", new string[0], new int[0]);
        Assert.IsTrue(m.IsEmpty);
        m.Set("", new[] { "Wood" }, new[] { 1 });
        Assert.IsTrue(m.IsEmpty);
    }

    [Test]
    public void IsReady_ComparesEachMaterial()
    {
        var m = new CraftPinModel();
        m.Set("Ring", new[] { "Wood", "Rock" }, new[] { 2, 1 });
        var have = new Dictionary<string, int> { { "Wood", 2 }, { "Rock", 0 } };
        Assert.IsFalse(m.IsReady(id => have.TryGetValue(id, out int n) ? n : 0));
        have["Rock"] = 1;
        Assert.IsTrue(m.IsReady(id => have.TryGetValue(id, out int n) ? n : 0));
    }

    [Test]
    public void Serialize_RoundTrip_AndToleratesGarbage()
    {
        var m = new CraftPinModel();
        m.Set("Chief's War Banner", new[] { "Fresh Green Leaves", "T0_Ore" }, new[] { 3, 2 });
        var back = CraftPinModel.Parse(m.Serialize());
        Assert.AreEqual("Chief's War Banner", back.Result);
        Assert.AreEqual("Fresh Green Leaves", back.IdAt(0));
        Assert.AreEqual(2, back.NeedAt(1));
        Assert.IsTrue(CraftPinModel.Parse("").IsEmpty);
        Assert.IsTrue(CraftPinModel.Parse("Ring|broken").IsEmpty);
        Assert.AreEqual(1, CraftPinModel.Parse("Ring|broken|Wood*4").Count);
    }
}
