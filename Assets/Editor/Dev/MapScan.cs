#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MAP-SCAN: 맵 씬 오브젝트 정적 스캔(플레이 없이). 메뉴 또는 배치모드 -executeMethod MapScan.Run.
/// 지면 파묻힘·공중 부유·경계 밖·Missing 참조·스폰/포털 도착점 벽 속·동적 Rigidbody 낙하물을 맵별로 출력한다.
/// 결과: Logs/mapscan.txt. 씬은 저장하지 않는다.
/// </summary>
public static class MapScan
{
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    const float FloatGap = 6f;   // 이보다 위에 지면이 없으면 부유
    const float BoundsPad = 25f; // 지면 합집합 + 여유 밖이면 경계 밖

    [MenuItem("Tools/Dev/Map Scan")]
    public static void Run()
    {
        var sb = new StringBuilder();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = GameObject.Find("Map");
        if (root == null) { Out("Map 루트 없음"); return; }
        int ground = LayerMask.GetMask("Ground");
        // 맵은 같은 좌표를 공유하는 것들이 있다 → 한 번에 하나만 켜서 스캔
        var maps = new List<Transform>();
        var was = new List<bool>();
        foreach (Transform m in root.transform) { maps.Add(m); was.Add(m.gameObject.activeSelf); m.gameObject.SetActive(false); }
        for (int i = 0; i < maps.Count; i++)
        {
            maps[i].gameObject.SetActive(true);
            Physics2D.SyncTransforms();
            sb.AppendLine("<!-- 원래 active=" + was[i] + " -->");
            ScanMap(maps[i], ground, sb);
            maps[i].gameObject.SetActive(false);
        }
        Out(sb.ToString());
    }

    public static void Dbg()
    {
        var sb = new StringBuilder();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = GameObject.Find("Map");
        foreach (Transform m in root.transform) { m.gameObject.SetActive(m.name == "Lv1_NearForest_SideRoad"); }
        Physics2D.SyncTransforms();
        int gm = LayerMask.GetMask("Ground");
        foreach (float x in new[] { -262.4f, -258f, -266f, -236.4f })
        {
            sb.AppendLine("x=" + x);
            foreach (var h in Physics2D.RaycastAll(new Vector2(x, 60f), Vector2.down, 100f, gm)) { sb.AppendLine("  hit y=" + h.point.y.ToString("F2") + " " + h.collider.name + " " + h.collider.GetType().Name); }
            foreach (var h in Physics2D.RaycastAll(new Vector2(x, -60f), Vector2.up, 120f, gm)) { sb.AppendLine("  UP hit y=" + h.point.y.ToString("F2") + " " + h.collider.name); }
            for (float yy = 8f; yy >= -24f; yy -= 2f) { sb.Append((Physics2D.OverlapPoint(new Vector2(x, yy), gm) != null ? "#" : ".")); }
            sb.AppendLine("  (y 8..-24 step2)");
            sb.AppendLine("  overlap(-0.3) " + (Physics2D.OverlapPoint(new Vector2(x, -0.3f), gm) != null));
        }
        foreach (var pt in Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (pt.name.Contains("ToNormalForest") || pt.name.Contains("ToSideRoad")) { sb.AppendLine(pt.name + " pos " + pt.transform.position + " dest " + (pt.destination != null ? pt.destination.name + " " + pt.destination.position : "null")); }
        }
        Out(sb.ToString());
    }

    static void Out(string s)
    {
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/mapscan.txt", s);
        Debug.Log("[MapScan] done " + s.Length + " chars");
    }

    static void ScanMap(Transform map, int groundMask, StringBuilder sb)
    {
        var info = map.GetComponent<MapInfo>();
        sb.AppendLine("## " + map.name + (info != null ? " | " + info.displayName : " | (MapInfo 없음)") + " | pos " + map.position + "");
        var grounds = new List<Collider2D>();
        foreach (var c in map.GetComponentsInChildren<Collider2D>(true))
        {
            if (((1 << c.gameObject.layer) & groundMask) != 0 && !c.isTrigger) { grounds.Add(c); }
        }
        sb.AppendLine("- Ground 콜라이더 " + grounds.Count);
        if (grounds.Count == 0) { sb.AppendLine(""); return; }
        Bounds gb = grounds[0].bounds;
        foreach (var g in grounds) { gb.Encapsulate(g.bounds); }
        foreach (var g in grounds) { sb.AppendLine("  · " + g.GetType().Name + " " + Path(g.transform, map) + " x " + g.bounds.min.x.ToString("F1") + ".." + g.bounds.max.x.ToString("F1") + " y " + g.bounds.min.y.ToString("F1") + ".." + g.bounds.max.y.ToString("F1") + (g.GetComponent<PlatformEffector2D>() != null || g.usedByEffector ? " oneway" : "")); }
        sb.AppendLine("- 지면 합집합 x " + gb.min.x.ToString("F1") + ".." + gb.max.x.ToString("F1") + " y " + gb.min.y.ToString("F1") + ".." + gb.max.y.ToString("F1"));
        var area = gb; area.Expand(new Vector3(BoundsPad * 2, BoundsPad * 2, 0));

        foreach (var t in map.GetComponentsInChildren<Transform>(true))
        {
            string path = Path(t, map);
            // Missing script
            foreach (var comp in t.GetComponents<Component>())
            {
                if (comp == null) { sb.AppendLine("- [MissingScript] " + path); continue; }
                if (comp is Transform) { continue; }
                var so = new SerializedObject(comp);
                var it = so.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue == null && it.objectReferenceInstanceIDValue != 0)
                    {
                        sb.AppendLine("- [MissingRef] " + path + " " + comp.GetType().Name + "." + it.propertyPath);
                    }
                }
            }
            var sp = t.GetComponent<EnemySpawner>();
            var portal = t.GetComponent<Portal>();
            var rb = t.GetComponent<Rigidbody2D>();
            var br = t.GetComponent<BreakableProp>();
            Vector2 p = t.position;
            if (sp != null)
            {
                Vector2[] tbl = info != null ? SpawnPointTable.Get(map.name, t.name) : null;
                if (tbl != null)
                {
                    for (int k = 0; k < tbl.Length; k++) { CheckPoint("SpawnPt", path + "#" + k + "(표)", new Vector2(tbl[k].x, tbl[k].y), groundMask, area, sb, true); }
                }
                else if (sp.enemyPositon != null)
                {
                    foreach (Transform c in sp.enemyPositon.transform) { CheckPoint("SpawnPt", path + "/" + c.name, c.position, groundMask, area, sb, true); }
                }
                else { sb.AppendLine("- [SpawnerNoPoints] " + path); }
            }
            if (portal != null)
            {
                CheckPoint("Portal", path, p, groundMask, area, sb, false);
                if (portal.destination == null) { sb.AppendLine("- [PortalNoDest] " + path); }
            }
            if (br != null) { CheckPoint("Breakable", path, p, groundMask, area, sb, true); }
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                sb.AppendLine("- [DynamicRB] " + path + " gravity " + rb.gravityScale + " pos " + F(p) + " layer " + LayerMask.LayerToName(t.gameObject.layer));
                CheckPoint("DynamicRB", path, p, groundMask, area, sb, true);
            }
        }
        // 이 맵을 목적지로 하는 포털 도착점(다른 맵의 포털도 포함)
        foreach (var pt in Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (pt.destination != null && pt.destination.IsChildOf(map))
            {
                CheckPoint("PortalDest", Path(pt.transform, pt.transform.root) + "->" + pt.destination.name, pt.destination.position, groundMask, area, sb, true);
            }
        }
        StepProfile(map, grounds, gb, groundMask, sb);
        // 지면 콜라이더끼리 심한 겹침(동일 bounds 중복)
        int dup = 0;
        for (int i = 0; i < grounds.Count; i++)
        {
            for (int j = i + 1; j < grounds.Count; j++)
            {
                var a = grounds[i]; var b = grounds[j];
                if (a.transform.IsChildOf(b.transform) || b.transform.IsChildOf(a.transform)) { continue; }
                Bounds ba = a.bounds, bb = b.bounds;
                if ((ba.center - bb.center).sqrMagnitude < 0.01f && (ba.size - bb.size).sqrMagnitude < 0.01f)
                {
                    dup++;
                }
            }
        }
        if (dup > 0) { sb.AppendLine("- [DupGround] 동일 bounds 지면 콜라이더 쌍 " + dup + " (타일맵 레이어 겹침 가능, 대개 무해)"); }
        sb.AppendLine("");
    }

    struct Surf { public string name; public float x0, x1, top; public bool oneway; }
    const float JumpUp = 2.6f;     // 1층계 단일 점프(풀홀드 2.87) 여유 반영
    const float JumpGap = 4f;      // 점프 수평 도달
    const float FallGap = 12f;

    // 표면(지면 평탄 구간·원웨이 발판)과 사다리로 도달 그래프를 만들어, 시작 지면에서 못 가는 발판/구간을 찾는다.
    static void StepProfile(Transform map, List<Collider2D> grounds, Bounds gb, int groundMask, StringBuilder sb)
    {
        var surfs = new List<Surf>();
        // 지면(원웨이 아닌 것): 윗면 평탄 구간. 원웨이 박스 위가 아니라 지형 합성 콜라이더만 대상으로 레이캐스트
        var solid = new List<Collider2D>();
        foreach (var g in grounds) { if (!(g is BoxCollider2D) || (!g.usedByEffector && g.GetComponent<PlatformEffector2D>() == null)) { solid.Add(g); } }
        float topY = gb.max.y + 5f;
        int runStart = -1; float runY = float.NaN, lastX = 0;
        var filter = new ContactFilter2D { useLayerMask = true, layerMask = groundMask, useTriggers = false };
        var hits = new RaycastHit2D[16];
        for (float x = gb.min.x; x <= gb.max.x + 0.01f; x += 0.25f)
        {
            float y = float.NaN;
            int n = Physics2D.Raycast(new Vector2(x, topY), Vector2.down, filter, hits, gb.size.y + 10f);
            for (int k = 0; k < n; k++)
            {
                if (solid.Contains(hits[k].collider)) { y = hits[k].point.y; break; }
            }
            bool cont = !float.IsNaN(y) && !float.IsNaN(runY) && Mathf.Abs(y - runY) < 0.3f;
            if (!cont)
            {
                if (runStart >= 0 && lastX - runStart * 0.25f - gb.min.x >= -1) { }
                if (!float.IsNaN(runY) && runStart >= 0) { surfs.Add(new Surf { name = "ground", x0 = gb.min.x + runStart * 0.25f, x1 = lastX, top = runY }); }
                runStart = float.IsNaN(y) ? -1 : (int)Mathf.Round((x - gb.min.x) / 0.25f);
                runY = y;
            }
            lastX = x;
        }
        if (runStart >= 0 && !float.IsNaN(runY)) { surfs.Add(new Surf { name = "ground", x0 = gb.min.x + runStart * 0.25f, x1 = lastX, top = runY }); }
        int groundCount = surfs.Count;
        foreach (var g in grounds)
        {
            if (g is BoxCollider2D && (g.usedByEffector || g.GetComponent<PlatformEffector2D>() != null))
            {
                surfs.Add(new Surf { name = Path(g.transform, map), x0 = g.bounds.min.x, x1 = g.bounds.max.x, top = g.bounds.max.y, oneway = true });
            }
        }
        // 사다리
        var ladders = new List<Bounds>();
        foreach (var t in map.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("Ladder"))
            {
                var c = t.GetComponent<Collider2D>();
                if (c != null) { ladders.Add(c.bounds); }
            }
        }
        sb.AppendLine("- 표면 " + surfs.Count + " (지면구간 " + groundCount + ", 발판 " + (surfs.Count - groundCount) + "), 사다리 " + ladders.Count);
        int N = surfs.Count;
        var reach = new bool[N];
        var q = new Queue<int>();
        // 시작: 가장 넓은 지면 구간
        int start = -1; float best = 0;
        for (int i = 0; i < groundCount; i++) { float w = surfs[i].x1 - surfs[i].x0; if (w > best) { best = w; start = i; } }
        if (start < 0) { return; }
        reach[start] = true; q.Enqueue(start);
        // 시드: 이 맵의 포털·도착점 아래 표면
        var seeds = new List<Vector2>();
        foreach (var pt in Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (pt.transform.IsChildOf(map)) { seeds.Add(pt.transform.position); }
            if (pt.destination != null && pt.destination.IsChildOf(map)) { seeds.Add(pt.destination.position); }
        }
        foreach (var sp in seeds)
        {
            int bi = -1; float bd = 1e9f;
            for (int i = 0; i < N; i++)
            {
                if (sp.x < surfs[i].x0 - 0.5f || sp.x > surfs[i].x1 + 0.5f) { continue; }
                float d = sp.y + 1.5f - surfs[i].top;
                if (d >= 0 && d < bd) { bd = d; bi = i; }
            }
            if (bi >= 0 && !reach[bi]) { reach[bi] = true; q.Enqueue(bi); }
        }
        while (q.Count > 0)
        {
            int u = q.Dequeue();
            for (int v = 0; v < N; v++)
            {
                if (reach[v]) { continue; }
                float gap = Mathf.Max(0f, Mathf.Max(surfs[v].x0 - surfs[u].x1, surfs[u].x0 - surfs[v].x1));
                float dy = surfs[v].top - surfs[u].top;
                bool ok = (dy <= 0.05f && gap <= FallGap) || (dy > 0.05f && dy <= JumpUp && gap <= JumpGap);
                if (!ok)
                {
                    foreach (var l in ladders)
                    {
                        bool onU = l.min.x <= surfs[u].x1 + 0.5f && l.max.x >= surfs[u].x0 - 0.5f && surfs[u].top >= l.min.y - 2.6f && surfs[u].top <= l.max.y + 0.7f;
                        bool onV = l.min.x <= surfs[v].x1 + 0.5f && l.max.x >= surfs[v].x0 - 0.5f && surfs[v].top >= l.min.y - 2.6f && surfs[v].top <= l.max.y + 0.7f;
                        if (onU && onV) { ok = true; break; }
                    }
                }
                if (ok) { reach[v] = true; q.Enqueue(v); }
            }
        }
        foreach (var l in ladders) { sb.AppendLine("  L ladder x " + l.min.x.ToString("F1") + ".." + l.max.x.ToString("F1") + " y " + l.min.y.ToString("F1") + ".." + l.max.y.ToString("F1")); }
        for (int i = 0; i < N; i++) { sb.AppendLine("  S " + (reach[i] ? "ok " : "NO ") + surfs[i].name + " x " + surfs[i].x0.ToString("F1") + ".." + surfs[i].x1.ToString("F1") + " top " + surfs[i].top.ToString("F2")); }
        for (int i = 0; i < N; i++)
        {
            if (!reach[i]) { sb.AppendLine("- [Unreachable] " + surfs[i].name + " x " + surfs[i].x0.ToString("F1") + ".." + surfs[i].x1.ToString("F1") + " top " + surfs[i].top.ToString("F2") + " (점프 " + JumpUp + "·사다리로 시작 지면에서 도달 불가)"); }
        }
    }

    static void CheckPoint(string kind, string path, Vector2 p, int groundMask, Bounds area, StringBuilder sb, bool needGroundBelow)
    {
        if (!area.Contains(new Vector3(p.x, p.y, area.center.z))) { sb.AppendLine("- [OutOfBounds] " + kind + " " + path + " " + F(p)); }
        if (IsBuried(p + Vector2.up * 0.1f, groundMask)) { sb.AppendLine("- [InGround] " + kind + " " + path + " " + F(p) + " (지형 속에 묻힘)"); return; }
        if (!needGroundBelow) { return; }
        var r = Physics2D.Raycast(p, Vector2.down, 200f, groundMask);
        if (r.collider == null) { sb.AppendLine("- [NoGroundBelow] " + kind + " " + path + " " + F(p)); }
        else if (r.distance > (kind == "SpawnPt" ? 12f : FloatGap)) { sb.AppendLine("- [Floating] " + kind + " " + path + " " + F(p) + " 지면까지 " + r.distance.ToString("F1")); }
    }

    // 합성 콜라이더가 Outlines 형이면 OverlapPoint 가 항상 false 라서, 위쪽 레이의 외곽선 교차 수 홀짝으로 내부를 판정한다.
    static bool IsBuried(Vector2 p, int groundMask)
    {
        int n = 0;
        foreach (var h in Physics2D.RaycastAll(p, Vector2.up, 120f, groundMask))
        {
            if (h.collider is CompositeCollider2D) { n++; }
        }
        return (n & 1) == 1;
    }

    static string F(Vector2 v) { return "(" + v.x.ToString("F1") + "," + v.y.ToString("F1") + ")"; }

    static string Path(Transform t, Transform stop)
    {
        var s = t.name;
        while (t.parent != null && t.parent != stop) { t = t.parent; s = t.name + "/" + s; }
        return s;
    }
}
#endif
