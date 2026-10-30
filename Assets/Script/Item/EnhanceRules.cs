/// <summary>
/// 장비 강화 +0~+10 규칙 (ProgressionDesign 3절). 표·확률·보너스만 담은 순수 클래스 — 난수는 주입받는다(테스트 가능).
/// </summary>
public static class EnhanceRules
{
    public const int MaxLevel = 10;
    public const string StoneName = "Enhance Stone"; // 아이템 키 (세이브 호환). 표시 이름은 "Floor Crystal"(층계 결정)
    public const string StoneDisplay = "Floor Crystal";
    public const int PityFails = 2; // 이만큼 연속 실패하면 다음은 확정 성공

    // 인덱스 = 현재 단계(0→1 시도는 [0])
    static readonly float[] SuccessRate = { 1f, 0.95f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.35f, 0.3f };
    static readonly int[] StoneCost = { 1, 1, 2, 2, 3, 3, 4, 5, 6, 8 };
    static readonly int[] GoldCost = { 50, 100, 150, 250, 400, 600, 900, 1300, 1800, 2500 };
    const int DropOnFailFrom = 5; // 현재 +5 이상(→+6 시도부터) 실패 시 1단계 하락

    public enum Result { Success, Fail, FailDown, MaxLevel }

    public static bool CanEnhance(int level) => level >= 0 && level < MaxLevel;
    public static float Rate(int level) => CanEnhance(level) ? SuccessRate[level] : 0f;
    public static int Stones(int level) => CanEnhance(level) ? StoneCost[level] : 0;
    public static int Gold(int level) => CanEnhance(level) ? GoldCost[level] : 0;
    public static bool FailDrops(int level) => level >= DropOnFailFrom;

    /// <param name="roll">0~1 난수</param>
    /// <param name="failStreak">연속 실패 수 (PityFails 이상이면 확정 성공)</param>
    public static Result Roll(int level, float roll, int failStreak, out int newLevel)
    {
        newLevel = level;
        if (!CanEnhance(level))
        {
            return Result.MaxLevel;
        }
        if (failStreak >= PityFails || roll < SuccessRate[level])
        {
            newLevel = level + 1;
            return Result.Success;
        }
        if (FailDrops(level))
        {
            newLevel = level - 1;
            return Result.FailDown;
        }
        return Result.Fail;
    }

    /// <summary>무기 데미지 배율: 단계당 +4%, +5·+10 달성 시 각각 +10% (최대 1.6)</summary>
    public static float WeaponMultiplier(int level)
    {
        float b = Bonus(level, 0.04f);
        if (level >= AdvancedEnhanceRules.MaxLevel)
        {
            b += 0.1f; // +15 달성 추가 보너스(무기만, FloorCoreContent 4.1)
        }
        return 1f + b;
    }

    /// <summary>방어구·장신구 수치 배율: 단계당 +5%, +5·+10 달성 시 각각 +10% (최대 1.7)</summary>
    public static float GearMultiplier(int level)
    {
        return 1f + Bonus(level, 0.05f);
    }

    static float Bonus(int level, float perLevel)
    {
        if (level <= 0)
        {
            return 0f;
        }
        if (level > AdvancedEnhanceRules.MaxLevel)
        {
            level = AdvancedEnhanceRules.MaxLevel; // +11~+15 도 단계당 같은 비율로 이어진다
        }
        float b = level * perLevel;
        if (level >= 5)
        {
            b += 0.1f;
        }
        if (level >= 10)
        {
            b += 0.1f;
        }
        return b;
    }

    /// <summary>표시 이름: +3 CliffBlade</summary>
    public static string DisplayName(string name, int level)
    {
        return level > 0 ? "+" + level + " " + name : name;
    }
}
