using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 소비 아이템 공유 쿨다운(디아블로식 물약 쿨, BasicFeatures.md 3절·Q-430).
/// 효과에 hp 가 있으면 "potion" 그룹(6초), 그 외는 "misc" 그룹(1초, 연타 낭비 방지). 같은 그룹끼리 쿨을 공유한다.
/// 퀵슬롯과 인벤 소비 키가 같은 인스턴스(Shared)를 써서 우회할 수 없다. 시간은 호출자가 넘긴다(테스트 가능).
/// </summary>
public sealed class ConsumeCooldown
{
    public const string PotionGroup = "potion";
    public const string MiscGroup = "misc";
    public const float PotionSeconds = 6f;
    public const float MiscSeconds = 1f;

    private readonly Dictionary<string, float> _readyAt = new Dictionary<string, float>();

    public static ConsumeCooldown Shared { get; private set; } = new ConsumeCooldown();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Shared = new ConsumeCooldown();
    }

    /// <summary>ItemSheet effectOb("hp +", "fullness +/def +" …) → 쿨다운 그룹</summary>
    public static string GroupOf(string effectOb)
    {
        if (string.IsNullOrEmpty(effectOb))
        {
            return MiscGroup;
        }
        foreach (string part in effectOb.Split('/'))
        {
            string trimmed = part.Trim();
            int space = trimmed.IndexOf(' ');
            string stat = space >= 0 ? trimmed.Substring(0, space) : trimmed;
            if (stat == "hp")
            {
                return PotionGroup;
            }
        }
        return MiscGroup;
    }

    public static float DurationOf(string group) => group == PotionGroup ? PotionSeconds : MiscSeconds;

    public bool IsReady(string group, float now) => Remaining(group, now) <= 0f;

    public float Remaining(string group, float now)
    {
        if (group == null || !_readyAt.TryGetValue(group, out float t))
        {
            return 0f;
        }
        return Mathf.Max(0f, t - now);
    }

    /// <summary>0~1, 막 쓴 직후 1</summary>
    public float Fraction(string group, float now)
    {
        float d = DurationOf(group);
        return d > 0f ? Mathf.Clamp01(Remaining(group, now) / d) : 0f;
    }

    public void Start(string group, float now)
    {
        if (group != null)
        {
            _readyAt[group] = now + DurationOf(group);
        }
    }
}
