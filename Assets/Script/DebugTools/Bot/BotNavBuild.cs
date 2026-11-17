using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동 그래프 굽기 — 순수 로직(EditMode 시험 대상). 입력 = 월드 좌표 지오메트리(단단한 다각형 묶음·단방향 발판·로프·포털), 출력 = BotNavGraph.
/// 1) 표면: 다각형 변 중 기울기 45도 이하이고 아래가 속·위가 빈 것 + 단방향 발판 윗변. 끝점이 맞닿는 조각은 꺾은선으로 잇는다.
/// 2) 머리 공간: 표면 위 0.5·1.2 가 단단하면 설 수 없다 → 그 자리에서 구간을 자른다(벽·계단 턱). 자른 끝은 막힌 끝(open=false).
/// 3) 간선: 열린 끝에서 떨어지기, 점프(위로 JumpUp·옆으로 GapMax), 높은 점프(2단), 단방향 발판 하향 점프, 로프 위아래 이웃 구간, 같은 맵 포털.
/// 수치는 PlayerController(jumpForce 13, gravityScale 3 → 홑점프 약 2.9)에서 보수적으로 잡았다.
/// </summary>
public static class BotNavBuild
{
    public const float MaxSlope = 1.0f;
    public const float SampleStep = 0.25f;
    public const float HeadLow = 0.5f;
    public const float HeadHigh = 1.2f;
    public const float MinSegLen = 0.5f;
    public const float JumpUp = 2.3f;
    public const float JumpUpHigh = 4.6f;
    /// <summary>점프로 로프 아래 끝을 잡을 수 있는 최대 높이(표면→로프 y0). 사람이 오른 SideRoad x=-229.5(2.53)를 포함하도록 2.6 (2.4 였을 때 구덩이에서 못 나가는 섬이 생겼다, 11-16)</summary>
    public const float RopeReachBelow = 2.6f;
    public const float GapMax = 3.2f;
    public const float JoinEps = 0.06f;

    /// <summary>콜라이더 하나의 닫힌 경로 묶음(짝홀 규칙으로 속 판정 — 구멍 경로 포함)</summary>
    public sealed class Solid
    {
        public readonly List<Vector2[]> Paths = new List<Vector2[]>();
        public Rect Bounds;
    }

    public sealed class Input
    {
        public string Map;
        public readonly List<Solid> Solids = new List<Solid>();
        public readonly List<Rect> Platforms = new List<Rect>();   // 단방향 발판(윗변 = yMax)
        public readonly List<Rect> Ropes = new List<Rect>();
        public readonly List<BotNavGraph.PortalRec> Portals = new List<BotNavGraph.PortalRec>();
    }

    private struct Piece
    {
        public Vector2 A, B;
        public bool OneWay;
    }

    public static BotNavGraph Build(Input input)
    {
        List<Piece> pieces = CollectPieces(input);
        List<List<Vector2>> chains;
        List<bool> chainOneWay;
        Chain(pieces, out chains, out chainOneWay);
        List<BotNavGraph.Seg> segs = new List<BotNavGraph.Seg>();
        for (int i = 0; i < chains.Count; i++)
        {
            SplitByHeadroom(input, chains[i], chainOneWay[i], segs);
        }
        BotNavGraph g = new BotNavGraph();
        g.map = input.Map;
        for (int i = 0; i < segs.Count; i++)
        {
            segs[i].id = i;
        }
        g.segs = segs.ToArray();
        g.ropes = new BotNavGraph.Rope[input.Ropes.Count];
        for (int i = 0; i < input.Ropes.Count; i++)
        {
            Rect r = input.Ropes[i];
            g.ropes[i] = new BotNavGraph.Rope { x = r.center.x, y0 = r.yMin, y1 = r.yMax };
        }
        List<BotNavGraph.Edge> edges = new List<BotNavGraph.Edge>();
        AddFallEdges(g, edges);
        AddJumpEdges(g, edges);
        AddRopeEdges(g, edges);
        AddPortals(g, input, edges);
        g.edges = edges.ToArray();
        return g;
    }

    // ───── 기하 ─────

    /// <summary>짝홀 규칙: p 가 이 묶음 속인가</summary>
    public static bool Inside(Solid s, Vector2 p)
    {
        if (!s.Bounds.Contains(p))
        {
            return false;
        }
        bool inside = false;
        for (int k = 0; k < s.Paths.Count; k++)
        {
            Vector2[] path = s.Paths[k];
            for (int i = 0, j = path.Length - 1; i < path.Length; j = i++)
            {
                Vector2 a = path[i], b = path[j];
                if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                {
                    inside = !inside;
                }
            }
        }
        return inside;
    }

    public static bool InsideAny(Input input, Vector2 p)
    {
        for (int i = 0; i < input.Solids.Count; i++)
        {
            if (Inside(input.Solids[i], p))
            {
                return true;
            }
        }
        return false;
    }

    public static Solid SolidFromPaths(IEnumerable<Vector2[]> paths)
    {
        Solid s = new Solid();
        bool any = false;
        foreach (Vector2[] p in paths)
        {
            if (p == null || p.Length < 3)
            {
                continue;
            }
            s.Paths.Add(p);
            for (int i = 0; i < p.Length; i++)
            {
                if (!any)
                {
                    s.Bounds = new Rect(p[i], Vector2.zero);
                    any = true;
                }
                else
                {
                    s.Bounds = Rect.MinMaxRect(Mathf.Min(s.Bounds.xMin, p[i].x), Mathf.Min(s.Bounds.yMin, p[i].y),
                        Mathf.Max(s.Bounds.xMax, p[i].x), Mathf.Max(s.Bounds.yMax, p[i].y));
                }
            }
        }
        s.Bounds = Rect.MinMaxRect(s.Bounds.xMin - 0.01f, s.Bounds.yMin - 0.01f, s.Bounds.xMax + 0.01f, s.Bounds.yMax + 0.01f);
        return s;
    }

    private static List<Piece> CollectPieces(Input input)
    {
        List<Piece> list = new List<Piece>();
        for (int si = 0; si < input.Solids.Count; si++)
        {
            Solid s = input.Solids[si];
            for (int k = 0; k < s.Paths.Count; k++)
            {
                Vector2[] path = s.Paths[k];
                for (int i = 0; i < path.Length; i++)
                {
                    Vector2 a = path[i], b = path[(i + 1) % path.Length];
                    float w = Mathf.Abs(b.x - a.x);
                    if (w < 0.05f || Mathf.Abs(b.y - a.y) / w > MaxSlope)
                    {
                        continue;
                    }
                    AddSurfaceParts(input, s, a.x < b.x ? a : b, a.x < b.x ? b : a, list);
                }
            }
        }
        for (int i = 0; i < input.Platforms.Count; i++)
        {
            Rect r = input.Platforms[i];
            list.Add(new Piece { A = new Vector2(r.xMin, r.yMax), B = new Vector2(r.xMax, r.yMax), OneWay = true });
        }
        return list;
    }

    /// <summary>변 a→b(a.x &lt; b.x)를 SampleStep 칸으로 나눠, 아래가 이 묶음 속이고 위가 비어 있는 칸만 조각으로 남긴다(위 행에 일부 덮인 변)</summary>
    private static void AddSurfaceParts(Input input, Solid s, Vector2 a, Vector2 b, List<Piece> list)
    {
        int n = Mathf.Max(1, Mathf.CeilToInt((b.x - a.x) / SampleStep));
        int start = -1;
        for (int i = 0; i <= n; i++)
        {
            bool ok = false;
            if (i < n)
            {
                Vector2 m = Vector2.Lerp(a, b, (i + 0.5f) / n);
                ok = Inside(s, m + new Vector2(0f, -0.05f)) && !InsideAny(input, m + new Vector2(0f, 0.05f));
            }
            if (ok && start < 0)
            {
                start = i;
            }
            else if (!ok && start >= 0)
            {
                list.Add(new Piece { A = Vector2.Lerp(a, b, (float)start / n), B = Vector2.Lerp(a, b, (float)i / n) });
                start = -1;
            }
        }
    }

    /// <summary>끝점이 맞닿는(같은 종류) 조각을 왼쪽→오른쪽 꺾은선으로 잇는다</summary>
    private static void Chain(List<Piece> pieces, out List<List<Vector2>> chains, out List<bool> oneWay)
    {
        pieces.Sort((p, q) => p.A.x.CompareTo(q.A.x));
        bool[] used = new bool[pieces.Count];
        chains = new List<List<Vector2>>();
        oneWay = new List<bool>();
        for (int i = 0; i < pieces.Count; i++)
        {
            if (used[i])
            {
                continue;
            }
            used[i] = true;
            List<Vector2> c = new List<Vector2> { pieces[i].A, pieces[i].B };
            bool ow = pieces[i].OneWay;
            bool grown = true;
            while (grown)
            {
                grown = false;
                Vector2 tail = c[c.Count - 1];
                for (int j = 0; j < pieces.Count; j++)
                {
                    if (!used[j] && pieces[j].OneWay == ow && (pieces[j].A - tail).sqrMagnitude < JoinEps * JoinEps)
                    {
                        used[j] = true;
                        c.Add(pieces[j].B);
                        grown = true;
                        break;
                    }
                }
            }
            chains.Add(c);
            oneWay.Add(ow);
        }
    }

    private static float ChainY(List<Vector2> c, float x)
    {
        for (int i = 1; i < c.Count; i++)
        {
            if (x <= c[i].x)
            {
                float span = c[i].x - c[i - 1].x;
                return span > 1e-4f ? Mathf.Lerp(c[i - 1].y, c[i].y, (x - c[i - 1].x) / span) : c[i].y;
            }
        }
        return c[c.Count - 1].y;
    }

    private static bool Free(Input input, float x, float y)
    {
        return !InsideAny(input, new Vector2(x, y + HeadLow)) && !InsideAny(input, new Vector2(x, y + HeadHigh));
    }

    /// <summary>꺾은선을 SampleStep 간격으로 걸으며 머리 공간이 막힌 곳에서 자른다</summary>
    private static void SplitByHeadroom(Input input, List<Vector2> c, bool oneWay, List<BotNavGraph.Seg> outSegs)
    {
        float x0 = c[0].x, x1 = c[c.Count - 1].x;
        int steps = Mathf.Max(1, Mathf.CeilToInt((x1 - x0) / SampleStep));
        float runStart = float.NaN;
        bool runOpenL = false;
        for (int i = 0; i <= steps + 1; i++)
        {
            float x = i <= steps ? Mathf.Min(x1, x0 + i * SampleStep) : float.NaN;
            bool free = i <= steps && Free(input, x, ChainY(c, x));
            if (free && float.IsNaN(runStart))
            {
                runStart = x;
                runOpenL = i == 0 && Free(input, x0 - 0.3f, c[0].y); // 꺾은선 끝 너머가 벽이면 막힌 끝
            }
            else if (!free && !float.IsNaN(runStart))
            {
                float end = i <= steps ? x - SampleStep : x1;
                bool openR = i > steps && Free(input, x1 + 0.3f, c[c.Count - 1].y);
                if (end - runStart >= MinSegLen)
                {
                    outSegs.Add(MakeSeg(c, runStart, end, oneWay, runOpenL, openR));
                }
                runStart = float.NaN;
            }
        }
    }

    private static BotNavGraph.Seg MakeSeg(List<Vector2> c, float a, float b, bool oneWay, bool openL, bool openR)
    {
        List<float> xs = new List<float> { a };
        for (int i = 0; i < c.Count; i++)
        {
            if (c[i].x > a + 0.01f && c[i].x < b - 0.01f)
            {
                xs.Add(c[i].x);
            }
        }
        xs.Add(b);
        BotNavGraph.Seg s = new BotNavGraph.Seg { px = xs.ToArray(), py = new float[xs.Count], oneWay = oneWay, openL = openL, openR = openR };
        for (int i = 0; i < xs.Count; i++)
        {
            s.py[i] = ChainY(c, xs[i]);
        }
        return s;
    }

    // ───── 간선 ─────

    private static void AddFallEdges(BotNavGraph g, List<BotNavGraph.Edge> edges)
    {
        for (int i = 0; i < g.segs.Length; i++)
        {
            BotNavGraph.Seg s = g.segs[i];
            for (int side = -1; side <= 1; side += 2)
            {
                if (side < 0 ? !s.openL : !s.openR)
                {
                    continue;
                }
                float ex = side < 0 ? s.X0 : s.X1;
                float fx = ex + side * 0.6f;
                int to = SegBelowExcept(g, fx, s.Y(ex) - 0.3f, i);
                if (to >= 0)
                {
                    edges.Add(new BotNavGraph.Edge { from = i, to = to, kind = BotNavGraph.KindFall, ax0 = ex, ax1 = ex, lx = fx, dir = side, cost = 0.5f });
                }
            }
        }
    }

    private static int SegBelowExcept(BotNavGraph g, float x, float y, int except)
    {
        int best = -1;
        float bestY = float.NegativeInfinity;
        for (int i = 0; i < g.segs.Length; i++)
        {
            if (i == except || !g.segs[i].Contains(x, 0.2f))
            {
                continue;
            }
            float sy = g.segs[i].Y(x);
            if (sy <= y && sy > bestY)
            {
                bestY = sy;
                best = i;
            }
        }
        return best;
    }

    private static void AddJumpEdges(BotNavGraph g, List<BotNavGraph.Edge> edges)
    {
        for (int a = 0; a < g.segs.Length; a++)
        {
            for (int b = 0; b < g.segs.Length; b++)
            {
                if (a != b)
                {
                    AddJumpPair(g, a, b, edges);
                }
            }
        }
    }

    private static void AddJumpPair(BotNavGraph g, int a, int b, List<BotNavGraph.Edge> edges)
    {
        BotNavGraph.Seg A = g.segs[a], B = g.segs[b];
        float o0 = Mathf.Max(A.X0, B.X0), o1 = Mathf.Min(A.X1, B.X1);
        if (o1 - o0 >= 0.6f)
        {
            float mid = (o0 + o1) * 0.5f;
            float dy = B.Y(mid) - A.Y(mid);
            if (dy > 0.4f && B.oneWay && dy <= JumpUpHigh)
            {
                int kind = dy <= JumpUp ? BotNavGraph.KindJump : BotNavGraph.KindJumpHigh;
                edges.Add(new BotNavGraph.Edge { from = a, to = b, kind = kind, ax0 = o0 + 0.2f, ax1 = o1 - 0.2f, sameX = true, cost = kind == BotNavGraph.KindJump ? 0.8f : 1.2f });
            }
            else if (dy < -0.4f && A.oneWay && SegBelowExcept(g, mid, A.Y(mid) - 0.2f, a) == b)
            {
                edges.Add(new BotNavGraph.Edge { from = a, to = b, kind = BotNavGraph.KindDownJump, ax0 = o0 + 0.2f, ax1 = o1 - 0.2f, sameX = true, cost = 0.8f });
            }
            return;
        }
        int dir = B.X0 >= A.X1 ? 1 : -1;
        float gap = dir > 0 ? B.X0 - A.X1 : A.X0 - B.X1;
        if (gap < -0.6f)
        {
            return;
        }
        float tx = dir > 0 ? A.X1 : A.X0;
        float lx = dir > 0 ? B.X0 + 0.4f : B.X1 - 0.4f;
        float dyG = B.Y(lx) - A.Y(tx);
        bool openEnd = dir > 0 ? A.openR : A.openL;
        if (dyG <= 0.3f && (gap < 0.6f || dyG < -6f))
        {
            return; // 바로 아래 이웃은 떨어지기 간선이 맡는다
        }
        float maxGap = dyG > 0f ? GapMax : GapMax + Mathf.Min(3f, -dyG) * 0.8f;
        if (gap > maxGap || dyG > JumpUpHigh || (!openEnd && dyG < 0.3f))
        {
            return;
        }
        int k = dyG <= JumpUp ? BotNavGraph.KindJump : BotNavGraph.KindJumpHigh;
        float ax0 = dir > 0 ? Mathf.Max(A.X0, tx - 0.6f) : tx;
        float ax1 = dir > 0 ? tx : Mathf.Min(A.X1, tx + 0.6f);
        edges.Add(new BotNavGraph.Edge { from = a, to = b, kind = k, ax0 = ax0, ax1 = ax1, lx = lx, dir = dir, cost = (k == BotNavGraph.KindJump ? 0.8f : 1.2f) + gap * 0.05f });
    }

    private static void AddRopeEdges(BotNavGraph g, List<BotNavGraph.Edge> edges)
    {
        List<int> on = new List<int>();
        for (int r = 0; r < g.ropes.Length; r++)
        {
            BotNavGraph.Rope rope = g.ropes[r];
            on.Clear();
            for (int i = 0; i < g.segs.Length; i++)
            {
                BotNavGraph.Seg s = g.segs[i];
                if (!s.Contains(rope.x, 0.15f))
                {
                    continue;
                }
                float y = s.Y(rope.x);
                if (y >= rope.y0 - RopeReachBelow && y <= rope.y1 + 1.2f) // 위 끝: 트리거를 벗어난 뒤 남은 상승(약 1.7)으로 올라설 수 있는 높이
                {
                    on.Add(i);
                }
            }
            on.Sort((p, q) => g.segs[p].Y(rope.x).CompareTo(g.segs[q].Y(rope.x)));
            for (int k = 0; k + 1 < on.Count; k++)
            {
                int lo = on[k], hi = on[k + 1];
                float ylo = g.segs[lo].Y(rope.x), yhi = g.segs[hi].Y(rope.x);
                if (yhi - ylo < 0.8f)
                {
                    continue;
                }
                float cost = 1f + (yhi - ylo) / BotNavGraph.WalkSpeed;
                edges.Add(new BotNavGraph.Edge { from = lo, to = hi, kind = BotNavGraph.KindRope, rope = r, ax0 = rope.x, ax1 = rope.x, lx = rope.x, cost = cost });
                if (g.segs[hi].oneWay || yhi < rope.y1 - 0.3f)
                {
                    edges.Add(new BotNavGraph.Edge { from = hi, to = lo, kind = BotNavGraph.KindRope, rope = r, ax0 = rope.x, ax1 = rope.x, lx = rope.x, cost = cost });
                }
            }
        }
    }

    private static void AddPortals(BotNavGraph g, Input input, List<BotNavGraph.Edge> edges)
    {
        g.portals = new BotNavGraph.PortalRec[input.Portals.Count];
        for (int i = 0; i < input.Portals.Count; i++)
        {
            BotNavGraph.PortalRec p = input.Portals[i];
            p.seg = g.SegBelow(p.x, p.y + 1.0f, 0.5f);
            if (p.seg >= 0 && p.y - g.segs[p.seg].Y(p.x) > 3.5f)
            {
                p.seg = -1; // 공중 포털(타는 자리를 못 찾음)
            }
            g.portals[i] = p;
            if (p.seg < 0 || !string.IsNullOrEmpty(p.toMap))
            {
                continue;
            }
            int to = g.SegBelow(p.toX, p.toY + 1.0f, 0.5f);
            if (to >= 0 && to != p.seg)
            {
                edges.Add(new BotNavGraph.Edge { from = p.seg, to = to, kind = BotNavGraph.KindPortal, ax0 = p.x - 0.3f, ax1 = p.x + 0.3f, lx = p.toX, cost = 2.5f });
            }
        }
    }
}
