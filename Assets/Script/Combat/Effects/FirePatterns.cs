using System;
using System.Globalization;
using UnityEngine;

/// <summary>탄 한 발의 생성 위치 오프셋·방향을 정하는 전략 (EquipEffectDesign §2.3 FireMode, P3)</summary>
public interface IFirePattern
{
    /// <summary>실제로 쏠 탄 수 (baseCount = 탄 수 효과 반영 후 기본 수)</summary>
    int Count(int baseCount);

    /// <summary>i 번째 탄의 피벗 기준 오프셋과 진행 방향(up). count 는 Count() 의 결과</summary>
    void Get(int i, int count, out Vector2 offset, out Vector2 dir);
}

/// <summary>기존 Skill.RoundSpawn 의 부채꼴 배치를 그대로 옮긴 기본 전략. FireMode 효과가 없으면 항상 이것을 쓴다</summary>
public sealed class DefaultFirePattern : IFirePattern
{
    readonly float _start;
    readonly float _end;
    readonly float _radius;
    readonly bool _faceLeft;

    public DefaultFirePattern(float startAngle, float endAngle, float radius, bool faceLeft)
    {
        _start = startAngle;
        _end = endAngle;
        _radius = radius;
        _faceLeft = faceLeft;
    }

    public int Count(int baseCount)
    {
        return baseCount;
    }

    public void Get(int i, int count, out Vector2 offset, out Vector2 dir)
    {
        float angleStep = count > 1 ? (_end - _start) / (count - 1) : 0f; // 1발이면 0 (W-24 NaN 방지)
        float angle = _start + i * angleStep;
        if (_faceLeft)
        {
            angle = 180f - angle;
        }
        float rad = angle * Mathf.Deg2Rad;
        offset = new Vector2(_radius * Mathf.Cos(rad), _radius * Mathf.Sin(rad));
        dir = offset.normalized;
    }
}

/// <summary>FireMode Rule 전략: Fan(p2=전체 각도)·Ring(전방위)·Parallel(p2=간격)·Backward(뒤로 같이)</summary>
public sealed class ModeFirePattern : IFirePattern
{
    public enum Mode
    {
        Fan,
        Ring,
        Parallel,
        Backward,
    }

    readonly Mode _mode;
    readonly float _p2;
    readonly float _start;
    readonly float _end;
    readonly float _radius;
    readonly bool _faceLeft;

    public ModeFirePattern(Mode mode, float p2, float startAngle, float endAngle, float radius, bool faceLeft)
    {
        _mode = mode;
        _p2 = p2;
        _start = startAngle;
        _end = endAngle;
        _radius = radius;
        _faceLeft = faceLeft;
    }

    public int Count(int baseCount)
    {
        return _mode == Mode.Backward ? baseCount * 2 : baseCount;
    }

    public void Get(int i, int count, out Vector2 offset, out Vector2 dir)
    {
        float center = (_start + _end) * 0.5f;
        float angle;
        float lateral = 0f; // Parallel 전용 옆 간격
        switch (_mode)
        {
            case Mode.Fan:
            {
                float arc = _p2 > 0f ? _p2 : Mathf.Abs(_end - _start);
                angle = count > 1 ? center - arc * 0.5f + arc * i / (count - 1) : center;
                break;
            }
            case Mode.Ring:
                angle = _start + 360f * i / Mathf.Max(1, count);
                break;
            case Mode.Parallel:
                angle = center;
                lateral = (i - (count - 1) * 0.5f) * (_p2 > 0f ? _p2 : 0.5f);
                break;
            default: // Backward: 앞쪽 절반은 기본 부채꼴, 뒤쪽 절반은 좌우 반전
            {
                int half = Mathf.Max(1, count / 2);
                int k = i % half;
                float a = half > 1 ? _start + k * (_end - _start) / (half - 1) : center;
                angle = i < half ? a : 180f - a;
                break;
            }
        }
        if (_faceLeft)
        {
            angle = 180f - angle;
        }
        float rad = angle * Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        offset = dir * _radius;
        if (lateral != 0f)
        {
            offset += new Vector2(-dir.y, dir.x) * lateral;
        }
    }
}

public static class FirePatterns
{
    /// <summary>mods.rules 의 FireMode 행으로 전략을 만든다. 없거나 모드 이름이 틀리면 기본 전략(기존 동작)</summary>
    public static IFirePattern Create(in SkillMods mods, float startAngle, float endAngle, float radius, bool faceLeft)
    {
        EffectEntry[] rules = mods.rules;
        if (rules != null)
        {
            for (int i = 0; i < rules.Length; i++)
            {
                EffectEntry e = rules[i];
                if (e.rule != EffectRule.FireMode)
                {
                    continue;
                }
                if (Enum.TryParse(e.p1, true, out ModeFirePattern.Mode mode) && Enum.IsDefined(typeof(ModeFirePattern.Mode), mode))
                {
                    float p2 = 0f;
                    float.TryParse(e.p2, NumberStyles.Float, CultureInfo.InvariantCulture, out p2);
                    return new ModeFirePattern(mode, p2, startAngle, endAngle, radius, faceLeft);
                }
            }
        }
        return new DefaultFirePattern(startAngle, endAngle, radius, faceLeft);
    }
}
