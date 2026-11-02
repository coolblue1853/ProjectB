using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 1층계 보강 콘텐츠 공용 CSV 읽기와 켜기/끄기 표 (Resources/Floor1Content.csv: id,enabled,...).
/// 끄려면 해당 id 의 enabled 를 0 으로 바꾼다 (코드 수정 없음). 설명은 Docs/Playtest/Floor1ContentList.md.
/// </summary>
public static class Floor1Data
{
    private const string FlagsResource = "Floor1Content";
    private static Dictionary<string, bool> s_flags;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_flags = null;
    }

    /// <summary>기능이 켜져 있는가. 표에 없는 id 는 꺼진 것으로 본다</summary>
    public static bool On(string id)
    {
        if (s_flags == null)
        {
            TextAsset csv = Resources.Load<TextAsset>(FlagsResource);
            s_flags = ParseFlags(csv != null ? csv.text : "");
        }
        return s_flags.TryGetValue(id, out bool on) && on;
    }

    public static Dictionary<string, bool> ParseFlags(string text)
    {
        var map = new Dictionary<string, bool>();
        foreach (string[] c in Rows(text))
        {
            if (c.Length >= 2)
            {
                map[c[0].Trim()] = c[1].Trim() == "1";
            }
        }
        return map;
    }

    /// <summary>머리글 줄을 뺀 쉼표 분할 행 (BOM·CR 제거, 빈 줄 건너뜀). 값 안에 쉼표를 쓰지 않는다</summary>
    public static List<string[]> Rows(string text)
    {
        var rows = new List<string[]>();
        string[] lines = text.Replace("\r", "").TrimStart((char)0xFEFF).Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim().Length > 0)
            {
                rows.Add(lines[i].Split(','));
            }
        }
        return rows;
    }

    public static int Int(string s)
    {
        return int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
    }

    public static float Float(string s)
    {
        return float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0f;
    }

    /// <summary>골드·EXP·아이템 보상 한 묶음 (NpcQuestRewards 를 재사용해 지급)</summary>
    public sealed class Reward
    {
        public int Gold;
        public int Exp;
        public string Item;
        public int Count;
        public string Label;

        public bool HasItem => !string.IsNullOrEmpty(Item) && Count > 0;

        private NpcQuestDef AsDef()
        {
            return new NpcQuestDef
            {
                Id = Label ?? "F1",
                Objective = NpcQuestObjective.Talk,
                Gold = Gold,
                Exp = Exp,
                RewardItem = Item,
                RewardCount = Count,
            };
        }

        /// <summary>가방에 아이템이 들어갈 공간이 있는가 (아이템이 없으면 항상 true)</summary>
        public bool CanReceive()
        {
            return NpcQuestRewards.CanReceive(AsDef());
        }

        public void Apply()
        {
            NpcQuestRewards.Apply(AsDef());
        }

        public string Describe()
        {
            var parts = new List<string>();
            if (Gold > 0)
            {
                parts.Add(Gold + "G");
            }
            if (Exp > 0)
            {
                parts.Add("EXP " + Exp);
            }
            if (HasItem)
            {
                parts.Add(Item + " x" + Count);
            }
            return string.Join(" · ", parts);
        }
    }
}
