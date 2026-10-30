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

    [Test]
    public void PeekAt_NewestFirst_AndVersionBumpsOnChange()
    {
        var m = new BuybackModel();
        int v0 = m.Version;
        m.Record(S("A", 1));
        m.Record(S("B", 2));
        Assert.AreEqual("B", m.PeekAt(0).Id);
        Assert.AreEqual("A", m.PeekAt(1).Id);
        int v1 = m.Version;
        Assert.Greater(v1, v0);
        m.Record(new BuybackModel.Sold { Id = "", Count = 1 });
        Assert.AreEqual(v1, m.Version); // 무시된 기록은 버전을 올리지 않는다
        m.Pop();
        Assert.Greater(m.Version, v1);
        Assert.IsNull(m.PeekAt(5).Id); // 범위 밖은 빈 값
        Assert.IsNull(m.PeekAt(-1).Id);
    }
}
