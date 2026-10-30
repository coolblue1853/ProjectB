using System;
using NUnit.Framework;
using ProjectB.EditorTools;

public class ExcavationRulesTests
{
    /// <summary>고정 시드 난수(0~1)</summary>
    private static Func<float> Seeded(int seed)
    {
        var r = new Random(seed);
        return () => (float)r.NextDouble();
    }

    [Test]
    public void Table_IdsExistInItemSheetWithSameRarity()
    {
        var items = ItemTable.Load();
        float total = 0f;
        foreach (GatherDef g in ExcavationRules.Floor1)
        {
            total += g.BaseWeight;
            if (g.Id == ExcavationRules.RecordId)
            {
                continue;
            }
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
        Assert.AreEqual(100f, total, 0.001f);
    }

    [Test]
    public void Generate_RespectsCountsAndConstraints()
    {
        for (int seed = 0; seed < 300; seed++)
        {
            DigSpec spec = seed % 2 == 0 ? ExcavationRules.Basic : ExcavationRules.RareSite;
            DigContent[] cells = ExcavationRules.Generate(spec, Seeded(seed));
            int relics = 0;
            int hazards = 0;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == DigContent.Relic)
                {
                    relics++;
                    Assert.AreEqual(0, ExcavationRules.Hint(cells, spec.Size, i), "유물끼리 붙음 seed " + seed);
                }
                else if (cells[i] == DigContent.Hazard)
                {
                    hazards++;
                }
            }
            Assert.AreEqual(spec.Relics, relics, "seed " + seed);
            Assert.AreEqual(spec.Hazards, hazards, "seed " + seed);
            Assert.AreEqual(DigContent.Empty, cells[ExcavationRules.Center(spec.Size)]);
        }
    }

    [Test]
    public void Hint_CountsOrthogonalRelicsOnly()
    {
        var cells = new DigContent[25];
        cells[0] = DigContent.Relic;   // (0,0)
        cells[2] = DigContent.Relic;   // (2,0)
        cells[6] = DigContent.Relic;   // (1,1) 대각선 — (0,0) 기준으로는 세지 않음
        Assert.AreEqual(3, ExcavationRules.Hint(cells, 5, 1)); // (1,0): 왼 (0,0) + 오 (2,0) + 아래 (1,1)
        Assert.AreEqual(0, ExcavationRules.Hint(cells, 5, 12));
        Assert.AreEqual(2, ExcavationRules.Hint(cells, 5, 5)); // (0,1): 위 (0,0) + 오 (1,1)
        Assert.AreEqual(0, ExcavationRules.Hint(cells, 5, 18)); // (3,3): 주변에 없음
    }

    [Test]
    public void Session_CenterRevealedFree_DigConsumesPick()
    {
        var s = new ExcavationSession(ExcavationRules.Basic, ExcavationRules.Floor1, Seeded(1));
        Assert.IsTrue(s.IsDug(12));
        Assert.AreEqual(12, s.Cursor);
        Assert.IsFalse(s.Dig(), "이미 드러난 칸은 곡괭이를 쓰지 않는다");
        Assert.AreEqual(ExcavationRules.Basic.Picks, s.PicksLeft);
        s.Move(-1, 0);
        Assert.AreEqual(11, s.Cursor);
        Assert.IsTrue(s.Dig());
        Assert.AreEqual(ExcavationRules.Basic.Picks - 1, s.PicksLeft);
        s.Move(-9, -9);
        Assert.AreEqual(0, s.Cursor, "판 밖으로 나가지 않는다");
    }

    [Test]
    public void Session_FindAllRelics_Succeeds_WithOneFindPerRelic()
    {
        var s = new ExcavationSession(ExcavationRules.Basic, ExcavationRules.Floor1, Seeded(7));
        foreach (int i in IndicesOf(s, DigContent.Relic))
        {
            MoveTo(s, i);
            Assert.IsTrue(s.Dig());
        }
        Assert.IsTrue(s.Done);
        Assert.AreEqual(DigResult.Success, s.Result);
        Assert.AreEqual(ExcavationRules.Basic.Relics, s.Finds.Count);
        Assert.AreEqual(ExcavationRules.Basic.Picks - ExcavationRules.Basic.Relics, s.PicksLeft);
    }

    [Test]
    public void Session_TwoHazards_Collapse_KeepsEarlierFinds()
    {
        var s = new ExcavationSession(ExcavationRules.Basic, ExcavationRules.Floor1, Seeded(3));
        MoveTo(s, IndicesOf(s, DigContent.Relic)[0]);
        s.Dig();
        int[] hazards = IndicesOf(s, DigContent.Hazard);
        MoveTo(s, hazards[0]);
        s.Dig();
        Assert.IsFalse(s.Done);
        Assert.AreEqual(50, s.Collapse);
        MoveTo(s, hazards[1]);
        s.Dig();
        Assert.AreEqual(DigResult.Collapsed, s.Result);
        Assert.AreEqual(1, s.Finds.Count, "실패해도 이미 캔 유물은 남는다");
    }

    [Test]
    public void Session_OutOfPicks_And_Timeout()
    {
        var s = new ExcavationSession(ExcavationRules.Basic, ExcavationRules.Floor1, Seeded(5));
        int dug = 0;
        for (int i = 0; i < 25 && !s.Done; i++)
        {
            if (s.ContentAt(i) == DigContent.Empty && !s.IsDug(i))
            {
                MoveTo(s, i);
                s.Dig();
                dug++;
            }
        }
        Assert.AreEqual(DigResult.OutOfPicks, s.Result);
        Assert.AreEqual(ExcavationRules.Basic.Picks, dug);

        var t = new ExcavationSession(ExcavationRules.Basic, ExcavationRules.Floor1, Seeded(5));
        t.Tick(ExcavationRules.Basic.TimeLimit - 0.1f);
        Assert.IsFalse(t.Done);
        t.Tick(0.2f);
        Assert.AreEqual(DigResult.TimedOut, t.Result);
        t.Cancel();
        Assert.AreEqual(DigResult.TimedOut, t.Result, "결과가 난 뒤 중단은 무시");
    }

    [Test]
    public void Hazards_VisibleBeforeDig_RelicsHidden()
    {
        var s = new ExcavationSession(ExcavationRules.Basic, ExcavationRules.Floor1, Seeded(11));
        Assert.AreEqual(DigContent.Hazard, s.ContentIfVisible(IndicesOf(s, DigContent.Hazard)[0]));
        Assert.AreEqual(DigContent.Empty, s.ContentIfVisible(IndicesOf(s, DigContent.Relic)[0]));
        Assert.AreEqual(-1, s.HintAt(IndicesOf(s, DigContent.Relic)[0]));
    }

    [Test]
    public void PickRecord_OnlyMissing_ThenFallback()
    {
        Assert.AreEqual(0, ExcavationRules.PickRecord(0, 5, 0f));
        Assert.AreEqual(4, ExcavationRules.PickRecord(0, 5, 0.99f));
        Assert.AreEqual(3, ExcavationRules.PickRecord(0b10111, 5, 0.5f), "남은 건 3번뿐");
        Assert.AreEqual(2, ExcavationRules.PickRecord(0b10011, 5, 0f));
        Assert.AreEqual(3, ExcavationRules.PickRecord(0b10011, 5, 0.99f));
        Assert.AreEqual(-1, ExcavationRules.PickRecord(0b11111, 5, 0.3f));
        Assert.AreEqual(FathersRecords.All.Length, 5);
    }

    [Test]
    public void SpecFor_RareChance()
    {
        Assert.IsTrue(ExcavationRules.SpecFor(0.1f).Rare);
        Assert.IsFalse(ExcavationRules.SpecFor(0.2f).Rare);
        Assert.AreEqual(600f, ExcavationRules.CooldownFor(ExcavationRules.RareSite));
    }

    private static int[] IndicesOf(ExcavationSession s, DigContent c)
    {
        var list = new System.Collections.Generic.List<int>();
        for (int i = 0; i < s.Size * s.Size; i++)
        {
            if (s.ContentAt(i) == c)
            {
                list.Add(i);
            }
        }
        return list.ToArray();
    }

    private static void MoveTo(ExcavationSession s, int index)
    {
        s.Move(index % s.Size - s.Cursor % s.Size, index / s.Size - s.Cursor / s.Size);
    }
}
