using UnityEngine;

/// <summary>
/// 몹 반사 자세 순수 규칙(Floor2Plan.md §5.5 #2). 자세 중 받은 피해의 Ratio 를 플레이어에게 돌려준다(최소 1, 자세 밖은 0).
/// 값은 임시 가정(OpenQuestions Q-AUTO14).
/// </summary>
public sealed class MobReflectRules
{
    public const float Ratio = 0.3f;

    private float _remaining;

    public bool Active => _remaining > 0f;

    public void Begin(float seconds)
    {
        _remaining = Mathf.Max(_remaining, seconds);
    }

    public void Tick(float dt)
    {
        if (_remaining > 0f)
        {
            _remaining = Mathf.Max(0f, _remaining - dt);
        }
    }

    public void Clear()
    {
        _remaining = 0f;
    }

    /// <summary>받은 피해 dealt 에 대한 반사 피해</summary>
    public int Reflect(int dealt)
    {
        if (!Active || dealt <= 0)
        {
            return 0;
        }
        return Mathf.Max(1, Mathf.RoundToInt(dealt * Ratio));
    }
}
