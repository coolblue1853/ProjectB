/// <summary>패드 LT 조합 규칙 (IN-2, InputAudit.md §6): RT 단독 사용처는 LT 가 눌린 동안 무시하고(LT = 방패 방어·조합 접두),
/// 보기 필터는 LT+RT 로 연다. 잠금은 LT+R3 로 유지</summary>
public static class PadChordRules
{
    /// <summary>RT 단독 동작(되사기·창고 G·프리셋 장착)</summary>
    public static bool RtAlone(bool lt, bool rtPressedThisFrame)
    {
        return rtPressedThisFrame && !lt;
    }

    /// <summary>보기 필터 순환(LT+RT)</summary>
    public static bool FilterChord(bool lt, bool rtPressedThisFrame)
    {
        return lt && rtPressedThisFrame;
    }

    /// <summary>잠금 토글(LT+R3)</summary>
    public static bool LockChord(bool lt, bool r3PressedThisFrame)
    {
        return lt && r3PressedThisFrame;
    }
}
