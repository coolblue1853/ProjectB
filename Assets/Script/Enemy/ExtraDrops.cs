using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 몹별 추가 드롭 표 (Resources/MonsterExtraDrops.csv: monsterId,item,probability).
/// 스포너의 DropManager 표(씬에 직렬화됨)는 건드리지 않고, 몹 프리팹 이름(MonsterSheet id)으로 얹는 계보 무기 드롭이다.
/// 확률은 DropManager 표와 같은 규칙(0~1, 드롭률 보너스 적용)이다. 4번째 열 condition=quest 는 NPC 퀘스트 진행 중에만 떨어진다.
/// </summary>
public static class ExtraDrops
{
    public struct Entry
    {
        public string Item;
        public float Probability;
        /// <summary>true 면 진행 중인 NPC 퀘스트가 이 아이템을 필요로 할 때만 굴린다 (CSV 4번째 열 "quest")</summary>
        public bool QuestOnly;
    }

    private const string ResourceName = "MonsterExtraDrops";
    private static Dictionary<string, List<Entry>> s_table;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_table = null;
    }

    /// <summary>몹 오브젝트 이름 → 표 id ("(Clone)" 제거)</summary>
    public static string MonsterId(string objectName)
    {
        return string.IsNullOrEmpty(objectName) ? "" : objectName.Replace("(Clone)", "").Trim();
    }

    /// <summary>이 몹의 추가 드롭 목록(없으면 null)</summary>
    public static List<Entry> For(string monsterId)
    {
        if (s_table == null)
        {
            TextAsset csv = Resources.Load<TextAsset>(ResourceName);
            s_table = Parse(csv != null ? csv.text : "");
        }
        return s_table.TryGetValue(monsterId, out List<Entry> list) ? list : null;
    }

    /// <summary>CSV 텍스트 파싱 (첫 줄은 머리글, 형식이 틀린 줄은 건너뛴다). 테스트에서도 쓴다</summary>
    public static Dictionary<string, List<Entry>> Parse(string text)
    {
        var table = new Dictionary<string, List<Entry>>();
        string[] lines = text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 3 || c[0].Trim().Length == 0)
            {
                continue;
            }
            if (!float.TryParse(c[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float p))
            {
                continue;
            }
            string id = c[0].Trim();
            if (!table.TryGetValue(id, out List<Entry> list))
            {
                list = new List<Entry>();
                table[id] = list;
            }
            list.Add(new Entry { Item = c[1].Trim(), Probability = Mathf.Clamp01(p), QuestOnly = c.Length > 3 && c[3].Trim() == "quest" });
        }
        return table;
    }
}
