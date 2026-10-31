using UnityEngine;

/// <summary>
/// 플레이어 → 몬스터 데미지 공식 (스펙: .claude-rules/Game/CombatSpec.md 4절).
/// 난수는 인자로 받아 순수 함수로 유지한다(EditMode 테스트 가능).
/// </summary>
public static class DamageCalculator
{
    /// <summary>기본 치명타 피해 % (치명타 배율 = 1 + (20 + 치피%) / 100)</summary>
    public const int BaseCritDamagePercent = 20;

    // 무기 damage 배열 인덱스
    public const int MinDmg = 0, MaxDmg = 1, CritRate = 2, CritDmg = 3, IncDmg = 4,
                     IgnoreDef = 5, SkillDmg = 6, FinalDmg = 7, AddDmg = 8, ArmorAtp = 9;

    /// <summary>장비·버프에서 오는 플레이어 공격 보정 (DatabaseManager의 static 값 스냅샷)</summary>
    public struct PlayerOffense
    {
        public int addBasicDmg;
        public int critRate;
        public int critDmg;
        public int incDmg;
        public int bleedingAddDmg;

        public static PlayerOffense FromDatabase() => new PlayerOffense
        {
            addBasicDmg = DatabaseManager.addbasicDmg,
            critRate = DatabaseManager.playerCritRate,
            critDmg = DatabaseManager.playerCritDmgRate,
            incDmg = DatabaseManager.incDmg,
            bleedingAddDmg = DatabaseManager.bleedingAddDmg,
        };
    }

    public struct Result
    {
        public int damage;      // 최종 피해
        public int extraDamage; // 추가타 (별도 숫자)
        public bool isCrit;
    }

    /// <summary>레벨 차 감소가 시작되는 몹-플레이어 레벨 차 (+4부터)</summary>
    public const int LevelGapStart = 4;
    public const int LevelGapPercentPerStep = 3;
    public const int LevelGapMaxPercent = 30;

    /// <summary>
    /// 플레이어가 주는 피해 배율(클래식 메이플). 몹 레벨 - 플레이어 레벨 >= 4 이면 초과 1당 -3%, 최대 -30%.
    /// 몹 레벨이 0(미지정)이면 1. MISS 없음. (ClassicMapleDesign.md 3.3)
    /// </summary>
    public static float LevelGapMul(int monsterLevel, int playerLevel)
    {
        if (monsterLevel <= 0)
        {
            return 1f;
        }
        int gap = monsterLevel - playerLevel;
        if (gap < LevelGapStart)
        {
            return 1f;
        }
        int percent = Mathf.Min(LevelGapMaxPercent, (gap - LevelGapStart + 1) * LevelGapPercentPerStep);
        return 1f - percent / 100f;
    }

    /// <param name="weapon">무기 damage 배열 (길이 10, 부족한 값은 0으로 본다)</param>
    /// <param name="baseRoll">0~1 난수: 최소~최대 피해 사이 위치 (양 끝 포함)</param>
    /// <param name="critRoll">0~100 난수: 치명타 확률과 비교</param>
    public static Result Calculate(int[] weapon, PlayerOffense player, int enemyDef, bool targetBleeding,
                                   bool isSkill, float dmgRatio, float baseRoll, float critRoll, float levelGapMul = 1f)
    {
        var result = new Result();
        if (weapon == null || weapon.Length < 2)
        {
            return result;
        }

        float min = weapon[MinDmg] + player.addBasicDmg;
        float max = Mathf.Max(min, weapon[MaxDmg] + player.addBasicDmg);
        float dmg = Mathf.Lerp(min, max, Mathf.Clamp01(baseRoll)) + Get(weapon, ArmorAtp);

        // 치명타
        result.isCrit = critRoll < Get(weapon, CritRate) + player.critRate;
        if (result.isCrit)
        {
            dmg *= 1f + (BaseCritDamagePercent + Get(weapon, CritDmg) + player.critDmg) / 100f;
        }

        // 피해 증가 (무기 + 장비 + 출혈 대상)
        int inc = Get(weapon, IncDmg) + player.incDmg + (targetBleeding ? player.bleedingAddDmg : 0);
        dmg *= 1f + inc / 100f;

        if (isSkill)
        {
            dmg *= 1f + Get(weapon, SkillDmg) / 100f;
        }

        // 방어: dmg² / (dmg + 실효 방어), 방무%만큼 방어를 줄인다
        float effectiveDef = Mathf.Max(0f, enemyDef * (1f - Mathf.Clamp01(Get(weapon, IgnoreDef) / 100f)));
        if (dmg > 0f && effectiveDef > 0f)
        {
            dmg = dmg * dmg / (dmg + effectiveDef);
        }

        float final = dmg * (1f + Get(weapon, FinalDmg) / 100f) * dmgRatio * levelGapMul;
        result.damage = final > 0f ? Mathf.Max(1, Mathf.RoundToInt(final)) : 0;
        result.extraDamage = Mathf.RoundToInt(final * Get(weapon, AddDmg) / 100f);
        return result;
    }

    /// <summary>몬스터 → 플레이어 받는 피해: floor(d² / (d + Def)), 받피증 반영</summary>
    public static int IncomingToPlayer(int rawDamage, int playerDef, float incomingMul)
    {
        float d = rawDamage * incomingMul;
        if (d <= 0f)
        {
            return 0;
        }
        return Mathf.FloorToInt(d * d / (d + Mathf.Max(0, playerDef)));
    }

    private static int Get(int[] arr, int index) => index < arr.Length ? arr[index] : 0;
}
