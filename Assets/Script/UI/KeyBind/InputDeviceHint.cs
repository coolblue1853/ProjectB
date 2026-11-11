using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// 마지막으로 버튼을 누른 장치가 게임패드인지 기록한다(U31 프롬프트 키·패드 문구 전환용).
/// 마우스·키보드는 모두 "키보드" 로 본다. 기본값은 키보드.
/// </summary>
public static class InputDeviceHint
{
    private static System.IDisposable _sub;

    /// <summary>마지막 버튼 입력이 게임패드였는가</summary>
    public static bool UsingGamepad { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        UsingGamepad = false;
        _sub?.Dispose();
        _sub = InputSystem.onAnyButtonPress.Call(OnButton);
    }

    private static void OnButton(InputControl control)
    {
        UsingGamepad = control.device is Gamepad;
    }
}
