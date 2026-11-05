using NUnit.Framework;

public class Floor2MobBehaviorRulesTests
{
    const Floor2MobBehaviorRules.StatueState D = Floor2MobBehaviorRules.StatueState.Dormant;
    const Floor2MobBehaviorRules.StatueState A = Floor2MobBehaviorRules.StatueState.Awake;

    [Test]
    public void Statue_WakeBoundary()
    {
        float w = Floor2MobBehaviorRules.WakeRange;
        Assert.AreEqual(D, Floor2MobBehaviorRules.StatueNext(D, w + 0.01f, false));
        Assert.AreEqual(A, Floor2MobBehaviorRules.StatueNext(D, w, false));
        Assert.AreEqual(A, Floor2MobBehaviorRules.StatueNext(D, w - 0.01f, false));
    }

    [Test]
    public void Statue_HitWakes_AwakeStaysAwake()
    {
        Assert.AreEqual(A, Floor2MobBehaviorRules.StatueNext(D, 99f, true));
        Assert.AreEqual(A, Floor2MobBehaviorRules.StatueNext(A, 99f, false));
    }

    [Test]
    public void Stance_CycleBoundary()
    {
        float c = Floor2MobBehaviorRules.StanceCycle;
        Assert.IsFalse(Floor2MobBehaviorRules.ShouldStartStance(true, false, c - 0.01f));
        Assert.IsTrue(Floor2MobBehaviorRules.ShouldStartStance(true, false, c));
        Assert.IsFalse(Floor2MobBehaviorRules.ShouldStartStance(true, true, c + 5f)); // 자세 중
        Assert.IsFalse(Floor2MobBehaviorRules.ShouldStartStance(false, false, c + 5f)); // 아직 석상
        Assert.Less(Floor2MobBehaviorRules.StanceSeconds, c);
    }

    [Test]
    public void Vine_RangeAndCooldown()
    {
        float r = Floor2MobBehaviorRules.VineRange;
        Assert.IsTrue(Floor2MobBehaviorRules.CanVineBind(r, 0f));
        Assert.IsFalse(Floor2MobBehaviorRules.CanVineBind(r + 0.01f, 0f));
        Assert.IsFalse(Floor2MobBehaviorRules.CanVineBind(r, 0.01f));
    }

    [Test]
    public void Ranged_RetreatShootApproach()
    {
        float rt = Floor2MobBehaviorRules.RetreatRange, sh = Floor2MobBehaviorRules.ShootRange;
        Assert.AreEqual(Floor2MobBehaviorRules.RangedAction.Retreat, Floor2MobBehaviorRules.RangedNext(rt - 0.01f, 0f));
        Assert.AreEqual(Floor2MobBehaviorRules.RangedAction.Shoot, Floor2MobBehaviorRules.RangedNext(rt, 0f)); // 후퇴 거리 경계는 후퇴 아님
        Assert.AreEqual(Floor2MobBehaviorRules.RangedAction.Shoot, Floor2MobBehaviorRules.RangedNext(sh, 0f));
        Assert.AreEqual(Floor2MobBehaviorRules.RangedAction.Approach, Floor2MobBehaviorRules.RangedNext(sh + 0.01f, 0f));
        Assert.AreEqual(Floor2MobBehaviorRules.RangedAction.Hold, Floor2MobBehaviorRules.RangedNext(6f, 0.5f));
        Assert.AreEqual(Floor2MobBehaviorRules.RangedAction.Retreat, Floor2MobBehaviorRules.RangedNext(1f, 0.5f)); // 쿨 중에도 후퇴
    }
}
