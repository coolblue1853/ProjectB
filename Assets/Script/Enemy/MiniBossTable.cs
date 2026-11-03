using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 미니보스 M1~M5 배치·확정 드롭 표 (Resources/MiniBosses.csv, Floor1Completion.md 4-4).
/// 컬럼: id,map,x,y,baseMonster,respawnSec,guaranteed — id 는 MonsterSheet 행, baseMonster 는 같은 구역 일반 몹 프리팹(MonsterSheet 행),
/// guaranteed 는 "아이템:개수|아이템:개수". 확률 드롭(계보 무기)은 MonsterExtraDrops.csv 의 id 행이다. 컬럼 추가는 맨 뒤에만.
/// </summary>
public static class MiniBossTable
{
    public struct Stack
    {
        public string Item;
        public int Count;
    }

    public class Row
    {
        public string Id, Map, BaseMonster;
        public float X, Y, RespawnSec;
        public List<Stack> Guaranteed = new List<Stack>();
    }

    private static List<Row> s_rows;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_rows = null;
    }

    public static IReadOnlyList<Row> All()
    {
        if (s_rows == null)
        {
            TextAsset csv = Resources.Load<TextAsset>("MiniBosses");
            s_rows = Parse(csv != null ? csv.text : "");
        }
        return s_rows;
    }

    /// <summary>CSV 텍스트 파싱 (머리글 건너뜀, 형식이 틀린 줄은 건너뜀). 테스트에서도 쓴다</summary>
    public static List<Row> Parse(string text)
    {
        var rows = new List<Row>();
        string[] lines = text.Replace("\r", "").TrimStart('\uFEFF').Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 7 || c[0].Trim().Length == 0)
            {
                continue;
            }
            var r = new Row { Id = c[0].Trim(), Map = c[1].Trim(), BaseMonster = c[4].Trim() };
            if (!Num(c[2], out r.X) || !Num(c[3], out r.Y) || !Num(c[5], out r.RespawnSec))
            {
                continue;
            }
            foreach (string part in c[6].Split('|'))
            {
                string[] kv = part.Split(':');
                if (kv.Length == 2 && int.TryParse(kv[1].Trim(), out int n) && n > 0)
                {
                    r.Guaranteed.Add(new Stack { Item = kv[0].Trim(), Count = n });
                }
            }
            rows.Add(r);
        }
        return rows;
    }

    private static bool Num(string s, out float v)
    {
        return float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }

    /// <summary>
    /// 확정 드롭 목록(같은 이름 반복 = 개수). 같은 날 재처치 감소(BossRetryRules) 배율 mult 를 개수에 곱하고 올림, 최소 1개.
    /// </summary>
    public static List<string> BuildLoot(Row row, float mult)
    {
        var loot = new List<string>();
        foreach (Stack s in row.Guaranteed)
        {
            int n = Mathf.Max(1, Mathf.CeilToInt(s.Count * mult));
            for (int i = 0; i < n; i++)
            {
                loot.Add(s.Item);
            }
        }
        return loot;
    }
}
