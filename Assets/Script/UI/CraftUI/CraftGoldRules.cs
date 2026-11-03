using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 세트 제작 골드 요구(Floor1Completion.md 14-4, Q-F1C4). 순수 규칙 + ArmorCraftRecipes.csv 의 gold 열 조회.
/// 열이 없거나 빈 값이면 0(요구 없음).
/// </summary>
public static class CraftGoldRules
{
    private static Dictionary<string, int> s_gold;

    /// <summary>CSV gold 칸 → 정수. 비었거나 음수·깨진 값은 0</summary>
    public static int ParseGold(string cell)
    {
        return int.TryParse(cell == null ? "" : cell.Trim(), out int g) && g > 0 ? g : 0;
    }

    /// <summary>골드가 부족한가(요구 0 이하면 항상 충분)</summary>
    public static bool Short(int cost, int money)
    {
        return cost > 0 && money < cost;
    }

    /// <summary>제작 후 잔액. 부족하면 그대로(차감 안 함)</summary>
    public static int Spend(int cost, int money)
    {
        return Short(cost, money) || cost <= 0 ? money : money - cost;
    }

    /// <summary>제작 창 비용 문구. 요구 0 이면 빈 문자열</summary>
    public static string CostLabel(int cost)
    {
        return cost > 0 ? cost + " G" : "";
    }

    /// <summary>결과 아이템 이름 → 제작 골드(방어구 레시피 CSV 기준). 없으면 0</summary>
    public static int GoldFor(string result)
    {
        if (string.IsNullOrEmpty(result))
        {
            return 0;
        }
        if (s_gold == null)
        {
            s_gold = new Dictionary<string, int>();
            TextAsset t = Resources.Load<TextAsset>(SelfCraftPages.ArmorResourcePath);
            foreach (SelfCraftPages.Recipe r in SelfCraftPages.Parse(t != null ? t.text : ""))
            {
                s_gold[r.Result] = r.Gold;
            }
        }
        return s_gold.TryGetValue(result, out int g) ? g : 0;
    }
}
