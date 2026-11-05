using UnityEngine;

/// <summary>
/// 방패 반격 버프 규칙(WeaponTypes.md 2.11, Floor1Completion.md §5). 순수 로직 — 홀드 방어가 성공하면 Arm,
/// 이후 Duration 초 동안 공격 피해 배율 Multiplier. 값은 임시 가정(OpenQuestions Q-AUTO6).
/// </summary>
public sealed class CounterBuffRules
{
    public const float Duration = 3f;
    public const float Multiplier = 1.2f;

    private CountdownTimer _timer;

    public bool Active => _timer.Active;
    public float Remaining => _timer.Remaining;

    /// <summary>막기 성공. 이미 활성이면 시간만 갱신(중첩 없음).</summary>
    public void Arm()
    {
        _timer.Start(Duration);
    }

    public void Tick(float dt)
    {
        _timer.Tick(dt);
    }

    public void Clear()
    {
        _timer.Clear();
    }

    public float DamageMultiplier => Active ? Multiplier : 1f;
}
