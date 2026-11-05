using UnityEngine;

/// <summary>방어 한 번의 판정 결과</summary>
public struct GuardOutcome
{
    /// <summary>실제로 받는 피해</summary>
    public int Taken;
    /// <summary>공격자에게 되돌릴 반사 피해</summary>
    public int Reflected;
    /// <summary>패링 성공(판정창 안) — 짧은 무적 + 반격</summary>
    public bool Parried;
    /// <summary>방어로 가로챘는가</summary>
    public bool Blocked;
}

/// <summary>
/// F2-GUARD 홀드 방어·반사·패링·방벽 순수 상태(Floor2Plan.md §5.5 #1·#8). 값은 임시 가정(OpenQuestions Q-AUTO17).
/// 누른 직후 ParryWindow 초 안의 피격 = 패링(피해 0, ParryInvuln 무적). 이후는 감쇠(+반사). 방벽은 최대 BarrierDuration 초.
/// </summary>
public sealed class GuardState
{
    public const float ParryWindow = 0.2f;
    public const float ParryInvuln = 0.5f;

    /// <summary>"BLOCK"(방어력 전부 흡수) 문구 표시 여부. 막기(GUARD/PARRY) 문구와 겹치지 않게 막은 경우는 제외(리뷰 #3)</summary>
    public static bool ShowBlockText(bool guarded, int damageAfterDef)
    {
        return !guarded && damageAfterDef <= 0;
    }
    public const float BarrierDuration = 3f;

    private readonly float _reduceRatio;
    private readonly float _reflectRatio;
    private bool _held;
    private float _heldTime;
    private bool _broken;

    /// <param name="reduceRatio">감쇠율(0~1)</param>
    /// <param name="reflectRatio">막은 피해 중 되돌릴 비율(0 = 반사 없음)</param>
    public GuardState(float reduceRatio = GuardRules.ReduceRatio, float reflectRatio = 0f)
    {
        _reduceRatio = reduceRatio;
        _reflectRatio = reflectRatio;
    }

    public bool Holding => _held && !_broken;
    public float HeldTime => _heldTime;
    public bool InParryWindow => Holding && _heldTime < ParryWindow;

    public void Press()
    {
        if (!_held)
        {
            _held = true;
            _heldTime = 0f;
            _broken = false;
        }
    }

    public void Release()
    {
        _held = false;
        _heldTime = 0f;
        _broken = false;
    }

    public void Tick(float dt)
    {
        if (!_held)
        {
            return;
        }
        _heldTime += dt;
        if (_heldTime >= BarrierDuration)
        {
            _broken = true; // 방벽 지속 초과 — 다시 눌러야 한다
        }
    }

    /// <summary>피해 damage 가 들어왔을 때의 결과. 방어 중이 아니면 Blocked=false 로 그대로.</summary>
    public GuardOutcome Resolve(int damage)
    {
        var o = new GuardOutcome { Taken = damage };
        if (!Holding || damage <= 0)
        {
            return o;
        }
        o.Blocked = true;
        if (InParryWindow)
        {
            o.Parried = true;
            o.Taken = 0;
            o.Reflected = Mathf.RoundToInt(damage * _reflectRatio);
            return o;
        }
        o.Taken = GuardRules.Resolve(damage, true, _reduceRatio);
        o.Reflected = Mathf.RoundToInt((damage - o.Taken) * _reflectRatio);
        return o;
    }
}
