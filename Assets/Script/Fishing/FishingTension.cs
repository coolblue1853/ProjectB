/// <summary>릴링 결과</summary>
public enum ReelOutcome { Reeling, Caught, Snapped, Escaped }

/// <summary>
/// 장력 미니게임 순수 시뮬레이션(MiniGames.md). 누르면 장력 상승·떼면 하강, 물고기 저항 주기마다 추가 상승.
/// 안전 구간에서 누르고 있으면 포획 진행도가 오르고, 느슨함(8 미만 2초 연속)·과장력(90 이상 1초 연속)·35초 초과는 실패.
/// 프레임 시간이 길어도 결과가 같도록 0.05초 단위로 나눠 계산한다.
/// </summary>
public sealed class FishingTension
{
    private const float SubStep = 0.05f;

    private readonly float _resistRate;
    private float _slack;
    private float _over;

    public float Tension { get; private set; }
    public float Progress { get; private set; }
    public float Elapsed { get; private set; }
    public float SafeMin { get; }
    public float SafeMax { get; }
    public ReelOutcome Outcome { get; private set; }

    public FishingTension(FishRarity rarity, RodDef rod)
    {
        FishingRules.SafeZone(rarity, rod, out float min, out float max);
        SafeMin = min;
        SafeMax = max;
        _resistRate = FishingRules.ResistRate(rarity);
        Tension = FishingRules.TensionStart;
    }

    /// <summary>지금 물고기가 저항 중인가(주기 첫 0.8초)</summary>
    public bool Resisting => Elapsed % FishingRules.ResistCycle < FishingRules.ResistDuration;

    public bool InSafeZone => Tension >= SafeMin && Tension <= SafeMax;

    public ReelOutcome Step(float dt, bool holding)
    {
        while (dt > 0f && Outcome == ReelOutcome.Reeling)
        {
            float h = dt < SubStep ? dt : SubStep;
            dt -= h;
            StepOnce(h, holding);
        }
        return Outcome;
    }

    private void StepOnce(float dt, bool holding)
    {
        float rate = (holding ? FishingRules.HoldRate : FishingRules.ReleaseRate) + (Resisting ? _resistRate : 0f);
        Elapsed += dt;
        Tension = Clamp(Tension + rate * dt, 0f, FishingRules.TensionMax);
        if (holding)
        {
            float p = InSafeZone ? FishingRules.ProgressInSafe : FishingRules.ProgressOutSafe;
            Progress = Clamp(Progress + p * dt, 0f, FishingRules.ProgressGoal);
        }
        _slack = Tension < FishingRules.SlackTension ? _slack + dt : 0f;
        _over = Tension >= FishingRules.SnapTension ? _over + dt : 0f;

        if (Progress >= FishingRules.ProgressGoal)
        {
            Outcome = ReelOutcome.Caught;
        }
        else if (_over >= FishingRules.SnapLimit)
        {
            Outcome = ReelOutcome.Snapped;
        }
        else if (_slack >= FishingRules.SlackLimit || Elapsed > FishingRules.ReelTimeLimit)
        {
            Outcome = ReelOutcome.Escaped;
        }
    }

    private static float Clamp(float v, float min, float max)
    {
        return v < min ? min : (v > max ? max : v);
    }
}
