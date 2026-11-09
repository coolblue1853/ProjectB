using System;
using System.Collections.Generic;

/// <summary>마을 사냥 의뢰 한 건 (Resources/Floor1Bounties.csv: id,zone,title,targets,count,minLevel,exp,gold,item,itemCount)</summary>
public sealed class BountyDef
{
    public string Id;
    public string Zone;
    public string Title;
    public string[] Targets;
    public int Count;
    public int MinLevel;
    public Floor1Data.Reward Reward;

    public bool Matches(string monsterId)
    {
        return Array.IndexOf(Targets, monsterId) >= 0;
    }
}

/// <summary>사냥 의뢰 진행 상태 (ES3 "f1.bounty" 한 덩어리). 하루(밤→낮 전환 1회)마다 3건이 새로 걸리고, 한 번에 하나만 받을 수 있다</summary>
[Serializable]
public class BountyState
{
    public int Day = 1;
    public List<string> Offered = new List<string>();
    public List<string> Done = new List<string>();
    public string Active = "";
    public int Progress;
}

public static class BountyRules
{
    public const int OfferCount = 3;

    public static List<BountyDef> Parse(string text)
    {
        var list = new List<BountyDef>();
        foreach (string[] c in Floor1Data.Rows(text))
        {
            if (c.Length < 10 || c[0].Trim().Length == 0)
            {
                continue;
            }
            list.Add(new BountyDef
            {
                Id = c[0].Trim(),
                Zone = c[1].Trim(),
                Title = c[2].Trim(),
                Targets = c[3].Trim().Split('|'),
                Count = Math.Max(1, Floor1Data.Int(c[4])),
                MinLevel = Floor1Data.Int(c[5]),
                Reward = new Floor1Data.Reward
                {
                    Exp = Floor1Data.Int(c[6]),
                    Gold = Floor1Data.Int(c[7]),
                    Item = c[8].Trim(),
                    Count = Floor1Data.Int(c[9]),
                    Label = c[0].Trim(),
                },
            });
        }
        return list;
    }

    /// <summary>그날의 의뢰 목록: 레벨 조건을 채운 것 중 날짜 시드로 섞어 앞에서 3건 (같은 날·같은 조건이면 항상 같다)</summary>
    public static List<string> PickOffers(IReadOnlyList<BountyDef> defs, int day, int level)
    {
        var pool = new List<BountyDef>();
        foreach (BountyDef d in defs)
        {
            if (d.MinLevel <= level)
            {
                pool.Add(d);
            }
        }
        var rng = new Random(day * 7919 + 13);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            BountyDef t = pool[i];
            pool[i] = pool[j];
            pool[j] = t;
        }
        var ids = new List<string>();
        for (int i = 0; i < pool.Count && ids.Count < OfferCount; i++)
        {
            ids.Add(pool[i].Id);
        }
        return ids;
    }

    /// <summary>새 하루: 목록을 다시 뽑고 완료·진행 기록을 비운다. 진행 중이던 의뢰는 넘어가지 않는다</summary>
    public static void NewDay(BountyState s, IReadOnlyList<BountyDef> defs, int day, int level)
    {
        s.Day = day;
        s.Offered = PickOffers(defs, day, level);
        s.Done.Clear();
        s.Active = "";
        s.Progress = 0;
    }

    public static BountyDef Find(IReadOnlyList<BountyDef> defs, string id)
    {
        foreach (BountyDef d in defs)
        {
            if (d.Id == id)
            {
                return d;
            }
        }
        return null;
    }

    public static bool CanAccept(BountyState s, string id)
    {
        return s.Active.Length == 0 && s.Offered.Contains(id) && !s.Done.Contains(id);
    }

    public static bool Accept(BountyState s, string id)
    {
        if (!CanAccept(s, id))
        {
            return false;
        }
        s.Active = id;
        s.Progress = 0;
        return true;
    }

    /// <summary>포기: 진행 중인 이 의뢰만 해제한다(오늘 목록 Offered 는 유지, Done 에 넣지 않는다)</summary>
    public static bool Abandon(BountyState s, string id)
    {
        if (s.Active != id || id.Length == 0)
        {
            return false;
        }
        s.Active = "";
        s.Progress = 0;
        return true;
    }

    /// <summary>처치 1건 반영. 진행 중 의뢰의 목표 몬스터면 true</summary>
    public static bool OnKill(BountyState s, IReadOnlyList<BountyDef> defs, string monsterId)
    {
        BountyDef d = s.Active.Length > 0 ? Find(defs, s.Active) : null;
        if (d == null || !d.Matches(monsterId) || s.Progress >= d.Count)
        {
            return false;
        }
        s.Progress++;
        return true;
    }

    public static bool IsReady(BountyState s, BountyDef d)
    {
        return d != null && s.Active == d.Id && s.Progress >= d.Count;
    }

    /// <summary>보고 완료 처리 (보상 지급은 호출한 쪽). 완료 목록에 올리고 진행 중을 비운다</summary>
    public static void Complete(BountyState s, string id)
    {
        s.Done.Add(id);
        s.Active = "";
        s.Progress = 0;
    }

    /// <summary>추적 HUD 두 줄: 의뢰 제목 / 진행 n/m 또는 보고 안내. 의뢰가 없으면 빈 문자열</summary>
    public static string TrackerLines(BountyState s, BountyDef d)
    {
        if (d == null || s.Active != d.Id)
        {
            return "";
        }
        bool ready = IsReady(s, d);
        string head = (ready ? "<color=#FFD633>" : "<color=#FFFFFF>") + "[의뢰] " + d.Title + "</color>\n";
        string body = ready ? "완료! 마을 게시대에서 보고" : "처치 " + s.Progress + "/" + d.Count;
        return head + "<color=#BBBBBB>  " + body + "</color>\n";
    }

    /// <summary>한 줄 상태 문구: 완료됨 / 진행 n/m / 보고 가능 / 수락 가능</summary>
    public static string StatusOf(BountyState s, BountyDef d)
    {
        if (s.Done.Contains(d.Id))
        {
            return "완료";
        }
        if (s.Active == d.Id)
        {
            return IsReady(s, d) ? "보고 가능" : "진행 " + s.Progress + "/" + d.Count;
        }
        return s.Active.Length > 0 ? "다른 의뢰 진행 중" : "수락 가능";
    }
}
