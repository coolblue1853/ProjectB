/// <summary>
/// 입력 버퍼: 버튼을 조금 일찍 눌러도(window초 이내) 행동이 가능해지는 순간 실행되게 한다.
/// 예) 착지 0.1초 전에 누른 점프, 후딜이 끝나기 직전에 누른 평타. 시간은 인자로 받아 테스트 가능.
/// </summary>
public sealed class InputBuffer
{
    readonly float window;
    float pressedAt = float.NegativeInfinity;

    public InputBuffer(float window)
    {
        this.window = window;
    }

    public void Press(float now) => pressedAt = now;

    public bool Peek(float now) => now - pressedAt <= window;

    /// <summary>버퍼된 입력이 있으면 소모하고 true</summary>
    public bool Consume(float now)
    {
        if (!Peek(now))
        {
            return false;
        }
        pressedAt = float.NegativeInfinity;
        return true;
    }

    public void Clear() => pressedAt = float.NegativeInfinity;
}

/// <summary>
/// 코요테 타임: 발판에서 걸어 내려간 직후 window초 동안은 아직 땅 위로 취급한다(지상 점프 허용).
/// </summary>
public sealed class CoyoteTimer
{
    readonly float window;
    bool grounded;
    float leftGroundAt = float.NegativeInfinity;
    bool jumpedSinceLeaving;

    public CoyoteTimer(float window)
    {
        this.window = window;
    }

    public void SetGrounded(bool isGrounded, float now)
    {
        if (grounded && !isGrounded)
        {
            leftGroundAt = now;
        }
        if (isGrounded)
        {
            jumpedSinceLeaving = false;
        }
        grounded = isGrounded;
    }

    public bool Grounded => grounded;

    /// <summary>땅을 떠났지만(점프가 아니라 낙하로) 아직 코요테 시간 안인지</summary>
    public bool InCoyote(float now) => !grounded && !jumpedSinceLeaving && now - leftGroundAt <= window;

    /// <summary>낙하로 떠난 뒤 코요테 시간이 지났는지 (메이플식: 이때부터 공중 점프 1회만 남김)</summary>
    public bool FellPastCoyote(float now) => !grounded && !jumpedSinceLeaving && now - leftGroundAt > window;

    public void NotifyJumped() => jumpedSinceLeaving = true;
}
