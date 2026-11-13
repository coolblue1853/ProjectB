using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 효과 상세 토글(REWORK-12 P6). 켜면 툴팁·장비 상세·세트 창의 "특수 효과" 줄 아래에 종류·적용 대상·비활성/상한 사유가 펼쳐진다.
/// 입력: 키보드 Tab / 패드 Y(North) / 마우스는 호버 툴팁이 떠 있을 때 같은 키. 창고가 열려 있으면 Tab 은 창고 이동 전환이라 양보한다.
/// 상태는 세션 한정(저장 안 함, ES3 키 없음).
/// </summary>
public static class EffectDetailMode
{
    private static int _frame = -1;

    public static bool On { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        On = false;
        _frame = -1;
    }

    public static bool Toggle()
    {
        On = !On;
        return On;
    }

    /// <summary>이번 프레임 토글 키가 눌렸으면 토글하고 true. 한 프레임에 한 번만(호버·상세창이 같이 불러도)</summary>
    public static bool Poll()
    {
        if (_frame == Time.frameCount)
        {
            return false;
        }
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        if (!((kb != null && kb.tabKey.wasPressedThisFrame) || (pad != null && pad.buttonNorth.wasPressedThisFrame)))
        {
            return false;
        }
        _frame = Time.frameCount;
        Toggle();
        return true;
    }

    /// <summary>창고 창이 열려 있으면 Tab 은 창고 이동 전환용 — 토글 입력을 받지 않는다</summary>
    public static bool InputAllowed()
    {
        InventoryManager inv = InventoryManager.instance;
        return inv == null || inv.chest == null || inv.chest.chestOb == null || !inv.chest.chestOb.activeSelf;
    }
}
