#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 봇 이동 그래프 굽기(B2, 에디터 전용). 열린 씬의 Map 루트 아래 맵마다 콜라이더 지오메트리를 읽어(비활성 맵도 — 활성 상태·씬은 바꾸지 않는다)
/// BotNavBuild 로 그래프를 만들고 Assets/Bot/Nav/&lt;맵&gt;.json 에 쓴다(Resources 밖 — 빌드 제외). 런타임 봇은 이 파일을 읽기만 한다.
/// 메뉴 Tools/Bot/Bake Nav Graphs, 프로브 Tools/probes/bot_nav_bake.cs. 지도가 바뀌면 다시 굽는다. 설계 PlayBotDesign.md 11절.
/// </summary>
public static class BotNavBaker
{
    public const string OutDir = BotNavGraph.Dir;

    [MenuItem("Tools/Bot/Bake Nav Graphs")]
    public static void BakeMenu()
    {
        Debug.Log("[PB][Bot] " + BakeAll());
    }

    /// <summary>모든 맵을 굽고 한 줄 요약을 돌려준다</summary>
    public static string BakeAll()
    {
        GameObject root = GameObject.Find("Map");
        if (root == null)
        {
            return "Map 루트 없음";
        }
        Directory.CreateDirectory(OutDir);
        StringBuilder sb = new StringBuilder();
        foreach (Transform m in root.transform)
        {
            BotNavGraph g = BotNavBuild.Build(Collect(m));
            File.WriteAllText(OutDir + "/" + m.name + ".json", JsonUtility.ToJson(g), new UTF8Encoding(false));
            sb.Append(m.name).Append(" seg=").Append(g.segs.Length).Append(" edge=").Append(g.edges.Length)
              .Append(" rope=").Append(g.ropes.Length).Append(" portal=").Append(CountSeated(g)).Append('/').Append(g.portals.Length).Append('\n');
        }
        AssetDatabase.Refresh();
        return sb.ToString();
    }

    private static int CountSeated(BotNavGraph g)
    {
        int n = 0;
        for (int i = 0; i < g.portals.Length; i++)
        {
            if (g.portals[i].seg >= 0)
            {
                n++;
            }
        }
        return n;
    }

    /// <summary>맵 루트(포함 안 함) 아래로 collider 까지 꺼진 오브젝트가 없는가. 맵 루트 자체가 꺼져 있는 것(다른 맵)은 보지 않는다</summary>
    private static bool ActiveBelow(Transform t, Transform map)
    {
        for (; t != null && t != map; t = t.parent)
        {
            if (!t.gameObject.activeSelf)
            {
                return false;
            }
        }
        return true;
    }

    public static BotNavBuild.Input Collect(Transform map)
    {
        BotNavBuild.Input input = new BotNavBuild.Input { Map = map.name };
        int ground = LayerMask.NameToLayer("Ground");
        int soft = LayerMask.NameToLayer("Ground Soft");
        int hard = LayerMask.NameToLayer("Ground Hard");
        int wall = LayerMask.NameToLayer("Wall");
        int boundary = LayerMask.NameToLayer("Boundary");
        s_extraTm = null;
        s_extra.Clear();
        AddRuntimeSteps(map);
        foreach (Collider2D c in map.GetComponentsInChildren<Collider2D>(true))
        {
            if (!ActiveBelow(c.transform, map))
            {
                continue; // 꺼진 견본·장식(Town 의 "sample (n)")은 런타임에 없다 — 안 빼면 허공 발판이 그래프에 생겨 "지형과 다름"으로 직진 이동이 된다
            }
            int layer = c.gameObject.layer;
            if (c.CompareTag("Ladder") && c is BoxCollider2D)
            {
                input.Ropes.Add(BoxRect((BoxCollider2D)c));
                continue;
            }
            bool solidLayer = layer == ground || layer == soft || layer == hard || layer == wall || layer == boundary;
            if (c.isTrigger || !solidLayer || c.compositeOperation != Collider2D.CompositeOperation.None)
            {
                continue;
            }
            CompositeCollider2D cc = c as CompositeCollider2D;
            BoxCollider2D box = c as BoxCollider2D;
            if (cc != null)
            {
                // 비활성 맵의 합성 콜라이더 경로는 옛 지오메트리가 남아 있는 경우가 있다(11-15 Lv1_Swamp 실측) → 타일맵이 있으면 칸에서 직접 만든다
                UnityEngine.Tilemaps.Tilemap tm = cc.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                UnityEngine.Tilemaps.TilemapCollider2D tc = cc.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
                if (tc != null && !tc.enabled)
                {
                    continue; // 타일맵 콜라이더가 꺼져 있으면 충돌이 없다(Town 의 "Deco" 타일)
                }
                if (tm != null)
                {
                    AddTileRuns(tm, input);
                }
                else
                {
                    AddComposite(cc, input);
                }
            }
            else if (box != null && c.GetComponent<PlatformEffector2D>() != null)
            {
                input.Platforms.Add(BoxRect(box));
            }
            else if (box != null)
            {
                Rect r = BoxRect(box);
                input.Solids.Add(BotNavBuild.SolidFromPaths(new[] { new[] { r.min, new Vector2(r.xMax, r.yMin), r.max, new Vector2(r.xMin, r.yMax) } }));
            }
        }
        foreach (Portal p in map.GetComponentsInChildren<Portal>(true))
        {
            if (p.destination == null)
            {
                continue;
            }
            Vector3 pos = p.transform.position;
            Vector3 to = p.destination.position;
            input.Portals.Add(new BotNavGraph.PortalRec
            {
                x = pos.x, y = pos.y, toMap = p.openMap != null ? p.openMap.name : "", toX = to.x, toY = to.y,
            });
        }
        return input;
    }

    private static void AddComposite(CompositeCollider2D cc, BotNavBuild.Input input)
    {
        List<Vector2[]> paths = new List<Vector2[]>();
        List<Vector2> buf = new List<Vector2>();
        for (int i = 0; i < cc.pathCount; i++)
        {
            buf.Clear();
            cc.GetPath(i, buf);
            Vector2[] w = new Vector2[buf.Count];
            for (int k = 0; k < buf.Count; k++)
            {
                w[k] = cc.transform.TransformPoint(buf[k]);
            }
            paths.Add(w);
        }
        if (paths.Count > 0)
        {
            input.Solids.Add(BotNavBuild.SolidFromPaths(paths));
        }
    }

    private static UnityEngine.Tilemaps.Tilemap s_extraTm;
    private static readonly HashSet<Vector3Int> s_extra = new HashSet<Vector3Int>();

    /// <summary>
    /// 런타임에 놓이는 타일(EarlyForestSteps 중간 발판)을 굽기에도 반영한다 — 씬에는 없고 플레이 때 생긴다(11-15 SideRoad 계단 턱 실측 0.9 어긋남).
    /// 게임과 같은 규칙: 기준점 칸에 타일이 있는 첫 충돌 타일맵.
    /// </summary>
    private static void AddRuntimeSteps(Transform map)
    {
        if (!EarlyForestSteps.AppliesTo(map.name))
        {
            return;
        }
        foreach (UnityEngine.Tilemaps.Tilemap tm in map.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true))
        {
            if (tm.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() == null || !tm.HasTile(CellOf(tm, EarlyForestSteps.ReferencePoint)))
            {
                continue;
            }
            s_extraTm = tm;
            foreach (Vector3 p in EarlyForestSteps.StepPositions)
            {
                Vector3Int first = CellOf(tm, p);
                for (int i = 0; i < EarlyForestSteps.Width; i++)
                {
                    s_extra.Add(first + Vector3Int.right * i);
                }
            }
            return;
        }
    }

    /// <summary>월드 → 칸(비활성 맵은 layoutGrid 변환이 0 을 돌려줘 직접 계산)</summary>
    private static Vector3Int CellOf(UnityEngine.Tilemaps.Tilemap tm, Vector3 world)
    {
        Grid grid = tm.GetComponentInParent<Grid>(true);
        Vector3 cs = grid != null ? grid.cellSize : Vector3.one;
        Vector3 l = tm.transform.InverseTransformPoint(world);
        return new Vector3Int(Mathf.FloorToInt(l.x / cs.x), Mathf.FloorToInt(l.y / cs.y), 0);
    }

    /// <summary>충돌 타일 칸을 행마다 가로로 이어 직사각형 묶음으로 넣는다(칸 = 정사각형 충돌로 본다)</summary>
    private static void AddTileRuns(UnityEngine.Tilemaps.Tilemap tm, BotNavBuild.Input input)
    {
        Grid grid = tm.GetComponentInParent<Grid>(true);
        Vector3 cs = grid != null ? grid.cellSize : Vector3.one;
        BoundsInt cb = tm.cellBounds;
        if (tm == s_extraTm)
        {
            foreach (Vector3Int c in s_extra)
            {
                cb.xMin = Mathf.Min(cb.xMin, c.x);
                cb.xMax = Mathf.Max(cb.xMax, c.x + 1);
                cb.yMin = Mathf.Min(cb.yMin, c.y);
                cb.yMax = Mathf.Max(cb.yMax, c.y + 1);
            }
        }
        for (int y = cb.yMin; y < cb.yMax; y++)
        {
            int run = int.MinValue;
            for (int x = cb.xMin; x <= cb.xMax; x++)
            {
                Vector3Int c = new Vector3Int(x, y, 0);
                bool solid = x < cb.xMax && ((tm.HasTile(c) && tm.GetColliderType(c) != UnityEngine.Tilemaps.Tile.ColliderType.None)
                    || (tm == s_extraTm && s_extra.Contains(c)));
                if (solid && run == int.MinValue)
                {
                    run = x;
                }
                else if (!solid && run != int.MinValue)
                {
                    Vector2 a = tm.transform.TransformPoint(new Vector3(run * cs.x, y * cs.y, 0f));
                    Vector2 b = tm.transform.TransformPoint(new Vector3(x * cs.x, (y + 1) * cs.y, 0f));
                    Rect r = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
                    input.Solids.Add(BotNavBuild.SolidFromPaths(new[] { new[] { r.min, new Vector2(r.xMax, r.yMin), r.max, new Vector2(r.xMin, r.yMax) } }));
                    run = int.MinValue;
                }
            }
        }
    }

    private static Rect BoxRect(BoxCollider2D b)
    {
        Vector2 h = b.size * 0.5f;
        Vector2 p0 = b.transform.TransformPoint(b.offset - h);
        Vector2 p1 = b.transform.TransformPoint(b.offset + h);
        return Rect.MinMaxRect(Mathf.Min(p0.x, p1.x), Mathf.Min(p0.y, p1.y), Mathf.Max(p0.x, p1.x), Mathf.Max(p0.y, p1.y));
    }
}
#endif
