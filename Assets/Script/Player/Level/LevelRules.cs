using UnityEngine;

/// <summary>
/// 레벨·EXP 순수 규칙 (ClassicMapleDesign.md 3.1·8절). 상태 없음 → EditMode 테스트 가능.
/// 필요 EXP: Lv1~15 는 8절 표, Lv16 이상은 임시 외삽(Lv15 의 증가율 1.195 에서 Lv69 의 1.05 까지 선형 감소).
/// </summary>
public static class LevelRules
{
    public const int MaxLevel = 70;
    public const int HpPerLevel = 8;
    public const int DeathExpLossPercent = 5;
    public const int EliteExpMul = 4;

    // 다음 레벨까지 필요한 EXP, 인덱스 = 현재 레벨 - 1 (Lv1~15, 8절)
    static readonly int[] Table =
    {
        500, 650, 800, 1200, 1450, 2900, 3500, 4200, 5000, 6000, 11500, 14000, 17000, 20500, 24500,
    };

    static int[] cache;

    /// <summary>현재 레벨 → 다음 레벨까지 필요한 EXP. 만렙(70)이면 0</summary>
    public static int ExpToNext(int level)
    {
        if (level < 1)
        {
            level = 1;
        }
        if (level >= MaxLevel)
        {
            return 0;
        }
        if (cache == null)
        {
            Build();
        }
        return cache[level - 1];
    }

    static void Build()
    {
        var a = new int[MaxLevel - 1];
        double v = Table[Table.Length - 1];
        for (int i = 0; i < a.Length; i++)
        {
            if (i < Table.Length)
            {
                a[i] = Table[i];
                continue;
            }
            int level = i + 1; // 현재 레벨
            double t = Mathf.Clamp01((level - 16f) / (MaxLevel - 1 - 16f));
            double rate = 1.195 + (1.05 - 1.195) * t;
            v = v * rate;
            a[i] = (int)System.Math.Round(v);
        }
        cache = a;
    }

    /// <summary>몹 처치 EXP: 일반 = 표 값, 정예 승격 x4, 보스 고정(정예 승격 없음)</summary>
    public static int KillExp(int baseExp, bool isElite, bool isBoss)
    {
        if (baseExp <= 0)
        {
            return 0;
        }
        return isElite && !isBoss ? baseExp * EliteExpMul : baseExp;
    }

    /// <summary>EXP 를 더한다. 여러 레벨이 한 번에 오를 수 있고 만렙에서는 EXP 가 0 으로 고정. 오른 레벨 수 반환</summary>
    public static int AddExp(ref int level, ref int exp, long gain)
    {
        level = Mathf.Clamp(level, 1, MaxLevel);
        if (level >= MaxLevel)
        {
            exp = 0;
            return 0;
        }
        long total = exp + (gain > 0 ? gain : 0);
        int ups = 0;
        while (level < MaxLevel)
        {
            int need = ExpToNext(level);
            if (total < need)
            {
                break;
            }
            total -= need;
            level++;
            ups++;
        }
        exp = level >= MaxLevel ? 0 : (int)total;
        return ups;
    }

    /// <summary>사망 페널티: 현재 레벨에서 쌓은 EXP 의 5% 감소(내림 후 차감), 레벨 다운 없음</summary>
    public static int ExpAfterDeath(int exp)
    {
        if (exp <= 0)
        {
            return 0;
        }
        return exp - exp * DeathExpLossPercent / 100;
    }

    /// <summary>최대 HP 성장분 (기본값 위에 더하는 증가분)</summary>
    public static int HpBonus(int level) => (Mathf.Clamp(level, 1, MaxLevel) - 1) * HpPerLevel;

    /// <summary>공격 성장분: 레벨당 +0.5 를 정수로 쌓는다 (2레벨마다 +1, 반올림 오차 방지)</summary>
    public static int AtkBonus(int level) => (Mathf.Clamp(level, 1, MaxLevel) - 1) / 2;

    /// <summary>방어 성장분: 공격과 같다 (레벨당 +0.5)</summary>
    public static int DefBonus(int level) => AtkBonus(level);
}
