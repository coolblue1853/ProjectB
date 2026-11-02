using System.Collections.Generic;
using UnityEngine;

/// <summary>도감 수집 보상 한 단계 (Resources/Floor1BestiaryRewards.csv: id,metric,threshold,gold,exp,item,count,title)</summary>
public sealed class BestiaryMilestoneDef
{
    public string Id;
    public string Metric; // kinds = 처치한 몬스터 종류 수, kills = 누적 처치 수
    public int Threshold;
    public string Title;
    public Floor1Data.Reward Reward;

    public bool Reached(int kinds, int kills)
    {
        return (Metric == "kinds" ? kinds : kills) >= Threshold;
    }
}

public static class BestiaryMilestoneRules
{
    public static List<BestiaryMilestoneDef> Parse(string text)
    {
        var list = new List<BestiaryMilestoneDef>();
        foreach (string[] c in Floor1Data.Rows(text))
        {
            string metric = c.Length > 1 ? c[1].Trim() : "";
            if (c.Length < 8 || c[0].Trim().Length == 0 || (metric != "kinds" && metric != "kills"))
            {
                continue;
            }
            list.Add(new BestiaryMilestoneDef
            {
                Id = c[0].Trim(),
                Metric = metric,
                Threshold = Floor1Data.Int(c[2]),
                Title = c[7].Trim(),
                Reward = new Floor1Data.Reward
                {
                    Gold = Floor1Data.Int(c[3]),
                    Exp = Floor1Data.Int(c[4]),
                    Item = c[5].Trim(),
                    Count = Floor1Data.Int(c[6]),
                    Label = c[0].Trim(),
                },
            });
        }
        return list;
    }

    /// <summary>아직 받지 않았고 조건을 채운 첫 단계 (없으면 null). 낮은 단계부터 하나씩</summary>
    public static BestiaryMilestoneDef NextClaimable(IReadOnlyList<BestiaryMilestoneDef> defs, ICollection<string> claimed, int kinds, int kills)
    {
        foreach (BestiaryMilestoneDef d in defs)
        {
            if (!claimed.Contains(d.Id) && d.Reached(kinds, kills))
            {
                return d;
            }
        }
        return null;
    }
}

/// <summary>
/// 도감 수집 보상: 도감 처치 종류·누적 처치가 단계에 닿으면 자동 지급하고 알림을 띄운다 (받은 단계는 ES3 "f1.bestiary.claimed").
/// 가방이 가득 차면 공간이 날 때까지 보류한다. 켜고 끄기: Floor1Content.csv 의 bestiary.
/// </summary>
public class BestiaryMilestones : MonoBehaviour
{
    public const string SaveKey = "f1.bestiary.claimed";
    private const float CheckInterval = 1f;

    private static BestiaryMilestones s_instance;

    private List<BestiaryMilestoneDef> _defs;
    private readonly HashSet<string> _claimed = new HashSet<string>();
    private float _nextCheck;

    public static IReadOnlyCollection<string> Claimed => s_instance != null ? s_instance._claimed : (IReadOnlyCollection<string>)new string[0];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (s_instance != null || !Floor1Data.On("bestiary"))
        {
            return;
        }
        TextAsset csv = Resources.Load<TextAsset>("Floor1BestiaryRewards");
        if (csv == null)
        {
            return;
        }
        var go = new GameObject("[BestiaryMilestones]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<BestiaryMilestones>();
        s_instance._defs = BestiaryMilestoneRules.Parse(csv.text);
        if (ES3.KeyExists(SaveKey))
        {
            foreach (string id in ES3.Load<List<string>>(SaveKey))
            {
                s_instance._claimed.Add(id);
            }
        }
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextCheck)
        {
            return;
        }
        _nextCheck = Time.unscaledTime + CheckInterval;
        if (PlayerController.instance == null || DatabaseManager.isOpenUI)
        {
            return;
        }
        BestiaryMilestoneDef d = BestiaryMilestoneRules.NextClaimable(_defs, _claimed, BestiaryRecord.KindsKilled, BestiaryRecord.TotalKills);
        if (d == null || !d.Reward.CanReceive())
        {
            return;
        }
        _claimed.Add(d.Id);
        ES3.Save(SaveKey, new List<string>(_claimed));
        d.Reward.Apply();
        PickupLog.Notice("도감 보상 - " + d.Title + ": " + d.Reward.Describe());
    }
}
