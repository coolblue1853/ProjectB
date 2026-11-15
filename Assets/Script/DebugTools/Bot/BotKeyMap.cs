using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// 봇 입력용 경로 ↔ 키/버튼 변환(순수). 액션 → 경로는 <see cref="InputBindings.Master"/>(저장된 리바인딩 반영)의 effectivePath 를 읽는다.
/// </summary>
public static class BotKeyMap
{
    public const string PlayerMap = "Player/";

    /// <summary>"Attack" → "Player/Attack", "UI/Select" 는 그대로</summary>
    public static string FullName(string action)
    {
        if (string.IsNullOrEmpty(action))
        {
            return "";
        }
        return action.IndexOf('/') >= 0 ? action : PlayerMap + action;
    }

    /// <summary>"&lt;Keyboard&gt;/leftArrow" → Key.LeftArrow. 숫자 "1" → Digit1. 키보드 경로가 아니면 Key.None</summary>
    public static Key KeyFromPath(string path)
    {
        string name = Suffix(path, InputBindings.KeyboardLayout);
        if (name == null || name.Length == 0)
        {
            return Key.None;
        }
        if (name.Length == 1 && char.IsDigit(name[0]))
        {
            name = "Digit" + name;
        }
        if (char.IsDigit(name[0]) || name.IndexOf('/') >= 0)
        {
            return Key.None; // 숫자 문자열을 Enum.TryParse 가 값으로 읽지 않게
        }
        Key k;
        return Enum.TryParse(name, true, out k) ? k : Key.None;
    }

    /// <summary>패드 경로의 종류</summary>
    public enum PadKind { None, Button, LeftTrigger, RightTrigger, StickLeft, StickRight, StickUp, StickDown }

    /// <summary>"&lt;Gamepad&gt;/buttonSouth" → Button(South), "&lt;Gamepad&gt;/leftStick/left" → StickLeft 등</summary>
    public static PadKind PadFromPath(string path, out GamepadButton button)
    {
        button = GamepadButton.South;
        string n = Suffix(path, InputBindings.GamepadLayout);
        if (string.IsNullOrEmpty(n))
        {
            return PadKind.None;
        }
        switch (n)
        {
            case "buttonSouth": button = GamepadButton.South; return PadKind.Button;
            case "buttonEast": button = GamepadButton.East; return PadKind.Button;
            case "buttonWest": button = GamepadButton.West; return PadKind.Button;
            case "buttonNorth": button = GamepadButton.North; return PadKind.Button;
            case "leftShoulder": button = GamepadButton.LeftShoulder; return PadKind.Button;
            case "rightShoulder": button = GamepadButton.RightShoulder; return PadKind.Button;
            case "start": button = GamepadButton.Start; return PadKind.Button;
            case "select": button = GamepadButton.Select; return PadKind.Button;
            case "leftStickPress": button = GamepadButton.LeftStick; return PadKind.Button;
            case "rightStickPress": button = GamepadButton.RightStick; return PadKind.Button;
            case "dpad/up": button = GamepadButton.DpadUp; return PadKind.Button;
            case "dpad/down": button = GamepadButton.DpadDown; return PadKind.Button;
            case "dpad/left": button = GamepadButton.DpadLeft; return PadKind.Button;
            case "dpad/right": button = GamepadButton.DpadRight; return PadKind.Button;
            case "leftTrigger": return PadKind.LeftTrigger;
            case "rightTrigger": return PadKind.RightTrigger;
            case "leftStick/left": return PadKind.StickLeft;
            case "leftStick/right": return PadKind.StickRight;
            case "leftStick/up": return PadKind.StickUp;
            case "leftStick/down": return PadKind.StickDown;
        }
        return PadKind.None;
    }

    private static string Suffix(string path, string layout)
    {
        if (string.IsNullOrEmpty(path) || !path.StartsWith(layout, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        int i = layout.Length;
        if (path.Length > i && path[i] == '/')
        {
            i++;
        }
        return path.Substring(i);
    }

    /// <summary>
    /// 액션의 키보드(또는 패드) 경로. 버튼 액션은 path 하나(neg = null).
    /// 1D 축 합성(Move·verticalCheck)은 그 장치의 negative/positive 칸을 neg/pos 로 돌려준다. 없으면 false.
    /// </summary>
    public static bool Resolve(InputAction action, bool pad, out string pos, out string neg)
    {
        pos = null;
        neg = null;
        if (action == null)
        {
            return false;
        }
        string layout = pad ? InputBindings.GamepadLayout : InputBindings.KeyboardLayout;
        var bindings = action.bindings;
        for (int i = 0; i < bindings.Count; i++)
        {
            InputBinding b = bindings[i];
            string p = b.effectivePath ?? "";
            if (b.isPartOfComposite)
            {
                if (!p.StartsWith(layout, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (b.name == "negative" && neg == null)
                {
                    neg = p;
                }
                else if (b.name == "positive" && pos == null)
                {
                    pos = p;
                }
                continue;
            }
            if (!b.isComposite && pos == null && neg == null && p.StartsWith(layout, StringComparison.OrdinalIgnoreCase))
            {
                pos = p;
                return true;
            }
        }
        return pos != null || neg != null;
    }
}
