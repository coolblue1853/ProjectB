using System;
using System.Collections.Generic;

/// <summary>흔적 신선도(선명 → 흐림 → 오래됨)</summary>
public enum Freshness { Fresh, Faint, Old }

/// <summary>덫 품질(보통 / 좋음 / 완벽)</summary>
public enum TrapQuality { Normal, Good, Perfect }

/// <summary>한 방향의 흔적(종류 Loc 키 + 신선도)</summary>
public readonly struct TraceOption
{
    public readonly string Trace;
    public readonly Freshness Fresh;

    public TraceOption(string trace, Freshness fresh)
    {
        Trace = trace;
        Fresh = fresh;
    }
}

/// <summary>드롭표 한 줄(이름·확률·ItemSheet type). 계보 재료·무기 고르기 입력</summary>
public readonly struct DropRow
{
    public readonly string Name;
    public readonly float Chance;
    public readonly string Type;

    public DropRow(string name, float chance, string type)
    {
        Name = name;
        Chance = chance;
        Type = type;
    }
}

/// <summary>
/// ⑥ 몬스터 흔적 추적 순수 규칙·수치(MiniGames.md "몬스터 흔적 추적 구현"). 출처: GPT 기획 대화(10-30) 수치표.
/// 흔적 판별 4단계(← ↑ → 중 목표 몹의 가장 신선한 흔적 고르기) → 덫 설치 타이밍 1회 → 목표 몹 정예 확정 등장.
/// 난수는 호출자가 0~1 값으로 넘긴다(테스트 가능).
/// </summary>
public static class TrackingRules
{
    public const int Stages = 4;
    public const int Options = 3;              // ← ↑ →
    public const int MistakeLimit = 3;         // 3번째 오답에 실패(2회까지 허용)
    public const float TimeLimit = 45f;        // 흔적 판별 전체(덫 단계는 따로)
    public const float ChoiceDelay = 0.25f;    // 시작한 ↑ 가 '위' 선택으로 새지 않게
    public const float TrapSpeed = 120f;       // 커서 왕복 0~100, 초당 120(주기 약 1.67초)
    public const float TrapPerfect = 6f;       // 중앙 ±6
    public const float TrapGood = 18f;         // 중앙 ±18(완벽 포함)
    public const float TrapTimeout = 5f;       // 안 누르면 보통
    public const float SuccessCooldown = 600f; // 지점별 10분
    public const float FailCooldown = 180f;    // 실패·중단 3분
    public const float TargetLifetime = 120f;  // 소환 정예가 안 잡히면 사라지는 시간
    public const float WeaponBonusChance = 0.01f; // 계보 무기 추가 독립 판정(+1%p)
    public const string DefaultTrace = "Strange tracks";

    /// <summary>프리팹 이름(_BD 뗀 것) → 흔적 종류(Loc 키). 없는 몹은 DefaultTrace</summary>
    private static readonly Dictionary<string, string> Traces = new Dictionary<string, string>
    {
        { "ForestWolf", "Wolf paw prints" }, { "WildBoar", "Tusk-dug soil" }, { "ForestSnail", "Shell slime trail" },
        { "Wisp", "Scorched embers" }, { "SporeShroom", "Spore patches" }, { "ForestBear", "Deep claw marks" },
        { "Ent", "Dragged root furrows" }, { "CliffGoblin", "Small boot prints" }, { "Ghost", "Frost marks" },
        { "Fairy", "Glittering dust" }, { "BogCroc", "Muddy tail drag" }, { "SwampFrog", "Slime puddles" },
        { "MarshCrab", "Pincer scratches" }, { "BogSlime", "Gel trail" }, { "SwampMosquito", "Blood drops" },
        { "BanditCutthroat", "Boot prints and torch ash" }, { "BanditArcher", "Boot prints and torch ash" },
        { "BanditShieldman", "Boot prints and torch ash" }, { "BanditTorchbearer", "Boot prints and torch ash" },
        { "GolemSmall", "Cracked stone dust" }, { "GolemMedium", "Cracked stone dust" }, { "RuinSpirit", "Drifting ruin motes" },
        { "Rabbit", "Hop prints" }, { "StoneRabbit", "Hop prints" }, { "LeftMole", "Burrow mounds" }, { "FlowerMole", "Burrow mounds" },
        { "RatThief", "Gnawed scraps" }, { "RatChief", "Gnawed scraps" }, { "Scarecrow", "Straw scraps" }, { "CreepingTerror", "Black ooze" },
    };

    private static string[] _distinct;

    /// <summary>흔적 종류 전체(중복 없음, 미끼 흔적 후보)</summary>
    public static string[] AllTraces
    {
        get
        {
            if (_distinct == null)
            {
                var set = new List<string>();
                foreach (string t in Traces.Values)
                {
                    if (!set.Contains(t))
                    {
                        set.Add(t);
                    }
                }
                _distinct = set.ToArray();
            }
            return _distinct;
        }
    }

    public static string TraceFor(string prefabName)
    {
        string key = prefabName.Replace("(Clone)", "").Trim();
        if (key.EndsWith("_BD"))
        {
            key = key.Substring(0, key.Length - 3);
        }
        return Traces.TryGetValue(key, out string trace) ? trace : DefaultTrace;
    }

    /// <summary>
    /// 한 단계 선택지: 정답 = 목표 흔적(선명 또는 흐림), 미끼 = 더 오래된 목표 흔적 1 + 다른 몹의 선명한 흔적 1. 자리는 섞는다.
    /// </summary>
    public static TraceOption[] MakeStage(string target, Func<float> random, out int correct)
    {
        Freshness right = random() < 0.5f ? Freshness.Fresh : Freshness.Faint;
        Freshness stale = right == Freshness.Fresh && random() < 0.5f ? Freshness.Faint : Freshness.Old;
        string other = PickOtherTrace(target, random());
        var options = new TraceOption[Options];
        correct = Pick(Options, random());
        int staleAt = (correct + 1 + Pick(2, random())) % Options;
        for (int i = 0; i < Options; i++)
        {
            if (i == correct)
            {
                options[i] = new TraceOption(target, right);
            }
            else if (i == staleAt)
            {
                options[i] = new TraceOption(target, stale);
            }
            else
            {
                options[i] = new TraceOption(other, Freshness.Fresh);
            }
        }
        return options;
    }

    /// <summary>목표와 다른 흔적 하나</summary>
    public static string PickOtherTrace(string target, float roll01)
    {
        string[] all = AllTraces;
        int start = Pick(all.Length, roll01);
        for (int k = 0; k < all.Length; k++)
        {
            string t = all[(start + k) % all.Length];
            if (t != target)
            {
                return t;
            }
        }
        return DefaultTrace;
    }

    /// <summary>덫 커서(0~100 왕복)</summary>
    public static float TrapCursor(float elapsed)
    {
        float d = elapsed * TrapSpeed % 200f;
        return d <= 100f ? d : 200f - d;
    }

    public static TrapQuality JudgeTrap(float cursor)
    {
        float off = Math.Abs(cursor - 50f);
        if (off <= TrapPerfect)
        {
            return TrapQuality.Perfect;
        }
        return off <= TrapGood ? TrapQuality.Good : TrapQuality.Normal;
    }

    /// <summary>덫 품질별 계보 재료 확정 개수(보통 1 / 좋음 2 / 완벽 3)</summary>
    public static int BonusCount(TrapQuality q)
    {
        return (int)q + 1;
    }

    /// <summary>덫 품질별 소환 직후 묶는 시간(보통 0 / 좋음 2초 / 완벽 3초)</summary>
    public static float StunSeconds(TrapQuality q)
    {
        switch (q)
        {
            case TrapQuality.Perfect:
                return 3f;
            case TrapQuality.Good:
                return 2f;
            default:
                return 0f;
        }
    }

    /// <summary>계보 재료 = 드롭표에서 확률이 가장 높은 Misc(곰 → Bear Pelt). 없으면 null</summary>
    public static string LineageMaterial(IList<DropRow> drops)
    {
        return Best(drops, "Misc");
    }

    /// <summary>계보 무기 = 드롭표에서 확률이 가장 높은 Equip(무기 2% > 방어구 0.3%). 없으면 null</summary>
    public static string LineageWeapon(IList<DropRow> drops)
    {
        return Best(drops, "Equip");
    }

    /// <summary>처치 보너스: 계보 재료 n개 + 1% 판정에 맞으면 계보 무기 1</summary>
    public static List<string> BonusLoot(IList<DropRow> drops, TrapQuality q, float weaponRoll01)
    {
        var loot = new List<string>();
        string mat = LineageMaterial(drops);
        if (mat != null)
        {
            for (int i = 0; i < BonusCount(q); i++)
            {
                loot.Add(mat);
            }
        }
        string weapon = LineageWeapon(drops);
        if (weapon != null && weaponRoll01 < WeaponBonusChance)
        {
            loot.Add(weapon);
        }
        return loot;
    }

    public static int Pick(int n, float roll01)
    {
        float v = roll01 < 0f ? 0f : (roll01 > 0.999999f ? 0.999999f : roll01);
        return Math.Min(n - 1, (int)(v * n));
    }

    private static string Best(IList<DropRow> drops, string type)
    {
        string best = null;
        float chance = -1f;
        foreach (DropRow d in drops)
        {
            if (d.Type == type && d.Chance > chance)
            {
                best = d.Name;
                chance = d.Chance;
            }
        }
        return best;
    }
}
