using UnityEngine;

/// <summary>
/// 정예 둥지 추적 배치(씬 파일을 고치지 않고 부트 때 맵 오브젝트 아래에 만든다, TrackingSpotPlacer 와 같은 방식).
/// 맵마다 흔적 3 + 둥지 1 = 표식 4개의 x/y. y 는 10-30 에 잰 지면 높이이고 표식이 처음 켜질 때 아래 지면에 다시 붙는다.
/// 흔적 추적 지점(보통 숲 −290·깊은 숲 −270)·채집 지점·포털과 겹치지 않게 골랐다. 표시는 코드 도트(발자국 / 나뭇가지 둥지).
/// </summary>
public static class NestHuntPlacer
{
    private struct Layout
    {
        public string Map;
        public Vector2[] Marks; // 흔적 0~2, 둥지

        public Layout(string map, params Vector2[] marks)
        {
            Map = map;
            Marks = marks;
        }
    }

    private static readonly Layout[] Layouts =
    {
        // 보통 숲: 오른쪽 시작 → 왼쪽 → 되돌아 오른쪽 → 왼쪽 끝 둥지(포털 −425 에서 10 이상)
        new Layout("Lv1_NormalForest", new Vector2(-322f, 0.4f), new Vector2(-380f, 1.4f), new Vector2(-345f, -1.6f), new Vector2(-413f, -0.6f)),
        // 깊은 숲: 가운데 시작 → 왼쪽 끝 → 오른쪽 → 오른쪽 둥지(발굴 −345·채집 −375·추적 −270 과 15 이상)
        new Layout("Lv1_DeepForest", new Vector2(-322f, 0.4f), new Vector2(-398f, -0.6f), new Vector2(-360f, 1.4f), new Vector2(-300f, 0.4f)),
    };

    private static Sprite _trail;
    private static Sprite _nest;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _trail = null;
        _nest = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        MapInfo[] maps = Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Layout layout in Layouts)
        {
            MapInfo map = Find(maps, layout.Map);
            if (map == null)
            {
                Debug.LogWarning("[PB][NestHunt] 맵 없음: " + layout.Map);
                continue;
            }
            Create(map.transform, layout);
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

    private static void Create(Transform parent, Layout layout)
    {
        var siteGo = new GameObject("NestHuntSite");
        siteGo.transform.SetParent(parent, false);
        var site = siteGo.AddComponent<NestHuntSite>();
        for (int i = 0; i < layout.Marks.Length; i++)
        {
            CreateMark(siteGo.transform, site, i, layout.Marks[i], parent.position.z);
        }
    }

    private static void CreateMark(Transform parent, NestHuntSite site, int index, Vector2 pos, float z)
    {
        bool nest = index == NestHuntRules.Trails;
        var go = new GameObject(nest ? "NestMark_Nest" : "NestMark_Trail" + index);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, z);
        go.layer = LayerMask.NameToLayer("Default");
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(nest ? 4f : 3f, 2.5f);
        box.offset = new Vector2(0f, 1f);

        var mark = new GameObject("Mark");
        mark.transform.SetParent(go.transform, false);
        var sr = mark.AddComponent<SpriteRenderer>();
        sr.sprite = nest ? NestSprite() : TrailSprite();
        sr.sortingOrder = 20;

        go.AddComponent<NestMark>().Init(site, index, sr);
    }

    /// <summary>흔적 표식(22×10, PPU 16): 땅에 찍힌 큰 발자국 둘(짙은 흙색) + 눌린 풀 몇 가닥</summary>
    private static Sprite TrailSprite()
    {
        if (_trail != null)
        {
            return _trail;
        }
        const int w = 22;
        const int h = 10;
        var print = new Color32(70, 52, 40, 255);
        var rim = new Color32(176, 150, 110, 255);
        var grass = new Color32(86, 110, 62, 255);
        Color32[] px = MiniGameSprites.Blank(w * h);
        int[] footX = { 5, 15 };
        int[] footY = { 1, 3 };
        for (int k = 0; k < footX.Length; k++)
        {
            int bx = footX[k];
            int by = footY[k];
            for (int dy = 0; dy < 3; dy++)
            {
                for (int dx = 0; dx < 4; dx++)
                {
                    px[(by + dy) * w + bx + dx] = print; // 발바닥
                }
            }
            px[(by - 1) * w + bx + 1] = rim;
            px[(by - 1) * w + bx + 2] = rim;
            px[(by + 3) * w + bx] = print; // 발가락 셋
            px[(by + 3) * w + bx + 2] = print;
            px[(by + 3) * w + bx + 3] = print;
        }
        int[] blades = { 1, 10, 20 };
        foreach (int x in blades)
        {
            px[0 * w + x] = grass;
            px[1 * w + x] = grass;
            px[2 * w + x + (x % 2)] = grass;
        }
        _trail = MiniGameSprites.MakeSprite(px, w, h);
        return _trail;
    }

    /// <summary>둥지 표식(32×18, PPU 16): 엮은 나뭇가지 더미 + 어두운 입구 + 뼈 조각과 붉은 눈 한 쌍</summary>
    private static Sprite NestSprite()
    {
        if (_nest != null)
        {
            return _nest;
        }
        const int w = 32;
        const int h = 18;
        var twig = new Color32(108, 80, 54, 255);
        var twigDark = new Color32(70, 52, 38, 255);
        var hollow = new Color32(22, 18, 20, 255);
        var bone = new Color32(218, 208, 180, 255);
        var eye = new Color32(220, 70, 60, 255);
        Color32[] px = MiniGameSprites.Blank(w * h);
        for (int y = 0; y < 12; y++)
        {
            int half = 14 - y * 6 / 12 - (y > 8 ? (y - 8) * 2 : 0);
            for (int x = 16 - half; x <= 15 + half; x++)
            {
                bool dark = (x * 3 + y * 5) % 7 < 2;
                px[y * w + x] = dark ? twigDark : twig;
            }
        }
        for (int y = 0; y < 7; y++) // 어두운 입구(위가 둥근 반원)
        {
            int half = y > 4 ? 3 - (y - 4) : 4;
            for (int x = 16 - half; x <= 15 + half; x++)
            {
                px[y * w + x] = hollow;
            }
        }
        px[3 * w + 13] = eye;
        px[3 * w + 18] = eye;
        px[0 * w + 4] = bone;
        px[0 * w + 5] = bone;
        px[1 * w + 6] = bone;
        px[0 * w + 26] = bone;
        px[0 * w + 27] = bone;
        px[0 * w + 28] = bone;
        _nest = MiniGameSprites.MakeSprite(px, w, h);
        return _nest;
    }
}
