using UnityEngine;

/// <summary>
/// 몹 런타임 디버프 순수 규칙(Floor2Plan.md §5.5 #3·#4·#12). 값은 임시 가정(OpenQuestions Q-AUTO12).
/// 둔화: 시간제한 이동 배율(가장 강한 값·가장 긴 시간 유지). 석화: 경직 연쇄 상한(끝난 뒤 재석화 면역 N초, 저항 반영).
/// 화상: 독(PosionAttack) 틱 재사용 — 피해/간격/횟수만 계산.
/// </summary>
public sealed class MobDebuffRules
{
    public const float PetrifyImmuneSeconds = 3f;
    public const float BurnInterval = 1f;
    public const int BurnCount = 4;
    public const float BurnRatio = 0.15f;

    private float _slowMul = 1f;
    private CountdownTimer _slow;
    private float _petrifyImmuneUntil = -999f;

    /// <summary>지금 곱할 이동 배율(순찰·추적 공통). 없으면 1</summary>
    public float SlowMul => _slow.Active ? _slowMul : 1f;
    public bool Slowed => _slow.Active;

    /// <summary>둔화 적용. mul 은 0.1~1로 제한. 더 강한(작은) 배율·더 긴 시간을 유지한다.</summary>
    public void ApplySlow(float mul, float seconds)
    {
        if (seconds <= 0f)
        {
            return;
        }
        mul = Mathf.Clamp(mul, 0.1f, 1f);
        if (!_slow.Active)
        {
            _slowMul = mul;
            _slow.Start(seconds);
            return;
        }
        _slowMul = Mathf.Min(_slowMul, mul);
        _slow.Extend(seconds);
    }

    public void Tick(float dt)
    {
        _slow.Tick(dt);
    }

    /// <summary>
    /// 석화(경직) 시도. 실제 경직 시간을 돌려준다(면역 중이거나 저항 1이면 0).
    /// 성공하면 (now + 경직 시간 + 면역 시간) 까지 재석화 불가.
    /// </summary>
    public float TryPetrify(float seconds, float resist, float now)
    {
        if (seconds <= 0f || now < _petrifyImmuneUntil)
        {
            return 0f;
        }
        float actual = seconds * (1f - Mathf.Clamp01(resist));
        if (actual <= 0f)
        {
            return 0f;
        }
        _petrifyImmuneUntil = now + actual + PetrifyImmuneSeconds;
        return actual;
    }

    public void Clear()
    {
        _slowMul = 1f;
        _slow.Clear();
        _petrifyImmuneUntil = -999f;
    }

    /// <summary>화상 틱 피해: 시전자 공격력의 BurnRatio(최소 1)</summary>
    public static int BurnDamage(int attackPower)
    {
        return Mathf.Max(1, Mathf.RoundToInt(attackPower * BurnRatio));
    }
}
