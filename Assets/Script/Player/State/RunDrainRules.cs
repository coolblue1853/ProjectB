/// <summary>달리기 스태미나 소모 규칙(순수). 입력이 잠기거나(UiLock·대화·창) 이동 입력이 없으면 소모하지 않는다.</summary>
public static class RunDrainRules
{
    public const float MoveThreshold = 0.1f;

    public static bool ShouldDrain(bool inputLocked, bool onLadder, float moveInput)
    {
        if (inputLocked)
        {
            return false;
        }
        return onLadder || System.Math.Abs(moveInput) >= MoveThreshold;
    }
}
