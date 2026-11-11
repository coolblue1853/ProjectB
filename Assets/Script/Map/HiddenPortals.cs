using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>숨은 포털 한 쌍 (Resources/HiddenPortals.csv 한 줄). 같은 맵 안 A↔B 양방향</summary>
public sealed class HiddenPortalRow
{
    public string Map;
    public string Id;
    public Vector2 A;
    public Vector2 B;
    public string Note;
}

/// <summary>
/// 숨은 포털 표 로더·검증 (FastTravel.md §6, P4). 컬럼: map,id,ax,ay,bx,by,note (추가는 맨 뒤). 쉼표 없는 평문 CSV.
/// 좌표 y 는 지면 + 약 1.1(포털·도착 높이). 실제 y 는 맵이 처음 켜질 때 지면에 맞춘다(HiddenPortalSnap).
/// </summary>
public static class HiddenPortalTable
{
    /// <summary>두 끝이 이보다 가까우면 이동 보조 의미가 없다 (유닛)</summary>
    public const float MinSeparation = 6f;

    public static List<HiddenPortalRow> Parse(string csv)
    {
        var list = new List<HiddenPortalRow>();
        string[] lines = (csv ?? "").TrimStart((char)0xFEFF).Replace("\r", "").Split('\n');
        var ids = new HashSet<string>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Length == 0)
            {
                continue;
            }
            string[] c = lines[i].Split(',');
            if (c.Length < 6 || !Num(c[2], out float ax) || !Num(c[3], out float ay) || !Num(c[4], out float bx) || !Num(c[5], out float by))
            {
                continue;
            }
            var row = new HiddenPortalRow { Map = c[0].Trim(), Id = c[1].Trim(), A = new Vector2(ax, ay), B = new Vector2(bx, by), Note = c.Length > 6 ? c[6].Trim() : "" };
            if (IsValid(row) && ids.Add(row.Map + "/" + row.Id))
            {
                list.Add(row);
            }
        }
        return list;
    }

    public static bool IsValid(HiddenPortalRow r)
    {
        return r != null && r.Map.Length > 0 && r.Id.Length > 0 && (r.A - r.B).magnitude >= MinSeparation;
    }

    private static bool Num(string s, out float v)
    {
        return float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }
}

/// <summary>부트 때 표대로 맵마다 숨은 포털을 만든다(씬 수정 없음, PitReturn·RoadLink 와 같은 방식). 끄기: 표에서 줄 삭제</summary>
public static class HiddenPortals
{
    private const string ResourceName = "HiddenPortals";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        TextAsset ta = Resources.Load<TextAsset>(ResourceName);
        if (ta == null)
        {
            return;
        }
        MapInfo[] maps = Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (HiddenPortalRow row in HiddenPortalTable.Parse(ta.text))
        {
            MapInfo map = System.Array.Find(maps, m => m.name == row.Map);
            Portal sibling = map != null ? map.GetComponentInChildren<Portal>(true) : null;
            if (map == null || sibling == null)
            {
                Debug.LogWarning("[PB][HiddenPortal] 맵/기준 포털 없음: " + row.Map);
                continue;
            }
            Build(map.transform, sibling, row);
        }
    }

    private static void Build(Transform map, Portal sibling, HiddenPortalRow row)
    {
        float z = sibling.transform.position.z;
        Portal a = Create(map, sibling, "HiddenPortal_" + row.Id + "_A", new Vector3(row.A.x, row.A.y, z));
        Portal b = Create(map, sibling, "HiddenPortal_" + row.Id + "_B", new Vector3(row.B.x, row.B.y, z));
        a.destination = b.transform;
        b.destination = a.transform;
    }

    private static Portal Create(Transform map, Portal sibling, string objectName, Vector3 position)
    {
        var go = new GameObject(objectName);
        go.SetActive(false); // 필드를 다 채운 뒤 켠다 (Portal.Awake 가 완성된 상태로 돈다)
        go.transform.SetParent(map, false);
        go.transform.position = position;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(4f, 3f); // 반경 2 안이면 힌트
        col.offset = new Vector2(0f, 0.4f);
        var portal = go.AddComponent<Portal>();
        portal.hiddenPortal = true;
        portal.labelOverride = "Other side";
        portal.proCamera = sibling.proCamera;
        go.AddComponent<HiddenPortalSnap>();
        go.AddComponent<HiddenPortalClick>().Init(portal, col);
        go.SetActive(true);
        return portal;
    }
}

/// <summary>맵이 처음 켜진 프레임에 포털 높이를 지면(+1.1)에 맞춘다. 맵이 꺼져 있으면 부트 때 레이캐스트가 안 맞아서 이렇게 미룬다</summary>
public sealed class HiddenPortalSnap : MonoBehaviour
{
    private const float FootOffset = 1.1f;
    private bool _done;

    private void Update()
    {
        if (_done)
        {
            return;
        }
        _done = true;
        Vector3 before = transform.position;
        GroundSnap.Snap(transform, 1.5f, 5f);
        if (transform.position.y != before.y)
        {
            transform.position += Vector3.up * FootOffset;
        }
    }
}

/// <summary>마우스 경로: 플레이어가 포털 범위 안일 때 포털 자리를 좌클릭하면 탄다 (키보드·패드는 Portal 의 ↑)</summary>
public sealed class HiddenPortalClick : MonoBehaviour
{
    private Portal _portal;
    private Collider2D _col;
    private Camera _cam;

    public void Init(Portal portal, Collider2D col)
    {
        _portal = portal;
        _col = col;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame || _portal == null || _portal.player == null || DatabaseManager.isOpenUI)
        {
            return;
        }
        if (_cam == null)
        {
            _cam = Camera.main;
        }
        if (_cam == null)
        {
            return;
        }
        Vector3 world = _cam.ScreenToWorldPoint(mouse.position.ReadValue());
        if (_col.OverlapPoint(world))
        {
            _portal.Use(_portal.player);
        }
    }
}
