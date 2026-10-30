using System;

/// <summary>
/// 정밀 단조 3타 판(순수 로직, FloorCoreContent 4.2). 채집 커서 규칙(<see cref="GatherRules"/>)을 재사용한다:
/// 0~100 왕복 커서가 목표 중심에 가까울 때 치면 완벽(±6)=3 / 훌륭(±14)=2 / 좋음(±25)=1 / 빗나감=0.
/// 시간 초과(타당 <see cref="StrikeTimeLimit"/>초)는 빗나감. 재료 소모는 이 판이 끝난 뒤 호출자가 한다.
/// </summary>
public sealed class ForgeSession
{
    public const float StartDelay = 0.35f;   // 열린 입력이 첫 타격으로 읽히지 않게
    public const float StrikeGap = 0.45f;    // 타격 사이 쉬는 시간
    public const float StrikeTimeLimit = 3f; // 타당 제한(3타 합쳐 최대 약 10초)

    private readonly Func<float> _rng;
    private readonly int[] _scores = new int[AdvancedEnhanceRules.Strikes];
    private int _count;
    private float _elapsed;   // 현재 목표가 나온 뒤 경과
    private float _gap = StartDelay;

    public ForgeSession(Func<float> rng)
    {
        _rng = rng;
        Target = GatherRules.TargetCenter(0.5f);
    }

    public float Target { get; private set; }
    public int StrikeCount => _count;
    public bool Done => _count >= AdvancedEnhanceRules.Strikes;
    public bool Waiting => !Done && _gap > 0f;
    public float Cursor => GatherRules.CursorAt(_elapsed);
    public int LastScore { get; private set; }
    public float Remaining => Math.Max(0f, StrikeTimeLimit - _elapsed);

    public int Total
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < _count; i++)
            {
                sum += _scores[i];
            }
            return sum;
        }
    }

    public int ScoreAt(int index)
    {
        return index >= 0 && index < _count ? _scores[index] : -1;
    }

    public void Tick(float dt)
    {
        if (Done)
        {
            return;
        }
        if (_gap > 0f)
        {
            _gap -= dt;
            if (_gap <= 0f)
            {
                Target = GatherRules.TargetCenter(_rng());
                _elapsed = 0f;
            }
            return;
        }
        _elapsed += dt;
        if (_elapsed >= StrikeTimeLimit)
        {
            Record(0);
        }
    }

    /// <summary>타격. 쉬는 중이거나 끝났으면 무시. 점수(0~3) 반환, 무시되면 -1</summary>
    public int Hit()
    {
        if (Done || _gap > 0f)
        {
            return -1;
        }
        int score = AdvancedEnhanceRules.StrikeScore(Cursor, Target);
        Record(score);
        return score;
    }

    private void Record(int score)
    {
        _scores[_count++] = score;
        LastScore = score;
        _gap = StrikeGap;
    }
}
