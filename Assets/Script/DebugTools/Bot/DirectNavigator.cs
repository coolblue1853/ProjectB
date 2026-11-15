using UnityEngine;

/// <summary>
/// B1 이동: 목표 쪽으로 직진. 목표가 위에 있고 가까우면 점프, 막히면 점프 → 반대로 걷기(BotStuck). 로프·포털 길찾기는 B2(IBotNavigator 교체).
/// </summary>
public sealed class DirectNavigator : IBotNavigator
{
    public const float ReverseSeconds = 1.5f;
    public const float ArriveDx = 0.3f;
    public const float MaxJumpDy = 4f;      // 이보다 높은 발판은 점프로 못 간다(로프·길찾기는 B2)

    private readonly BotStuck _stuck = new BotStuck();
    private float _reverseUntil;
    private int _reverseDir;
    private bool _started;
    private float _lastSteer;

    public int StuckJumps { get; private set; }
    public int Reverses { get; private set; }

    public void Reset()
    {
        _started = false;
        _reverseUntil = 0f;
    }

    public bool Steer(BotContext ctx, float dx, float dy, ref BotIntent intent)
    {
        PlayObservation o = ctx.Obs;
        if (!_started || ctx.Now - _lastSteer > 0.3f)
        {
            _stuck.Reset(ctx.Now, o.X); // 공격하느라 멈췄다 다시 걸을 때 옛 기준점으로 막힘 판정하지 않게
            _started = true;
        }
        _lastSteer = ctx.Now;
        if (ctx.Now < _reverseUntil)
        {
            intent.MoveX = _reverseDir;
            return false;
        }
        intent.MoveX = Mathf.Abs(dx) < ArriveDx ? 0 : dx > 0f ? 1 : -1; // 바로 위·아래면 좌우로 떨지 않는다
        if (dy > 1.5f && dy < MaxJumpDy && Mathf.Abs(dx) < 2.5f && o.Grounded)
        {
            intent.Jump = true; // 위 발판의 목표
        }
        bool moveLocked = o.Flags.IndexOf('M') >= 0; // 공격·스킬 이동 잠금(MoveLock) 중 제자리는 막힘이 아니다(11-15 B2 오탐 12건)
        switch (_stuck.Update(ctx.Now, o.X, intent.MoveX != 0 && !moveLocked))
        {
            case BotStuck.Act.Jump:
                intent.MoveX = 0; // 턱에 붙인 채 밀면 계단 모서리에서 점프가 안 나간다(남은 점프 0) — 떼고 뛴다
                intent.Jump = true;
                StuckJumps++;
                return false;
            case BotStuck.Act.Reverse:
                _reverseDir = -intent.MoveX;
                _reverseUntil = ctx.Now + ReverseSeconds;
                intent.MoveX = _reverseDir;
                Reverses++;
                return false;
        }
        return true;
    }

    public bool BlockedAhead(BotContext ctx, int dir)
    {
        return false;
    }

    public bool TravelTo(BotContext ctx, string map, ref BotIntent intent)
    {
        return false; // 길찾기는 GraphNavigator
    }
}
