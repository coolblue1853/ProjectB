using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// 창·필드 고정 단축키(도감 B·지도 M·미니맵 N·방어 G·스킬 K·퀘스트 Q, U37)의 현재 키.
/// 키 설정(InputBindings.Master 의 Player 맵 키보드 칸)에서 읽는다. 키 설정에서 바꾸면 다음 프레임부터 새 키가 눌린다.
/// 패드는 기존 고정 제스처(LT+Select 등)라 이 키 설정 대상이 아니다.
/// </summary>
public static class Hotkeys
{
    public const string Bestiary = "Bestiary";
    public const string WorldMap = "WorldMap";
    public const string Minimap = "Minimap";
    public const string Guard = "Guard";
    public const string SkillWindow = "SkillWindow";
    public const string QuestWindow = "QuestWindow";

    public static readonly string[] All = { Bestiary, WorldMap, Minimap, Guard, SkillWindow, QuestWindow };

    private static readonly Dictionary<string, ButtonControl> _cache = new Dictionary<string, ButtonControl>();
    private static Keyboard _cacheKeyboard;
    private static int _cacheVersion = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _cache.Clear();
        _cacheKeyboard = null;
        _cacheVersion = -1;
    }

    /// <summary>이번 프레임에 눌렸다. 키보드가 없으면 false</summary>
    public static bool Pressed(string action)
    {
        ButtonControl c = Find(action);
        return c != null && c.wasPressedThisFrame;
    }

    /// <summary>지금 누르고 있다. 키보드가 없으면 false</summary>
    public static bool Held(string action)
    {
        ButtonControl c = Find(action);
        return c != null && c.isPressed;
    }

    /// <summary>안내 문구용 키 이름(미지정이면 "-")</summary>
    public static string KeyName(string action)
    {
        return InputBindings.Display(action, false);
    }

    private static ButtonControl Find(string action)
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return null;
        }
        if (kb != _cacheKeyboard || _cacheVersion != InputBindings.Version)
        {
            _cache.Clear();
            _cacheKeyboard = kb;
            _cacheVersion = InputBindings.Version;
        }
        if (!_cache.TryGetValue(action, out ButtonControl control))
        {
            control = Resolve(kb, action);
            _cache[action] = control;
        }
        return control;
    }

    private static ButtonControl Resolve(Keyboard kb, string action)
    {
        InputAction a = InputBindings.Master.asset.FindAction("Player/" + action);
        int index = InputBindings.BindingIndex(a, false);
        if (index < 0)
        {
            return null;
        }
        string path = a.bindings[index].effectivePath;
        return string.IsNullOrEmpty(path) ? null : InputControlPath.TryFindControl(kb, path) as ButtonControl;
    }
}
