using System;
using System.Collections.Generic;

/// <summary>
/// 일괄 제작·요리 공용 순수 규칙(REWORK-11, Cooking.md). 수량 선택 창(QuantityPopup)의 최대값을 구한다.
/// 재료 있는 만큼 · 골드 있는 만큼 · 상한(요리 10, 제작 99) 중 가장 작은 값.
/// </summary>
public static class BatchRules
{
    /// <summary>요리 한 번에 만들 수 있는 최대 개수(한 판 = 한 번의 미니게임 결과를 전부에 적용)</summary>
    public const int CookCap = 10;
    /// <summary>대장장이 자가 제작(소모품·재료) 일괄 상한</summary>
    public const int CraftCap = 99;

    /// <summary>
    /// 재료·골드로 만들 수 있는 최대 개수(0 이상, cap 이하). needs = (아이템 이름 → 1회 필요 수). 필요 수 0 이하인 줄은 무시.
    /// goldPerCraft 가 0 이하면 골드 무관.
    /// </summary>
    public static int MaxCount(IEnumerable<KeyValuePair<string, int>> needs, Func<string, int> have, int goldPerCraft, int money, int cap)
    {
        int max = Math.Max(0, cap);
        foreach (KeyValuePair<string, int> need in needs)
        {
            if (need.Value <= 0)
            {
                continue;
            }
            max = Math.Min(max, have(need.Key) / need.Value);
        }
        if (goldPerCraft > 0)
        {
            max = Math.Min(max, Math.Max(0, money) / goldPerCraft);
        }
        return Math.Max(0, max);
    }
}
