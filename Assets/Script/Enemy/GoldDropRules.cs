using UnityEngine;

/// <summary>
/// 몹 골드(메소) 직접 드롭 규칙 (ClassicMapleDesign.md 14절). 상태 없음 → EditMode 테스트 가능.
/// 금액 = 몹 레벨 x (3~5) + 2, 정예 x3, 보스 x10. 일반 몹은 60% 확률, 정예·보스는 확정.
/// 더미 개수: 일반 1, 정예 2, 보스 3 (총액을 균등 분할). 모양 단계는 더미 하나의 금액으로 정한다.
/// </summary>
public static class GoldDropRules
{
    public const float NormalChance = 0.6f;
    public const int EliteMul = 3, BossMul = 10;
    public const int Tier1Max = 19, Tier2Max = 59; // 1단계(코인 1개) < 20 <= 2단계(코인 더미) < 60 <= 3단계(자루)

    /// <summary>한 마리가 떨어뜨리는 총 골드. t = 0~1 난수. 레벨 0 이하(표에 없는 몹)는 0</summary>
    public static int Total(int level, bool isElite, bool isBoss, float t)
    {
        if (level <= 0)
        {
            return 0;
        }
        float baseGold = level * Mathf.Lerp(3f, 5f, Mathf.Clamp01(t)) + 2f;
        int mul = isBoss ? BossMul : isElite ? EliteMul : 1;
        return Mathf.Max(1, Mathf.RoundToInt(baseGold * mul));
    }

    /// <summary>드롭 여부: 정예·보스는 확정, 일반은 60%. roll = 0~1 난수</summary>
    public static bool ShouldDrop(bool isElite, bool isBoss, float roll)
    {
        return isElite || isBoss || roll < NormalChance;
    }

    public static int PileCount(bool isElite, bool isBoss)
    {
        return isBoss ? 3 : isElite ? 2 : 1;
    }

    /// <summary>총액을 n 개로 나눈다(합계 보존, 앞쪽 더미가 나머지를 1씩 더 받는다)</summary>
    public static int Share(int total, int piles, int index)
    {
        if (piles <= 1)
        {
            return total;
        }
        int each = total / piles;
        return each + (index < total % piles ? 1 : 0);
    }

    /// <summary>모양 단계 0~2</summary>
    public static int Tier(int pileAmount)
    {
        return pileAmount <= Tier1Max ? 0 : pileAmount <= Tier2Max ? 1 : 2;
    }
}
