using UnityEngine;

/// <summary>
/// 상승 부하 (TowerWorld.md 7절, 메이드 인 어비스식). 위로 갈수록 부하 L이 걸리고, 탐험가 칭호로 저항 R을 올린다.
/// 부족분 D = max(0, L − R) → 주는 피해 ×(1 − 0.02D, 최소 0.1), 받는 피해 ×(1 + 0.02D). 내려오면(현재 맵 L만 적용) 즉시 해제.
/// 계산은 순수 함수(테스트), 상태(현재 맵 부하·칭호)는 정적 필드 + ES3.
/// </summary>
public static class AscentLoad
{
    public const int ResistPerRank = 15;
    public const float PenaltyPerPoint = 0.02f, MinOutgoing = 0.1f;
    public const int MaxRank = 7;
    public static readonly string[] RankNames = { "Novice", "Dawn", "Cloudsea", "Deepgreen", "Crimson", "Blackstone", "Abyss", "Whitelight" };
    const string RankKey = "explorer.rank";

    public static int CurrentLoad { get; private set; }
    public static int Rank { get; private set; }
    public static int Resistance => Resist(Rank);
    public static int Deficit => DeficitOf(CurrentLoad, Rank);

    public static event System.Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        CurrentLoad = 0;
        Rank = 0;
        Changed = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        Rank = ES3.KeyExists(RankKey) ? Mathf.Clamp(ES3.Load<int>(RankKey), 0, MaxRank) : 0;
        Portal.MapChanged -= OnMapChanged;
        Portal.MapChanged += OnMapChanged;
    }

    static void OnMapChanged(GameObject openMap)
    {
        MapInfo info = openMap != null ? openMap.GetComponent<MapInfo>() : null;
        SetLoad(info != null ? info.ascentLoad : 0);
    }

    public static void SetLoad(int load)
    {
        CurrentLoad = Mathf.Max(0, load);
        GameLog.Info(GameLog.Cat.Map, "상승 부하 " + CurrentLoad + " / 저항 " + Resistance + " (부족 " + Deficit + ")");
        Changed?.Invoke();
    }

    /// <summary>칭호 승급 (낮아지지 않음). 층계 보스 처치 시 BossLoot가 호출</summary>
    public static bool GrantRank(int rank)
    {
        rank = Mathf.Clamp(rank, 0, MaxRank);
        if (rank <= Rank)
        {
            return false;
        }
        Rank = rank;
        ES3.Save(RankKey, Rank);
        BossBanner.Show("Rank Up", RankName(Rank).ToUpperInvariant() + " EXPLORER", true); // 승급 연출
        GameLog.Info(GameLog.Cat.Player, "탐험가 승급: " + RankName(Rank) + " (" + Rank + ")");
        Changed?.Invoke();
        return true;
    }

    public static string RankName(int rank)
    {
        return RankNames[Mathf.Clamp(rank, 0, MaxRank)];
    }

    // ── 순수 계산 ──
    public static int Resist(int rank) => Mathf.Clamp(rank, 0, MaxRank) * ResistPerRank;
    public static int DeficitOf(int load, int rank) => Mathf.Max(0, load - Resist(rank));
    public static float OutgoingMul(int deficit) => Mathf.Max(MinOutgoing, 1f - PenaltyPerPoint * deficit);
    public static float IncomingMul(int deficit) => 1f + PenaltyPerPoint * deficit;

    public static float OutgoingMul() => OutgoingMul(Deficit);
    public static float IncomingMul() => IncomingMul(Deficit);
}
