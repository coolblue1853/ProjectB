using System.Collections.Generic;

/// <summary>
/// 사람 신호 판정(막힘·혼란·지루함 후보) — 순수 로직(시간·위치는 호출자가 넘긴다, EditMode 테스트 대상).
/// 연타: 같은 액션을 MashGap 초 안 간격으로 MashCount 번 이상(한 연속에 한 번만 알림).
/// 제자리 왕복: 이동 방향이 OscWindow 초 안에 OscTurns 번 바뀌고 그 사이 가로 이동 폭이 OscMaxSpan 미만.
/// 창 바로 닫기: 창이 열린 뒤 QuickClose 초 안에 닫힘. 무입력: 입력 이벤트도 누름도 IdleSeconds 초 넘게 없음(끝날 때 알림).
/// 일시정지: 들어간 시각과 길이(끝날 때 알림).
/// </summary>
public sealed class PlayRecHuman
{
    public const float MashGap = 0.15f;
    public const int MashCount = 3;
    public const float OscWindow = 2f;
    public const int OscTurns = 3;
    public const float OscMaxSpan = 1.5f;
    public const float QuickClose = 1f;
    public const float IdleSeconds = 3f;

    public enum Kind { Mash, Oscillate, QuickClose, Idle, Pause }

    public struct Signal
    {
        public Kind Kind;
        public string A;
        public float T;
        public float Dur;
        public int N;
    }

    private sealed class Run
    {
        public float Last = -99f;
        public int Count;
        public bool Fired;
    }

    private readonly Dictionary<string, Run> _runs = new Dictionary<string, Run>();
    private readonly Dictionary<string, float> _openAt = new Dictionary<string, float>();
    private readonly float[] _turnT = new float[OscTurns];
    private readonly float[] _turnX = new float[OscTurns];
    private int _turns;
    private int _lastSign;
    private float _lastActivity;
    private bool _idle;
    private bool _paused;
    private float _pauseAt;

    public void Reset(float now)
    {
        _runs.Clear();
        _openAt.Clear();
        _turns = 0;
        _lastSign = 0;
        _lastActivity = now;
        _idle = false;
        _paused = false;
    }

    /// <summary>버튼 누름 한 번. 연타가 성립한 순간 true(같은 연속에서는 한 번만).</summary>
    public bool OnPress(string action, float t, out Signal s)
    {
        s = default(Signal);
        Run r;
        if (!_runs.TryGetValue(action, out r))
        {
            r = new Run();
            _runs[action] = r;
        }
        if (t - r.Last <= MashGap)
        {
            r.Count++;
        }
        else
        {
            r.Count = 1;
            r.Fired = false;
        }
        r.Last = t;
        if (!r.Fired && r.Count >= MashCount)
        {
            r.Fired = true;
            s = new Signal { Kind = Kind.Mash, A = action, T = t, N = r.Count };
            return true;
        }
        return false;
    }

    /// <summary>이동 방향(-1·0·+1) 변화. 0 은 무시한다. 제자리 왕복이 성립하면 true 후 다시 센다.</summary>
    public bool OnMoveSign(int sign, float t, float x, out Signal s)
    {
        s = default(Signal);
        if (sign == 0)
        {
            return false;
        }
        if (_lastSign != 0 && sign != _lastSign)
        {
            for (int i = 0; i < OscTurns - 1; i++)
            {
                _turnT[i] = _turnT[i + 1];
                _turnX[i] = _turnX[i + 1];
            }
            _turnT[OscTurns - 1] = t;
            _turnX[OscTurns - 1] = x;
            if (_turns < OscTurns)
            {
                _turns++;
            }
            if (_turns >= OscTurns && t - _turnT[0] <= OscWindow && Span() < OscMaxSpan)
            {
                s = new Signal { Kind = Kind.Oscillate, T = _turnT[0], Dur = t - _turnT[0], N = OscTurns };
                _turns = 0;
                _lastSign = sign;
                return true;
            }
        }
        _lastSign = sign;
        return false;
    }

    /// <summary>창 열림/닫힘. 열린 뒤 QuickClose 초 안에 닫히면 true.</summary>
    public bool OnWindow(string name, bool open, float t, out Signal s)
    {
        s = default(Signal);
        if (open)
        {
            _openAt[name] = t;
            return false;
        }
        float at;
        if (!_openAt.TryGetValue(name, out at))
        {
            return false;
        }
        _openAt.Remove(name);
        if (t - at <= QuickClose)
        {
            s = new Signal { Kind = Kind.QuickClose, A = name, T = at, Dur = t - at };
            return true;
        }
        return false;
    }

    /// <summary>입력 이벤트가 있었다(무입력 구간이 있었으면 끝나며 true).</summary>
    public bool OnActivity(float t, out Signal s)
    {
        s = default(Signal);
        bool fired = false;
        if (_idle)
        {
            s = new Signal { Kind = Kind.Idle, T = _lastActivity, Dur = t - _lastActivity };
            fired = true;
            _idle = false;
        }
        _lastActivity = t;
        return fired;
    }

    /// <summary>틱마다. 누르고 있는 키가 있으면 활동으로 본다. 무입력이 끝나는 순간에만 true.</summary>
    public bool Tick(float t, bool anyHeld, out Signal s)
    {
        if (anyHeld)
        {
            return OnActivity(t, out s);
        }
        s = default(Signal);
        if (!_idle && t - _lastActivity > IdleSeconds)
        {
            _idle = true;
        }
        return false;
    }

    public bool IsIdle => _idle;

    /// <summary>일시정지 상태. 끝나는 순간 true(시작 시각·길이).</summary>
    public bool OnPause(bool paused, float t, out Signal s)
    {
        s = default(Signal);
        if (paused == _paused)
        {
            return false;
        }
        _paused = paused;
        if (paused)
        {
            _pauseAt = t;
            return false;
        }
        s = new Signal { Kind = Kind.Pause, T = _pauseAt, Dur = t - _pauseAt };
        return true;
    }

    private float Span()
    {
        float lo = _turnX[0];
        float hi = _turnX[0];
        for (int i = 1; i < OscTurns; i++)
        {
            if (_turnX[i] < lo)
            {
                lo = _turnX[i];
            }
            if (_turnX[i] > hi)
            {
                hi = _turnX[i];
            }
        }
        return hi - lo;
    }

    public static string Name(Kind k)
    {
        switch (k)
        {
            case Kind.Mash: return "mash";
            case Kind.Oscillate: return "osc";
            case Kind.QuickClose: return "qclose";
            case Kind.Idle: return "idle";
            default: return "pause";
        }
    }
}
