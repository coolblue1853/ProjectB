using System;

public enum NestPhase { Idle, Hunting, NestRevealed, Success, TimedOut, Cancelled }

/// <summary>
/// ② 정예 둥지 추적 순수 규칙·수치(FloorCoreContent.md 1절, MiniGames.md 흔적 추적의 상위판).
/// 흔적 지점 3곳을 순서대로 조사하면 둥지 표식이 나타나고, 둥지를 조사하면 정예가 확정 출현한다.
/// 제한 시간은 첫 흔적을 조사한 순간부터 센다. 수치는 Q-445(임시 가정).
/// </summary>
public static class NestHuntRules
{
    public const int Trails = 3;                 // 흔적 3개 → 둥지(표식 번호 3)
    public const int Marks = Trails + 1;
    public const float TimeLimit = 50f;          // 40~60초의 가운데
    public const float InvestigateTime = 0.8f;   // 표식 하나를 살피는 시간(이동하면 중단)
    public const float SuccessCooldown = 600f;   // 맵별 10분(흔적 추적과 같은 값)
    public const float FailCooldown = 180f;      // 실패·중단 3분
    public const float EliteLifetime = 120f;     // 출현한 정예가 안 잡히면 사라지는 시간
    public const float HintStep = 5f;            // 거리 힌트 반올림 단위(m)
    public const float HereRange = 4f;           // 이 안이면 "바로 앞"

    /// <summary>from → to 의 방향(−1 왼쪽, +1 오른쪽)과 5m 단위 거리. 거리가 HereRange 안이면 0 방향(바로 앞)</summary>
    public static void Hint(float fromX, float toX, out int dir, out int meters)
    {
        float dx = toX - fromX;
        float abs = Math.Abs(dx);
        if (abs <= HereRange)
        {
            dir = 0;
            meters = 0;
            return;
        }
        dir = dx < 0f ? -1 : 1;
        meters = (int)(Math.Round(abs / HintStep, MidpointRounding.AwayFromZero) * HintStep);
        if (meters < (int)HintStep)
        {
            meters = (int)HintStep;
        }
    }
}

/// <summary>
/// 둥지 추적 한 판의 상태 머신(순수). 표식 번호 0~2 가 흔적, 3 이 둥지. 다음 번호만 조사할 수 있다.
/// 시간은 첫 흔적(0번) 조사 때부터 흐르고, 제한 시간이 지나면 TimedOut.
/// </summary>
public sealed class NestHuntState
{
    public NestPhase Phase { get; private set; }
    /// <summary>조사를 마친 표식 수(= 다음에 조사할 표식 번호)</summary>
    public int Found { get; private set; }
    public float Elapsed { get; private set; }
    public float Remaining => Math.Max(0f, NestHuntRules.TimeLimit - Elapsed);
    public bool Running => Phase == NestPhase.Hunting || Phase == NestPhase.NestRevealed;
    public bool Finished => Phase == NestPhase.Success || Phase == NestPhase.TimedOut || Phase == NestPhase.Cancelled;

    /// <summary>index 번 표식을 지금 조사할 수 있는가(처음이면 0번, 진행 중이면 다음 번호만)</summary>
    public bool CanInvestigate(int index)
    {
        return (Phase == NestPhase.Idle || Running) && index == Found && index < NestHuntRules.Marks;
    }

    /// <summary>조사 완료 보고. 순서가 맞으면 true</summary>
    public bool Investigate(int index)
    {
        if (!CanInvestigate(index))
        {
            return false;
        }
        Found++;
        if (index == NestHuntRules.Trails)
        {
            Phase = NestPhase.Success;
        }
        else
        {
            Phase = Found == NestHuntRules.Trails ? NestPhase.NestRevealed : NestPhase.Hunting;
        }
        return true;
    }

    public void Tick(float dt)
    {
        if (!Running)
        {
            return;
        }
        Elapsed += dt;
        if (Elapsed >= NestHuntRules.TimeLimit)
        {
            Phase = NestPhase.TimedOut;
        }
    }

    /// <summary>도중 중단(맵 이탈 등). 끝난 판은 무시</summary>
    public void Cancel()
    {
        if (Running)
        {
            Phase = NestPhase.Cancelled;
        }
    }
}
