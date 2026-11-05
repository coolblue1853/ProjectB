using UnityEngine;

/// <summary>
/// 지속 피해 장판 틱 규칙(Floor2Plan.md §5.2·§5.5 #5). 순수 로직 — 시간만 먹여 "이번에 몇 번 틱할지"를 돌려준다.
/// 첫 틱은 FirstTickDelay 뒤, 이후 Interval 마다, Duration 이 끝나면 만료. 값은 임시 가정(OpenQuestions Q-AUTO13).
/// 플레이어 무기용 EnemyDamageZone 이 쓰고, 몹 장판(SporeCloud 계열)도 같은 규칙으로 옮길 수 있다.
/// </summary>
public sealed class GroundZoneRules
{
    public const float DefaultDuration = 3f;
    public const float DefaultInterval = 0.5f;
    public const float FirstTickDelay = 0.2f;

    private readonly float _duration;
    private readonly float _interval;
    private float _elapsed;
    private float _nextTick;

    public GroundZoneRules(float duration = DefaultDuration, float interval = DefaultInterval)
    {
        _duration = Mathf.Max(0.05f, duration);
        _interval = Mathf.Max(0.05f, interval);
        _nextTick = FirstTickDelay;
    }

    public bool Expired => _elapsed >= _duration;
    public float Elapsed => _elapsed;
    /// <summary>남은 시간 비율 0~1 (페이드용)</summary>
    public float RemainingRatio => Mathf.Clamp01(1f - _elapsed / _duration);

    /// <summary>dt 만큼 진행하고, 그동안 발생한 틱 수를 돌려준다. 만료 시각 이후의 틱은 없다.</summary>
    public int Step(float dt)
    {
        if (Expired)
        {
            return 0;
        }
        _elapsed = Mathf.Min(_duration, _elapsed + dt);
        int ticks = 0;
        while (_nextTick <= _elapsed + 1e-5f)
        {
            ticks++;
            _nextTick += _interval;
        }
        return ticks;
    }
}
