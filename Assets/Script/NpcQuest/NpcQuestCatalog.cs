using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>NPC 퀘스트 목표 종류 (Resources/QuestsFloor1.csv objective 열)</summary>
public enum NpcQuestObjective
{
    Talk,
    Kill,
    Collect,
    Craft,
    Visit,
}

/// <summary>NPC 퀘스트 한 건의 정의 (ClassicMapleDesign.md 10절)</summary>
public sealed class NpcQuestDef
{
    public string Id;
    public string Title;
    public string Giver;
    public int MinLevel;
    public string Requires;
    public NpcQuestObjective Objective;
    public string Target;
    public int Count;
    public int Exp;
    public int Gold;
    public string RewardItem;
    public int RewardCount;
    public string Description;

    /// <summary>수집 목표 중 인벤토리 아이템이 아니라 발굴 기록 개수로 판정하는 특수 대상 (Q602)</summary>
    public bool IsRecordsTarget => Objective == NpcQuestObjective.Collect && Target == FathersRecords.SaveKey;
}

/// <summary>
/// 1층계 NPC 퀘스트 표 (Resources/QuestsFloor1.csv). 기존 QuestCatalog(게시판·촌장 반복 의뢰)와 별개다.
/// CSV: id,title,giver,minLevel,requires,objective,target,count,exp,gold,rewardItem,rewardCount,description (쉼표 없는 평문).
/// </summary>
public static class NpcQuestCatalog
{
    private const string ResourceName = "QuestsFloor1";
    private static List<NpcQuestDef> s_all;
    private static Dictionary<string, NpcQuestDef> s_byId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_all = null;
        s_byId = null;
    }

    public static IReadOnlyList<NpcQuestDef> All
    {
        get
        {
            EnsureLoaded();
            return s_all;
        }
    }

    public static NpcQuestDef Find(string id)
    {
        EnsureLoaded();
        return id != null && s_byId.TryGetValue(id, out NpcQuestDef d) ? d : null;
    }

    private static void EnsureLoaded()
    {
        if (s_all != null)
        {
            return;
        }
        TextAsset csv = Resources.Load<TextAsset>(ResourceName);
        s_all = Parse(csv != null ? csv.text : "");
        s_byId = new Dictionary<string, NpcQuestDef>();
        foreach (NpcQuestDef d in s_all)
        {
            s_byId[d.Id] = d;
        }
    }

    /// <summary>CSV 텍스트 파싱 (첫 줄 머리글, 형식이 틀린 줄은 건너뜀). 테스트에서도 쓴다</summary>
    public static List<NpcQuestDef> Parse(string text)
    {
        var list = new List<NpcQuestDef>();
        string[] lines = text.Replace("\r", "").TrimStart((char)0xFEFF).Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 13 || c[0].Trim().Length == 0)
            {
                continue;
            }
            if (!TryObjective(c[5].Trim(), out NpcQuestObjective obj)
                || !int.TryParse(c[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int minLv)
                || !int.TryParse(c[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
            {
                continue;
            }
            list.Add(new NpcQuestDef
            {
                Id = c[0].Trim(),
                Title = c[1].Trim(),
                Giver = c[2].Trim(),
                MinLevel = minLv,
                Requires = c[4].Trim(),
                Objective = obj,
                Target = c[6].Trim(),
                Count = Mathf.Max(1, count),
                Exp = ToInt(c[8]),
                Gold = ToInt(c[9]),
                RewardItem = c[10].Trim(),
                RewardCount = ToInt(c[11]),
                Description = c[12].Trim(),
            });
        }
        return list;
    }

    private static int ToInt(string s)
    {
        return int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
    }

    private static bool TryObjective(string s, out NpcQuestObjective obj)
    {
        switch (s)
        {
            case "talk": obj = NpcQuestObjective.Talk; return true;
            case "kill": obj = NpcQuestObjective.Kill; return true;
            case "collect": obj = NpcQuestObjective.Collect; return true;
            case "craft": obj = NpcQuestObjective.Craft; return true;
            case "visit": obj = NpcQuestObjective.Visit; return true;
        }
        obj = NpcQuestObjective.Talk;
        return false;
    }

    /// <summary>NPC id → 표시 이름</summary>
    public static string NpcName(string npcId)
    {
        switch (npcId)
        {
            case "elder": return "촌장";
            case "smith": return "대장장이 브란";
            case "merchant": return "상인 일사";
            case "ruen": return "사냥꾼 루엔";
            case "sena": return "약초꾼 세나";
            case "eve": return "떠돌이 탐험가 이브";
        }
        return npcId;
    }

    /// <summary>visit 목표 지점 id → 표시 이름</summary>
    public static string VisitName(string markerId)
    {
        switch (markerId)
        {
            case "forest_night_post": return "일반 숲 야간 관측지";
            case "deep_nest": return "깊은 숲 정예 둥지";
            case "tower_altar": return "탑 근교 네 갈래 별 제단";
        }
        return markerId;
    }
}
