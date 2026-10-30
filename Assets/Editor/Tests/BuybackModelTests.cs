using NUnit.Framework;

public class BuybackModelTests
{
    private static BuybackModel.Sold S(string id, int price)
    {
        return new BuybackModel.Sold { Id = id, Count = 1, Tier = 0, Upgrade = 2, Price = price };
    }

    [Test]
    public void LastIn_FirstOut()
    {
        var m = new BuybackModel();
        m.Record(S("A", 10));
        m.Record(S("B", 20));
        Assert.IsTrue(m.TryPeek(out var s));
        Assert.AreEqual("B", s.Id);
        m.Pop();
        m.TryPeek(out s);
        Assert.AreEqual("A", s.Id);
        Assert.AreEqual(2, s.Upgrade);
    }

    [Test]
    public void Capacity_DropsOldest_AndIgnoresEmpty()
    {
        var m = new BuybackModel();
        for (int i = 0; i < BuybackModel.Capacity + 3; i++)
        {
            m.Record(S("I" + i, i));
        }
        m.Record(new BuybackModel.Sold { Id = "", Count = 1 });
        m.Record(new BuybackModel.Sold { Id = "X", Count = 0 });
        Assert.AreEqual(BuybackModel.Capacity, m.Count);
        for (int i = 0; i < BuybackModel.Capacity - 1; i++)
        {
            m.Pop();
        }
        m.TryPeek(out var s);
        Assert.AreEqual("I3", s.Id);
        m.Pop();
        Assert.IsFalse(m.TryPeek(out s));
    }
}
