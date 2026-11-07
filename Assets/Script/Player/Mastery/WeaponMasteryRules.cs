using System;

/// <summary>
/// 무기 숙련도 순수 규칙 (WeaponSkillProgression.md 3절). 곡선·해금표·키 규칙만 둔다. 상태는 WeaponMasteryData/Service.
/// 숙련 레벨은 저장하지 않고 누적 EXP 에서 계산한다(곡선을 조정해도 세이브가 재계산된다).
/// </summary>
public static class WeaponMasteryRules
{
    /// <summary>
    /// 개별 무기 숙련 사용 여부(WSKILL-V2, V.5). false(기본) = 타입 숙련이 해금을 정하고 따라잡기·StartMastery·Lv5 보너스는 꺼진다.
    /// 키별 EXP 누적은 꺼져도 계속된다. 불 스테이지 개조에서 true. static bool 이라 프로브로 토글 가능.
    /// </summary>
    public static bool PerWeaponMastery = false;

    public const int MaxLevel = 5;
    /// <summary>누적 EXP 상한(오버플로 방지)</summary>
    public const int ExpCap = 1000000;
    /// <summary>구세이브 무기 시작 EXP = 만렙(Lv5). Lv3 시작은 스킬 3·4번 슬롯이 잠겨 기존 플레이어가 스킬을 잃는다</summary>
    public const int LegacyStartExp = 12000;
    const string CloneSuffix = "(Clone)";

    // 1→2, 2→3, 3→4, 4→5 필요 EXP (기준표, 무기 계수 1.0)
    static readonly int[] Curve = { 400, 1100, 3000, 7500 };

    /// <summary>다음 숙련 레벨까지 필요한 EXP. 만렙이면 0</summary>
    public static int ExpToNext(int level, float scale = 1f)
    {
        if (level >= MaxLevel)
        {
            return 0;
        }
        int idx = Math.Max(level, 1) - 1;
        return (int)Math.Round(Curve[idx] * Math.Max(scale, 0.01f));
    }

    /// <summary>해당 레벨에 도달하는 누적 EXP (Lv1 = 0)</summary>
    public static int TotalExpForLevel(int level, float scale = 1f)
    {
        int total = 0;
        for (int lv = 1; lv < Math.Min(level, MaxLevel); lv++)
        {
            total += ExpToNext(lv, scale);
        }
        return total;
    }

    /// <summary>누적 EXP → 숙련 레벨(1~5)</summary>
    public static int LevelFromExp(int totalExp, float scale = 1f)
    {
        int level = 1;
        int remain = Math.Max(totalExp, 0);
        while (level < MaxLevel)
        {
            int need = ExpToNext(level, scale);
            if (remain < need)
            {
                break;
            }
            remain -= need;
            level++;
        }
        return level;
    }

    /// <summary>현재 레벨 안에서 쌓은 EXP (툴팁용). 만렙이면 0</summary>
    public static int ExpInLevel(int totalExp, float scale = 1f)
    {
        int lv = LevelFromExp(totalExp, scale);
        return lv >= MaxLevel ? 0 : Math.Max(totalExp, 0) - TotalExpForLevel(lv, scale);
    }

    /// <summary>스킬 개수 n 중 rank 번째(0부터, 슬롯 번호 순) 스킬이 열리는 숙련 레벨. 표는 문서 3.2</summary>
    public static int UnlockLevel(int rank, int skillCount)
    {
        if (rank < 0 || skillCount <= 0)
        {
            return 1;
        }
        switch (skillCount)
        {
            case 1: return 1;
            case 2: return rank == 0 ? 1 : 3;
            case 3: return rank == 0 ? 1 : rank == 1 ? 2 : 4;
            default: return Math.Min(rank + 1, MaxLevel - 1); // 4개 이상: Lv1..Lv4 로 순차
        }
    }

    public static bool IsUnlocked(int rank, int skillCount, int masteryLevel)
    {
        return masteryLevel >= UnlockLevel(rank, skillCount);
    }

    /// <summary>따라잡기: 같은 계열에 이 레벨 이상 무기가 있으면 신규 무기 EXP x2(임시)</summary>
    public const int CatchUpSourceLevel = 3;
    /// <summary>따라잡기 배율. 신규 무기가 CatchUpUntilLevel 에 도달하면 끝</summary>
    public const int CatchUpMultiplier = 2;
    public const int CatchUpUntilLevel = 3;

    /// <summary>EXP 획득 배율. 같은 계열에 Lv3+ 무기가 있고 이 무기가 Lv3 미만이면 2, 아니면 1</summary>
    public static int GainMultiplier(int weaponLevel, bool sameKindHasLv3)
    {
        return sameKindHasLv3 && weaponLevel < CatchUpUntilLevel ? CatchUpMultiplier : 1;
    }

    /// <summary>시작 숙련 Lv(StartMastery, 1~5로 보정) → 최초 누적 EXP</summary>
    public static int StartExp(int startMastery)
    {
        return TotalExpForLevel(Math.Max(1, Math.Min(startMastery, MaxLevel)));
    }

    /// <summary>만렙 보너스 데미지 배율(+5%, 임시)</summary>
    public static float Bonus(int masteryLevel)
    {
        return masteryLevel >= MaxLevel ? 1.05f : 1f;
    }

    /// <summary>생성된 프리팹 이름("Key(Clone)") → 숙련 키</summary>
    public static string KeyFromName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return string.Empty;
        }
        string n = objectName.Trim();
        while (n.EndsWith(CloneSuffix, StringComparison.Ordinal))
        {
            n = n.Substring(0, n.Length - CloneSuffix.Length).TrimEnd();
        }
        return n;
    }
}
