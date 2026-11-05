/// <summary>피해 가로채기 공급자. 방어 중인 무기 기믹이 PlayerGuard.Source 로 등록한다.</summary>
public interface IGuardSource
{
    /// <summary>방어 중이면 damage 를 줄이고 true. 아니면 변경 없이 false.</summary>
    bool Intercept(ref int damage);
}

/// <summary>
/// damage2Player 앞단 공용 가로채기 지점(AUTO-15). 방패·F2-GUARD 가 같은 훅을 쓴다.
/// </summary>
public static class PlayerGuard
{
    public static IGuardSource Source { get; set; }

    /// <summary>직전 가로채기가 패링이었는가(PlayerHealthManager 가 짧은 무적·반격 처리)</summary>
    public static bool LastParried { get; set; }

    public static bool TryIntercept(ref int damage)
    {
        return Source != null && Source.Intercept(ref damage);
    }
}
