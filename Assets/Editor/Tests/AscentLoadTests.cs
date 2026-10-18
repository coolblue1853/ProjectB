using NUnit.Framework;

public class AscentLoadTests
{
    [Test]
    public void NoDeficit_NoPenalty()
    {
        Assert.AreEqual(0, AscentLoad.DeficitOf(10, 1));
        Assert.AreEqual(1f, AscentLoad.OutgoingMul(0), 1e-5f);
        Assert.AreEqual(1f, AscentLoad.IncomingMul(0), 1e-5f);
    }

    [Test]
    public void Deficit_ScalesDamage()
    {
        int d = AscentLoad.DeficitOf(40, 1); // 40 - 15
        Assert.AreEqual(25, d);
        Assert.AreEqual(0.5f, AscentLoad.OutgoingMul(d), 1e-5f);
        Assert.AreEqual(1.5f, AscentLoad.IncomingMul(d), 1e-5f);
    }

    [Test]
    public void Outgoing_FloorsAtTenPercent()
    {
        Assert.AreEqual(0.1f, AscentLoad.OutgoingMul(100), 1e-5f);
    }

    [Test]
    public void Rank_ClampedAndNamed()
    {
        Assert.AreEqual(105, AscentLoad.Resist(99));
        Assert.AreEqual("Whitelight", AscentLoad.RankName(7));
        Assert.AreEqual("Novice", AscentLoad.RankName(-3));
    }
}
