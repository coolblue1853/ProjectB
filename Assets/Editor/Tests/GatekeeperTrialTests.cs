using NUnit.Framework;

public class GatekeeperTrialTests
{
    [Test]
    public void Readiness_Boundaries()
    {
        Assert.AreEqual(0f, GatekeeperTrial.Readiness(0, 0, 0), 1e-5f);
        Assert.AreEqual(1f, GatekeeperTrial.Readiness(3, 3, 15), 1e-5f);
        Assert.AreEqual(1f, GatekeeperTrial.Readiness(9, 9, 99), 1e-5f);
        Assert.AreEqual(0f, GatekeeperTrial.Readiness(-1, -1, -1), 1e-5f);
        Assert.AreEqual(0.4f, GatekeeperTrial.Readiness(3, 0, 0), 1e-5f);
        Assert.AreEqual(0.3f, GatekeeperTrial.Readiness(0, 3, 0), 1e-5f);
        Assert.AreEqual(0.3f, GatekeeperTrial.Readiness(0, 0, 15), 1e-5f);
    }

    [Test]
    public void Multipliers_MatchSpec()
    {
        Assert.AreEqual(1.5f, GatekeeperTrial.HpMultiplier(0f), 1e-5f);
        Assert.AreEqual(1f, GatekeeperTrial.HpMultiplier(1f), 1e-5f);
        Assert.AreEqual(1.2f, GatekeeperTrial.DamageMultiplier(0f), 1e-5f);
        Assert.AreEqual(1f, GatekeeperTrial.DamageMultiplier(1f), 1e-5f);
        Assert.AreEqual(7500, UnityEngine.Mathf.RoundToInt(5000 * GatekeeperTrial.HpMultiplier(0f)));
    }

    [Test]
    public void KillTimeRatio_InTargetRange()
    {
        float r = GatekeeperTrial.KillTimeRatio(0f, 1f);
        Assert.GreaterOrEqual(r, 1.3f);
        Assert.LessOrEqual(r, 1.6f);
    }

    [Test]
    public void SetKinds_CountsThreePieceSetsOnly()
    {
        var d = new System.Collections.Generic.Dictionary<string, int> { { "A", 3 }, { "B", 2 }, { "C", 5 }, { "", 4 } };
        Assert.AreEqual(2, GatekeeperTrial.SetKinds(d));
        Assert.AreEqual(0, GatekeeperTrial.SetKinds(null));
    }

    [Test]
    public void IsGatekeeper_MatchesCloneName()
    {
        Assert.IsTrue(GatekeeperTrial.IsGatekeeper("Gatekeeper_BD(Clone)"));
        Assert.IsFalse(GatekeeperTrial.IsGatekeeper("Rabbit"));
    }
}
