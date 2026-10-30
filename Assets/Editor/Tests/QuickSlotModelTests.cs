using NUnit.Framework;

public class QuickSlotModelTests
{
    [Test]
    public void Assign_SameIdMovesFromOtherSlot()
    {
        var m = new QuickSlotModel();
        m.Assign(0, "Potion");
        m.Assign(2, "Potion");
        Assert.IsNull(m[0]);
        Assert.AreEqual("Potion", m[2]);
    }

    [Test]
    public void Assign_EmptyClearsAndOutOfRangeIgnored()
    {
        var m = new QuickSlotModel();
        m.Assign(1, "Food");
        Assert.IsTrue(m.Assign(1, ""));
        Assert.IsNull(m[1]);
        Assert.IsFalse(m.Assign(QuickSlotModel.SlotCount, "Food"));
        Assert.IsFalse(m.Assign(-1, "Food"));
    }

    [Test]
    public void Assign_SameValue_NoChangeEvent()
    {
        var m = new QuickSlotModel();
        int changed = 0;
        m.Changed += () => changed++;
        m.Assign(0, "Potion");
        m.Assign(0, "Potion");
        Assert.AreEqual(1, changed);
    }

    [Test]
    public void SaveRoundTrip_EmptyAsBlankString()
    {
        var m = new QuickSlotModel();
        m.Assign(0, "Potion");
        m.Assign(3, "Torch");
        string[] saved = m.ToSaveArray();
        CollectionAssert.AreEqual(new[] { "Potion", "", "", "Torch" }, saved);

        var loaded = new QuickSlotModel();
        loaded.LoadFrom(saved);
        Assert.AreEqual("Potion", loaded[0]);
        Assert.IsNull(loaded[1]);
        Assert.AreEqual("Torch", loaded[3]);
    }

    [Test]
    public void LoadFrom_NullShortAndDuplicates_Tolerated()
    {
        var m = new QuickSlotModel();
        m.LoadFrom(null);
        Assert.IsNull(m[0]);
        m.LoadFrom(new[] { "Potion", "Potion" });
        Assert.AreEqual("Potion", m[0]);
        Assert.IsNull(m[1]);
        Assert.IsNull(m[3]);
    }

    [Test]
    public void Cooldown_GroupFromEffect()
    {
        Assert.AreEqual(ConsumeCooldown.PotionGroup, ConsumeCooldown.GroupOf("hp +"));
        Assert.AreEqual(ConsumeCooldown.MiscGroup, ConsumeCooldown.GroupOf("fullness +/def +"));
        Assert.AreEqual(ConsumeCooldown.PotionGroup, ConsumeCooldown.GroupOf("fullness +/hp +"));
        Assert.AreEqual(ConsumeCooldown.MiscGroup, ConsumeCooldown.GroupOf(""));
        Assert.AreEqual(ConsumeCooldown.MiscGroup, ConsumeCooldown.GroupOf(null));
    }

    [Test]
    public void Cooldown_SharedPerGroup()
    {
        var cd = new ConsumeCooldown();
        Assert.IsTrue(cd.IsReady(ConsumeCooldown.PotionGroup, 10f));
        cd.Start(ConsumeCooldown.PotionGroup, 10f);
        Assert.IsFalse(cd.IsReady(ConsumeCooldown.PotionGroup, 10f + ConsumeCooldown.PotionSeconds - 0.01f));
        Assert.IsTrue(cd.IsReady(ConsumeCooldown.MiscGroup, 10.5f)); // 다른 그룹은 영향 없음
        Assert.IsTrue(cd.IsReady(ConsumeCooldown.PotionGroup, 10f + ConsumeCooldown.PotionSeconds));
        Assert.AreEqual(1f, cd.Fraction(ConsumeCooldown.PotionGroup, 10f), 1e-4f);
    }
}
