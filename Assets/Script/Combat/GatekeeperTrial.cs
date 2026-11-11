using UnityEngine;

/// <summary>
/// 졸업 시험(탑 문지기) 소프트 게이트 규칙 (Floor1Completion 14-1). 순수 함수라 EditMode 테스트로 고정한다.
/// 달성도 a(0~1) = 인장 .4 + 세트 .3 + 도감 .3. 문지기 HP ×(1+0.5(1-a)), 피해 ×(1+0.2(1-a)).
/// 입장(보스 스폰) 시점의 상태로 한 번 계산한다. 수치는 초안.
/// </summary>
public static class GatekeeperTrial
{
    public const string BossName = "Gatekeeper_BD";
    public const int SealGoal = 3, SetGoal = 3, BestiaryGoal = 15, SetMinPieces = 3;
    public const float SealWeight = 0.4f, SetWeight = 0.3f, BestiaryWeight = 0.3f;
    public const float HpBonus = 0.5f, DamageBonus = 0.2f;
    /// <summary>달성도 a=1(완전 준비) 보상: 처치 때 보너스 강화석(Q 11-11, Floor1Completion 14-1)</summary>
    public const int FullReadinessBonusStones = 2;
    const float FullReadiness = 0.999f;
    /// <summary>마지막 문지기 스폰 시 달성도(처치 보상 계산이 읽는다). 스폰 전 -1</summary>
    public static float LastSpawnReadiness = -1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        LastSpawnReadiness = -1f;
    }

    /// <summary>이 보스 처치 때 더할 보너스 강화석 수(문지기이고 스폰 때 a=1 이었을 때만)</summary>
    public static int BonusStones(string bossName)
    {
        return IsGatekeeper(bossName) && LastSpawnReadiness >= FullReadiness ? FullReadinessBonusStones : 0;
    }

    /// <summary>입장 알림 한 줄: a&lt;1 이면 "시험 달성도 N% — 문지기가 강해졌다", a=1 이면 보너스 안내</summary>
    public static string EntryNotice(float readiness)
    {
        int pct = Mathf.RoundToInt(Mathf.Clamp01(readiness) * 100f);
        return readiness >= FullReadiness
            ? "시험 달성도 100% — 처치하면 강화석 +" + FullReadinessBonusStones
            : "시험 달성도 " + pct + "% — 문지기가 강해졌다";
    }

    static readonly string[] SealIds = { "ZoneSeal_Swamp", "ZoneSeal_Fortress", "ZoneSeal_Tower" };

    /// <summary>seals = 보유 인장 종류 수, sets = 3부위 이상 장착한 세트 종류 수, kinds = 도감 처치 종류 수. 범위 밖 값은 자른다.</summary>
    public static float Readiness(int seals, int sets, int kinds)
    {
        float s = Mathf.Clamp(seals, 0, SealGoal) / (float)SealGoal;
        float t = Mathf.Clamp(sets, 0, SetGoal) / (float)SetGoal;
        float k = Mathf.Clamp(kinds, 0, BestiaryGoal) / (float)BestiaryGoal;
        return s * SealWeight + t * SetWeight + k * BestiaryWeight;
    }

    public static float HpMultiplier(float readiness)
    {
        return 1f + HpBonus * (1f - Mathf.Clamp01(readiness));
    }

    public static float DamageMultiplier(float readiness)
    {
        return 1f + DamageBonus * (1f - Mathf.Clamp01(readiness));
    }

    /// <summary>처치 시간 비율 a=0 대비 a=1. 보스 DPS 가 일정하면 HP 배수와 같다(피해 계수는 플레이어 생존에만 영향).</summary>
    public static float KillTimeRatio(float readinessLow, float readinessHigh)
    {
        return HpMultiplier(readinessLow) / HpMultiplier(readinessHigh);
    }

    public static int SetKinds(System.Collections.Generic.IDictionary<string, int> equippedPieces)
    {
        int n = 0;
        if (equippedPieces != null)
        {
            foreach (var kv in equippedPieces)
            {
                if (!string.IsNullOrEmpty(kv.Key) && kv.Value >= SetMinPieces)
                {
                    n++;
                }
            }
        }
        return n;
    }

    /// <summary>현재 게임 상태에서 달성도를 읽는다(런타임 전용).</summary>
    public static float CurrentReadiness()
    {
        int seals = 0;
        InventoryManager inv = InventoryManager.instance;
        if (inv != null)
        {
            foreach (string id in SealIds)
            {
                if (inv.CountItem(id) > 0)
                {
                    seals++;
                }
            }
        }
        return Readiness(seals, SetKinds(DatabaseManager.setsEffectStack), BestiaryRecord.KindsKilled);
    }

    public static bool IsGatekeeper(string objectName)
    {
        return !string.IsNullOrEmpty(objectName) && objectName.StartsWith(BossName);
    }
}
