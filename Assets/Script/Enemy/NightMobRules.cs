using System.Globalization;
using UnityEngine;

/// <summary>
/// 밤 스폰 몹 배율(PlayBalance_1115 총괄 결정): HP, 공격(접촉·스킬 피해), EXP.
/// 값은 Resources/NightMobMultipliers.csv 한 줄(hpMul,atkMul,expMul). 스폰 때 한 번 적용하고 이미 살아 있는 몹은 건드리지 않는다.
/// 정예·보스 배율과는 곱으로 쌓인다. 순수 계산은 이 클래스, 적용은 EnemyHealth.ApplyNightMultipliers.
/// </summary>
public static class NightMobRules
{
    public readonly struct Mul
    {
        public readonly float Hp, Atk, Exp;

        public Mul(float hp, float atk, float exp)
        {
            Hp = hp;
            Atk = atk;
            Exp = exp;
        }
    }

    public static readonly Mul Identity = new Mul(1f, 1f, 1f);
    /// <summary>CSV 가 없거나 깨졌을 때의 기본값(11-15 총괄 결정)</summary>
    public static readonly Mul Default = new Mul(1.4f, 1.3f, 1.15f);

    private static Mul? s_cached;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_cached = null;
    }

    public static Mul Current
    {
        get
        {
            if (!s_cached.HasValue)
            {
                TextAsset csv = Resources.Load<TextAsset>("NightMobMultipliers");
                s_cached = Parse(csv != null ? csv.text : "");
            }
            return s_cached.Value;
        }
    }

    /// <summary>헤더 아래 첫 유효 행을 읽는다. 값이 0 이하·NaN 이면 그 칸만 기본값. 행이 없으면 Default</summary>
    public static Mul Parse(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Default;
        }
        string[] lines = text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 3)
            {
                continue;
            }
            return new Mul(Field(c[0], Default.Hp), Field(c[1], Default.Atk), Field(c[2], Default.Exp));
        }
        return Default;
    }

    private static float Field(string s, float fallback)
    {
        if (float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v)
            && !float.IsNaN(v) && !float.IsInfinity(v) && v > 0f)
        {
            return v;
        }
        return fallback;
    }

    /// <summary>정수 값에 배율을 곱해 반올림. 원래 값이 0 이하면 그대로(0 EXP 몹이 생기지 않게 최소 1 보장은 하지 않는다)</summary>
    public static int Scale(int value, float mul)
    {
        return value <= 0 ? value : Mathf.Max(1, Mathf.RoundToInt(value * mul));
    }

    /// <summary>밤이면 배율, 낮이면 1</summary>
    public static Mul For(bool night)
    {
        return night ? Current : Identity;
    }
}
