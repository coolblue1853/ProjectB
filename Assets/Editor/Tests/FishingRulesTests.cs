using NUnit.Framework;
using ProjectB.EditorTools;

public class FishingRulesTests
{
    private static readonly RodDef OldRod = FishingRules.Rod(0);

    [Test]
    public void Floor1_IdsExistInItemSheetWithSameRarity()
    {
        var items = ItemTable.Load();
        foreach (FishDef f in FishingRules.Floor1)
        {
            ItemRow row = null;
            foreach (ItemRow r in items.Rows)
            {
                if (r.Name == f.Id)
                {
                    row = r;
                    break;
                }
            }
            Assert.IsNotNull(row, "ItemSheet 에 없음: " + f.Id);
            Assert.AreEqual(f.Rarity.ToString(), row.Rarity, "등급 불일치: " + f.Id);
        }
    }

    [Test]
    public void EvaluateCast_Boundaries()
    {
        Assert.AreEqual(CastQuality.Short, FishingRules.EvaluateCast(0.29f));
        Assert.AreEqual(CastQuality.Normal, FishingRules.EvaluateCast(0.5f));
        Assert.AreEqual(CastQuality.Optimal, FishingRules.EvaluateCast(0.65f));
        Assert.AreEqual(CastQuality.Optimal, FishingRules.EvaluateCast(0.95f));
        Assert.AreEqual(CastQuality.Normal, FishingRules.EvaluateCast(1.2f));
    }

    [Test]
    public void WaitSeconds_OptimalAndRodShorten()
    {
        Assert.AreEqual(3f, FishingRules.WaitSeconds(0f, CastQuality.Normal, OldRod), 1e-4f);
        Assert.AreEqual(8f * 0.9f, FishingRules.WaitSeconds(0.999999f, CastQuality.Optimal, OldRod), 1e-3f);
        Assert.AreEqual(3f * 0.8f, FishingRules.WaitSeconds(0f, CastQuality.Normal, FishingRules.Rod(2)), 1e-4f);
        Assert.Greater(FishingRules.WaitSeconds(0.5f, CastQuality.Short, OldRod), FishingRules.WaitSeconds(0.5f, CastQuality.Normal, OldRod));
    }

    [Test]
    public void RollFish_BaseProbabilitiesMatchTable()
    {
        FishDef[] t = FishingRules.Floor1;
        // 누적 50 / 80 / 97 / 100
        Assert.AreEqual(0, FishingRules.RollFish(t, 0.49f, OldRod, CastQuality.Normal));
        Assert.AreEqual(1, FishingRules.RollFish(t, 0.51f, OldRod, CastQuality.Normal));
        Assert.AreEqual(2, FishingRules.RollFish(t, 0.96f, OldRod, CastQuality.Normal));
        Assert.AreEqual(3, FishingRules.RollFish(t, 0.975f, OldRod, CastQuality.Normal));
        Assert.AreEqual(3, FishingRules.RollFish(t, 1f, OldRod, CastQuality.Normal));
    }

    [Test]
    public void RollFish_BetterRodRaisesRareShare()
    {
        FishDef[] t = FishingRules.Floor1;
        float rareOld = RareShare(t, OldRod, CastQuality.Normal);
        float rareSky = RareShare(t, FishingRules.Rod(2), CastQuality.Normal);
        float rareOptimal = RareShare(t, OldRod, CastQuality.Optimal);
        Assert.AreEqual(0.20f, rareOld, 1e-4f);
        Assert.Greater(rareSky, rareOld);
        Assert.Greater(rareOptimal, rareOld);
    }

    [Test]
    public void SafeZone_ByRarityAndRod()
    {
        FishingRules.SafeZone(FishRarity.C, OldRod, out float min, out float max);
        Assert.AreEqual(25f, min);
        Assert.AreEqual(75f, max);
        FishingRules.SafeZone(FishRarity.U, FishingRules.Rod(1), out min, out max);
        Assert.AreEqual(35f, min);
        Assert.AreEqual(65f, max);
    }

    [Test]
    public void Tension_HoldOnlyRises_ThenSnaps()
    {
        var t = new FishingTension(FishRarity.C, OldRod);
        ReelOutcome o = ReelOutcome.Reeling;
        for (int i = 0; i < 200 && o == ReelOutcome.Reeling; i++)
        {
            o = t.Step(0.05f, true);
        }
        Assert.AreEqual(ReelOutcome.Snapped, o);
        Assert.GreaterOrEqual(t.Tension, FishingRules.SnapTension);
    }

    [Test]
    public void Tension_NeverHold_Escapes()
    {
        var t = new FishingTension(FishRarity.C, OldRod);
        ReelOutcome o = t.Step(10f, false);
        Assert.AreEqual(ReelOutcome.Escaped, o);
        Assert.AreEqual(0f, t.Progress);
    }

    [Test]
    public void Tension_KeepInSafeZone_Catches()
    {
        Assert.AreEqual(ReelOutcome.Caught, PlaySafe(FishRarity.C, out float elapsed));
        Assert.LessOrEqual(elapsed, FishingRules.ReelTimeLimit);
        Assert.AreEqual(ReelOutcome.Caught, PlaySafe(FishRarity.U, out _));
    }

    [Test]
    public void Tension_LargeFrameSameAsSmallSteps()
    {
        var a = new FishingTension(FishRarity.R, OldRod);
        var b = new FishingTension(FishRarity.R, OldRod);
        a.Step(1f, true);
        for (int i = 0; i < 20; i++)
        {
            b.Step(0.05f, true);
        }
        Assert.AreEqual(b.Tension, a.Tension, 1e-3f);
        Assert.AreEqual(b.Progress, a.Progress, 1e-3f);
    }

    [Test]
    public void Session_FullFlow_CatchesFish()
    {
        var s = new FishingSession(OldRod, FishingRules.Floor1, () => 0f);
        s.Tick(0.4f, true, false);
        s.Tick(0.4f, true, false);   // 0.8초 홀드 = 최적
        s.Tick(0.016f, false, false); // 놓아서 던짐
        Assert.AreEqual(FishingPhase.Waiting, s.Phase);
        Assert.AreEqual(CastQuality.Optimal, s.Cast);
        s.Tick(3f, false, false);      // 대기 3×0.9 = 2.7초
        Assert.AreEqual(FishingPhase.Bite, s.Phase);
        s.Tick(0.1f, true, true);
        Assert.AreEqual(FishingPhase.Reeling, s.Phase);
        Assert.AreEqual("Forest Carp", s.Fish.Id);
        for (int i = 0; i < 2000 && s.Phase == FishingPhase.Reeling; i++)
        {
            bool hold = s.Tension.Tension < (s.Tension.SafeMin + s.Tension.SafeMax) * 0.5f;
            s.Tick(0.02f, hold, false);
        }
        Assert.AreEqual(FishingResult.Caught, s.Result);
        Assert.AreEqual(1, s.Count);
    }

    [Test]
    public void Session_LateOrEarlyPress_Fails()
    {
        var late = new FishingSession(OldRod, FishingRules.Floor1, () => 0f);
        late.Tick(0.5f, false, false);
        late.Tick(4f, false, false);
        Assert.AreEqual(FishingPhase.Bite, late.Phase);
        late.Tick(0.7f, false, false);
        Assert.AreEqual(FishingResult.MissedBite, late.Result);

        var early = new FishingSession(OldRod, FishingRules.Floor1, () => 0f);
        early.Tick(0.5f, false, false);
        early.Tick(0.5f, true, true);
        Assert.AreEqual(FishingResult.ReeledEmpty, early.Result);

        var cancel = new FishingSession(OldRod, FishingRules.Floor1, () => 0f);
        cancel.Cancel();
        Assert.AreEqual(FishingResult.Cancelled, cancel.Result);
    }

    private static float RareShare(FishDef[] t, RodDef rod, CastQuality q)
    {
        float total = 0f;
        float rare = 0f;
        foreach (FishDef f in t)
        {
            float w = FishingRules.Weight(f, rod, q);
            total += w;
            if (f.Rarity != FishRarity.C)
            {
                rare += w;
            }
        }
        return rare / total;
    }

    /// <summary>안전 구간 가운데를 기준으로 누르고 떼는 단순 플레이</summary>
    private static ReelOutcome PlaySafe(FishRarity rarity, out float elapsed)
    {
        var t = new FishingTension(rarity, OldRod);
        float mid = (t.SafeMin + t.SafeMax) * 0.5f;
        ReelOutcome o = ReelOutcome.Reeling;
        for (int i = 0; i < 3000 && o == ReelOutcome.Reeling; i++)
        {
            o = t.Step(0.02f, t.Tension < mid);
        }
        elapsed = t.Elapsed;
        return o;
    }
}
