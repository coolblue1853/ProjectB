using UnityEngine;

/// <summary>
/// 흔적 추적 지점 배치(씬 파일을 고치지 않고 부트 때 맵 오브젝트 아래에 만든다, ExcavationSitePlacer 와 같은 방식).
/// y 는 10-30 에 잰 지면 높이이고, 지점이 처음 켜질 때 아래 지면에 다시 붙는다. 표시는 코드 도트(탐험가 표식 말뚝 + 땅의 발자국).
/// 지금(층계 1): 보통 숲 −290·깊은 숲 −270·늪 −380 — 포털과 25 이상, 다른 미니게임 지점과 50 이상 떨어진 자리(10-30 레이캐스트로 잼).
/// </summary>
public static class TrackingSpotPlacer
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
        new Placement("Lv1_NormalForest", -290f, 1.4f),
        new Placement("Lv1_DeepForest", -270f, 2.4f),
        new Placement("Lv1_Swamp", -380f, -179.5f),
    };

    private static Sprite _sign;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _sign = null;
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
                Debug.LogWarning("[PB][Tracking] 맵 없음: " + Placements[i].Map);
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
        var go = new GameObject("TrackingSpot_" + index);
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
        sr.sprite = SignSprite();
        sr.sortingOrder = 20;

        go.AddComponent<TrackingSpot>().Init(sr);
    }

    /// <summary>흔적 표식(24×18, PPU 16): 기울어진 나무 말뚝 + 찢어진 붉은 천 + 땅에 찍힌 발자국 세 개(짙은 흙색)</summary>
    private static Sprite SignSprite()
    {
        if (_sign != null)
        {
            return _sign;
        }
        const int w = 24;
        const int h = 18;
        var wood = new Color32(104, 78, 54, 255);
        var woodDark = new Color32(66, 48, 36, 255);
        var cloth = new Color32(150, 58, 50, 255);
        var clothDark = new Color32(104, 40, 38, 255);
        var print = new Color32(48, 40, 34, 255);
        Color32[] px = MiniGameSprites.Blank(w * h);
        for (int y = 0; y < 16; y++)
        {
            int x = 6 + y / 6; // 오른쪽으로 살짝 기운 말뚝
            px[y * w + x] = wood;
            px[y * w + x + 1] = woodDark;
        }
        for (int y = 10; y < 15; y++)
        {
            int len = 5 - (14 - y) / 2 - (y % 2);
            for (int i = 0; i < len; i++)
            {
                int x = 9 + i;
                px[y * w + x] = i == len - 1 ? clothDark : cloth;
            }
        }
        int[] printX = { 13, 17, 21 };
        int[] printY = { 0, 1, 0 };
        for (int k = 0; k < printX.Length; k++)
        {
            int bx = printX[k];
            int by = printY[k];
            px[by * w + bx] = print;
            px[by * w + bx + 1] = print;
            px[(by + 2) * w + bx - 1] = print;
            px[(by + 2) * w + bx + 2] = print;
            px[(by + 1) * w + bx] = print;
        }
        _sign = MiniGameSprites.MakeSprite(px, w, h);
        return _sign;
    }
}
