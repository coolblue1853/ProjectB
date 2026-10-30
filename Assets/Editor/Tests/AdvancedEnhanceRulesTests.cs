using NUnit.Framework;

public class AdvancedEnhanceRulesTests
{
    // ── +10 이하 회귀: 기존 수치 그대로 ──
    [Test]
    public void BaseTable_Unchanged()
    {
        float[] rate = { 1f, 0.95f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.35f, 0.3f };
        int[] stones = { 1, 1, 2, 2, 3, 3, 4, 5, 6, 8 };
        int[] gold = { 50, 100, 150, 250, 400, 600, 900, 1300, 1800, 2500 };
        for (int lv = 0; lv < 10; lv++)
        {
            Assert.AreEqual(rate[lv], EnhanceRules.Rate(lv), 1e-5f, "rate " + lv);
            Assert.AreEqual(stones[lv], EnhanceRules.Stones(lv), "stone " + lv);
            Assert.AreEqual(gold[lv], EnhanceRules.Gold(lv), "gold " + lv);
        }
        Assert.IsFalse(EnhanceRules.CanEnhance(10));
        Assert.AreEqual(1.6f, EnhanceRules.WeaponMultiplier(10), 1e-5f);
        Assert.AreEqual(1.7f, EnhanceRules.GearMultiplier(10), 1e-5f);
    }

    // ── +11~+15 표 ──
    [Test]
    public void AdvancedTable()
    {
        int[] pct = { 65, 50, 35, 25, 15 };
        int[] stones = { 5, 6, 7, 8, 8 };
        int[] gold = { 1000, 1500, 2500, 4000, 6000 };
        for (int i = 0; i < 5; i++)
        {
            int lv = 10 + i;
            Assert.IsTrue(AdvancedEnhanceRules.IsAdvanced(lv));
            Assert.AreEqual(pct[i], AdvancedEnhanceRules.BasePct(lv));
            Assert.AreEqual(stones[i], AdvancedEnhanceRules.Stones(lv));
            Assert.AreEqual(gold[i], AdvancedEnhanceRules.Gold(lv));
        }
        Assert.IsFalse(AdvancedEnhanceRules.IsAdvanced(9));
        Assert.IsFalse(AdvancedEnhanceRules.IsAdvanced(15));
    }

    [Test]
    public void Multipliers_PlusFifteen()
    {
        Assert.AreEqual(1.64f, EnhanceRules.WeaponMultiplier(11), 1e-5f);
        Assert.AreEqual(1.9f, EnhanceRules.WeaponMultiplier(15), 1e-5f);
        Assert.AreEqual(1.95f, EnhanceRules.GearMultiplier(15), 1e-5f);
        Assert.AreEqual(1.9f, EnhanceRules.WeaponMultiplier(20), 1e-5f);
    }

    // ── 천장 보조 ──
    [Test]
    public void Pity_StepsAndCaps()
    {
        Assert.AreEqual(0, AdvancedEnhanceRules.PityPct(0));
        Assert.AreEqual(2, AdvancedEnhanceRules.PityPct(1));
        Assert.AreEqual(10, AdvancedEnhanceRules.PityPct(5));
        Assert.AreEqual(10, AdvancedEnhanceRules.PityPct(50));
        Assert.AreEqual(0.69f, AdvancedEnhanceRules.FinalRate(10, 2, 0), 1e-5f); // 65 + 4
        Assert.AreEqual(0.75f, AdvancedEnhanceRules.FinalRate(10, 99, 0), 1e-5f); // 65 + 10
    }

    [Test]
    public void FinalRate_CapsAtNinety()
    {
        Assert.AreEqual(0.87f, AdvancedEnhanceRules.FinalRate(10, 5, 9), 1e-5f); // 65 + 10 + 12
        Assert.LessOrEqual(AdvancedEnhanceRules.FinalRate(10, 99, 9), 0.9f);
    }

    // ── 단조 ──
    [Test]
    public void StrikeScore_Bands()
    {
        Assert.AreEqual(3, AdvancedEnhanceRules.StrikeScore(50f, 50f));
        Assert.AreEqual(3, AdvancedEnhanceRules.StrikeScore(56f, 50f));
        Assert.AreEqual(2, AdvancedEnhanceRules.StrikeScore(56.1f, 50f));
        Assert.AreEqual(2, AdvancedEnhanceRules.StrikeScore(36f, 50f));
        Assert.AreEqual(1, AdvancedEnhanceRules.StrikeScore(36f - 0.1f, 50f));
        Assert.AreEqual(1, AdvancedEnhanceRules.StrikeScore(75f, 50f));
        Assert.AreEqual(0, AdvancedEnhanceRules.StrikeScore(75.1f, 50f));
    }

    [Test]
    public void ScoreToBonus()
    {
        int[] expect = { 0, 0, 0, 4, 4, 8, 8, 10, 10, 12 };
        for (int s = 0; s <= 9; s++)
        {
            Assert.AreEqual(expect[s], AdvancedEnhanceRules.ForgeBonusPct(s), "score " + s);
        }
    }

    [Test]
    public void Refund_HalfRoundedUpOnlyAtNine()
    {
        Assert.AreEqual(3, AdvancedEnhanceRules.RefundStones(5, 9));
        Assert.AreEqual(4, AdvancedEnhanceRules.RefundStones(8, 9));
        Assert.AreEqual(4, AdvancedEnhanceRules.RefundStones(7, 9));
        Assert.AreEqual(0, AdvancedEnhanceRules.RefundStones(8, 8));
    }

    [Test]
    public void Session_ThreePerfectsScoreNine()
    {
        var s = new ForgeSession(() => 0.5f); // 목표 50
        s.Tick(ForgeSession.StartDelay + 0.01f);
        Assert.IsFalse(s.Waiting);
        int hits = 0;
        float guard = 0f;
        while (!s.Done && guard < 60f)
        {
            if (!s.Waiting && System.Math.Abs(s.Cursor - s.Target) < 2f)
            {
                Assert.AreEqual(3, s.Hit());
                hits++;
            }
            s.Tick(0.01f);
            guard += 0.01f;
        }
        Assert.IsTrue(s.Done);
        Assert.AreEqual(3, hits);
        Assert.AreEqual(9, s.Total);
    }

    [Test]
    public void Session_IgnoresHitWhileWaiting_AndTimesOutAsMiss()
    {
        var s = new ForgeSession(() => 0.5f);
        Assert.AreEqual(-1, s.Hit()); // 시작 지연 중
        s.Tick(ForgeSession.StartDelay + 0.01f);
        for (int i = 0; i < 3; i++)
        {
            s.Tick(ForgeSession.StrikeTimeLimit + 0.01f); // 못 침 → 빗나감
            s.Tick(ForgeSession.StrikeGap + 0.01f);
        }
        Assert.IsTrue(s.Done);
        Assert.AreEqual(0, s.Total);
    }

    // ── 게이트 ──
    [Test]
    public void Gate_BitsPerTarget()
    {
        Assert.IsFalse(ForgeState.IsUnlocked(10, 0));
        Assert.IsTrue(ForgeState.IsUnlocked(10, 1));
        Assert.IsTrue(ForgeState.IsUnlocked(11, 1));
        Assert.IsFalse(ForgeState.IsUnlocked(12, 1));
        Assert.IsTrue(ForgeState.IsUnlocked(12, 2));
        Assert.IsFalse(ForgeState.IsUnlocked(13, 2));
        Assert.IsTrue(ForgeState.IsUnlocked(13, 4));
        Assert.IsFalse(ForgeState.IsUnlocked(14, 7));
        Assert.IsTrue(ForgeState.IsUnlocked(14, 8));
        Assert.IsFalse(ForgeState.IsUnlocked(15, 15));
    }
}
