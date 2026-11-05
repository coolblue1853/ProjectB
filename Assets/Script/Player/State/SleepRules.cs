using UnityEngine;

/// <summary>
/// 플레이어 수면 순수 규칙(Floor2Plan.md §5.5 #6, PlayerStateDesign.md 2.2 토큰). 값은 임시 가정(OpenQuestions Q-AUTO14).
/// 경직보다 길다(MinSeconds), 피격 시 해제(단 잠든 직후 WakeGrace 초는 무시), 이미 자는 중이면 재적용 불가.
/// </summary>
public sealed class SleepRules
{
    public const float MinSeconds = 3f;
    public const float MaxSeconds = 6f;
    public const float WakeGrace = 0.3f;

    private float _remaining;
    private float _asleepFor;

    public bool Sleeping => _remaining > 0f;
    public float Remaining => _remaining;

    /// <summary>수면 적용. 요청 시간은 [MinSeconds, MaxSeconds] 로 보정. 이미 자는 중이면 false.</summary>
    public bool TryApply(float seconds)
    {
        if (Sleeping)
        {
            return false;
        }
        _remaining = Mathf.Clamp(seconds, MinSeconds, MaxSeconds);
        _asleepFor = 0f;
        return true;
    }

    /// <summary>시간 진행. 이번 틱에 깨어나면 true.</summary>
    public bool Tick(float dt)
    {
        if (!Sleeping)
        {
            return false;
        }
        _asleepFor += dt;
        _remaining = Mathf.Max(0f, _remaining - dt);
        return !Sleeping;
    }

    /// <summary>피격. 잠든 직후 유예 안이면 무시, 아니면 깨운다. 깨어났으면 true.</summary>
    public bool Hit()
    {
        if (!Sleeping || _asleepFor < WakeGrace)
        {
            return false;
        }
        _remaining = 0f;
        return true;
    }

    public void Clear()
    {
        _remaining = 0f;
        _asleepFor = 0f;
    }
}
