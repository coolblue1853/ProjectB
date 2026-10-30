using System;
using System.Collections.Generic;
using NUnit.Framework;

public class TrackingRulesTests
{
    private static Func<float> Seeded(int seed)
    {
        var r = new Random(seed);
        return () => (float)r.NextDouble();
    }

    [Test]
    public void Stage_HasOneCorrectFreshestTargetTrace()
    {
        Func<float> rnd = Seeded(1);
        for (int n = 0; n < 500; n++)
        {
            TraceOption[] o = TrackingRules.MakeStage("Deep claw marks", rnd, out int correct);
            Assert.AreEqual(TrackingRules.Options, o.Length);
            Assert.AreEqual("Deep claw marks", o[correct].Trace);
            Assert.AreNotEqual(Freshness.Old, o[correct].Fresh);
            int sameTrace = 0;
            for (int i = 0; i < o.Length; i++)
            {
                if (i == correct)
                {
                    continue;
                }
                if (o[i].Trace == "Deep claw marks")
                {
                    sameTrace++;
                    Assert.Greater((int)o[i].Fresh, (int)o[correct].Fresh, "미끼 목표 흔적은 정답보다 오래돼야 한다");
                }
                else
                {
                    Assert.AreEqual(Freshness.Fresh, o[i].Fresh);
                }
            }
            Assert.AreEqual(1, sameTrace);
        }
    }

    [Test]
    public void Stage_CorrectPositionCoversAllDirections()
    {
        Func<float> rnd = Seeded(2);
        var seen = new HashSet<int>();
        for (int n = 0; n < 200; n++)
        {
            TrackingRules.MakeStage("Gel trail", rnd, out int correct);
            seen.Add(correct);
        }
        Assert.AreEqual(3, seen.Count);
    }

    [Test]
    public void TraceFor_StripsCloneAndSuffix()
    {
        Assert.AreEqual("Deep claw marks", TrackingRules.TraceFor("ForestBear_BD(Clone)"));
        Assert.AreEqual("Boot prints and torch ash", TrackingRules.TraceFor("BanditArcher_BD"));
        Assert.AreEqual(TrackingRules.DefaultTrace, TrackingRules.TraceFor("Nobody_BD"));
        Assert.AreNotEqual("Deep claw marks", TrackingRules.PickOtherTrace("Deep claw marks", 0.37f));
    }

    [Test]
    public void Trap_CursorPingPongsAndJudges()
    {
        Assert.AreEqual(0f, TrackingRules.TrapCursor(0f), 0.001f);
        Assert.AreEqual(60f, TrackingRules.TrapCursor(0.5f), 0.001f);
        Assert.AreEqual(80f, TrackingRules.TrapCursor(1f), 0.001f); // 120 → 되돌아 80
        Assert.AreEqual(TrapQuality.Perfect, TrackingRules.JudgeTrap(56f));
        Assert.AreEqual(TrapQuality.Good, TrackingRules.JudgeTrap(32f));
        Assert.AreEqual(TrapQuality.Normal, TrackingRules.JudgeTrap(31f));
        Assert.AreEqual(1, TrackingRules.BonusCount(TrapQuality.Normal));
        Assert.AreEqual(3, TrackingRules.BonusCount(TrapQuality.Perfect));
        Assert.AreEqual(0f, TrackingRules.StunSeconds(TrapQuality.Normal));
        Assert.AreEqual(2f, TrackingRules.StunSeconds(TrapQuality.Good));
        Assert.AreEqual(3f, TrackingRules.StunSeconds(TrapQuality.Perfect));
    }

    [Test]
    public void BonusLoot_MaterialCountAndWeaponRoll()
    {
        var drops = new List<DropRow>
        {
            new DropRow("Bear Pelt", 0.35f, "Misc"), new DropRow("BearClaw", 0.02f, "Equip"), new DropRow("Food", 0.1f, "Consum"),
            new DropRow("GroveCap", 0.003f, "Equip"), new DropRow("Potion", 0.04f, "Consum"),
        };
        Assert.AreEqual("Bear Pelt", TrackingRules.LineageMaterial(drops));
        Assert.AreEqual("BearClaw", TrackingRules.LineageWeapon(drops));
        List<string> loot = TrackingRules.BonusLoot(drops, TrapQuality.Good, 0.5f);
        CollectionAssert.AreEqual(new[] { "Bear Pelt", "Bear Pelt" }, loot);
        loot = TrackingRules.BonusLoot(drops, TrapQuality.Perfect, 0.005f);
        CollectionAssert.AreEqual(new[] { "Bear Pelt", "Bear Pelt", "Bear Pelt", "BearClaw" }, loot);
    }

    [Test]
    public void Session_CorrectChoicesReachTrapThenSuccess()
    {
        var s = new TrackingSession("Frost marks", Seeded(3));
        for (int i = 0; i < TrackingRules.Stages; i++)
        {
            Assert.AreEqual(TrackPhase.Trail, s.Phase);
            Assert.IsTrue(s.Choose(s.Correct));
        }
        Assert.AreEqual(TrackPhase.Trap, s.Phase);
        s.Tick(50f / TrackingRules.TrapSpeed); // 커서 50 = 완벽
        s.SetTrap();
        Assert.AreEqual(TrackResult.Success, s.Result);
        Assert.AreEqual(TrapQuality.Perfect, s.Quality);
    }

    [Test]
    public void Session_WrongChoiceCrossesAndThirdFails()
    {
        var s = new TrackingSession("Gel trail", Seeded(4));
        int wrong = (s.Correct + 1) % 3;
        Assert.IsFalse(s.Choose(wrong));
        Assert.IsTrue(s.IsCrossed(wrong));
        Assert.IsFalse(s.Choose(wrong)); // 지운 방향은 다시 세지 않는다
        Assert.AreEqual(1, s.Mistakes);
        Assert.IsTrue(s.Choose(s.Correct));
        Assert.IsFalse(s.IsCrossed(wrong), "다음 단계는 새 판");
        Assert.IsFalse(s.Choose((s.Correct + 1) % 3));
        Assert.IsFalse(s.Choose((s.Correct + 2) % 3));
        Assert.AreEqual(TrackResult.TooManyMistakes, s.Result);
        Assert.IsTrue(s.Done);
    }

    [Test]
    public void Session_TimeoutAndTrapAutoNormal()
    {
        var s = new TrackingSession("Gel trail", Seeded(5));
        s.Tick(TrackingRules.TimeLimit + 0.1f);
        Assert.AreEqual(TrackResult.TimedOut, s.Result);

        s = new TrackingSession("Gel trail", Seeded(6));
        for (int i = 0; i < TrackingRules.Stages; i++)
        {
            s.Choose(s.Correct);
        }
        s.Tick(TrackingRules.TimeLimit + 1f); // 덫 단계에서는 판별 시간 제한을 보지 않는다
        Assert.AreEqual(TrackResult.Success, s.Result);
        Assert.AreEqual(TrapQuality.Normal, s.Quality);
    }

    [Test]
    public void Session_CancelOnlyBeforeDone()
    {
        var s = new TrackingSession("Gel trail", Seeded(7));
        s.Cancel();
        Assert.AreEqual(TrackResult.Cancelled, s.Result);
        s.Tick(1f);
        Assert.AreEqual(1f, s.ResultElapsed, 0.001f);
    }
}
