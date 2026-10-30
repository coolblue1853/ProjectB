using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 키 설정(리바인딩) 저장·적용 (BasicFeatures.md 5절).
/// - 편집은 항상 비활성인 마스터 KeyAction(Master)에 하고, Commit 때 오버라이드 JSON 을 ES3 settings.bindings(공용 파일)에 저장한다.
/// - 게임 곳곳의 `new KeyAction()` 인스턴스는 InputSystem.onActionChange 로 처음 켜질 때 잡아서 다음 프레임에 같은 오버라이드를 입힌다
///   (생성 지점 20여 곳을 고치지 않기 위해). 저장된 오버라이드가 바뀌면 살아 있는 인스턴스 전부에 다시 입힌다.
/// - 재지정 대상은 Player 맵의 Rebindable 액션만. 이동(Move)·ESC·ENTER 는 고정 키라 겹치면 거절, 재지정 대상끼리 겹치면 서로 바꾼다.
/// </summary>
public static class InputBindings
{
    public const string SaveKey = "settings.bindings";
    public const string AssetName = "KeyAction";
    public const string KeyboardLayout = "<Keyboard>";
    public const string GamepadLayout = "<Gamepad>";

    public static readonly string[] Rebindable =
    {
        "Jump", "Dash", "Run", "Attack", "SkillA", "SkillS", "SkillD", "SkillF",
        "Quick1", "Quick2", "Quick3", "Quick4", "Artifact", "ArtifactNext",
    };

    // ESC·ENTER·패드 Start(일시정지) + 하드코딩·UI 단축키(도감 B, 지도 M). UI 맵의 인벤 열기·제작·포털 키는 IsReserved 가 에셋에서 읽는다
    private static readonly string[] ReservedPaths = { "<Keyboard>/escape", "<Keyboard>/enter", "<Gamepad>/start", "<Keyboard>/b", "<Keyboard>/m" };
    private static readonly string[] ReservedActions = { "UI/OpenInventory", "UI/OpenCraft", "Portal/PortalOn" };

    /// <summary>오버라이드가 바뀌었다(키 표시 갱신용)</summary>
    public static event Action Changed;

    private static readonly HashSet<InputActionAsset> _live = new HashSet<InputActionAsset>(); // 켜져 있는 인스턴스만(꺼지면 빠짐)
    private static readonly List<InputActionAsset> _pending = new List<InputActionAsset>();
    private static KeyAction _master;
    private static string _json = "";
    private static bool _hooked;
    private static bool _applying; // 오버라이드 적용(재해석) 중 생기는 켜짐/꺼짐 알림은 무시한다 — 다시 대기열에 넣으면 무한 반복

    public enum AssignResult { Ok, Swapped, Reserved, Invalid }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        if (_hooked)
        {
            InputSystem.onActionChange -= OnActionChange;
            _hooked = false;
        }
        _live.Clear();
        _pending.Clear();
        _applying = false;
        _master?.Dispose();
        _master = null;
        _json = "";
        Changed = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        _json = LoadSaved();
        if (!_hooked)
        {
            InputSystem.onActionChange += OnActionChange;
            _hooked = true;
        }
        var go = new GameObject("[InputBindings]");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<InputBindingsRunner>();
    }

    private static string LoadSaved()
    {
        try
        {
            return ES3.KeyExists(SaveKey, SaveSlots.SharedFile) ? ES3.Load<string>(SaveKey, SaveSlots.SharedFile) : "";
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "키 설정 읽기 실패, 기본 키 사용: " + e.Message);
            return "";
        }
    }

    /// <summary>편집·표시용 마스터(항상 비활성). 저장된 오버라이드가 입혀져 있다</summary>
    public static KeyAction Master
    {
        get
        {
            if (_master == null)
            {
                _master = new KeyAction();
                _applying = true;
                try
                {
                    ApplyTo(_master.asset, _json);
                }
                finally
                {
                    _applying = false;
                }
            }
            return _master;
        }
    }

    private static void OnActionChange(object obj, InputActionChange change)
    {
        if (_applying)
        {
            return;
        }
        bool enabled = change == InputActionChange.ActionMapEnabled || change == InputActionChange.ActionEnabled;
        bool disabled = change == InputActionChange.ActionMapDisabled || change == InputActionChange.ActionDisabled;
        if (!enabled && !disabled)
        {
            return;
        }
        InputActionAsset asset = null;
        if (obj is InputActionMap map)
        {
            asset = map.asset;
        }
        else if (obj is InputAction action && action.actionMap != null)
        {
            asset = action.actionMap.asset;
        }
        if (asset == null)
        {
            return;
        }
        if (disabled)
        {
            // 다 꺼진 인스턴스는 놓는다(Dispose 하지 않는 생성 지점이 많아 붙잡고 있으면 계속 쌓임). 다시 켜지면 다시 잡혀 최신 키가 입혀진다
            if (!asset.enabled)
            {
                _live.Remove(asset);
            }
            return;
        }
        if (_live.Contains(asset) || (_master != null && asset == _master.asset) || asset.name != AssetName)
        {
            return;
        }
        _live.Add(asset);
        _pending.Add(asset); // 켜지는 콜백 안에서 바인딩을 바꾸지 않고 다음 Update 에 입힌다
    }

    /// <summary>새로 잡힌 인스턴스에 오버라이드를 입힌다(InputBindingsRunner 가 매 프레임 부름, 대기 없으면 즉시 반환)</summary>
    internal static void FlushPending()
    {
        if (_pending.Count == 0)
        {
            return;
        }
        // 복사본으로 돌고, 적용 중 알림은 _applying 으로 막는다(적용이 액션을 껐다 켜며 다시 대기열에 넣던 무한 반복 방지)
        InputActionAsset[] batch = _pending.ToArray();
        _pending.Clear();
        _applying = true;
        try
        {
            foreach (InputActionAsset a in batch)
            {
                if (a != null)
                {
                    ApplyTo(a, _json);
                }
            }
        }
        finally
        {
            _applying = false;
        }
    }

    /// <summary>오버라이드 적용. JSON 이 깨졌으면 기본 키로 두고 저장값을 버린다(매 프레임 예외 방지)</summary>
    private static void ApplyTo(InputActionAsset asset, string json)
    {
        asset.RemoveAllBindingOverrides();
        if (string.IsNullOrEmpty(json))
        {
            return;
        }
        try
        {
            asset.LoadBindingOverridesFromJson(json);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "키 설정 적용 실패, 기본 키 사용: " + e.Message);
            asset.RemoveAllBindingOverrides();
            if (json == _json)
            {
                _json = "";
            }
        }
    }

    /// <summary>마스터의 현재 오버라이드를 저장하고 살아 있는 인스턴스 전부에 입힌다</summary>
    public static void Commit()
    {
        _json = Master.asset.SaveBindingOverridesAsJson();
        try
        {
            ES3.Save(SaveKey, _json, SaveSlots.SharedFile);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "키 설정 저장 실패(이번 실행에만 적용): " + e.Message);
        }
        // 리바인딩 완료 콜백(입력 이벤트 처리 중)에서 불리므로 살아 있는 인스턴스에는 다음 Update 에 입힌다
        _live.RemoveWhere(a => a == null);
        foreach (InputActionAsset a in _live)
        {
            if (!_pending.Contains(a))
            {
                _pending.Add(a);
            }
        }
        GameLog.Info(GameLog.Cat.Player, "키 설정 저장");
        Changed?.Invoke();
    }

    /// <summary>기본 키로 되돌리고 저장</summary>
    public static void ResetToDefaults()
    {
        Master.asset.RemoveAllBindingOverrides();
        Commit();
    }

    /// <summary>action 의 키보드/게임패드 바인딩 칸 번호. 게임패드는 경로가 빈 칸(미지정)도 포함. 없으면 -1</summary>
    public static int BindingIndex(InputAction action, bool gamepad)
    {
        if (action == null)
        {
            return -1;
        }
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding b = action.bindings[i];
            if (b.isComposite || b.isPartOfComposite)
            {
                continue;
            }
            string path = b.path ?? "";
            if (gamepad ? (path.StartsWith(GamepadLayout) || path.Length == 0) : path.StartsWith(KeyboardLayout))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>표시 문자열(미지정이면 "-")</summary>
    public static string Display(string actionName, bool gamepad)
    {
        InputAction action = Master.asset.FindAction("Player/" + actionName);
        int i = BindingIndex(action, gamepad);
        if (i < 0)
        {
            return "-";
        }
        string s = action.GetBindingDisplayString(i);
        if (string.IsNullOrEmpty(s))
        {
            return "-";
        }
        // 칸이 좁아 긴 패드 이름은 줄인다(Left Stick Press → LS)
        return s.Replace("Left Stick Press", "LS").Replace("Right Stick Press", "RS");
    }

    /// <summary>
    /// asset 의 Player/actionName 키보드(또는 게임패드) 칸을 newPath 로 바꾼다(오버라이드, 저장은 하지 않음).
    /// 다른 재지정 액션과 겹치면 그 액션에 내 이전 키를 준다(Swapped, swappedWith 에 이름). 고정 키와 겹치면 Reserved.
    /// </summary>
    public static AssignResult Assign(InputActionAsset asset, string actionName, bool gamepad, string newPath, out string swappedWith)
    {
        swappedWith = null;
        InputActionMap map = asset != null ? asset.FindActionMap("Player") : null;
        InputAction action = map != null ? map.FindAction(actionName) : null;
        int index = BindingIndex(action, gamepad);
        if (index < 0 || string.IsNullOrEmpty(newPath) || Array.IndexOf(Rebindable, actionName) < 0)
        {
            return AssignResult.Invalid;
        }
        if (IsReserved(map, newPath))
        {
            return AssignResult.Reserved;
        }
        string oldPath = action.bindings[index].effectivePath ?? "";
        if (SamePath(oldPath, newPath))
        {
            return AssignResult.Ok;
        }
        foreach (string other in Rebindable)
        {
            if (other == actionName)
            {
                continue;
            }
            InputAction o = map.FindAction(other);
            int oi = BindingIndex(o, gamepad);
            if (oi >= 0 && SamePath(o.bindings[oi].effectivePath, newPath))
            {
                o.ApplyBindingOverride(oi, oldPath);
                swappedWith = other;
                break;
            }
        }
        action.ApplyBindingOverride(index, newPath);
        return swappedWith != null ? AssignResult.Swapped : AssignResult.Ok;
    }

    private static bool IsReserved(InputActionMap map, string path)
    {
        foreach (string r in ReservedPaths)
        {
            if (SamePath(r, path))
            {
                return true;
            }
        }
        foreach (string ra in ReservedActions)
        {
            InputAction a = map.asset != null ? map.asset.FindAction(ra) : null;
            if (a == null)
            {
                continue;
            }
            foreach (InputBinding b in a.bindings)
            {
                if (SamePath(b.effectivePath, path))
                {
                    return true; // 인벤 열기·제작·포털
                }
            }
        }
        foreach (InputBinding b in map.bindings)
        {
            if (b.isComposite || Array.IndexOf(Rebindable, b.action) >= 0)
            {
                continue;
            }
            if (SamePath(b.effectivePath, path))
            {
                return true; // 이동 등 고정 키
            }
        }
        return false;
    }

    private static bool SamePath(string a, string b)
    {
        return !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b) && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
