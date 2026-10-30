using UnityEngine;

/// <summary>
/// 낚시터 배치(씬 파일을 고치지 않고 부트 때 맵 오브젝트 아래에 만든다). 맵은 MapInfo 이름으로 찾는다.
/// 지금: 1층계 늪(Lv1_Swamp) 물가 2곳 — 지면 y=-180, 포털(x -417, -186)·사다리와 떨어진 자리. 물결 표시는 코드 도트.
/// </summary>
public static class FishingSpotPlacer
{
    private struct Placement
    {
        public string Map;
        public Vector2 Position;
        public int Floor;

        public Placement(string map, float x, float y, int floor)
        {
            Map = map;
            Position = new Vector2(x, y);
            Floor = floor;
        }
    }

    private static readonly Placement[] Placements =
    {
        new Placement("Lv1_Swamp", -330f, -179.5f, 1),
        new Placement("Lv1_Swamp", -262f, -179.5f, 1),
    };

    private static Sprite _ripple;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _ripple = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        MapInfo[] maps = Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < Placements.Length; i++)
        {
            MapInfo map = Find(maps, Placements[i].Map);
            if (map == null)
            {
                Debug.LogWarning("[PB][Fishing] 맵 없음: " + Placements[i].Map);
                continue;
            }
            Create(map.transform, Placements[i], i);
        }
    }

    private static MapInfo Find(MapInfo[] maps, string name)
    {
        foreach (MapInfo m in maps)
        {
            if (m.name == name)
            {
                return m;
            }
        }
        return null;
    }

    private static void Create(Transform parent, Placement p, int index)
    {
        var go = new GameObject("FishingSpot_" + index);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(p.Position.x, p.Position.y, parent.position.z);
        go.layer = LayerMask.NameToLayer("Default");
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(3f, 2.5f);
        box.offset = new Vector2(0f, 1f);
        go.AddComponent<FishingSpot>().Floor = p.Floor;

        var mark = new GameObject("Ripple");
        mark.transform.SetParent(go.transform, false);
        mark.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        var sr = mark.AddComponent<SpriteRenderer>();
        sr.sprite = RippleSprite();
        sr.sortingOrder = 20;
        mark.AddComponent<RippleBob>();
    }

    /// <summary>물결 표시(16×5 코드 도트, PPU 16): 옅은 하늘색 두 줄 + 가운데 반짝임</summary>
    private static Sprite RippleSprite()
    {
        if (_ripple != null)
        {
            return _ripple;
        }
        const int w = 16;
        const int h = 5;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var clear = new Color32(0, 0, 0, 0);
        var light = new Color32(150, 205, 220, 200);
        var deep = new Color32(70, 120, 150, 180);
        var glint = new Color32(235, 250, 255, 230);
        var px = new Color32[w * h];
        for (int i = 0; i < px.Length; i++)
        {
            px[i] = clear;
        }
        for (int x = 1; x < 15; x++)
        {
            px[0 * w + x] = deep;
        }
        for (int x = 3; x < 13; x++)
        {
            px[2 * w + x] = light;
        }
        for (int x = 6; x < 10; x++)
        {
            px[4 * w + x] = light;
        }
        px[2 * w + 7] = glint;
        px[2 * w + 8] = glint;
        tex.SetPixels32(px);
        tex.Apply();
        _ripple = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 16f);
        return _ripple;
    }
}
