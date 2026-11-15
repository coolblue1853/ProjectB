using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 입력 기록: 모든 InputAction 변화를 k:"in" 으로 남기고(R1 그대로), 학습 틱용 "지금 누르고 있는 액션" 비트마스크와 이동 축을 낸다.
/// 비트 순서는 입력 에셋의 Player → UI → Portal 맵 순서(에셋을 고치지 않는 한 세션 간 같다). 처음 보는 이름은 뒤에 붙고 k:"tkh" 를 다시 쓴다.
/// 버튼 시작(Started)·이동 방향 변화는 Pressed 로 알린다(사람 신호·회피 반응 집계용). 게임 입력을 바꾸지 않는다(읽기만).
/// </summary>
public sealed class PlayRecInput
{
    private const int MaxBits = 63;
    private static readonly string[] BitMaps = { "Player", "UI", "Portal" };

    /// <summary>name = 맵/액션, moveSign = 이동 액션의 새 방향(-1·+1, 버튼이면 0)</summary>
    public event Action<string, int> Pressed;

    public float MoveX;
    public bool HeaderDirty;
    public IList<string> BitNames => _bitNames;

    private readonly Dictionary<InputAction, ActInfo> _acts = new Dictionary<InputAction, ActInfo>();
    private readonly Dictionary<string, Dedupe> _dedupe = new Dictionary<string, Dedupe>();
    private readonly List<InputAction> _enabled = new List<InputAction>(256);
    private readonly List<string> _bitNames = new List<string>(64);
    private readonly Dictionary<string, int> _bitOf = new Dictionary<string, int>(64);
    private int _moveSign;

    private sealed class Dedupe
    {
        public int Frame = -1;
        public int Phase = -1;
        public float X;
        public float Y;
    }

    private sealed class ActInfo
    {
        public string Name;
        public Dedupe Shared;
        public bool SkipChecked;
        public bool Skip;
        public string Dev = "?";
        public float LastX;
        public float LastY;
        public bool IsMove;
        public int Bit = -2; // -2 미정, -1 비트 없음(값 액션·맵 밖)
    }

    public void Subscribe()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    public void Unsubscribe()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    /// <summary>지금 누르고 있는 버튼 액션 비트 + 이동 축(Player/Move, UI/verticalCheck). 켜진 액션만 본다.</summary>
    public long HeldMask(out float mx, out float my)
    {
        mx = 0f;
        my = 0f;
        long mask = 0;
        _enabled.Clear();
        InputSystem.ListEnabledActions(_enabled);
        for (int i = 0; i < _enabled.Count; i++)
        {
            InputAction a = _enabled[i];
            ActInfo info = InfoOf(a);
            if (info.Bit == -2)
            {
                EnsureBits(a);
                info.Bit = a.type == InputActionType.Button && a.actionMap != null && _bitOf.TryGetValue(info.Name, out int b) ? b : -1;
            }
            if (info.IsMove)
            {
                mx = a.ReadValue<float>();
                continue;
            }
            if (info.Name == "UI/verticalCheck")
            {
                my = a.ReadValue<float>();
                continue;
            }
            if (info.Bit >= 0 && a.IsPressed())
            {
                mask |= 1L << info.Bit;
            }
        }
        _enabled.Clear();
        return mask;
    }

    /// <summary>처음 본 에셋의 Player·UI·Portal 맵 액션을 에셋 순서대로 비트에 올린다.</summary>
    private void EnsureBits(InputAction sample)
    {
        InputActionAsset asset = sample.actionMap != null ? sample.actionMap.asset : null;
        if (asset != null)
        {
            for (int m = 0; m < BitMaps.Length; m++)
            {
                InputActionMap map = asset.FindActionMap(BitMaps[m]);
                if (map == null)
                {
                    continue;
                }
                for (int i = 0; i < map.actions.Count; i++)
                {
                    InputAction a = map.actions[i];
                    if (a.type == InputActionType.Button)
                    {
                        AddBit(map.name + "/" + a.name);
                    }
                }
            }
        }
        if (sample.actionMap != null && sample.type == InputActionType.Button && Array.IndexOf(BitMaps, sample.actionMap.name) >= 0)
        {
            AddBit(sample.actionMap.name + "/" + sample.name);
        }
    }

    private void AddBit(string name)
    {
        if (_bitOf.ContainsKey(name) || _bitNames.Count >= MaxBits)
        {
            return;
        }
        _bitOf[name] = _bitNames.Count;
        _bitNames.Add(name);
        HeaderDirty = true;
    }

    private ActInfo InfoOf(InputAction action)
    {
        ActInfo info;
        if (!_acts.TryGetValue(action, out info))
        {
            string name = (action.actionMap != null ? action.actionMap.name + "/" : "") + action.name;
            Dedupe shared;
            if (!_dedupe.TryGetValue(name, out shared))
            {
                shared = new Dedupe();
                _dedupe[name] = shared;
            }
            info = new ActInfo { Name = name, Shared = shared, IsMove = name == "Player/Move" };
            _acts[action] = info;
        }
        return info;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (!PlayRecorder.Enabled || !PlayRecWriter.IsOpen)
        {
            return;
        }
        int phase;
        string ph;
        switch (change)
        {
            case InputActionChange.ActionStarted: phase = 0; ph = "S"; break;
            case InputActionChange.ActionPerformed: phase = 1; ph = "P"; break;
            case InputActionChange.ActionCanceled: phase = 2; ph = "C"; break;
            default: return;
        }
        InputAction action = obj as InputAction;
        if (action == null)
        {
            return;
        }
        ActInfo info = InfoOf(action);
        InputControl control = action.activeControl;
        Type valueType = action.activeValueType;
        if (control != null)
        {
            info.Dev = DeviceName(control.device);
            if (!info.SkipChecked)
            {
                info.SkipChecked = true;
                info.Skip = control.device is Pointer && valueType == typeof(Vector2); // 마우스 이동 같은 고빈도 값
            }
        }
        if (info.Skip)
        {
            return;
        }
        float x = 0f;
        float y = 0f;
        bool vec = false;
        if (phase != 2 && valueType == typeof(Vector2))
        {
            Vector2 v2 = action.ReadValue<Vector2>();
            x = v2.x;
            y = v2.y;
            vec = true;
        }
        else if (phase != 2 && valueType == typeof(float))
        {
            x = action.ReadValue<float>();
        }
        if (info.IsMove)
        {
            MoveX = phase == 2 ? 0f : x; // 막힘 판정용(throttle·중복 제거 전에 갱신)
        }
        // 같은 이름의 액션 인스턴스가 여럿이라 같은 프레임·같은 값은 한 번만 남긴다
        Dedupe dd = info.Shared;
        int frame = Time.frameCount;
        if (dd.Frame == frame && dd.Phase == phase && Mathf.Approximately(dd.X, x) && Mathf.Approximately(dd.Y, y))
        {
            return;
        }
        // 축·벡터의 Performed 는 변화량이 클 때만(스틱 미세 떨림 제외)
        if (phase == 1 && (valueType == typeof(Vector2) || valueType == typeof(float)))
        {
            float dx = x - info.LastX;
            float dy = y - info.LastY;
            bool zeroCross = (x == 0f) != (info.LastX == 0f) || (y == 0f) != (info.LastY == 0f);
            if (!zeroCross && dx * dx + dy * dy < 0.0625f)
            {
                return;
            }
        }
        dd.Frame = frame;
        dd.Phase = phase;
        dd.X = x;
        dd.Y = y;
        info.LastX = x;
        info.LastY = y;
        WriteRow(info, ph, vec, x, y);
        Notify(info, phase, action.type, x);
    }

    private static void WriteRow(ActInfo info, string ph, bool vec, float x, float y)
    {
        lock (PlayRecWriter.Gate)
        {
            try
            {
                System.Text.StringBuilder sb = PlayRecWriter.Begin("in");
                PlayRecJson.KeyStr(sb, "a", info.Name);
                PlayRecJson.KeyStr(sb, "ph", ph);
                PlayRecJson.KeyStr(sb, "dev", info.Dev);
                if (vec)
                {
                    PlayRecJson.KeyNum(sb, "x", x, 3);
                    PlayRecJson.KeyNum(sb, "y", y, 3);
                }
                else
                {
                    PlayRecJson.KeyNum(sb, "v", x, 3);
                }
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    private void Notify(ActInfo info, int phase, InputActionType type, float x)
    {
        Action<string, int> h = Pressed;
        if (h == null)
        {
            return;
        }
        if (info.IsMove)
        {
            int sign = phase == 2 ? 0 : (x > 0.3f ? 1 : x < -0.3f ? -1 : 0);
            if (sign != 0 && sign != _moveSign)
            {
                h(info.Name, sign);
            }
            _moveSign = sign;
            return;
        }
        if (phase == 0 && type == InputActionType.Button)
        {
            h(info.Name, 0);
        }
    }

    private static string DeviceName(InputDevice d)
    {
        if (d is Keyboard)
        {
            return "kb";
        }
        if (d is Gamepad)
        {
            return "pad";
        }
        if (d is Pointer)
        {
            return "mouse";
        }
        return "other";
    }
}
