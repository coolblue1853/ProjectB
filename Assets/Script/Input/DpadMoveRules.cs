/// <summary>십자 이동 옵션 규칙 (IN-1, InputAudit.md §6): 기본 OFF. ON 일 때만 십자 좌우를 이동 입력에 합산한다.
/// 스틱(또는 키보드) 입력이 있으면 스틱 우선, 십자 좌우 동시 입력은 상쇄. ON 이면 퀵슬롯 2·3 패드 입력은 무시</summary>
public static class DpadMoveRules
{
    public const int QuickSlotLeft = 1;
    public const int QuickSlotRight = 2;

    public static float Resolve(bool enabled, float stick, bool dpadLeft, bool dpadRight)
    {
        if (!enabled || stick != 0f || dpadLeft == dpadRight)
        {
            return stick;
        }
        return dpadLeft ? -1f : 1f;
    }

    public static bool BlocksQuickSlot(bool enabled, int slot)
    {
        return enabled && (slot == QuickSlotLeft || slot == QuickSlotRight);
    }
}
