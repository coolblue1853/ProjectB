using System;
using System.Collections.Generic;

/// <summary>
/// 이름 붙은 UI 입력 잠금 (PlayerStateDesign Phase 4 일부). 창마다 자기 InputLock 토큰을 가져서,
/// 한 창을 닫아도 다른 창이 연 잠금은 유지된다. 읽기는 기존 DatabaseManager.isOpenUI(= Has(InputLock)) 그대로.
/// 레거시 UI(인벤토리·창고·상점)는 함께 열리고 닫혀 "InvGroup" 하나, 제작은 "Craft". isOpenUI 는 읽기 전용(B-114).
/// </summary>
public static class UiLock
{
    static readonly Dictionary<string, IDisposable> tokens = new Dictionary<string, IDisposable>();

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        tokens.Clear();
    }

    public static void Set(string owner, bool locked)
    {
        if (locked)
        {
            if (!tokens.ContainsKey(owner))
            {
                tokens[owner] = PlayerStatus.Acquire(StatusFlag.InputLock, "UI:" + owner);
            }
        }
        else if (tokens.TryGetValue(owner, out IDisposable t))
        {
            t.Dispose();
            tokens.Remove(owner);
        }
    }

    public static bool IsHeld(string owner) => tokens.ContainsKey(owner);
}
