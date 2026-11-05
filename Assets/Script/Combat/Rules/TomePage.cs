using UnityEngine;

/// <summary>마도서 장 전환 규칙(WeaponTypes.md 2.9). 순수 로직: 불/냉기/번개 3장 순환, 전환 쿨 3초.</summary>
public sealed class TomePage
{
    public const int PageCount = 3;
    public const float SwitchCooldown = 3f;

    private int _page;
    private CountdownTimer _cooldown;

    public int Page => _page;
    public bool CanSwitch => !_cooldown.Active;
    public float CooldownRemaining => _cooldown.Remaining;

    public bool TrySwitch()
    {
        if (!CanSwitch)
        {
            return false;
        }
        _page = (_page + 1) % PageCount;
        _cooldown.Start(SwitchCooldown);
        return true;
    }

    public void Tick(float dt)
    {
        _cooldown.Tick(dt);
    }
}
