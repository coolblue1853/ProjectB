/// <summary>
/// 남은 시간 타이머(DedupAudit #2). Start=덮어쓰기, Extend=더 큰 값 유지, Tick=0 밑으로 안 내려감. 순수 구조체.
/// </summary>
public struct CountdownTimer
{
    private float _remaining;

    public float Remaining => _remaining;
    public bool Active => _remaining > 0f;

    /// <summary>남은 시간을 seconds 로 덮어쓴다(재시작).</summary>
    public void Start(float seconds)
    {
        _remaining = seconds;
    }

    /// <summary>남은 시간과 seconds 중 큰 값 유지(짧게 연장해도 줄지 않음).</summary>
    public void Extend(float seconds)
    {
        if (seconds > _remaining)
        {
            _remaining = seconds;
        }
    }

    public void Tick(float dt)
    {
        if (_remaining > 0f)
        {
            _remaining = _remaining - dt > 0f ? _remaining - dt : 0f;
        }
    }

    public void Clear()
    {
        _remaining = 0f;
    }
}
