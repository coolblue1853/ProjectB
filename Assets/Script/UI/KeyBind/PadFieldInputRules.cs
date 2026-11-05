/// <summary>패드 제스처 결과(탭 = 짧게 눌렀다 뗌, 홀드 = 기준 시간 넘게 누름 — 한 번만)</summary>
public enum PadGesture
{
    None,
    Tap,
    Hold,
}

/// <summary>
/// 버튼 하나의 짧게/길게 판정(AUTO-49). 눌림 시각만 쓰는 순수 구조체라 EditMode 에서 시간을 주입해 검증한다.
/// </summary>
public struct PadHoldTracker
{
    private bool _active;
    private bool _fired;
    private float _start;

    /// <summary>프레임마다 한 번 호출. 홀드는 기준 시간이 지난 프레임에 한 번, 탭은 홀드가 안 나간 채 뗀 프레임에 한 번</summary>
    public PadGesture Step(bool pressedNow, bool isPressed, float time, float holdSeconds)
    {
        if (pressedNow)
        {
            _active = true;
            _fired = false;
            _start = time;
        }
        if (!_active)
        {
            return PadGesture.None;
        }
        if (!isPressed)
        {
            _active = false;
            return _fired ? PadGesture.None : PadGesture.Tap;
        }
        if (!_fired && time - _start >= holdSeconds)
        {
            _fired = true;
            return PadGesture.Hold;
        }
        return PadGesture.None;
    }

    public void Cancel()
    {
        _active = false;
    }
}

/// <summary>패드 필드 입력 상수와 순수 규칙(InputAudit P2·P3·P4·P5·P8)</summary>
public static class PadFieldInputRules
{
    /// <summary>퀘스트 창: SELECT 길게</summary>
    public const float QuestHoldSeconds = 0.8f;

    /// <summary>퀵슬롯 4: L3 길게(짧게 = 퀵슬롯 1)</summary>
    public const float Quick4HoldSeconds = 0.5f;

    /// <summary>퀵슬롯 4 칸 인덱스</summary>
    public const int Quick4Slot = 3;

    /// <summary>탭 순환: cur 에서 dir(+1/-1)만큼 이동, 양 끝은 반대쪽으로 넘어간다. count 가 1 이하면 0</summary>
    public static int StepTab(int cur, int count, int dir)
    {
        if (count <= 1)
        {
            return 0;
        }
        int next = (cur + dir) % count;
        return next < 0 ? next + count : next;
    }

    /// <summary>LT 를 누른 채 다른 버튼을 눌렀는가(조합 입력). LT 단독 동작과 구분</summary>
    public static bool IsLtChord(bool ltHeld, bool buttonPressedThisFrame)
    {
        return ltHeld && buttonPressedThisFrame;
    }
}
