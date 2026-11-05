/// <summary>
/// 패드 LT 충돌 규칙(AUTO-46, Q-INPUT). 방패 장착 중에는 LT 가 방어 전용이라 SkillS 의 LT 입력은 무시한다. 키보드 S 는 그대로.
/// </summary>
public static class GuardInputRules
{
    /// <summary>SkillS 슬롯 인덱스(AttackManager.skillAction)</summary>
    public const int SkillSSlot = 1;

    /// <summary>이 스킬 입력을 무시해야 하는가</summary>
    public static bool SuppressSkillTrigger(int slot, bool shieldEquipped, bool fromLeftTrigger)
    {
        return slot == SkillSSlot && shieldEquipped && fromLeftTrigger;
    }
}
