using UnityEngine;

/// <summary>
/// 하루 시계(밤→낮 전환마다 +1). 보스 재처치 보상 감소가 "같은 날"을 가르는 데 쓴다. ES3 "f1.day" (추가 전용 키).
/// 하루의 기준은 BountyBoard 와 같은 TimeChange 밤→낮 전환이지만 저장은 따로 둔다(사냥 의뢰를 꺼도 동작).
/// </summary>
public class Floor1Clock : MonoBehaviour
{
    public const string SaveKey = "f1.day";
    private static Floor1Clock s_instance;
    private static int s_day = -1;
    private bool _wasDay = true;
    private bool _init;

    public static int Day
    {
        get
        {
            if (s_day < 0)
            {
                s_day = SaveUtil.LoadOr(SaveKey, 1);
            }
            return s_day;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
        s_day = -1;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (s_instance != null)
        {
            return;
        }
        var go = new GameObject("[Floor1Clock]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<Floor1Clock>();
    }

    private void Update()
    {
        TimeChange tc = TimeChange.instance;
        if (tc == null)
        {
            return;
        }
        if (!_init)
        {
            _init = true;
            _wasDay = tc.isDaytime;
        }
        if (!_wasDay && tc.isDaytime)
        {
            s_day = Day + 1;
            ES3.Save(SaveKey, s_day);
        }
        _wasDay = tc.isDaytime;
    }

    /// <summary>검증용: 하루를 넘긴다</summary>
    public static void DebugNextDay()
    {
        s_day = Day + 1;
        ES3.Save(SaveKey, s_day);
    }
}

/// <summary>보스 재처치 보상 감소 순수 규칙 (테스트 대상). 같은 날 같은 보스: 1번째 100%, 2번째 50%, 3번째부터 25%</summary>
public static class BossRetryRules
{
    public static float Multiplier(int killsToday)
    {
        if (killsToday <= 0)
        {
            return 1f;
        }
        return killsToday == 1 ? 0.5f : 0.25f;
    }

    /// <summary>강화석 개수: 줄어들어도 최소 1개</summary>
    public static int Stones(int baseStones, float multiplier)
    {
        return Mathf.Max(1, Mathf.CeilToInt(baseStones * multiplier));
    }

    /// <summary>세트 부위 확정 지급 여부: 100% 이면 확정, 아니면 multiplier 확률</summary>
    public static bool GivesSetPiece(float multiplier, float roll01)
    {
        return multiplier >= 1f || roll01 < multiplier;
    }

    /// <summary>고유템 확률: 기본 확률 × multiplier</summary>
    public static float UniqueChance(float baseChance, float multiplier)
    {
        return baseChance * multiplier;
    }

    /// <summary>고유템 천장(처치 수) 카운트에 이번 처치를 셀지: 보상이 절반 이상일 때만</summary>
    public static bool CountsForPity(float multiplier)
    {
        return multiplier >= 0.5f;
    }

    public static string Describe(float multiplier)
    {
        return Mathf.RoundToInt(multiplier * 100f) + "%";
    }
}

/// <summary>보스 이름별 "오늘 처치 수" 기록. ES3 "f1.bossToday.보스" = "날짜:횟수" (날짜가 바뀌면 0 으로 본다)</summary>
public static class BossRetry
{
    private static string Key(string boss)
    {
        return "f1.bossToday." + boss;
    }

    public static bool Enabled => Floor1Data.On("bossretry");

    public static int KillsToday(string boss)
    {
        if (!Enabled || !ES3.KeyExists(Key(boss)))
        {
            return 0;
        }
        string[] p = ES3.Load<string>(Key(boss)).Split(':');
        return p.Length == 2 && int.TryParse(p[0], out int day) && int.TryParse(p[1], out int n) && day == Floor1Clock.Day ? n : 0;
    }

    public static void Record(string boss)
    {
        if (!Enabled)
        {
            return;
        }
        ES3.Save(Key(boss), Floor1Clock.Day + ":" + (KillsToday(boss) + 1));
    }
}
