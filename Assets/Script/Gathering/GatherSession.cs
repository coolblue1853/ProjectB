using System;

public enum GatherResult { None, Success, Failed, TimedOut, Cancelled }

/// <summary>
/// 채집·채굴 한 번(순수 상태 머신). 입력은 매 틱 pressed(이번 프레임에 ↑ 누름)만 받는다. 보상 지급은 GatherNode 몫.
/// </summary>
public abstract class GatherSession
{
    protected readonly GatherDef[] Table;
    protected readonly Func<float> Random;

    public bool Done { get; private set; }
    public GatherResult Result { get; private set; }
    /// <summary>시작 뒤 경과(결과가 나면 결과 뒤 경과)</summary>
    public float Elapsed { get; private set; }
    public GatherDef Item { get; private set; }
    public int Count { get; private set; }

    public abstract GatherKind Kind { get; }
    public abstract float TimeLimit { get; }

    protected GatherSession(GatherDef[] table, Func<float> random)
    {
        Table = table;
        Random = random;
    }

    public void Tick(float dt, bool pressed)
    {
        Elapsed += dt;
        if (Done)
        {
            return;
        }
        Step(dt);
        if (pressed && OnPress())
        {
            return;
        }
        if (!Done && Elapsed >= TimeLimit)
        {
            Finish(GatherResult.TimedOut);
        }
    }

    /// <summary>도중 중단(이동·피격·사망·이탈). 결과가 났으면 무시</summary>
    public void Cancel()
    {
        if (!Done)
        {
            Finish(GatherResult.Cancelled);
        }
    }

    /// <summary>매 틱 진행(누름 처리 전)</summary>
    protected virtual void Step(float dt)
    {
    }

    /// <summary>↑ 누름 처리. 세션이 끝났으면 true</summary>
    protected abstract bool OnPress();

    protected void Succeed(float rBonus, float uBonus, int extraCount)
    {
        Item = Table[GatherRules.Roll(Table, Random(), rBonus, uBonus)];
        Count = GatherRules.RollCount(Item, Random()) + extraCount;
        Finish(GatherResult.Success);
    }

    protected void Finish(GatherResult result)
    {
        Result = result;
        Done = true;
        Elapsed = 0f;
    }
}

/// <summary>④ 표본 채집: 0~100 왕복 커서를 목표 구간에서 ↑ 로 멈춘다. 완벽 = 수량 +1, 좋음 = 기본, 빗나감 = 시도 소모(3회)</summary>
public sealed class SpecimenSession : GatherSession
{
    private float _cursorTime;
    private float _pauseLeft;

    public float Target { get; private set; }
    public int AttemptsUsed { get; private set; }
    public SpecimenJudge LastJudge { get; private set; }
    public float Cursor => GatherRules.CursorAt(_cursorTime);
    public bool Paused => _pauseLeft > 0f;

    public override GatherKind Kind => GatherKind.Specimen;
    public override float TimeLimit => GatherRules.SpecimenTimeLimit;

    public SpecimenSession(GatherDef[] table, Func<float> random) : base(table, random)
    {
        Target = GatherRules.TargetCenter(Random());
    }

    /// <summary>커서는 빗나간 뒤 멈춤 동안 서 있다</summary>
    protected override void Step(float dt)
    {
        if (_pauseLeft > 0f)
        {
            _pauseLeft -= dt;
            return;
        }
        _cursorTime += dt;
    }

    protected override bool OnPress()
    {
        if (_pauseLeft > 0f)
        {
            return false;
        }
        AttemptsUsed++;
        LastJudge = GatherRules.Judge(Cursor, Target);
        if (LastJudge != SpecimenJudge.Miss)
        {
            Succeed(0f, 0f, LastJudge == SpecimenJudge.Perfect ? GatherRules.PerfectBonusCount : 0);
            return true;
        }
        if (AttemptsUsed >= GatherRules.SpecimenAttempts)
        {
            Finish(GatherResult.Failed);
            return true;
        }
        _pauseLeft = GatherRules.MissPause;
        Target = GatherRules.TargetCenter(Random());
        for (int i = 0; i < 4 && Math.Abs(Cursor - Target) <= GatherRules.GoodHalf; i++)
        {
            Target = GatherRules.TargetCenter(Random()); // 멈춘 커서 바로 위에 새 목표가 나오면 공짜 '좋음'이라 다시 뽑는다
        }
        return false;
    }
}

/// <summary>⑤ 광맥 채굴: ↑ 마다 곡괭이질. 약점이 빛날 때(정타) 균열 +2, 아니면 +1. 균열 5 = 파괴, 정타율로 R/U 확률 보정</summary>
public sealed class MiningSession : GatherSession
{
    private float _lastSwing = -999f;

    public int Cracks { get; private set; }
    public int Swings { get; private set; }
    public int Crits { get; private set; }
    public bool LastWasCrit { get; private set; }
    public float CritRate => Swings > 0 ? (float)Crits / Swings : 0f;
    public bool Glowing => GatherRules.IsCritTime(Elapsed);

    public override GatherKind Kind => GatherKind.Ore;
    public override float TimeLimit => GatherRules.MineTimeLimit;

    public MiningSession(GatherDef[] table, Func<float> random) : base(table, random)
    {
    }

    protected override bool OnPress()
    {
        if (Elapsed - _lastSwing < GatherRules.SwingInterval)
        {
            return false;
        }
        _lastSwing = Elapsed;
        Swings++;
        LastWasCrit = GatherRules.IsCritTime(Elapsed);
        if (LastWasCrit)
        {
            Crits++;
        }
        Cracks += LastWasCrit ? GatherRules.CrackCrit : GatherRules.CrackNormal;
        if (Cracks < GatherRules.CrackGoal)
        {
            return false;
        }
        Cracks = GatherRules.CrackGoal;
        GatherRules.RareBonus(CritRate, out float r, out float u);
        Succeed(r, u, 0);
        return true;
    }
}
