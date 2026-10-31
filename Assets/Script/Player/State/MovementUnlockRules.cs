using UnityEngine;

/// <summary>Q-446 이동 개방 순수 규칙. 공격·장비 스킬은 제한하지 않는다.</summary>
public static class MovementUnlockRules
{
    public static bool GroundDash(int floor, int brooch) { return floor >= 2 || brooch >= 1; }
    public static bool DoubleJump(int floor) { return floor >= 2; }
    public static bool AirTurn(int floor) { return floor >= 2; }
    public static bool Glide(int floor) { return floor >= 2; }
    public static bool AirDash(int floor) { return floor >= 3; }

    public static int JumpLimit(int configured, int floor, bool debugAll)
    {
        return DoubleJump(floor) || debugAll ? Mathf.Clamp(configured, 1, 2) : 1;
    }

    public static bool CanDash(int floor, int brooch, bool grounded, bool debugAll)
    {
        return debugAll || (grounded ? GroundDash(floor, brooch) : AirDash(floor));
    }

    // 반대 공중 입력은 제동까지만 허용한다.
    public static float AirInput(float input, float launchDirection, bool airborne, bool unlocked)
    {
        return airborne && !unlocked && input * launchDirection < 0f ? 0f : input;
    }
}
