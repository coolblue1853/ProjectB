/// <summary>피해 가로채기 공급자. 방어 중인 무기 기믹이 PlayerGuard.Source 로 등록한다.</summary>
public interface IGuardSource
{
    /// <summary>방어 중이면 damage 를 줄이고 true. 아니면 변경 없이 false.</summary>
    /// <param name="parried">패링으로 처리됐는가(true 면 짧은 무적·반격)</param>
    bool Intercept(ref int damage, out bool parried);
}

/// <summary>
/// damage2Player 앞단 공용 가로채기 지점(AUTO-15). 방패·F2-GUARD 가 같은 훅을 쓴다.
/// </summary>
public static class PlayerGuard
{
    public static IGuardSource Source { get; set; }

    /// <summary>parried: 이번 가로채기가 패링이었는가(PlayerHealthManager 가 짧은 무적·반격 처리). 가로채지 않았으면 false</summary>
    public static bool TryIntercept(ref int damage, out bool parried)
    {
        parried = false;
        return Source != null && Source.Intercept(ref damage, out parried);
    }

    public static bool TryIntercept(ref int damage)
    {
        return TryIntercept(ref damage, out _);
    }
}
