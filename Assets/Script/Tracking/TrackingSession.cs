using System;

public enum TrackPhase { Trail, Trap, Done }

public enum TrackResult { None, Success, TooManyMistakes, TimedOut, Cancelled }

/// <summary>
/// 흔적 추적 한 판(순수 상태 머신). 방향 선택(Choose)·덫 설치(SetTrap)·틱(Tick)만 받는다. 소환·보상은 TrackingSpot 몫.
/// 흔적 판별 4단계(오답은 그 방향을 지우고 같은 단계에서 다시 고름, 3번째 오답 = 실패, 45초) → 덫(커서 왕복, 5초 안 누르면 보통) → 성공.
/// </summary>
public sealed class TrackingSession
{
    private readonly Func<float> _random;
    private readonly bool[] _crossed = new bool[TrackingRules.Options];
    private TraceOption[] _options;

    public string Target { get; }
    public TrackPhase Phase { get; private set; }
    public int Stage { get; private set; }
    public int Mistakes { get; private set; }
    public int Correct { get; private set; }
    public TrapQuality Quality { get; private set; }
    public TrackResult Result { get; private set; }
    public bool Done => Phase == TrackPhase.Done;
    /// <summary>흔적 판별 경과(덫 단계에서는 멈춤)</summary>
    public float Elapsed { get; private set; }
    /// <summary>덫 단계 경과</summary>
    public float TrapElapsed { get; private set; }
    /// <summary>결과가 난 뒤 경과</summary>
    public float ResultElapsed { get; private set; }
    public float TrapCursor => TrackingRules.TrapCursor(TrapElapsed);

    public TrackingSession(string targetTrace, Func<float> random)
    {
        Target = targetTrace;
        _random = random;
        NextStage();
    }

    public TraceOption OptionAt(int dir)
    {
        return _options[dir];
    }

    public bool IsCrossed(int dir)
    {
        return _crossed[dir];
    }

    public void Tick(float dt)
    {
        switch (Phase)
        {
            case TrackPhase.Trail:
                Elapsed += dt;
                if (Elapsed >= TrackingRules.TimeLimit)
                {
                    Finish(TrackResult.TimedOut);
                }
                break;
            case TrackPhase.Trap:
                TrapElapsed += dt;
                if (TrapElapsed >= TrackingRules.TrapTimeout)
                {
                    Quality = TrapQuality.Normal;
                    Finish(TrackResult.Success);
                }
                break;
            default:
                ResultElapsed += dt;
                break;
        }
    }

    /// <summary>방향 선택(0 = ←, 1 = ↑, 2 = →). 정답이면 true. 지운 방향·다른 단계면 무시(false)</summary>
    public bool Choose(int dir)
    {
        if (Phase != TrackPhase.Trail || dir < 0 || dir >= TrackingRules.Options || _crossed[dir])
        {
            return false;
        }
        if (dir != Correct)
        {
            _crossed[dir] = true;
            Mistakes++;
            if (Mistakes >= TrackingRules.MistakeLimit)
            {
                Finish(TrackResult.TooManyMistakes);
            }
            return false;
        }
        Stage++;
        if (Stage >= TrackingRules.Stages)
        {
            Phase = TrackPhase.Trap;
        }
        else
        {
            NextStage();
        }
        return true;
    }

    /// <summary>덫 설치(커서 위치로 품질 판정) → 성공</summary>
    public void SetTrap()
    {
        if (Phase != TrackPhase.Trap)
        {
            return;
        }
        Quality = TrackingRules.JudgeTrap(TrapCursor);
        Finish(TrackResult.Success);
    }

    /// <summary>도중 중단(ESC·피격·사망·이탈). 결과가 났으면 무시</summary>
    public void Cancel()
    {
        if (!Done)
        {
            Finish(TrackResult.Cancelled);
        }
    }

    private void NextStage()
    {
        _options = TrackingRules.MakeStage(Target, _random, out int correct);
        Correct = correct;
        Array.Clear(_crossed, 0, _crossed.Length);
    }

    private void Finish(TrackResult result)
    {
        Result = result;
        Phase = TrackPhase.Done;
        ResultElapsed = 0f;
    }
}
