using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 필드 패드 입력 집계(AUTO-49, InputAudit P2·P3·P4·P5). 프레임당 한 번 계산해 소비자가 같은 값을 읽는다.
/// SELECT: 짧게 = 아티팩트 교체, 길게 = 퀘스트 창. L3: 짧게 = 퀵슬롯 1, 길게 = 퀵슬롯 4. LT+SELECT = 도감, LT+START = 지도.
/// 소비자는 읽기 전에 Refresh()를 매 프레임 불러야 한다(제스처 판정이 프레임 폴링이라 빠지면 놓친다).
/// 눌린 순간 UI 가 열려 있었는지(InUi)를 함께 돌려줘, 인벤토리 안 SELECT·L3 동작과 겹치지 않게 한다.
/// </summary>
public static class PadFieldInput
{
    private static int _frame = -1;
    private static PadHoldTracker _select;
    private static PadHoldTracker _l3;
    private static bool _selectInUi;
    private static bool _l3InUi;

    public static PadGesture Select { get; private set; }
    public static bool SelectInUi { get; private set; }
    public static PadGesture L3 { get; private set; }
    public static bool L3InUi { get; private set; }
    public static bool LtSelect { get; private set; }
    public static bool LtStart { get; private set; }

    // 소비자(아티팩트·퀘스트 창 등)가 비활성이어도 홀드 타이머가 흐르도록 매 프레임 Refresh 를 부르는 구동기(ReviewCode_1105b #3)
    private sealed class Driver : MonoBehaviour
    {
        private void Update()
        {
            Refresh();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateDriver()
    {
        var go = new GameObject("PadFieldInputDriver") { hideFlags = HideFlags.HideAndDontSave };
        Object.DontDestroyOnLoad(go);
        go.AddComponent<Driver>();
    }

    /// <summary>이번 프레임 소비자가 공통으로 부르는 갱신(여러 번 불러도 한 번만 계산)</summary>
    public static void Refresh()
    {
        if (_frame == Time.frameCount)
        {
            return;
        }
        _frame = Time.frameCount;
        Select = PadGesture.None;
        L3 = PadGesture.None;
        LtSelect = false;
        LtStart = false;
        Gamepad pad = Gamepad.current;
        if (pad == null)
        {
            _select.Cancel();
            _l3.Cancel();
            return;
        }
        bool lt = pad.leftTrigger.isPressed;
        LtSelect = PadFieldInputRules.IsLtChord(lt, pad.selectButton.wasPressedThisFrame);
        LtStart = PadFieldInputRules.IsLtChord(lt, pad.startButton.wasPressedThisFrame);
        float now = Time.unscaledTime;
        if (pad.selectButton.wasPressedThisFrame)
        {
            _selectInUi = DatabaseManager.isOpenUI;
        }
        if (pad.leftStickButton.wasPressedThisFrame)
        {
            _l3InUi = DatabaseManager.isOpenUI;
        }
        if (lt)
        {
            // 누르는 도중 LT 를 더하면 조합으로 보고 진행 중 제스처를 버린다(SELECT 길게 중 LT → 퀘스트 창 오발동 방지)
            _select.Cancel();
            _l3.Cancel();
            _selectInUi = false; // 취소한 제스처의 UI 상태가 다음 눌림까지 남지 않게(ReviewCode_1105b #4)
        }
        // LT 조합(LT+SELECT 등)은 제스처로 세지 않는다
        Select = _select.Step(pad.selectButton.wasPressedThisFrame && !lt, pad.selectButton.isPressed, now, PadFieldInputRules.QuestHoldSeconds);
        L3 = _l3.Step(pad.leftStickButton.wasPressedThisFrame && !lt, pad.leftStickButton.isPressed, now, PadFieldInputRules.Quick4HoldSeconds);
        SelectInUi = _selectInUi;
        L3InUi = _l3InUi;
    }

    /// <summary>이 프레임 START 가 LT 조합이면 참 — 가방 열기·일시정지가 같이 반응하지 않게 막는 데 쓴다</summary>
    public static bool StartChordThisFrame
    {
        get
        {
            Refresh();
            return LtStart;
        }
    }

    /// <summary>액션이 이번에 패드로 눌렸는가(키보드와 같은 액션이라 장치로 구분)</summary>
    public static bool IsFromGamepad(InputAction action)
    {
        InputControl c = action.activeControl;
        return c != null && c.device is Gamepad;
    }
}
