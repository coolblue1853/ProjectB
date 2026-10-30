using System;
using System.Collections.Generic;

public enum CookMethod { Grill, Boil }

/// <summary>요리 결과 등급</summary>
public enum CookGrade { Fail, Normal, Good, Perfect }

/// <summary>레시피 재료 한 줄(ItemSheet 이름 + 개수)</summary>
public readonly struct CookIngredient
{
    public readonly string Id;
    public readonly int Count;

    public CookIngredient(string id, int count)
    {
        Id = id;
        Count = count;
    }
}

/// <summary>화로 전용 레시피. Id 는 결과 요리의 ItemSheet 이름</summary>
public sealed class CookRecipe
{
    public readonly string Id;
    public readonly CookMethod Method;
    public readonly ItemRarity Rarity;
    public readonly CookIngredient[] Ingredients;

    public CookRecipe(string id, CookMethod method, ItemRarity rarity, params CookIngredient[] ingredients)
    {
        Id = id;
        Method = method;
        Rarity = rarity;
        Ingredients = ingredients;
    }
}

/// <summary>
/// ② 탐험가 요리(캠프 화로) 순수 규칙·수치(MiniGames.md "탐험가 요리 구현"). 출처: GPT 기획 초안(10-30) + 메인 조정.
/// 굽기 = 12초에 뒤집기 3회(회마다 1.5초 창에서 왕복 커서를 목표 중심에 맞춤), 끓이기 = 18초 동안 안전 화력 구간 유지.
/// 난수는 호출자가 0~1 값으로 넘긴다(테스트 가능).
/// </summary>
public static class CookingRules
{
    // 굽기
    public const int GrillRounds = 3;
    public const float GrillRoundTime = 4f;       // 회당 구간(12초 / 3)
    public const float GrillWindowTime = 1.5f;    // 구간 끝 1.5초가 입력 창
    public const float GrillTotalTime = GrillRounds * GrillRoundTime;
    public const float GrillTargetMin = 30f;      // 목표 중심(0~100 막대) 무작위 범위
    public const float GrillTargetMax = 70f;
    public const float PerfectError = 8f;
    public const float GoodAverage = 15f;
    public const float NormalAverage = 30f;
    public const float MissError = 60f;           // 입력 못 한 회의 오차로 평균에 넣는 값(미입력 2회 = 실패)
    public const int MissFailCount = 2;

    // 끓이기
    public const float BoilTime = 18f;
    public const float HeatStart = 50f;
    public const float HeatUpRate = 40f;          // 홀드 중 초당 상승
    public const float HeatDownRate = 30f;        // 뗀 동안 초당 하강
    public const float HeatDriftMax = 20f;        // 초당 ±20% 변동(0.5초마다 새로 뽑음)
    public const float HeatDriftStep = 0.5f;
    public const float SafeMin = 40f;
    public const float SafeMax = 65f;
    public const float OverheatLevel = 85f;
    public const float OverheatFail = 3f;
    public const float PerfectRatio = 0.9f;
    public const float GoodRatio = 0.75f;
    public const float NormalRatio = 0.5f;

    // 공통
    public const float StoveCooldown = 90f;
    public const string BurntId = "Burnt Meal";

    public static readonly CookRecipe[] Recipes =
    {
        new CookRecipe("Herb Roasted Rabbit", CookMethod.Grill, ItemRarity.C,
            new CookIngredient("Rabbit Meat", 2), new CookIngredient("GreenHurb", 1), new CookIngredient("Wood", 1)),
        new CookRecipe("Moss Loach Broth", CookMethod.Boil, ItemRarity.C,
            new CookIngredient("Moss Loach", 2), new CookIngredient("Forest Mushroom", 1), new CookIngredient("GreenHurb", 1)),
        new CookRecipe("Ember Fox Skewer", CookMethod.Grill, ItemRarity.R,
            new CookIngredient("Fox Meat", 2), new CookIngredient("Wood", 2), new CookIngredient("GreenHurb", 1)),
        new CookRecipe("Glowwing Carp Soup", CookMethod.Boil, ItemRarity.R,
            new CookIngredient("Forest Carp", 2), new CookIngredient("Glow Moth Wing", 1), new CookIngredient("Forest Mushroom", 1)),
        new CookRecipe("Amber Herb Stew", CookMethod.Boil, ItemRarity.R,
            new CookIngredient("Amber Trout", 1), new CookIngredient("GreenHurb", 2), new CookIngredient("Forest Mushroom", 2)),
        new CookRecipe("Spirit Carp Moon Roast", CookMethod.Grill, ItemRarity.U,
            new CookIngredient("Spirit Koi", 1), new CookIngredient("Moonpetal", 1), new CookIngredient("Wood", 2)),
    };

    /// <summary>가진 재료로 만들 수 있는 레시피만(표 순서). have = 아이템 이름 → 개수 조회</summary>
    public static List<CookRecipe> Available(Func<string, int> have)
    {
        var list = new List<CookRecipe>();
        foreach (CookRecipe r in Recipes)
        {
            if (CanCook(r, have))
            {
                list.Add(r);
            }
        }
        return list;
    }

    public static bool CanCook(CookRecipe recipe, Func<string, int> have)
    {
        foreach (CookIngredient i in recipe.Ingredients)
        {
            if (have(i.Id) < i.Count)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>굽기 판정. errors = 입력한 회의 오차(%), misses = 입력 못 한 회 수</summary>
    public static CookGrade GradeGrill(IReadOnlyList<float> errors, int misses)
    {
        if (misses >= MissFailCount)
        {
            return CookGrade.Fail;
        }
        float sum = misses * MissError;
        float max = misses > 0 ? MissError : 0f;
        foreach (float e in errors)
        {
            sum += e;
            max = Math.Max(max, e);
        }
        float avg = sum / GrillRounds;
        if (avg > NormalAverage)
        {
            return CookGrade.Fail;
        }
        if (max <= PerfectError)
        {
            return CookGrade.Perfect;
        }
        return avg <= GoodAverage ? CookGrade.Good : CookGrade.Normal;
    }

    /// <summary>끓이기 판정. ratio = 안전 구간 유지율(0~1), overheated = 85%↑ 3초 연속으로 즉시 실패했는가</summary>
    public static CookGrade GradeBoil(float ratio, bool overheated)
    {
        if (overheated || ratio < NormalRatio)
        {
            return CookGrade.Fail;
        }
        if (ratio >= PerfectRatio)
        {
            return CookGrade.Perfect;
        }
        return ratio >= GoodRatio ? CookGrade.Good : CookGrade.Normal;
    }

    /// <summary>결과 개수: 완벽 2, 좋음·보통 1, 실패(탄 요리) 1</summary>
    public static int ResultCount(CookGrade grade)
    {
        return grade == CookGrade.Perfect ? 2 : 1;
    }

    /// <summary>결과 아이템 이름(실패면 탄 요리)</summary>
    public static string ResultId(CookRecipe recipe, CookGrade grade)
    {
        return grade == CookGrade.Fail ? BurntId : recipe.Id;
    }

    /// <summary>왕복 커서 0→100→0 (period 초에 한 번 왕복)</summary>
    public static float PingPong(float t, float period)
    {
        float x = (t % period) / period;
        return (x < 0.5f ? x * 2f : (1f - x) * 2f) * 100f;
    }
}
