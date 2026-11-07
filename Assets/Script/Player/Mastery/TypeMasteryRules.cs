using System;

/// <summary>
/// 타입별 숙련 순수 규칙 (WeaponSkillProgression.md V.1). 곡선·칭호·포인트만 둔다. 상태는 TypeMasteryData/Service.
/// 레벨·포인트는 저장하지 않고 누적 EXP 에서 계산한다(곡선을 조정해도 세이브가 재계산된다).
/// </summary>
public static class TypeMasteryRules
{
    public const int MaxLevel = 10;
    /// <summary>누적 EXP 상한(오버플로 방지)</summary>
    public const int ExpCap = 1000000;
    /// <summary>등급 칭호 단계 수(견습·숙련자·전문가·달인·명인)</summary>
    public const int TitleCount = 5;

    // 1→2 ... 9→10 필요 EXP. 1~4 구간은 개별 무기 숙련 곡선과 같다(구세이브·해금 표 호환). 누적 152000
    static readonly int[] Curve = { 400, 1100, 3000, 7500, 12000, 18000, 26000, 36000, 48000 };

    /// <summary>다음 레벨까지 필요한 EXP. 만렙이면 0</summary>
    public static int ExpToNext(int level)
    {
        if (level >= MaxLevel)
        {
            return 0;
        }
        return Curve[Math.Max(level, 1) - 1];
    }

    /// <summary>해당 레벨에 도달하는 누적 EXP (Lv1 = 0)</summary>
    public static int TotalExpForLevel(int level)
    {
        int total = 0;
        for (int lv = 1; lv < Math.Min(level, MaxLevel); lv++)
        {
            total += ExpToNext(lv);
        }
        return total;
    }

    /// <summary>누적 EXP → 숙련 레벨(1~10)</summary>
    public static int LevelFromExp(int totalExp)
    {
        int level = 1;
        int remain = Math.Max(totalExp, 0);
        while (level < MaxLevel)
        {
            int need = ExpToNext(level);
            if (remain < need)
            {
                break;
            }
            remain -= need;
            level++;
        }
        return level;
    }

    /// <summary>현재 레벨 안에서 쌓은 EXP. 만렙이면 0</summary>
    public static int ExpInLevel(int totalExp)
    {
        int lv = LevelFromExp(totalExp);
        return lv >= MaxLevel ? 0 : Math.Max(totalExp, 0) - TotalExpForLevel(lv);
    }

    /// <summary>등급 칭호 단계 0~4 (Lv1~2 = 0, Lv9~10 = 4). Loc 키 Mastery.Title.(n+1)</summary>
    public static int TitleIndex(int level)
    {
        return Math.Max(0, Math.Min((level - 1) / 2, TitleCount - 1));
    }

    /// <summary>레벨까지 받은 트리 포인트(Lv2 부터 레벨당 1). 저장하지 않는 파생값</summary>
    public static int EarnedPoints(int level)
    {
        return Math.Max(0, Math.Min(level, MaxLevel) - 1);
    }
}
