using UnityEngine;

/// <summary>
/// 평타 리듬(순수 로직): 사람 기록의 연타 묶음(attack.burstLen)·묶음 안 간격(attack.burstWithin)·묶음 사이 간격(attack.burstGap)을 따른다.
/// </summary>
public sealed class BotAttackRhythm
{
    public const float FallbackWithin = 0.2f;
    public const float FallbackGap = 0.9f;
    public const int FallbackLen = 4;

    private readonly BotProfile _profile;
    private readonly System.Random _rng;
    private float _next = float.NegativeInfinity;
    private int _left;

    public BotAttackRhythm(BotProfile profile, System.Random rng)
    {
        _profile = profile;
        _rng = rng;
    }

    public void Reset()
    {
        _left = 0;
        _next = float.NegativeInfinity;
    }

    /// <summary>now 에 평타를 눌러야 하면 true(누른 것으로 치고 다음 시각을 정한다)</summary>
    public bool Due(float now)
    {
        if (now < _next)
        {
            return false;
        }
        if (_left <= 0)
        {
            _left = Mathf.Max(1, Mathf.RoundToInt(_profile.Sample("attack.burstLen", _rng, FallbackLen)));
        }
        _left--;
        _next = now + (_left > 0
            ? _profile.Sample("attack.burstWithin", _rng, FallbackWithin)
            : _profile.Sample("attack.burstGap", _rng, FallbackGap));
        return true;
    }
}

/// <summary>
/// 막힘 판정(순수 로직). 이동을 누르는데 위치가 안 바뀌면 점프 → 그래도 안 되면 반대로 걷기. 목표 진척이 3초 넘게 없으면 경고 신호.
/// </summary>
public sealed class BotStuck
{
    public const float MoveEps = 0.3f;
    public const float JumpAfter = 0.8f;
    public const float ReverseAfter = 2.0f;
    public const float WarnAfter = 3.0f;

    public enum Act { None, Jump, Reverse }

    private float _anchorX;
    private float _anchorT;
    private bool _jumped;
    private float _lastProgress;

    public void Reset(float now, float x)
    {
        _anchorX = x;
        _anchorT = now;
        _jumped = false;
        _lastProgress = now;
    }

    /// <summary>진척(가까워짐·피해·처치·줍기)이 있었다</summary>
    public void Progress(float now)
    {
        _lastProgress = now;
    }

    /// <summary>진척 없이 WarnAfter 초가 지났나</summary>
    public bool NoProgress(float now)
    {
        return now - _lastProgress > WarnAfter;
    }

    /// <summary>moving = 좌우 이동을 누르는 중. 위치가 움직이면 기준점을 옮긴다</summary>
    public Act Update(float now, float x, bool moving)
    {
        if (!moving || Mathf.Abs(x - _anchorX) > MoveEps)
        {
            _anchorX = x;
            _anchorT = now;
            _jumped = false;
            return Act.None;
        }
        float held = now - _anchorT;
        if (!_jumped && held > JumpAfter)
        {
            _jumped = true;
            return Act.Jump;
        }
        if (held > ReverseAfter)
        {
            _anchorT = now;
            _jumped = false;
            return Act.Reverse;
        }
        return Act.None;
    }
}

/// <summary>
/// 길이 없어 못 가는 상태(막힘)가 이어진 시간을 센다(순수). 닿지 않는 드롭 쪽으로 가려 하는데 이동 그래프에 길이 없으면(간선 금지 등)
/// 제자리에서 10초 넘게 서 있던 것(11-16 SideRoad x≈-313 do=drop)을 짧게 끊는 데 쓴다.
/// </summary>
public sealed class BotStall
{
    private float _since = -1f;

    public void Clear()
    {
        _since = -1f;
    }

    /// <summary>blocked 가 seconds 넘게 이어지면 참(한 번 참을 주고 다시 센다)</summary>
    public bool Update(float now, bool blocked, float seconds)
    {
        if (!blocked)
        {
            _since = -1f;
            return false;
        }
        if (_since < 0f)
        {
            _since = now;
            return false;
        }
        if (now - _since <= seconds)
        {
            return false;
        }
        _since = -1f;
        return true;
    }
}
