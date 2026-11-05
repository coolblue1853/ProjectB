/// <summary>
/// 다음 허용 시각 쿨다운(DedupAudit #3). 시각은 호출부가 주입한다(Time.time 등). 순수 구조체.
/// 기본값은 시각 0 부터 준비 상태.
/// now 는 한 객체 안에서 같은 시계(예: 전부 Time.time 또는 전부 unscaledTime)로 주입한다 — 섞으면 비교가 어긋난다.
/// </summary>
public struct Cooldown
{
    private float _readyAt;

    /// <summary>다음 허용 시각</summary>
    public float ReadyAt => _readyAt;

    /// <summary>now 가 허용 시각 이상이면 준비됨(경계 포함).</summary>
    public bool Ready(float now)
    {
        return now >= _readyAt;
    }

    /// <summary>now 부터 interval 초 뒤로 다음 허용 시각을 잡는다.</summary>
    public void Arm(float now, float interval)
    {
        _readyAt = now + interval;
    }
}
