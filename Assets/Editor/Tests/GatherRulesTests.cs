using NUnit.Framework;
using ProjectB.EditorTools;

public class GatherRulesTests
{
    [Test]
    public void Tables_IdsExistInItemSheetWithSameRarity()
    {
        var items = ItemTable.Load();
        foreach (GatherDef[] table in new[] { GatherRules.SpecimenFloor1, GatherRules.OreFloor1 })
        {
            foreach (GatherDef g in table)
            {
                ItemRow row = null;
                foreach (ItemRow r in items.Rows)
                {
                    if (r.Name == g.Id)
                    {
                        row = r;
                        break;
                    }
                }
                Assert.IsNotNull(row, "ItemSheet 에 없음: " + g.Id);
                Assert.AreEqual(g.Rarity.ToString(), row.Rarity, "등급 불일치: " + g.Id);
            }
        }
    }

    [Test]
    public void CursorAt_PingPongs()
    {
        Assert.AreEqual(0f, GatherRules.CursorAt(0f), 0.001f);
        Assert.AreEqual(85f, GatherRules.CursorAt(1f), 0.001f);
        float turn = GatherRules.CursorMax / GatherRules.CursorSpeed;
        Assert.AreEqual(100f, GatherRules.CursorAt(turn), 0.001f);
        Assert.AreEqual(50f, GatherRules.CursorAt(turn + 50f / GatherRules.CursorSpeed), 0.001f);
        Assert.AreEqual(0f, GatherRules.CursorAt(2f * turn), 0.01f);
    }

    [Test]
    public void Judge_Boundaries()
    {
        Assert.AreEqual(SpecimenJudge.Perfect, GatherRules.Judge(55f, 50f));
        Assert.AreEqual(SpecimenJudge.Good, GatherRules.Judge(65f, 50f));
        Assert.AreEqual(SpecimenJudge.Good, GatherRules.Judge(35.5f, 50f));
        Assert.AreEqual(SpecimenJudge.Miss, GatherRules.Judge(65.5f, 50f));
    }

    [Test]
    public void IsCritTime_GlowWindow()
    {
        Assert.IsFalse(GatherRules.IsCritTime(0.5f));
        Assert.IsTrue(GatherRules.IsCritTime(0.6f));
        Assert.IsTrue(GatherRules.IsCritTime(0.75f));
        Assert.IsTrue(GatherRules.IsCritTime(0.9f + 1.5f));
        Assert.IsFalse(GatherRules.IsCritTime(1.0f));
    }

    [Test]
    public void RareBonus_ShiftsFromCommon()
    {
        GatherRules.RareBonus(0.4f, out float r0, out float u0);
        Assert.AreEqual(0f, r0);
        GatherRules.RareBonus(0.5f, out float r1, out float u1);
        Assert.AreEqual(5f, r1);
        Assert.AreEqual(1f, u1);
        GatherRules.RareBonus(1f, out float r2, out float u2);
        Assert.AreEqual(10f, r2);
        Assert.AreEqual(3f, u2);
        GatherDef[] t = GatherRules.OreFloor1;
        Assert.AreEqual(67f, GatherRules.Weight(t, 0, r2, u2), 0.001f);
        Assert.AreEqual(27f, GatherRules.Weight(t, 1, r2, u2), 0.001f);
        Assert.AreEqual(6f, GatherRules.Weight(t, 2, r2, u2), 0.001f);
    }

    [Test]
    public void Roll_PicksByCumulativeWeight()
    {
        GatherDef[] t = GatherRules.SpecimenFloor1;
        Assert.AreEqual(0, GatherRules.Roll(t, 0f, 0f, 0f));
        Assert.AreEqual(1, GatherRules.Roll(t, 0.46f, 0f, 0f));
        Assert.AreEqual(2, GatherRules.Roll(t, 0.81f, 0f, 0f));
        Assert.AreEqual(3, GatherRules.Roll(t, 0.99f, 0f, 0f));
    }

    [Test]
    public void Specimen_PerfectAddsOne()
    {
        // 목표 = 20 + 60*0.5 = 50, 커서가 50 에 오도록 진행 후 누름. 롤 0.5 → 두 번째 C(숲버섯), 수량 1~2 중 2 + 1
        var s = new SpecimenSession(GatherRules.SpecimenFloor1, () => 0.5f);
        s.Tick(50f / GatherRules.CursorSpeed, false);
        s.Tick(0f, true);
        Assert.IsTrue(s.Done);
        Assert.AreEqual(GatherResult.Success, s.Result);
        Assert.AreEqual(SpecimenJudge.Perfect, s.LastJudge);
        Assert.AreEqual("Forest Mushroom", s.Item.Id);
        Assert.AreEqual(3, s.Count);
    }

    [Test]
    public void Specimen_ThreeMissesFail()
    {
        var s = new SpecimenSession(GatherRules.SpecimenFloor1, () => 0.5f);
        for (int i = 0; i < 3; i++)
        {
            s.Tick(0f, true); // 커서 0, 목표 50 → 빗나감
            s.Tick(GatherRules.MissPause + 0.01f, false);
            if (i < 2)
            {
                Assert.IsFalse(s.Done); // 멈춤 동안 커서는 서 있어 계속 0 → 빗나감
            }
        }
        Assert.AreEqual(GatherResult.Failed, s.Result);
        Assert.AreEqual(3, s.AttemptsUsed);
    }

    [Test]
    public void Specimen_MissRerollsTargetAwayFromCursor()
    {
        float[] seq = { 0.5f, 1f / 6f, 0.99f };
        int k = 0;
        var s = new SpecimenSession(GatherRules.SpecimenFloor1, () => seq[System.Math.Min(k++, seq.Length - 1)]);
        s.Tick(30f / GatherRules.CursorSpeed, false); // 커서 30, 목표 50 → 빗나감
        s.Tick(0f, true);
        Assert.IsFalse(s.Done);
        Assert.Greater(System.Math.Abs(s.Cursor - s.Target), GatherRules.GoodHalf); // 30 이 뽑혀도 다시 뽑아 멀어짐
    }

    [Test]
    public void Specimen_TimesOut()
    {
        var s = new SpecimenSession(GatherRules.SpecimenFloor1, () => 0.5f);
        for (int i = 0; i < 130; i++)
        {
            s.Tick(0.1f, false);
        }
        Assert.AreEqual(GatherResult.TimedOut, s.Result);
    }

    [Test]
    public void Mining_AllCritsBreakInThreeSwingsWithBonus()
    {
        var s = new MiningSession(GatherRules.OreFloor1, () => 0.999f);
        s.Tick(0.75f, true);        // 정타 +2
        s.Tick(1.5f, true);         // 2.25 정타 +2
        s.Tick(1.5f, true);         // 3.75 정타 +2 → 6 ≥ 5
        Assert.IsTrue(s.Done);
        Assert.AreEqual(3, s.Crits);
        Assert.AreEqual(GatherResult.Success, s.Result);
        Assert.AreEqual("Starfall Ore", s.Item.Id);
    }

    [Test]
    public void Mining_NormalSwingsAndSwingInterval()
    {
        var s = new MiningSession(GatherRules.OreFloor1, () => 0f);
        s.Tick(0.1f, true);         // 일반 +1
        s.Tick(0.1f, true);         // 0.1초 뒤 → 연타 무시
        Assert.AreEqual(1, s.Cracks);
        Assert.AreEqual(1, s.Swings);
        for (int i = 0; i < 4; i++)
        {
            s.Tick(1.5f, true);     // 1.7, 3.2, 4.7, 6.2 → 주기 안 0.2초, 모두 비발광
        }
        Assert.AreEqual(GatherResult.Success, s.Result);
        Assert.AreEqual(0, s.Crits);
        Assert.AreEqual("T0_Ore", s.Item.Id);
    }

    [Test]
    public void Cancel_StopsAndIgnoredAfterDone()
    {
        var s = new MiningSession(GatherRules.OreFloor1, () => 0f);
        s.Cancel();
        Assert.AreEqual(GatherResult.Cancelled, s.Result);
        var t = new SpecimenSession(GatherRules.SpecimenFloor1, () => 0.5f);
        for (int i = 0; i < 130; i++)
        {
            t.Tick(0.1f, false);
        }
        t.Cancel();
        Assert.AreEqual(GatherResult.TimedOut, t.Result);
    }
}
