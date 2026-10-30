using UnityEngine;

/// <summary>
/// 채집 지점 배치(씬 파일을 고치지 않고 부트 때 맵 오브젝트 아래에 만든다, FishingSpotPlacer 와 같은 방식).
/// y 는 10-30 에 잰 지면 높이(발판 없는 자리)이고, 지점이 처음 켜질 때 아래 지면에 다시 붙는다(지형이 바뀌어도 안전). 표시는 코드 도트(풀 덤불 / 결정 박힌 바위).
/// 지금(층계 1): 표본 = 가까운 숲 샛길 2·보통 숲 1·깊은 숲 1, 광맥 = 탑 근교 2·산적 성채 1 — 포털과 15 이상 떨어진 자리.
/// </summary>
public static class GatherNodePlacer
{
    private struct Placement
    {
        public string Map;
        public Vector2 Position;
        public GatherKind Kind;

        public Placement(string map, float x, float y, GatherKind kind)
        {
            Map = map;
            Position = new Vector2(x, y);
            Kind = kind;
        }
    }

    private static readonly Placement[] Placements =
    {
        new Placement("Lv1_NearForest_SideRoad", -350f, -1.6f, GatherKind.Specimen),
        new Placement("Lv1_NearForest_SideRoad", -420f, -0.6f, GatherKind.Specimen),
        new Placement("Lv1_NormalForest", -360f, 1.4f, GatherKind.Specimen),
        new Placement("Lv1_DeepForest", -375f, 1.4f, GatherKind.Specimen),
        new Placement("Lv1_TowerOutskirts", -330f, 0.4f, GatherKind.Ore),
        new Placement("Lv1_TowerOutskirts", -370f, 1.4f, GatherKind.Ore),
        new Placement("Lv1_BanditFortress", -320f, 0.4f, GatherKind.Ore),
    };

    private const int Floor = 1;

    private static Sprite _bush;
    private static Sprite _vein;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _bush = null;
        _vein = null;
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
                Debug.LogWarning("[PB][Gather] 맵 없음: " + Placements[i].Map);
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
        var go = new GameObject((p.Kind == GatherKind.Ore ? "OreVein_" : "Specimen_") + index);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(p.Position.x, p.Position.y, parent.position.z);
        go.layer = LayerMask.NameToLayer("Default");
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(2.5f, 2.5f);
        box.offset = new Vector2(0f, 1f);

        var mark = new GameObject("Mark");
        mark.transform.SetParent(go.transform, false);
        var sr = mark.AddComponent<SpriteRenderer>();
        sr.sprite = p.Kind == GatherKind.Ore ? VeinSprite() : BushSprite();
        sr.sortingOrder = 20;

        go.AddComponent<GatherNode>().Init(p.Kind, Floor, sr);
    }

    /// <summary>표본 덤불(16×12, PPU 16): 어두운 잎 무더기 + 밝은 잎 + 노란 반짝임 2점</summary>
    private static Sprite BushSprite()
    {
        if (_bush != null)
        {
            return _bush;
        }
        const int w = 16;
        const int h = 12;
        var dark = new Color32(46, 74, 52, 255);
        var mid = new Color32(78, 116, 70, 255);
        var light = new Color32(132, 168, 96, 255);
        var glint = new Color32(240, 220, 120, 255);
        Color32[] px = MiniGameSprites.Blank(w * h);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - 7.5f) / 7.5f;
                float dy = y / 11f;
                if (dx * dx + dy * dy * 1.1f > 1f)
                {
                    continue;
                }
                px[y * w + x] = y < 3 ? dark : ((x + y) % 3 == 0 ? light : mid);
            }
        }
        px[8 * w + 5] = glint;
        px[6 * w + 11] = glint;
        _bush = MiniGameSprites.MakeSprite(px, w, h);
        return _bush;
    }

    /// <summary>광맥 바위(16×14, PPU 16): 회갈색 바위 + 청록 결정 3개</summary>
    private static Sprite VeinSprite()
    {
        if (_vein != null)
        {
            return _vein;
        }
        const int w = 16;
        const int h = 14;
        var dark = new Color32(58, 54, 60, 255);
        var rock = new Color32(98, 90, 92, 255);
        var lit = new Color32(132, 124, 120, 255);
        var crystal = new Color32(96, 196, 200, 255);
        var shine = new Color32(210, 250, 245, 255);
        Color32[] px = MiniGameSprites.Blank(w * h);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - 7.5f) / 8f;
                float dy = y / 13f;
                if (dx * dx + dy * dy > 1f)
                {
                    continue;
                }
                px[y * w + x] = y < 2 ? dark : (x < 7 && y > 6 ? lit : rock);
            }
        }
        int[,] crystals = { { 5, 5 }, { 9, 8 }, { 11, 4 } };
        for (int i = 0; i < crystals.GetLength(0); i++)
        {
            int cx = crystals[i, 0];
            int cy = crystals[i, 1];
            px[cy * w + cx] = crystal;
            px[(cy + 1) * w + cx] = crystal;
            px[cy * w + cx + 1] = crystal;
            px[(cy + 2) * w + cx] = shine;
        }
        _vein = MiniGameSprites.MakeSprite(px, w, h);
        return _vein;
    }
}
