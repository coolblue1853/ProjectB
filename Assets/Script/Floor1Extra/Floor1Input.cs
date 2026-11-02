using UnityEngine;

/// <summary>1층계 보강 콘텐츠가 같이 쓰는 ↑ 입력 (사용처가 하나라도 켜져 있을 때만 활성, Acquire/Release 짝)</summary>
public static class Floor1Input
{
    private static KeyAction s_input;
    private static int s_users;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        if (s_input != null)
        {
            s_input.Disable();
            s_input.Dispose();
        }
        s_input = null;
        s_users = 0;
    }

    public static void Acquire()
    {
        if (s_users++ == 0)
        {
            if (s_input == null)
            {
                s_input = new KeyAction();
            }
            s_input.UI.Up.Enable();
        }
    }

    public static void Release()
    {
        if (s_users > 0 && --s_users == 0 && s_input != null)
        {
            s_input.UI.Up.Disable();
        }
    }

    public static bool UpPressed
    {
        get
        {
            return s_input != null && Time.timeScale > 0f && s_input.UI.Up.WasPressedThisFrame();
        }
    }
}
