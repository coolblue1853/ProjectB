using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectB.EditorTools;

public class CookingRulesTests
{
    /// <summary>항상 같은 값을 돌려주는 난수(0.5 = 목표 50, 화력 변동 0)</summary>
    private static Func<float> Const(float v)
    {
        return () => v;
    }

    private static CookingSession Start(CookMethod method, Func<float> random)
    {
        CookRecipe recipe = null;
        foreach (CookRecipe r in CookingRules.Recipes)
        {
            if (r.Method == method)
            {
                recipe = r;
                break;
            }
        }
        var session = new CookingSession(new List<CookRecipe> { recipe }, random);
        Assert.IsNotNull(session.Confirm());
        return session;
    }

    /// <summary>굽기 한 회를 진행: 창이 열린 뒤 커서가 (목표 + offset) 근처에 오면 누른다(null = 안 누름). 0.01초 단위라 한 틱에 커서가 약 1.3 움직인다</summary>
    private static void PlayGrillRound(CookingSession s, float? offset)
    {
        int round = s.Round;
        bool pressed = false;
        while (s.Phase == CookPhase.Play && s.Round == round)
        {
            bool press = false;
            if (!pressed && offset.HasValue && s.WindowOpen && Math.Abs(s.GrillCursor - (s.GrillTarget + offset.Value)) <= 2f)
            {
                press = true;
                pressed = true;
            }
            s.Tick(0.01f, false, press);
        }
    }

    [Test]
    public void Recipes_IdsExistInItemSheetWithSameRarity()
    {
        var items = ItemTable.Load();
        var names = new Dictionary<string, ItemRow>();
        foreach (ItemRow r in items.Rows)
        {
            names[r.Name] = r;
        }
        foreach (CookRecipe recipe in CookingRules.Recipes)
        {
            Assert.IsTrue(names.ContainsKey(recipe.Id), "ItemSheet 에 없음: " + recipe.Id);
            Assert.AreEqual("Consum", names[recipe.Id].Type);
            Assert.AreEqual(recipe.Rarity.ToString(), names[recipe.Id].Rarity, "등급 불일치: " + recipe.Id);
            foreach (CookIngredient i in recipe.Ingredients)
            {
                Assert.IsTrue(names.ContainsKey(i.Id), "재료가 ItemSheet 에 없음: " + i.Id);
            }
        }
        Assert.IsTrue(names.ContainsKey(CookingRules.BurntId));
    }

    [Test]
    public void Recipes_EffectsParseAndBuffNamesAreKnown()
    {
        var items = ItemTable.Load();
        foreach (ItemRow r in items.Rows)
        {
            if (r.AcqPath != "cook" || r.Type != "Consum")
            {
                continue;
            }
            string[] ob = r.EffectOb.Split('/');
            string[] pow = r.EffectPow.Split('/');
            Assert.AreEqual(ob.Length, pow.Length, r.Name);
            int buffs = 0;
            for (int i = 0; i < ob.Length; i++)
            {
                string[] parts = ob[i].Split(' ');
                if (FoodBuff.TryParseKind(parts[0], out FoodBuffKind _))
                {
                    buffs++;
                    string[] detail = pow[i].Split('_');
                    Assert.AreEqual(2, detail.Length, r.Name + " 버프는 값_초");
                    Assert.IsTrue(int.TryParse(detail[0], out int _) && int.TryParse(detail[1], out int _), r.Name);
                }
            }
            Assert.LessOrEqual(buffs, 1, r.Name + " 요리 버프는 1종만");
        }
    }

    [Test]
    public void Available_OnlyRecipesWithAllIngredients()
    {
        var have = new Dictionary<string, int> { { "Rabbit Meat", 2 }, { "GreenHurb", 1 }, { "Wood", 1 }, { "Fox Meat", 2 } };
        Func<string, int> count = id => have.TryGetValue(id, out int n) ? n : 0;
        List<CookRecipe> list = CookingRules.Available(count);
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual("Herb Roasted Rabbit", list[0].Id);
        have["Wood"] = 2;
        Assert.AreEqual(2, CookingRules.Available(count).Count); // 여우꼬치(여우고기 2 + 나무 2 + 약초 1) 가능
    }

    [Test]
    public void GradeGrill_Thresholds()
    {
        Assert.AreEqual(CookGrade.Perfect, CookingRules.GradeGrill(new[] { 0f, 8f, 3f }, 0));
        Assert.AreEqual(CookGrade.Good, CookingRules.GradeGrill(new[] { 9f, 14f, 15f }, 0));          // 평균 12.7, 한 번이 8 초과 = 완벽 아님
        Assert.AreEqual(CookGrade.Normal, CookingRules.GradeGrill(new[] { 20f, 30f, 25f }, 0));      // 평균 25
        Assert.AreEqual(CookGrade.Fail, CookingRules.GradeGrill(new[] { 30f, 30f, 35f }, 0));        // 평균 31.7 > 30
        Assert.AreEqual(CookGrade.Normal, CookingRules.GradeGrill(new[] { 5f, 5f }, 1));             // 1회 미입력: (60+5+5)/3 = 23.3
        Assert.AreEqual(CookGrade.Fail, CookingRules.GradeGrill(new[] { 0f }, 2));                   // 2회 미입력 = 실패
        Assert.AreNotEqual(CookGrade.Perfect, CookingRules.GradeGrill(new[] { 0f, 0f, 31f }, 0));    // 한 번이라도 30 초과면 완벽 불가
    }

    [Test]
    public void GradeBoil_Thresholds()
    {
        Assert.AreEqual(CookGrade.Perfect, CookingRules.GradeBoil(0.9f, false));
        Assert.AreEqual(CookGrade.Good, CookingRules.GradeBoil(0.89f, false));
        Assert.AreEqual(CookGrade.Good, CookingRules.GradeBoil(0.75f, false));
        Assert.AreEqual(CookGrade.Normal, CookingRules.GradeBoil(0.74f, false));
        Assert.AreEqual(CookGrade.Normal, CookingRules.GradeBoil(0.5f, false));
        Assert.AreEqual(CookGrade.Fail, CookingRules.GradeBoil(0.49f, false));
        Assert.AreEqual(CookGrade.Fail, CookingRules.GradeBoil(1f, true)); // 과열은 즉시 실패
    }

    [Test]
    public void ResultCount_AndId()
    {
        CookRecipe recipe = CookingRules.Recipes[0];
        Assert.AreEqual(2, CookingRules.ResultCount(CookGrade.Perfect));
        Assert.AreEqual(1, CookingRules.ResultCount(CookGrade.Good));
        Assert.AreEqual(1, CookingRules.ResultCount(CookGrade.Normal));
        Assert.AreEqual(1, CookingRules.ResultCount(CookGrade.Fail));
        Assert.AreEqual(recipe.Id, CookingRules.ResultId(recipe, CookGrade.Good));
        Assert.AreEqual(CookingRules.BurntId, CookingRules.ResultId(recipe, CookGrade.Fail));
    }

    [Test]
    public void Grill_PerfectRounds_GivePerfect()
    {
        CookingSession s = Start(CookMethod.Grill, Const(0.5f));
        for (int i = 0; i < CookingRules.GrillRounds; i++)
        {
            PlayGrillRound(s, 0f);
        }
        Assert.AreEqual(CookPhase.Result, s.Phase);
        Assert.AreEqual(CookGrade.Perfect, s.Grade);
        Assert.AreEqual(0, s.Misses);
        Assert.LessOrEqual(CookingRules.GrillTotalTime, 12.001f);
    }

    [Test]
    public void Grill_NoInput_FailsAfterTwelveSeconds()
    {
        CookingSession s = Start(CookMethod.Grill, Const(0.5f));
        float t = 0f;
        while (s.Phase == CookPhase.Play && t < 20f)
        {
            s.Tick(0.05f, false, false);
            t += 0.05f;
        }
        Assert.AreEqual(CookGrade.Fail, s.Grade);
        Assert.AreEqual(3, s.Misses);
        Assert.AreEqual(12f, t, 0.2f);
    }

    [Test]
    public void Grill_SinglePressIgnoredOutsideWindow()
    {
        CookingSession s = Start(CookMethod.Grill, Const(0.5f));
        s.Tick(0.1f, false, true); // 창이 열리기 전 누름 = 무시
        Assert.AreEqual(0, s.Errors.Count);
        Assert.AreEqual(CookPhase.Play, s.Phase);
    }

    [Test]
    public void Boil_NoHold_CoolsOutOfZoneAndFails()
    {
        CookingSession s = Start(CookMethod.Boil, Const(0.5f)); // 변동 0, 계속 떼고 있으면 50 → 0 으로 식음
        float t = 0f;
        while (s.Phase == CookPhase.Play && t < 30f)
        {
            s.Tick(0.05f, false, false);
            t += 0.05f;
        }
        Assert.AreEqual(CookGrade.Fail, s.Grade);
        Assert.IsFalse(s.Overheated);
        Assert.AreEqual(CookingRules.BoilTime, t, 0.2f);
    }

    [Test]
    public void Boil_HoldsToStayInZone_GivesPerfect()
    {
        CookingSession s = Start(CookMethod.Boil, Const(0.5f));
        float t = 0f;
        while (s.Phase == CookPhase.Play && t < 30f)
        {
            bool hold = s.Heat < 52f; // 안전 구간(40~65) 가운데 근처에서 줄타기
            s.Tick(0.05f, hold, false);
            t += 0.05f;
        }
        Assert.AreEqual(CookPhase.Result, s.Phase);
        Assert.AreEqual(CookGrade.Perfect, s.Grade);
        Assert.IsFalse(s.Overheated);
    }

    [Test]
    public void Boil_HeatAbove85ForThreeSeconds_FailsImmediately()
    {
        CookingSession s = Start(CookMethod.Boil, Const(0.5f));
        float t = 0f;
        while (s.Phase == CookPhase.Play && t < 30f)
        {
            s.Tick(0.05f, true, false); // 계속 홀드
            t += 0.05f;
        }
        Assert.IsTrue(s.Overheated);
        Assert.AreEqual(CookGrade.Fail, s.Grade);
        // 50 → 85 는 0.875초, 이어 3초 = 약 3.9초 안에 끝난다(18초를 채우지 않음)
        Assert.Less(t, 5f);
        Assert.Greater(t, 3.5f);
    }

    [Test]
    public void Boil_OverheatTimerResetsWhenCooled()
    {
        CookingSession s = Start(CookMethod.Boil, Const(0.5f));
        // 85 이상으로 올린 뒤 2초 안에 식히면 즉시 실패하지 않는다
        while (s.Heat < CookingRules.OverheatLevel)
        {
            s.Tick(0.05f, true, false);
        }
        for (int i = 0; i < 20; i++) // 1초 더 가열
        {
            s.Tick(0.05f, true, false);
        }
        Assert.Greater(s.OverheatTime, 0.5f);
        while (s.Heat >= CookingRules.OverheatLevel)
        {
            s.Tick(0.05f, false, false);
        }
        Assert.AreEqual(CookPhase.Play, s.Phase);
        s.Tick(0.05f, false, false);
        Assert.AreEqual(0f, s.OverheatTime, 0.0001f);
    }

    [Test]
    public void Cancel_BeforeAndAfterStart()
    {
        var s = new CookingSession(CookingRules.Available(_ => 99), Const(0.5f));
        s.Cancel();
        Assert.IsTrue(s.Cancelled);
        Assert.AreEqual(CookPhase.Result, s.Phase);
        var empty = new CookingSession(new List<CookRecipe>(), Const(0.5f));
        Assert.IsNull(empty.Confirm()); // 재료가 없으면 시작 불가
        empty.Select(1);
        Assert.AreEqual(CookPhase.Select, empty.Phase);
    }

    [Test]
    public void Select_ClampsAtEnds()
    {
        var s = new CookingSession(CookingRules.Available(_ => 99), Const(0.5f));
        s.Select(-1);
        Assert.AreEqual(0, s.Index);
        for (int i = 0; i < 20; i++)
        {
            s.Select(1);
        }
        Assert.AreEqual(CookingRules.Recipes.Length - 1, s.Index);
        Assert.AreEqual(CookingRules.Recipes[CookingRules.Recipes.Length - 1], s.Confirm());
    }

    [Test]
    public void FishRareBonus_ShiftsWeightFromCommonToRare()
    {
        FishDef[] table = FishingRules.Floor1;
        RodDef rod = FishingRules.Rod(0);
        int rareHits0 = 0;
        int rareHits2 = 0;
        const int n = 20000;
        for (int i = 0; i < n; i++)
        {
            float roll = (i + 0.5f) / n;
            if (table[FishingRules.RollFish(table, roll, rod, CastQuality.Normal)].Rarity != ItemRarity.C)
            {
                rareHits0++;
            }
            if (table[FishingRules.RollFish(table, roll, rod, CastQuality.Normal, 2f)].Rarity != ItemRarity.C)
            {
                rareHits2++;
            }
        }
        float diff = (rareHits2 - rareHits0) / (float)n * 100f;
        Assert.AreEqual(2f, diff, 0.2f); // 희귀(R/U) 확률 +2%p
    }

    [Test]
    public void FoodBuff_AscentReductionIsPercentOfLoad()
    {
        Assert.AreEqual(0, FoodBuff.AscentReduction(60)); // 버프 없음
        Assert.AreEqual(FoodBuffKind.None, FoodBuff.Kind);
        Assert.IsTrue(FoodBuff.TryParseKind("ascent", out FoodBuffKind k) && k == FoodBuffKind.Ascent);
        Assert.IsFalse(FoodBuff.TryParseKind("def", out FoodBuffKind _));
    }
}
