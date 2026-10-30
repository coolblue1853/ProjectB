using UnityEngine;

/// <summary>요리 버프 종류(MiniGames.md "탐험가 요리 구현"). ItemSheet effectOb 이름: ascent / gather / atk / fish</summary>
public enum FoodBuffKind { None, Ascent, Gather, Attack, FishRare }

/// <summary>
/// 요리 버프(한 번에 1종). 새 요리를 먹으면 교체되고, 같은 종류면 시간만 갱신된다. 기존 방어 음식 버프(AttackManager.foodBuff)와도 하나만 유지한다.
/// 연결된 효과: 공격력(증뎀 가산, 이 클래스가 모디파이어 관리) · 상승 부하 저항(AscentLoad.Deficit) · 낚시 희귀 확률(FishingSpot).
/// 채집 속도는 값만 보관하고 아직 효과가 없다(Docs/OpenQuestions.md Q-443).
/// </summary>
public static class FoodBuff
{
    public const string AttackModId = "Buff:FoodAttack";

    private static FoodBuffKind _kind;
    private static float _power;
    private static float _expireAt;

    public static FoodBuffKind Kind => IsActive ? _kind : FoodBuffKind.None;
    public static float Power => IsActive ? _power : 0f;
    public static float Remaining => IsActive ? _expireAt - Time.time : 0f;
    private static bool IsActive => _kind != FoodBuffKind.None && Time.time < _expireAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _kind = FoodBuffKind.None;
        _power = 0f;
        _expireAt = 0f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        var go = new GameObject("[FoodBuff]");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<Driver>();
    }

    /// <summary>effectOb 이름 → 종류. 맞는 이름이 아니면 false</summary>
    public static bool TryParseKind(string effectName, out FoodBuffKind kind)
    {
        switch (effectName)
        {
            case "ascent": kind = FoodBuffKind.Ascent; return true;
            case "gather": kind = FoodBuffKind.Gather; return true;
            case "atk": kind = FoodBuffKind.Attack; return true;
            case "fish": kind = FoodBuffKind.FishRare; return true;
            default: kind = FoodBuffKind.None; return false;
        }
    }

    /// <summary>버프 적용(교체). power 는 % (낚시는 %p), seconds 는 지속 시간</summary>
    public static void Apply(FoodBuffKind kind, float power, float seconds)
    {
        Clear();
        _kind = kind;
        _power = power;
        _expireAt = Time.time + seconds;
        if (kind == FoodBuffKind.Attack)
        {
            PlayerStats.Add(new StatModifier(StatId.IncDmg, ModType.Add, power) { Id = AttackModId, Owner = "FoodBuff" });
        }
        NotifyAscent(kind);
        GameLog.Info(GameLog.Cat.Item, "FoodBuff " + kind + " +" + power + " " + seconds + "s");
    }

    /// <summary>해제(만료·교체)</summary>
    public static void Clear()
    {
        FoodBuffKind old = _kind;
        _kind = FoodBuffKind.None;
        _power = 0f;
        _expireAt = 0f;
        PlayerStats.RemoveById(AttackModId);
        NotifyAscent(old);
    }

    /// <summary>해당 종류의 버프가 켜져 있으면 값, 아니면 0</summary>
    public static float Bonus(FoodBuffKind kind)
    {
        return IsActive && _kind == kind ? _power : 0f;
    }

    /// <summary>상승 부하 저항 버프: 맵 부하의 power% 만큼 부족분을 줄인다(반올림)</summary>
    public static int AscentReduction(int load)
    {
        return Mathf.RoundToInt(load * Bonus(FoodBuffKind.Ascent) / 100f);
    }

    private static void NotifyAscent(FoodBuffKind kind)
    {
        if (kind == FoodBuffKind.Ascent)
        {
            AscentLoad.Refresh();
        }
    }

    private sealed class Driver : MonoBehaviour
    {
        private void Update()
        {
            if (_kind != FoodBuffKind.None && Time.time >= _expireAt)
            {
                Clear();
            }
        }
    }
}
