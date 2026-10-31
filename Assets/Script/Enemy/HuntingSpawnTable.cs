using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>1층계 사냥 스포너 보정. 씬 직렬화는 보존하고 첫 풀 생성 전에만 적용한다.</summary>
public static class HuntingSpawnTable
{
    public sealed class Row
    {
        public int Count;
        public float Cycle;
        public string Profile;
        public int[] PointWeights;
    }

    private static Dictionary<string, Row> s_rows;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_rows = null;
    }

    public static Row Get(string map, string spawner)
    {
        if (s_rows == null)
        {
            TextAsset csv = Resources.Load<TextAsset>("HuntingSpawnsFloor1");
            s_rows = Parse(csv != null ? csv.text : "");
        }
        return s_rows.TryGetValue(map + "/" + spawner, out Row row) ? row : null;
    }

    public static Dictionary<string, Row> Parse(string text)
    {
        var rows = new Dictionary<string, Row>();
        string[] lines = text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 5 || string.IsNullOrWhiteSpace(c[0]) || string.IsNullOrWhiteSpace(c[1])
                || !int.TryParse(c[2], out int count) || count <= 0
                || !float.TryParse(c[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float cycle)
                || float.IsNaN(cycle) || float.IsInfinity(cycle) || cycle <= 0f)
            {
                continue;
            }
            rows[c[0].Trim() + "/" + c[1].Trim()] = new Row
            {
                Count = count, Cycle = cycle, Profile = c[4].Trim(),
                PointWeights = c.Length > 5 ? ParseWeights(c[5], count) : null,
            };
        }
        return rows;
    }

    private static int[] ParseWeights(string text, int count)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }
        string[] parts = text.Split('|');
        var weights = new int[parts.Length];
        int sum = 0;
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out weights[i]) || weights[i] < 0)
            {
                return null;
            }
            sum += weights[i];
        }
        return sum == count ? weights : null;
    }

    /// <summary>동시 슬롯 수와 기존 발판 수를 분리한다. 가중치 하나는 그 발판을 쓰는 슬롯 수다.</summary>
    public static int[] PositionIndices(int count, int pointCount, int[] weights)
    {
        var indices = new int[count];
        int slot = 0;
        if (weights != null && weights.Length == pointCount)
        {
            for (int point = 0; point < weights.Length; point++)
            {
                for (int n = 0; n < weights[point] && slot < count; n++)
                {
                    indices[slot++] = point;
                }
            }
        }
        for (; slot < count; slot++)
        {
            indices[slot] = pointCount > 0 ? slot % pointCount : 0;
        }
        return indices;
    }
}
