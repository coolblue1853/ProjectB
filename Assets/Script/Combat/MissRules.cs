using UnityEngine;

/// <summary>
/// 미스(회피) 규칙 순수 함수 (CombatSpec.md 4-1절, REWORK-13). 난수는 인자로 받는다(EditMode 테스트 가능).
/// 플레이어 공격: 몹 Lv - 플레이어 Lv >= LevelGapStart 이면 미스. 몹 공격: 플레이어 Lv - 몹 Lv >= LevelGapStart 이면 미스.
/// 보스·몹 Lv 미지정(0)은 레벨 미스 없음. 명중은 내 미스율을, 회피는 몹 공격 미스율을 포인트(%p)로 보정한다.
/// </summary>
public static class MissRules
{
    public const int StepPercent = 6;          // 기준 차이 초과 1당 +6%p (기준 차이 자체가 6%)
    public const int PlayerAttackMissMax = 60; // 내 공격 미스율 상한
    public const int MobAttackMissMax = 50;    // 몹 공격 미스율 상한

    /// <summary>장비·버프의 명중/회피 보너스(%p). 현재 공급원 없음 — 0 (스탯 도입 시 여기에 합산)</summary>
    public static int accuracyBonus;
    public static int evasionBonus;

    private static int Curve(int gap)
    {
        if (gap < DamageCalculator.LevelGapStart)
        {
            return 0;
        }
        return (gap - DamageCalculator.LevelGapStart + 1) * StepPercent;
    }

    /// <summary>내 공격이 빗나갈 확률(0~60 %). 보스·몹 Lv 0 은 0</summary>
    public static int PlayerAttackMissPercent(int monsterLevel, int playerLevel, bool isBoss, int accuracy)
    {
        if (isBoss || monsterLevel <= 0)
        {
            return 0;
        }
        return Mathf.Clamp(Curve(monsterLevel - playerLevel) - accuracy, 0, PlayerAttackMissMax);
    }

    /// <summary>몹 공격이 빗나갈 확률(0~50 %). 보스·몹 Lv 0 은 0(회피로도 못 피한다)</summary>
    public static int MobAttackMissPercent(int monsterLevel, int playerLevel, bool isBoss, int evasion)
    {
        if (isBoss || monsterLevel <= 0)
        {
            return 0;
        }
        return Mathf.Clamp(Curve(playerLevel - monsterLevel) + evasion, 0, MobAttackMissMax);
    }

    /// <param name="roll">0~100 난수. roll &lt; percent 이면 미스</param>
    public static bool IsMiss(int percent, float roll) => percent > 0 && roll < percent;

    public static bool PlayerAttackMisses(int monsterLevel, bool isBoss) =>
        IsMiss(PlayerAttackMissPercent(monsterLevel, LevelSystem.Level, isBoss, accuracyBonus), Random.Range(0f, 100f));

    public static bool MobAttackMisses(int monsterLevel, bool isBoss) =>
        IsMiss(MobAttackMissPercent(monsterLevel, LevelSystem.Level, isBoss, evasionBonus), Random.Range(0f, 100f));
}
