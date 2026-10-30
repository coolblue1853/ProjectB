using UnityEngine;

/// <summary>
/// 발굴 유적 배치(씬 파일을 고치지 않고 부트 때 맵 오브젝트 아래에 만든다, GatherNodePlacer 와 같은 방식).
/// y 는 10-30 에 잰 지면 높이이고, 유적이 처음 켜질 때 아래 지면에 다시 붙는다. 표시는 코드 도트(네 갈래 별이 새겨진 무너진 벽돌 벽).
/// 지금(층계 1): 탑 근교(탑 외벽) 1·산적 성채(폐허) 1·깊은 숲(숲 폐허) 1 — 포털과 30 이상, 채집 지점과 25 이상 떨어진 자리(10-30 레이캐스트로 잼).
/// </summary>
public static class ExcavationSitePlacer
{
    private struct Placement
    {
        public string Map;
        public Vector2 Position;

        public Placement(string map, float x, float y)
        {
            Map = map;
            Position = new Vector2(x, y);
        }
    }

    private static readonly Placement[] Placements =
    {
        new Placement("Lv1_TowerOutskirts", -285f, 1.4f),
        new Placement("Lv1_BanditFortress", -360f, 1.4f),
        new Placement("Lv1_DeepForest", -345f, -1.6f),
    };

    private const int Floor = 1;

    private static Sprite _wall;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _wall = null;
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
                Debug.LogWarning("[PB][Excavation] 맵 없음: " + Placements[i].Map);
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
        var go = new GameObject("ExcavationSite_" + index);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(p.Position.x, p.Position.y, parent.position.z);
        go.layer = LayerMask.NameToLayer("Default");
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(3f, 2.5f);
        box.offset = new Vector2(0f, 1f);

        var mark = new GameObject("Mark");
        mark.transform.SetParent(go.transform, false);
        var sr = mark.AddComponent<SpriteRenderer>();
        sr.sprite = WallSprite();
        sr.sortingOrder = 20;

        go.AddComponent<ExcavationSite>().Init(Floor, sr);
    }

    /// <summary>무너진 유적 벽(24×20, PPU 16): 벽돌 줄눈 + 위쪽 이끼 + 가운데 네 갈래 별 문장(바랜 금색) + 금 간 자국</summary>
    private static Sprite WallSprite()
    {
        if (_wall != null)
        {
            return _wall;
        }
        const int w = 24;
        const int h = 20;
        var mortar = new Color32(58, 52, 54, 255);
        var stone = new Color32(112, 104, 98, 255);
        var lit = new Color32(140, 132, 120, 255);
        var moss = new Color32(74, 104, 66, 255);
        var star = new Color32(196, 160, 86, 255);
        var crack = new Color32(36, 32, 36, 255);
        Color32[] px = MiniGameSprites.Blank(w * h);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int ragged = (x * 7 % 5) + (x > 15 ? 4 : 0); // 오른쪽이 무너져 낮다
                if (y > h - 1 - ragged)
                {
                    continue;
                }
                int row = y / 4;
                bool joint = y % 4 == 0 || (x + (row % 2) * 3) % 6 == 0;
                px[y * w + x] = joint ? mortar : (y > h - 7 && x < 12 ? lit : stone);
                if (y >= h - 2 - ragged && y <= h - 1 - ragged && x % 3 != 1)
                {
                    px[y * w + x] = moss;
                }
            }
        }
        int cx = 9;
        int cy = 9;
        for (int d = -3; d <= 3; d++)
        {
            px[(cy + d) * w + cx] = star;
            px[cy * w + cx + d] = star;
        }
        px[(cy + 1) * w + cx + 1] = star;
        px[(cy - 1) * w + cx - 1] = star;
        px[(cy + 1) * w + cx - 1] = star;
        px[(cy - 1) * w + cx + 1] = star;
        int[] crackX = { 16, 17, 17, 18, 18, 19 };
        for (int i = 0; i < crackX.Length; i++)
        {
            px[(3 + i * 2) * w + crackX[i]] = crack;
            px[(4 + i * 2) * w + crackX[i]] = crack;
        }
        _wall = MiniGameSprites.MakeSprite(px, w, h);
        return _wall;
    }
}
