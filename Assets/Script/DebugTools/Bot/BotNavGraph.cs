using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 봇 이동 그래프(B2) 데이터·조회·길찾기 — 순수 로직(EditMode 시험 대상). 에디터에서 맵 지오메트리로 미리 굽고(BotNavBuild·BotNavBaker),
/// 런타임은 Assets/Bot/Nav/&lt;맵&gt;.json 을 읽기만 한다(에디터 플레이 전용 — Resources 밖이라 빌드에 안 들어간다. 빌드에서는 그래프 없음 → 직진 이동). 설계는 PlayBotDesign.md 11절.
/// 노드 = 발판 구간(Seg: 걸어서 이어지는 표면 한 줄), 간선 = 진짜 입력 한 동작(떨어지기·점프·높은 점프·하향 점프·로프·같은 맵 포털).
/// 다른 맵으로 가는 포털은 Portals 에 따로 두고 맵 사이 경로(BotMapRoute)에서 쓴다.
/// </summary>
[System.Serializable]
public sealed class BotNavGraph
{
    public const int KindFall = 0;
    public const int KindJump = 1;
    public const int KindJumpHigh = 2;   // 2단 점프가 있어야 한다
    public const int KindDownJump = 3;
    public const int KindRope = 4;
    public const int KindPortal = 5;
    public const float WalkSpeed = 8f;   // 길찾기 비용용(실제 moveSpeed 10 보다 보수적)
    public const float OnSegDy = 0.6f;   // 발이 표면에서 이만큼 안이면 그 구간 위

    [System.Serializable]
    public sealed class Seg
    {
        public int id;
        public float[] px;   // 표면 꺾은선 x(오름차순)
        public float[] py;
        public bool oneWay;  // 단방향 발판(아래서 뚫고 올라가기·하향 점프 가능)
        public bool openL;   // 왼쪽 끝이 벽이 아니라 낭떠러지
        public bool openR;

        public float X0 => px[0];
        public float X1 => px[px.Length - 1];

        /// <summary>x 에서 표면 높이(범위 밖이면 끝 값)</summary>
        public float Y(float x)
        {
            if (x <= px[0])
            {
                return py[0];
            }
            for (int i = 1; i < px.Length; i++)
            {
                if (x <= px[i])
                {
                    float span = px[i] - px[i - 1];
                    float t = span > 1e-4f ? (x - px[i - 1]) / span : 0f;
                    return Mathf.Lerp(py[i - 1], py[i], t);
                }
            }
            return py[py.Length - 1];
        }

        public bool Contains(float x, float pad)
        {
            return x >= X0 - pad && x <= X1 + pad;
        }
    }

    [System.Serializable]
    public sealed class Edge
    {
        public int from;
        public int to;
        public int kind;
        public float ax0;    // 출발 구간에서 동작을 시작할 x 범위
        public float ax1;
        public float lx;     // 도착 x(sameX 면 출발한 x 그대로)
        public bool sameX;
        public int dir;      // 동작 중 누를 좌우(-1·0·1)
        public int rope = -1;
        public float cost;   // 동작 자체 비용(초)
    }

    [System.Serializable]
    public sealed class Rope
    {
        public float x;
        public float y0;
        public float y1;
    }

    [System.Serializable]
    public sealed class PortalRec
    {
        public float x;
        public float y;
        public int seg = -1;
        public string toMap;   // 비면 같은 맵 안 이동
        public float toX;
        public float toY;
    }

    public string map;
    public int version = 1;
    public Seg[] segs = new Seg[0];
    public Edge[] edges = new Edge[0];
    public Rope[] ropes = new Rope[0];
    public PortalRec[] portals = new PortalRec[0];

    [System.NonSerialized] private List<int>[] _out;

    /// <summary>발이 (x, feetY) 일 때 서 있는 구간(없으면 -1). 가장 가까운 표면</summary>
    public int SegAt(float x, float feetY)
    {
        int best = -1;
        float bestD = OnSegDy;
        for (int i = 0; i < segs.Length; i++)
        {
            Seg s = segs[i];
            if (!s.Contains(x, 0.35f))
            {
                continue;
            }
            float d = Mathf.Abs(s.Y(x) - feetY);
            if (d < bestD)
            {
                bestD = d;
                best = i;
            }
        }
        return best;
    }

    /// <summary>(x, y) 바로 아래(또는 y 이하)에서 가장 높은 구간. 없으면 -1</summary>
    public int SegBelow(float x, float y, float pad = 0.2f)
    {
        int best = -1;
        float bestY = float.NegativeInfinity;
        for (int i = 0; i < segs.Length; i++)
        {
            Seg s = segs[i];
            if (!s.Contains(x, pad))
            {
                continue;
            }
            float sy = s.Y(x);
            if (sy <= y && sy > bestY)
            {
                bestY = sy;
                best = i;
            }
        }
        return best;
    }

    public List<int> OutEdges(int seg)
    {
        if (_out == null || _out.Length != segs.Length)
        {
            _out = new List<int>[segs.Length];
            for (int i = 0; i < segs.Length; i++)
            {
                _out[i] = new List<int>();
            }
            for (int e = 0; e < edges.Length; e++)
            {
                if (edges[e].from >= 0 && edges[e].from < segs.Length)
                {
                    _out[edges[e].from].Add(e);
                }
            }
        }
        return _out[seg];
    }

    /// <summary>간선을 x 에서 출발했을 때 걷기 + 동작 비용과 도착 x</summary>
    public float StepCost(Edge e, float x, out float arriveX)
    {
        float tx = Mathf.Clamp(x, e.ax0, e.ax1);
        arriveX = e.sameX ? tx : e.lx;
        return Mathf.Abs(x - tx) / WalkSpeed + e.cost;
    }

    /// <summary>
    /// 구간 그래프 다익스트라(구간마다 가장 싸게 도착한 x 하나만 들고 가는 근사). 결과 = 간선 번호 순서(같은 구간이면 빈 목록), 못 가면 null.
    /// allowHigh = 2단 점프 가능, banned = 잠시 쓰지 않을 간선.
    /// </summary>
    public List<int> FindPath(int from, float fromX, int goal, float goalX, bool allowHigh, ICollection<int> banned = null)
    {
        if (from < 0 || goal < 0 || from >= segs.Length || goal >= segs.Length)
        {
            return null;
        }
        if (from == goal)
        {
            return new List<int>();
        }
        int n = segs.Length;
        float[] dist = new float[n];
        float[] atX = new float[n];
        int[] via = new int[n];
        bool[] done = new bool[n];
        for (int i = 0; i < n; i++)
        {
            dist[i] = float.PositiveInfinity;
            via[i] = -1;
        }
        dist[from] = 0f;
        atX[from] = fromX;
        for (int iter = 0; iter < n; iter++)
        {
            int u = -1;
            for (int i = 0; i < n; i++)
            {
                if (!done[i] && (u < 0 || dist[i] < dist[u]))
                {
                    u = i;
                }
            }
            if (u < 0 || float.IsPositiveInfinity(dist[u]))
            {
                break;
            }
            done[u] = true;
            if (u == goal)
            {
                break;
            }
            List<int> outs = OutEdges(u);
            for (int k = 0; k < outs.Count; k++)
            {
                int ei = outs[k];
                Edge e = edges[ei];
                if ((e.kind == KindJumpHigh && !allowHigh) || (banned != null && banned.Contains(ei)) || done[e.to])
                {
                    continue;
                }
                float ax;
                float c = dist[u] + StepCost(e, atX[u], out ax);
                if (e.to == goal)
                {
                    c += Mathf.Abs(ax - goalX) / WalkSpeed;
                }
                if (c < dist[e.to])
                {
                    dist[e.to] = c;
                    atX[e.to] = ax;
                    via[e.to] = ei;
                }
            }
        }
        if (via[goal] < 0)
        {
            return null;
        }
        List<int> path = new List<int>();
        int cur = goal;
        int guard = 0;
        while (cur != from && guard++ < n)
        {
            int ei = via[cur];
            path.Add(ei);
            cur = edges[ei].from;
        }
        path.Reverse();
        return path;
    }

    /// <summary>toMap 으로 가는 포털(없으면 -1). 여러 개면 x 가 가까운 것</summary>
    public int PortalTo(string toMap, float nearX)
    {
        int best = -1;
        float bestD = float.MaxValue;
        for (int i = 0; i < portals.Length; i++)
        {
            PortalRec p = portals[i];
            if (p.seg < 0 || p.toMap != toMap)
            {
                continue;
            }
            float d = Mathf.Abs(p.x - nearX);
            if (d < bestD)
            {
                bestD = d;
                best = i;
            }
        }
        return best;
    }

    // ───── 읽기(런타임) ─────

    /// <summary>구운 그래프 폴더(프로젝트 기준). Resources 밖 = 플레이어 빌드에 포함되지 않는다</summary>
    public const string Dir = "Assets/Bot/Nav";
    private static readonly Dictionary<string, BotNavGraph> s_cache = new Dictionary<string, BotNavGraph>();
    private static bool s_all;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_cache.Clear(); // 도메인 리로드를 끈 플레이 진입·다시 굽기 뒤 낡은 그래프가 남지 않게
        s_all = false;
    }

    /// <summary>맵 이름의 그래프(없으면 null). 한 번 읽으면 캐시. 에디터에서만 읽는다</summary>
    public static BotNavGraph Load(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
        {
            return null;
        }
        BotNavGraph g;
        if (s_cache.TryGetValue(mapName, out g))
        {
            return g;
        }
        g = ReadFile(Dir + "/" + mapName + ".json");
        s_cache[mapName] = g;
        return g;
    }

    /// <summary>구운 그래프 전부(맵 사이 경로용)</summary>
    public static IEnumerable<BotNavGraph> LoadAll()
    {
#if UNITY_EDITOR
        if (!s_all)
        {
            s_all = true;
            if (System.IO.Directory.Exists(Dir))
            {
                foreach (string f in System.IO.Directory.GetFiles(Dir, "*.json"))
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(f);
                    if (!s_cache.ContainsKey(name))
                    {
                        s_cache[name] = ReadFile(f);
                    }
                }
            }
        }
#endif
        foreach (BotNavGraph g in s_cache.Values)
        {
            if (g != null)
            {
                yield return g;
            }
        }
    }

    private static BotNavGraph ReadFile(string path)
    {
#if UNITY_EDITOR
        return System.IO.File.Exists(path) ? FromJson(System.IO.File.ReadAllText(path)) : null;
#else
        return null;
#endif
    }

    public static BotNavGraph FromJson(string json)
    {
        try
        {
            BotNavGraph g = JsonUtility.FromJson<BotNavGraph>(json);
            return g != null && g.segs != null ? g : null;
        }
        catch (System.ArgumentException)
        {
            return null;
        }
    }
}
