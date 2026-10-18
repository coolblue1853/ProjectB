using NUnit.Framework;

public class PickupLogModelTests
{
    [Test]
    public void SameItemWithinWindow_MergesCount()
    {
        var m = new PickupLogModel();
        m.Add("Wood", 1, "C", 0f);
        m.Add("Wood", 2, "C", 1.2f);
        Assert.AreEqual(1, m.Lines.Count);
        Assert.AreEqual(3, m.Lines[0].count);
        Assert.AreEqual(1.2f + PickupLogModel.Lifetime, m.Lines[0].expireAt, 1e-4f);
    }

    [Test]
    public void SameItemAfterWindow_NewLine()
    {
        var m = new PickupLogModel();
        m.Add("Wood", 1, "C", 0f);
        m.Add("Wood", 1, "C", 1.6f);
        Assert.AreEqual(2, m.Lines.Count);
    }

    [Test]
    public void MaxLines_DropsOldest()
    {
        var m = new PickupLogModel();
        for (int i = 0; i < 7; i++)
        {
            m.Add("Item" + i, 1, "C", i * 0.1f);
        }
        Assert.AreEqual(PickupLogModel.MaxLines, m.Lines.Count);
        Assert.AreEqual("Item2", m.Lines[0].itemName);
        Assert.AreEqual("Item6", m.Lines[m.Lines.Count - 1].itemName);
    }

    [Test]
    public void RareLivesLonger_AndFades()
    {
        var m = new PickupLogModel();
        m.Add("Wood", 1, "C", 0f);
        m.Add("Dagger", 1, "U", 0f);
        Assert.IsTrue(m.Lines[1].emphasis);
        Assert.AreEqual(1, m.Tick(3.0f));
        Assert.AreEqual("Dagger", m.Lines[0].itemName);
        Assert.AreEqual(0.5f, m.Lines[0].Alpha(4.75f), 1e-4f);
        Assert.AreEqual(1, m.Tick(5.0f));
        Assert.AreEqual(0, m.Lines.Count);
    }
}
