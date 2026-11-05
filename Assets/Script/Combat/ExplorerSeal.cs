using UnityEngine;

/// <summary>
/// 탐사 인장 (FloorTransition.md FT-SEAL, Floor2Plan.md §3.3). 구역 보스 첫 처치마다 +3 상승 저항. ES3 "explorer.seals"(개수, 추가 전용 키).
/// 보스 쪽은 BossReward.grantsSeal 이 첫 처치에 Grant 를 부른다. 계산은 순수 함수(테스트), 개수는 정적 필드 + ES3.
/// </summary>
public static class ExplorerSeal
{
    public const string Key = "explorer.seals";
    public const int ResistPerSeal = 3;
    /// <summary>층계당 최대 4개 × 임시 3층계 = 12 (3층계 이후 늘릴 때 상수만 고친다)</summary>
    public const int MaxSeals = 12;

    public static int Count { get; private set; }
    public static int Bonus => BonusOf(Count);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Count = 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        Count = ES3.KeyExists(Key) ? Mathf.Clamp(ES3.Load<int>(Key), 0, MaxSeals) : 0;
        AscentLoad.Refresh();
    }

    /// <summary>세이브 초기화 때 호출(현재 호출부 없음 - 초기화 기능 추가 시 연결). 메모리 개수만 0, ES3 키 삭제는 호출 측 몫</summary>
    public static void ResetCount()
    {
        Count = 0;
        AscentLoad.Refresh();
    }

    /// <summary>인장 1개 획득. 상한이면 false</summary>
    public static bool Grant()
    {
        if (Count >= MaxSeals)
        {
            return false;
        }
        Count++;
        ES3.Save(Key, Count);
        GameLog.Info(GameLog.Cat.Player, "탐사 인장 +1 (" + Count + "/" + MaxSeals + ", 저항 +" + Bonus + ")");
        AscentLoad.Refresh();
        return true;
    }

    // ── 순수 계산 ──
    public static int BonusOf(int seals) => Mathf.Clamp(seals, 0, MaxSeals) * ResistPerSeal;
}
