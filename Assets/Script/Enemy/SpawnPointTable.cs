using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 스폰 위치 덮어쓰기(Resources/SpawnPointsFloor1.csv: map,spawner,points="x:y|x:y"). 씬은 그대로 두고 첫 풀 생성 전에만 적용한다.
/// 11-01 사용자: 마을 바로 앞에 임시로 몰아 둔 나무·광석(Tree·Stone)을 맵 곳곳에 알아서 배치. 좌표는 같은 맵 몹 발판(지면) 좌표를 쓴다.
/// </summary>
public static class SpawnPointTable
{
    private static Dictionary<string, Vector2[]> s_rows;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_rows = null;
    }

    public static Vector2[] Get(string map, string spawner)
    {
        if (s_rows == null)
        {
            TextAsset csv = Resources.Load<TextAsset>("SpawnPointsFloor1");
            s_rows = Parse(csv != null ? csv.text : "");
        }
        return s_rows.TryGetValue(map + "/" + spawner, out Vector2[] points) ? points : null;
    }

    public static Dictionary<string, Vector2[]> Parse(string text)
    {
        var rows = new Dictionary<string, Vector2[]>();
        string[] lines = text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 3 || string.IsNullOrWhiteSpace(c[0]) || string.IsNullOrWhiteSpace(c[1]))
            {
                continue;
            }
            var points = new List<Vector2>();
            foreach (string p in c[2].Split('|'))
            {
                string[] xy = p.Split(':');
                if (xy.Length == 2
                    && float.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                    && float.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                {
                    points.Add(new Vector2(x, y));
                }
            }
            if (points.Count > 0)
            {
                rows[c[0].Trim() + "/" + c[1].Trim()] = points.ToArray();
            }
        }
        return rows;
    }
}
