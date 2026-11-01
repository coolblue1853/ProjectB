using System;
using System.Collections.Generic;

/// <summary>
/// 이름 붙은 UI 입력 잠금 (PlayerStateDesign Phase 4 일부). 창마다 자기 InputLock 토큰을 가져서,
/// 한 창을 닫아도 다른 창이 연 잠금은 유지된다. 읽기는 기존 DatabaseManager.isOpenUI(= Has(InputLock)) 그대로.
/// 레거시 UI(인벤토리·창고·상점)는 함께 열리고 닫혀 "InvGroup" 하나, 제작은 "Craft". isOpenUI 는 읽기 전용(B-114).
/// </summary>
public static class UiLock
{
    private static readonly List<string> order = new List<string>();
    private static readonly UiBackFrame backFrame = new UiBackFrame();

    public static void CaptureBackFrame()
    {
        backFrame.Begin(UnityEngine.Time.frameCount, order.Count > 0 ? order[order.Count - 1] : null);
    }

    /// <summary>프레임 시작 시 맨 위 창만 BACK을 한 번 소비한다. 닫힌 뒤에도 대상은 바뀌지 않는다.</summary>
    public static bool TryBack(string owner, bool pressed)
    {
        CaptureBackFrame();
        return pressed && backFrame.TryConsume(owner);
    }

    static readonly Dictionary<string, IDisposable> tokens = new Dictionary<string, IDisposable>();

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        tokens.Clear();
        order.Clear();
        backFrame.Reset();
    }

    public static void Set(string owner, bool locked)
    {
        if (locked)
        {
            if (!tokens.ContainsKey(owner))
            {
                tokens[owner] = PlayerStatus.Acquire(StatusFlag.InputLock, "UI:" + owner);
                order.Add(owner);
            }
        }
        else if (tokens.TryGetValue(owner, out IDisposable t))
        {
            t.Dispose();
            tokens.Remove(owner);
            order.Remove(owner);
        }
    }

    public static bool IsHeld(string owner) => tokens.ContainsKey(owner);
}
