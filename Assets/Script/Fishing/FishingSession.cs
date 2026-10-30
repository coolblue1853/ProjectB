using System;

public enum FishingPhase { Casting, Waiting, Bite, Reeling, Result }

public enum FishingResult { None, Caught, Snapped, Escaped, MissedBite, ReeledEmpty, Cancelled }

/// <summary>
/// 낚시 한 번의 상태 머신(순수): 캐스팅(홀드) → 대기 → 입질(0.6초 안에 누름) → 장력 → 결과.
/// 입력은 매 틱 held(누르고 있음)·pressed(이번 프레임에 누름)로 받는다. 보상 지급은 하지 않는다(FishingSpot 몫).
/// </summary>
public sealed class FishingSession
{
    private readonly RodDef _rod;
    private readonly FishDef[] _table;
    private readonly Func<float> _random;
    private float _waitTarget;

    public FishingPhase Phase { get; private set; }
    public FishingResult Result { get; private set; }
    public CastQuality Cast { get; private set; }
    public float PhaseTime { get; private set; }
    public FishDef Fish { get; private set; }
    public int Count { get; private set; }
    public FishingTension Tension { get; private set; }

    /// <summary>random: 0~1 난수 공급자(테스트에서 고정값)</summary>
    public FishingSession(RodDef rod, FishDef[] table, Func<float> random)
    {
        _rod = rod;
        _table = table;
        _random = random;
        Phase = FishingPhase.Casting;
    }

    public void Tick(float dt, bool held, bool pressed)
    {
        PhaseTime += dt;
        switch (Phase)
        {
            case FishingPhase.Casting:
                if (!held || PhaseTime >= FishingRules.CastMax)
                {
                    Cast = FishingRules.EvaluateCast(PhaseTime);
                    _waitTarget = FishingRules.WaitSeconds(_random(), Cast, _rod);
                    Enter(FishingPhase.Waiting);
                }
                break;
            case FishingPhase.Waiting:
                if (pressed)
                {
                    Finish(FishingResult.ReeledEmpty); // 입질 전에 당기면 빈 바늘
                }
                else if (PhaseTime >= _waitTarget)
                {
                    Enter(FishingPhase.Bite);
                }
                break;
            case FishingPhase.Bite:
                if (pressed)
                {
                    Hook();
                }
                else if (PhaseTime > FishingRules.BiteWindow)
                {
                    Finish(FishingResult.MissedBite);
                }
                break;
            case FishingPhase.Reeling:
                ReelOutcome o = Tension.Step(dt, held);
                if (o == ReelOutcome.Caught)
                {
                    Finish(FishingResult.Caught);
                }
                else if (o == ReelOutcome.Snapped)
                {
                    Finish(FishingResult.Snapped);
                }
                else if (o == ReelOutcome.Escaped)
                {
                    Finish(FishingResult.Escaped);
                }
                break;
        }
    }

    /// <summary>도중 중단(이동·피격·사망). 결과 단계면 무시</summary>
    public void Cancel()
    {
        if (Phase != FishingPhase.Result)
        {
            Finish(FishingResult.Cancelled);
        }
    }

    private void Hook()
    {
        int index = FishingRules.RollFish(_table, _random(), _rod, Cast);
        Fish = _table[index];
        Count = FishingRules.RollCount(Fish, _random());
        Tension = new FishingTension(Fish.Rarity, _rod);
        Enter(FishingPhase.Reeling);
    }

    private void Finish(FishingResult result)
    {
        Result = result;
        Enter(FishingPhase.Result);
    }

    private void Enter(FishingPhase phase)
    {
        Phase = phase;
        PhaseTime = 0f;
    }
}
