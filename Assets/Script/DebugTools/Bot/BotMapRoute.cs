using System.Collections.Generic;

/// <summary>
/// 맵 사이 경로(순수 로직): 구운 그래프들의 "타는 자리가 있는" 포털로 맵 인접표를 만들고 너비 우선으로 다음 맵을 고른다.
/// 마을(Town)은 목적지가 마을일 때만 지난다(사냥 순회가 마을 포털로 새지 않게).
/// </summary>
public static class BotMapRoute
{
    public const string Town = "Town";

    /// <summary>from → to 로 가는 다음 맵(같으면 to, 못 가면 null)</summary>
    public static string NextHop(IEnumerable<BotNavGraph> graphs, string from, string to)
    {
        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
        {
            return null;
        }
        if (from == to)
        {
            return to;
        }
        Dictionary<string, List<string>> adj = new Dictionary<string, List<string>>();
        foreach (BotNavGraph g in graphs)
        {
            if (g == null || string.IsNullOrEmpty(g.map))
            {
                continue;
            }
            List<string> list = new List<string>();
            for (int i = 0; i < g.portals.Length; i++)
            {
                BotNavGraph.PortalRec p = g.portals[i];
                if (p.seg >= 0 && !string.IsNullOrEmpty(p.toMap) && !list.Contains(p.toMap))
                {
                    list.Add(p.toMap);
                }
            }
            adj[g.map] = list;
        }
        Dictionary<string, string> parent = new Dictionary<string, string>();
        Queue<string> q = new Queue<string>();
        parent[from] = null;
        q.Enqueue(from);
        while (q.Count > 0)
        {
            string u = q.Dequeue();
            List<string> next;
            if (!adj.TryGetValue(u, out next))
            {
                continue;
            }
            for (int i = 0; i < next.Count; i++)
            {
                string v = next[i];
                if (parent.ContainsKey(v) || (v == Town && to != Town))
                {
                    continue;
                }
                parent[v] = u;
                if (v == to)
                {
                    string cur = v;
                    while (parent[cur] != from)
                    {
                        cur = parent[cur];
                    }
                    return cur;
                }
                q.Enqueue(v);
            }
        }
        return null;
    }
}
