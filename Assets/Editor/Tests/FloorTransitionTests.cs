using NUnit.Framework;

public class FloorTransitionTests
{
    [Test]
    public void SealBonus_ThreePerSeal_Capped()
    {
        Assert.AreEqual(0, ExplorerSeal.BonusOf(0));
        Assert.AreEqual(9, ExplorerSeal.BonusOf(3));
        Assert.AreEqual(36, ExplorerSeal.BonusOf(99)); // 12개 상한
        Assert.AreEqual(0, ExplorerSeal.BonusOf(-2));
    }

    [Test]
    public void Deficit_WithSeals_RelievesFloor2Zones()
    {
        // 칭호 2(저항 30) + 인장 3개(+9): 구역 ③ 부하 45 → 부족 6 (인장 없이는 15)
        Assert.AreEqual(15, AscentLoad.DeficitOf(45, 2));
        Assert.AreEqual(6, AscentLoad.DeficitOf(45, 2, ExplorerSeal.BonusOf(3)));
        Assert.AreEqual(0, AscentLoad.DeficitOf(35, 2, ExplorerSeal.BonusOf(3)));
    }

    [Test]
    public void Deficit_GearResistAdds_NegativeIgnored()
    {
        Assert.AreEqual(10, AscentLoad.DeficitOf(40, 2, 0));
        Assert.AreEqual(5, AscentLoad.DeficitOf(40, 2, 5));
        Assert.AreEqual(10, AscentLoad.DeficitOf(40, 2, -9));
    }

    [Test]
    public void RecallBlockedByBoss_RangeBoundary()
    {
        Assert.IsFalse(RecallRules.BlockedByBoss(-1f));
        Assert.IsTrue(RecallRules.BlockedByBoss(0f));
        Assert.IsTrue(RecallRules.BlockedByBoss(RecallRules.BossBlockRange));
        Assert.IsFalse(RecallRules.BlockedByBoss(RecallRules.BossBlockRange + 0.1f));
    }
}
