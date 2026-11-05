using UnityEngine;

/// <summary>투창 규칙(WeaponTypes.md 2.10). 순수 로직: 던지면 빈손, 박힌 창은 5초 함정, 줍거나 소환으로 회수.</summary>
public sealed class ThrownSpear
{
    public const float TrapDuration = 5f;

    public enum State { InHand, InFlight, Planted }

    private State _state = State.InHand;
    private CountdownTimer _trap;

    public State Current => _state;
    public bool CanThrow => _state == State.InHand;
    public bool TrapActive => _state == State.Planted && _trap.Active;

    public bool Throw()
    {
        if (!CanThrow)
        {
            return false;
        }
        _state = State.InFlight;
        return true;
    }

    /// <summary>지면에 도착. 이후 TrapDuration 초 동안 함정.</summary>
    public void Land()
    {
        if (_state == State.InFlight)
        {
            _state = State.Planted;
            _trap.Start(TrapDuration);
        }
    }

    /// <summary>줍기·소환 회수. 날아가는 중에는 불가.</summary>
    public bool Retrieve()
    {
        if (_state != State.Planted)
        {
            return false;
        }
        _state = State.InHand;
        _trap.Clear();
        return true;
    }

    public void Tick(float dt)
    {
        if (_state == State.Planted)
        {
            _trap.Tick(dt);
        }
    }
}
