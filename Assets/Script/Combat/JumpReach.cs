using UnityEngine;

/// <summary>
/// 점프 도달 높이 순수 계산(MAP-SCAN 11-04). 지형 계단/발판 높이가 단일 점프로 넘을 수 있는지 판정하는 기준.
/// PlayerController: jumpForce=13, gravityScale=3, Physics2D 중력 -9.81, 가변 점프 CombatRules(JumpCutMul 0.7·JumpMinHold 0.08).
/// </summary>
public static class JumpReach
{
    public const float GravityAccel = 9.81f * 3f;

    /// <summary>키를 끝까지 누른 점프 한 번의 최고 높이</summary>
    public static float FullHeight(float jumpForce)
    {
        return jumpForce * jumpForce / (2f * GravityAccel);
    }

    /// <summary>키를 최소 홀드(0.08초)만 누르고 뗀 점프(탭)의 최고 높이: 홀드 동안 상승 + 남은 속도의 절반으로 상승</summary>
    public static float TapHeight(float jumpForce)
    {
        float t = CombatRules.JumpMinHold;
        float rise = jumpForce * t - 0.5f * GravityAccel * t * t;
        float vy = (jumpForce - GravityAccel * t) * CombatRules.JumpCutMul;
        return rise + vy * vy / (2f * GravityAccel);
    }

    /// <summary>탭 점프로 높이 step 의 계단을 오를 수 있는가(콜라이더 모서리 여유 margin 포함)</summary>
    public static bool CanStepWithTap(float jumpForce, float step, float margin = 0.2f)
    {
        return TapHeight(jumpForce) >= step + margin;
    }
}
