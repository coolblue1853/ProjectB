using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// 봇의 가상 입력 장치(진짜 입력). 가상 Keyboard "BotKeyboard" / Gamepad "BotPad" 를 만들어 상태를 바꾼다.
/// 게임 함수를 부르거나 transform 을 만지지 않는다 — 게임은 사람 키 입력과 같은 InputAction 경로로 받는다.
///
/// 키보드 경로(기본 Direct): 에디터 기본 설정(PointersAndKeyboardsRespectGameViewFocus)은 게임 뷰가 포커스가 없으면
/// 키보드·포인터 *이벤트*를 플레이어 업데이트에서 빼 에디터 업데이트로 돌린다(InputManager.OnUpdate). 가상 키보드도 Keyboard 라 같은 규칙에 걸린다.
/// 그래서 이벤트 큐(QueueStateEvent)를 쓰지 않고 <see cref="InputState.Change{TState}(InputControl, TState, InputUpdateType, InputEventPtr)"/> 로
/// 플레이어(Dynamic) 업데이트 직후(<see cref="InputSystem.onAfterUpdate"/>)에 상태를 쓴다. 액션은 상태 변화 감시로 똑같이 발동하고,
/// 그 프레임 Update 들에서 WasPressedThisFrame 이 참이다. 설정 파일은 바꾸지 않는다. 자세한 결론은 PlayBotDesign.md 10절.
/// 패드 경로: Gamepad 는 포커스 규칙 예외라 QueueStateEvent 로 넣는다.
/// </summary>
public sealed class BotInput : IDisposable
{
    public enum Route { Keyboard, Pad }

    public const float DefaultPress = 0.1f;   // 프로필 칸이 비었을 때 누름 시간
    public const float DefaultJitter = 0.025f; // 키 사이 미세 흔들림 상한(초)

    private struct Op
    {
        public float T;
        public string Action;
        public bool Down;
    }

    private struct Binding
    {
        public string Pos;
        public string Neg;
    }

    public Route Path { get; }
    /// <summary>false 면 키보드도 QueueStateEvent(게임 뷰 포커스가 있거나 설정을 바꾼 경우의 비교용)</summary>
    public bool KeyboardDirect = true;
    /// <summary>Press/Hold/Release 에 넣는 흔들림 상한(재생은 0)</summary>
    public float JitterMax = DefaultJitter;

    private readonly BotProfile _profile;
    private readonly System.Random _rng;
    private readonly List<Op> _ops = new List<Op>(32);
    private readonly HashSet<string> _held = new HashSet<string>();
    private readonly HashSet<string> _rawDown = new HashSet<string>();
    private readonly Dictionary<string, Binding> _bind = new Dictionary<string, Binding>();
    private int _bindVersion = -1;
    private Keyboard _kb;
    private Gamepad _pad;
    private KeyboardState _ks;
    private GamepadState _gs;
    private Vector2 _stick;
    private float _now;
    private bool _dirty;
    private bool _kbPending;
    private bool _hooked;

    public Keyboard KeyboardDevice => _kb;
    public Gamepad PadDevice => _pad;
    public Vector2 StickValue => _stick;

    public BotInput(BotProfile profile, Route route, int seed)
    {
        _profile = profile ?? new BotProfile();
        _rng = new System.Random(seed);
        Path = route;
        if (route == Route.Keyboard)
        {
            _kb = InputSystem.AddDevice<Keyboard>("BotKeyboard");
        }
        else
        {
            _pad = InputSystem.AddDevice<Gamepad>("BotPad");
        }
        InputSystem.onAfterUpdate += OnAfterUpdate;
        _hooked = true;
    }

    // ───────────── 액션 API ─────────────

    /// <summary>action 을 duration 초 누른다(음수면 프로필 press.&lt;action&gt; 분포에서 뽑는다). 흔들림만큼 늦게 시작</summary>
    public void Press(string action, float duration = -1f)
    {
        string full = BotKeyMap.FullName(action);
        if (duration < 0f)
        {
            duration = SamplePress(full);
        }
        float t0 = _now + Jitter();
        Schedule(t0, full, true);
        Schedule(t0 + Mathf.Max(0.02f, duration), full, false);
    }

    public void Hold(string action)
    {
        Schedule(_now + Jitter(), BotKeyMap.FullName(action), true);
    }

    public void Release(string action)
    {
        Schedule(_now + Jitter(), BotKeyMap.FullName(action), false);
    }

    /// <summary>이동 축. x → Player/Move(좌우), y → UI/verticalCheck(위·아래). 키보드는 ±0.5 넘으면 화살표 키</summary>
    public void Stick(float x, float y)
    {
        Vector2 v = new Vector2(Mathf.Clamp(x, -1f, 1f), Mathf.Clamp(y, -1f, 1f));
        if (v != _stick)
        {
            _stick = v;
            _dirty = true;
        }
    }

    public bool IsHeld(string action)
    {
        return _held.Contains(BotKeyMap.FullName(action));
    }

    /// <summary>재생용 저수준: 장치 경로 하나를 바로 누르거나 뗀다(흔들림·예약 없음)</summary>
    public void SetPath(string path, bool down)
    {
        if (down ? _rawDown.Add(path) : _rawDown.Remove(path))
        {
            _dirty = true;
        }
    }

    public bool IsPathDown(string path)
    {
        return _rawDown.Contains(path);
    }

    public void ReleaseAll()
    {
        _ops.Clear();
        _held.Clear();
        _rawDown.Clear();
        _stick = Vector2.zero;
        _dirty = true;
        Apply();
    }

    /// <summary>매 프레임(소유자 Update). 시각이 된 예약을 처리하고 바뀐 상태를 장치에 넣는다</summary>
    public void Tick(float now)
    {
        _now = now;
        for (int i = 0; i < _ops.Count;)
        {
            Op op = _ops[i];
            if (op.T > now)
            {
                i++;
                continue;
            }
            _ops.RemoveAt(i);
            if (op.Down ? _held.Add(op.Action) : _held.Remove(op.Action))
            {
                _dirty = true;
            }
        }
        if (_dirty)
        {
            Apply();
        }
    }

    public void Dispose()
    {
        if (_hooked)
        {
            InputSystem.onAfterUpdate -= OnAfterUpdate;
            _hooked = false;
        }
        if (_kb != null && _kb.added)
        {
            InputSystem.RemoveDevice(_kb); // 제거 때 쓰던 액션은 장치 변경으로 취소된다
        }
        if (_pad != null && _pad.added)
        {
            InputSystem.RemoveDevice(_pad);
        }
        _kb = null;
        _pad = null;
    }

    // ───────────── 내부 ─────────────

    private float Jitter()
    {
        return JitterMax > 0f ? (float)_rng.NextDouble() * JitterMax : 0f;
    }

    private float SamplePress(string full)
    {
        string key = full.StartsWith(BotKeyMap.PlayerMap) ? full.Substring(BotKeyMap.PlayerMap.Length) : full;
        return _profile.Sample("press." + key, _rng, DefaultPress);
    }

    private void Schedule(float t, string action, bool down)
    {
        _ops.Add(new Op { T = t, Action = action, Down = down });
    }

    private Binding Bind(string full)
    {
        if (_bindVersion != InputBindings.Version)
        {
            _bind.Clear(); // 리바인딩 반영
            _bindVersion = InputBindings.Version;
        }
        Binding b;
        if (!_bind.TryGetValue(full, out b))
        {
            InputAction a = InputBindings.Master.asset.FindAction(full);
            BotKeyMap.Resolve(a, Path == Route.Pad, out b.Pos, out b.Neg);
            _bind[full] = b;
        }
        return b;
    }

    private void Apply()
    {
        _dirty = false;
        if (Path == Route.Keyboard)
        {
            BuildKeyboard();
            if (KeyboardDirect)
            {
                _kbPending = true; // onAfterUpdate(Dynamic) 에서 쓴다
            }
            else if (_kb != null)
            {
                InputSystem.QueueStateEvent(_kb, _ks);
            }
        }
        else
        {
            BuildPad();
            if (_pad != null)
            {
                InputSystem.QueueStateEvent(_pad, _gs);
            }
        }
    }

    private void OnAfterUpdate()
    {
        if (!_kbPending || _kb == null || !_kb.added || InputState.currentUpdateType != InputUpdateType.Dynamic)
        {
            return;
        }
        _kbPending = false;
        InputState.Change(_kb, _ks);
    }

    private void BuildKeyboard()
    {
        _ks = new KeyboardState();
        foreach (string a in _held)
        {
            SetKey(Bind(a).Pos);
        }
        foreach (string p in _rawDown)
        {
            SetKey(p);
        }
        if (Mathf.Abs(_stick.x) > 0.5f)
        {
            Binding m = Bind("Player/Move");
            SetKey(_stick.x > 0f ? m.Pos : m.Neg);
        }
        if (Mathf.Abs(_stick.y) > 0.5f)
        {
            Binding v = Bind("UI/verticalCheck");
            SetKey(_stick.y > 0f ? v.Pos : v.Neg);
        }
    }

    private void SetKey(string path)
    {
        Key k = BotKeyMap.KeyFromPath(path);
        if (k != Key.None)
        {
            _ks.Set(k, true);
        }
    }

    private void BuildPad()
    {
        _gs = new GamepadState();
        Vector2 stick = _stick;
        foreach (string a in _held)
        {
            SetPad(Bind(a).Pos, ref stick);
        }
        foreach (string p in _rawDown)
        {
            SetPad(p, ref stick);
        }
        _gs.leftStick = stick;
    }

    private void SetPad(string path, ref Vector2 stick)
    {
        GamepadButton btn;
        switch (BotKeyMap.PadFromPath(path, out btn))
        {
            case BotKeyMap.PadKind.Button: _gs = _gs.WithButton(btn, true); break;
            case BotKeyMap.PadKind.LeftTrigger: _gs.leftTrigger = 1f; break;
            case BotKeyMap.PadKind.RightTrigger: _gs.rightTrigger = 1f; break;
            case BotKeyMap.PadKind.StickLeft: stick.x = -1f; break;
            case BotKeyMap.PadKind.StickRight: stick.x = 1f; break;
            case BotKeyMap.PadKind.StickUp: stick.y = 1f; break;
            case BotKeyMap.PadKind.StickDown: stick.y = -1f; break;
        }
    }
}
