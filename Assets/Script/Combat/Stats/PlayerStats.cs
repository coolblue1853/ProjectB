using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>플레이어 전역 스탯 (PlayerStateDesign 2.5). 퍼센트 포인트 스탯은 Base 0 + Add 만 쓴다</summary>
public enum StatId { AttackSpeed, MoveSpeed, Def, CritRate, CritDmg, BasicDmg, PoisonDmg, IncDmg, IncomingDmg, BleedIncDmg, DropRate, HitCount, MaxHp }

/// <summary>적용 순서 = 정렬 순서: 더하기 → 퍼센트 → 곱하기</summary>
public enum ModType { Add = 100, Percent = 200, Multiply = 300 }

/// <summary>
/// 스탯 하나에 붙는 변경치. 해제 키는 셋 중 하나: Source(장비 인스턴스) · Group(세트 "Set:Rabbit_3") · Id(버프 "Buff:AttSpeed:123").
/// </summary>
public sealed class StatModifier
{
    public readonly StatId Stat;
    public readonly ModType Type;
    public readonly float Value;
    public object Source;
    public string Group;
    public string Id;
    public string Owner; // 디버그 표기

    public StatModifier(StatId stat, ModType type, float value)
    {
        Stat = stat;
        Type = type;
        Value = value;
    }
}

/// <summary>
/// Modifier 기반 스탯 코어 (PlayerStateDesign Phase 9).
/// 값 = (Base + Legacy + ΣAdd) × (1 + ΣPercent/100) × Π(1 + Multiply/100).
/// 기존 static int 필드(DatabaseManager.playerDef 등)는 프로퍼티 facade 로 GetInt/SetLegacy 를 거친다 → `+=`/`-=` 코드가 그대로 동작하고,
/// 이후 Phase 10 에서 출처(장비·세트·버프)를 하나씩 Add/Remove 로 옮긴다.
/// </summary>
public static class PlayerStats
{
    static readonly int StatCount = Enum.GetValues(typeof(StatId)).Length;

    sealed class Stat
    {
        public float Base, Legacy, Value;
        public readonly List<StatModifier> Mods = new List<StatModifier>();
    }

    static Stat[] stats = Create();

    /// <summary>값이 바뀐 스탯 (정수 반올림 값 기준이 아니라 실수 값 기준)</summary>
    public static event Action<StatId> Changed;

    static Stat[] Create()
    {
        var a = new Stat[StatCount];
        for (int i = 0; i < a.Length; i++)
        {
            a[i] = new Stat();
        }
        return a;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetAll()
    {
        stats = Create();
        Changed = null;
    }

    public static float Get(StatId id) => stats[(int)id].Value;

    /// <summary>정수 값: RoundToInt (0.9999 가 0 으로 내려가지 않게)</summary>
    public static int GetInt(StatId id) => Mathf.RoundToInt(stats[(int)id].Value);

    public static void SetBase(StatId id, float value)
    {
        stats[(int)id].Base = value;
        Recalc(id);
    }

    public static void Add(StatModifier mod)
    {
        if (mod == null)
        {
            return;
        }
        Stat s = stats[(int)mod.Stat];
        if (mod.Type != ModType.Add && !Mathf.Approximately(s.Legacy, 0f))
        {
            Debug.LogWarning("[PB][Stats] 레거시 쓰기가 남은 " + mod.Stat + " 에 " + mod.Type + " 모디파이어 — facade 역산이 근사가 된다 (" + mod.Owner + ")");
        }
        s.Mods.Add(mod);
        Recalc(mod.Stat);
    }

    public static int RemoveBySource(object source) => source == null ? 0 : RemoveWhere(m => ReferenceEquals(m.Source, source));

    public static int RemoveByGroup(string group) => string.IsNullOrEmpty(group) ? 0 : RemoveWhere(m => m.Group == group);

    public static int RemoveById(string id) => string.IsNullOrEmpty(id) ? 0 : RemoveWhere(m => m.Id == id);

    static int RemoveWhere(Predicate<StatModifier> match)
    {
        int removed = 0;
        for (int i = 0; i < stats.Length; i++)
        {
            int n = stats[i].Mods.RemoveAll(match);
            if (n > 0)
            {
                removed += n;
                Recalc((StatId)i);
            }
        }
        return removed;
    }

    /// <summary>
    /// 레거시 대입(`DatabaseManager.playerDef = x`, `+=`, `-=`): 결과 정수값이 value 가 되도록 Legacy 덧셈 항을 조정한다.
    /// 퍼센트·곱하기 모디파이어가 있으면 그 배율로 역산한다.
    /// </summary>
    public static void SetLegacy(StatId id, int value)
    {
        Stat s = stats[(int)id];
        float scale = Scale(s);
        float addSum = AddSum(s);
        s.Legacy = Mathf.Approximately(scale, 0f) ? s.Legacy : value / scale - s.Base - addSum;
        Recalc(id);
    }

    static float AddSum(Stat s)
    {
        float sum = 0f;
        foreach (StatModifier m in s.Mods)
        {
            if (m.Type == ModType.Add)
            {
                sum += m.Value;
            }
        }
        return sum;
    }

    static float Scale(Stat s)
    {
        float pct = 0f, mul = 1f;
        foreach (StatModifier m in s.Mods)
        {
            if (m.Type == ModType.Percent)
            {
                pct += m.Value;
            }
            else if (m.Type == ModType.Multiply)
            {
                mul *= 1f + m.Value / 100f;
            }
        }
        return (1f + pct / 100f) * mul;
    }

    static void Recalc(StatId id)
    {
        Stat s = stats[(int)id];
        float v = (s.Base + s.Legacy + AddSum(s)) * Scale(s);
        if (!Mathf.Approximately(v, s.Value))
        {
            s.Value = v;
            Changed?.Invoke(id);
        }
    }

    /// <summary>프로브용: 스탯별 값·레거시·모디파이어 수</summary>
    public static string Dump()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < stats.Length; i++)
        {
            Stat s = stats[i];
            if (Mathf.Approximately(s.Value, 0f) && s.Mods.Count == 0)
            {
                continue;
            }
            sb.Append((StatId)i).Append('=').Append(s.Value.ToString("0.##"));
            sb.Append(" (legacy ").Append(s.Legacy.ToString("0.##")).Append(", mods ").Append(s.Mods.Count).Append(") ");
        }
        return sb.Length == 0 ? "(all zero)" : sb.ToString();
    }
}
