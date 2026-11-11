using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>도감 수집 보상 한 단계 (Resources/Floor1BestiaryRewards.csv: id,metric,threshold,gold,exp,item,count,title)</summary>
public sealed class BestiaryMilestoneDef
{
    public string Id;
    public string Metric; // kinds = 처치한 몬스터 종류 수, kills = 누적 처치 수, zone = Zone 의 일반 몬스터 전종 처치(B08~B13)
    public string Zone;   // zone 단계 전용: MonsterSheet zone 값('|' 로 여러 개). 문서의 9번째 열
    public int Threshold;
    public string Title;
    public Floor1Data.Reward Reward;

    public bool Reached(int kinds, int kills)
    {
        return Reached(kinds, kills, null);
    }

    public bool Reached(int kinds, int kills, Func<string, bool> zoneCleared)
    {
        if (Metric == "zone")
        {
            return zoneCleared != null && zoneCleared(Zone);
        }
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
            if (c.Length < 8 || c[0].Trim().Length == 0 || (metric != "kinds" && metric != "kills" && metric != "zone"))
            {
                continue;
            }
            list.Add(new BestiaryMilestoneDef
            {
                Id = c[0].Trim(),
                Metric = metric,
                Threshold = Floor1Data.Int(c[2]),
                Title = c[7].Trim(),
                Zone = c.Length > 8 ? c[8].Trim() : "",
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
        return NextClaimable(defs, claimed, kinds, kills, null);
    }

    public static BestiaryMilestoneDef NextClaimable(IReadOnlyList<BestiaryMilestoneDef> defs, ICollection<string> claimed, int kinds, int kills, Func<string, bool> zoneCleared)
    {
        foreach (BestiaryMilestoneDef d in defs)
        {
            if (!claimed.Contains(d.Id) && d.Reached(kinds, kills, zoneCleared))
            {
                return d;
            }
        }
        return null;
    }
}

/// <summary>구역 전종 처치 판정 (B08~B13). 종 목록은 MonsterSheet 에서 zone 값이 정확히 같은 1층계 행(밤·보스·미니보스 표기가 붙은 행 제외)</summary>
public static class BestiaryZoneRules
{
    private static readonly Dictionary<string, List<string>> s_species = new Dictionary<string, List<string>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_species.Clear();
    }

    /// <summary>zoneKey('|' 로 여러 zone) 의 일반 몬스터 id 목록 (캐시)</summary>
    public static List<string> SpeciesOf(string zoneKey)
    {
        if (s_species.TryGetValue(zoneKey, out List<string> cached))
        {
            return cached;
        }
        var zones = new HashSet<string>(zoneKey.Split('|'));
        var list = new List<string>();
        foreach (MonsterTable.Row r in MonsterTable.All())
        {
            if (r.floor == 1 && zones.Contains(r.zone))
            {
                list.Add(r.id);
            }
        }
        if (list.Count > 0)
        {
            s_species[zoneKey] = list; // 빈 목록(테이블 로드 전)은 캐시하지 않는다
        }
        return list;
    }

    /// <summary>종이 하나 이상이고 전부 한 번 이상 처치했으면 true</summary>
    public static bool Cleared(IReadOnlyList<string> species, Func<string, int> killsOf)
    {
        if (species == null || species.Count == 0)
        {
            return false;
        }
        foreach (string id in species)
        {
            if (killsOf(id) <= 0)
            {
                return false;
            }
        }
        return true;
    }

    public static bool Cleared(string zoneKey)
    {
        return !string.IsNullOrEmpty(zoneKey) && Cleared(SpeciesOf(zoneKey), BestiaryRecord.Kills);
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
    private Cooldown _check;

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
        if (!_check.Ready(Time.unscaledTime))
        {
            return;
        }
        _check.Arm(Time.unscaledTime, CheckInterval);
        if (PlayerController.instance == null || DatabaseManager.isOpenUI)
        {
            return;
        }
        BestiaryMilestoneDef d = BestiaryMilestoneRules.NextClaimable(_defs, _claimed, BestiaryRecord.KindsKilled, BestiaryRecord.TotalKills, BestiaryZoneRules.Cleared);
        if (d == null || !d.Reward.CanReceive())
        {
            return;
        }
        _claimed.Add(d.Id);
        ES3.Save(SaveKey, new List<string>(_claimed));
        d.Reward.Apply();
        ExtraEquipRewards.OnBestiaryMilestone(d.Id); // 도감 15종(B04) → 칭호 슬롯 해금 (SLOT-3)
        PickupLog.Notice(Loc.T("Bestiary reward - ") + d.Title + ": " + d.Reward.Describe());
    }
}
