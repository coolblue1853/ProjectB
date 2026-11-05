using UnityEngine;

/// <summary>
/// 용의 숨결 지속 판정 규칙 (Floor2Plan.md 5.5 #13, F2-BREATH, AUTO-25). 충전 → 지속(틱 피해) → 쿨 3단계 순수 상태기계.
/// 지속 틱은 GroundZoneRules, 막기 감소는 GuardRules.Resolve 를 호출만 한다(중복 구현 없음). 보스 연결·이펙트·부채꼴 판정 없음.
/// 임시 가정: 충전 1.2s, 지속 2.5s, 틱 0.5s, 쿨 8s.
/// </summary>
public sealed class BreathRules
{
    public const float ChargeSeconds = 1.2f;
    public const float ActiveSeconds = 2.5f;
    public const float TickInterval = 0.5f;
    public const float CooldownSeconds = 8f;

    public enum Phase { Ready, Charging, Breathing, Cooldown }

    Phase _phase = Phase.Ready;
    float _timer;
    GroundZoneRules _zone;

    public Phase Current => _phase;
    public bool CanStart => _phase == Phase.Ready;
    /// <summary>판정이 나가는 중(지속 단계). 충전 중에는 피해 없음</summary>
    public bool Dealing => _phase == Phase.Breathing;

    /// <summary>충전 시작. Ready 일 때만 true</summary>
    public bool TryStart()
    {
        if (_phase != Phase.Ready)
        {
            return false;
        }
        _phase = Phase.Charging;
        _timer = 0f;
        return true;
    }

    /// <summary>dt 진행. 남은 시간은 다음 단계로 이월한다. 이번 호출에서 발생한 지속 피해 틱 수를 돌려준다.</summary>
    public int Step(float dt)
    {
        int ticks = 0;
        while (dt > 0f && _phase != Phase.Ready)
        {
            if (_phase == Phase.Charging)
            {
                float need = ChargeSeconds - _timer;
                if (dt < need)
                {
                    _timer += dt;
                    return ticks;
                }
                dt -= need;
                _phase = Phase.Breathing;
                _zone = new GroundZoneRules(ActiveSeconds, TickInterval);
            }
            else if (_phase == Phase.Breathing)
            {
                float need = ActiveSeconds - _zone.Elapsed;
                float use = Mathf.Min(dt, need);
                ticks += _zone.Step(use);
                dt -= use;
                if (!_zone.Expired)
                {
                    return ticks;
                }
                _phase = Phase.Cooldown;
                _timer = 0f;
            }
            else
            {
                float need = CooldownSeconds - _timer;
                if (dt < need)
                {
                    _timer += dt;
                    return ticks;
                }
                dt -= need;
                _phase = Phase.Ready;
            }
        }
        return ticks;
    }

    /// <summary>틱 1회의 플레이어 피해. 방패 막는 중이면 GuardRules 감소(양수 피해는 최소 1)</summary>
    public static int TickDamage(int baseDamage, bool guarding)
    {
        return GuardRules.Resolve(baseDamage, guarding);
    }
}
