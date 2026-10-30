using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 캠프 화로 배치(씬 파일을 고치지 않고 부트 때 맵 오브젝트 아래에 만든다, ExcavationSitePlacer 와 같은 방식).
/// y 는 지면 높이이고, 화로가 처음 켜질 때 아래 지면에 다시 붙는다. 표시는 코드 도트(돌 테두리 + 장작 + 불꽃)와 LightFlicker 불빛.
/// 지금(층계 1): 초반 숲 샛길(야영지 자리) 1 · 늪 1 — 포털·다른 미니게임 지점과 떨어진 자리.
/// </summary>
public static class CookingStovePlacer
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
        new Placement("Lv1_NearForest_SideRoad", -375f, 1.4f),
        new Placement("Lv1_Swamp", -296f, -180f),
    };

    private static Sprite _fire;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _fire = null;
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
                Debug.LogWarning("[PB][Cooking] 맵 없음: " + Placements[i].Map);
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
        var go = new GameObject("CookingStove_" + index);
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
        sr.sprite = FireSprite();
        sr.sortingOrder = 20;

        var glow = new GameObject("Glow");
        glow.transform.SetParent(go.transform, false);
        glow.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        var light = glow.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = new Color(1f, 0.62f, 0.28f);
        light.intensity = 0.9f;
        light.pointLightOuterRadius = 4.5f;
        light.pointLightInnerRadius = 0.4f;
        glow.AddComponent<LightFlicker>();

        go.AddComponent<CookingStove>().Init(sr);
    }

    /// <summary>모닥불(24×18, PPU 16): 돌 테두리 + 엇갈린 장작 + 세 겹 불꽃(빨강·주황·노랑)</summary>
    private static Sprite FireSprite()
    {
        if (_fire != null)
        {
            return _fire;
        }
        const int w = 24;
        const int h = 18;
        var stoneDark = new Color32(70, 66, 72, 255);
        var stone = new Color32(118, 112, 110, 255);
        var stoneLit = new Color32(150, 142, 132, 255);
        var wood = new Color32(98, 64, 40, 255);
        var woodLit = new Color32(132, 88, 52, 255);
        var red = new Color32(200, 64, 40, 255);
        var orange = new Color32(240, 140, 48, 255);
        var yellow = new Color32(255, 220, 110, 255);
        var ash = new Color32(44, 38, 40, 255);
        Color32[] px = MiniGameSprites.Blank(w * h);

        // 돌 테두리(아래 5줄 타원 고리)
        for (int x = 2; x < 22; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                float dx = (x - 11.5f) / 10f;
                float dy = (y - 2f) / 2.6f;
                float d = dx * dx + dy * dy;
                if (d <= 1f && d >= 0.45f)
                {
                    px[y * w + x] = (x + y) % 3 == 0 ? stoneLit : ((x + y) % 5 == 0 ? stoneDark : stone);
                }
                else if (d < 0.45f)
                {
                    px[y * w + x] = ash;
                }
            }
        }
        // 장작 두 개(X 모양)
        for (int i = 0; i < 9; i++)
        {
            px[(2 + i / 3) * w + 6 + i] = i % 2 == 0 ? wood : woodLit;
            px[(2 + i / 3) * w + 17 - i] = i % 2 == 0 ? woodLit : wood;
        }
        // 불꽃(가운데 위로 뾰족)
        for (int y = 4; y < 16; y++)
        {
            int half = y < 8 ? 4 : (y < 12 ? 3 - (y - 8) / 2 : Mathf.Max(0, 14 - y));
            for (int x = 12 - half; x <= 11 + half; x++)
            {
                int core = Mathf.Abs(x - 11) <= (half - 1) ? 1 : 0;
                px[y * w + x] = y < 7 ? (core == 1 ? yellow : orange) : (y < 11 ? (core == 1 ? orange : red) : red);
            }
        }
        _fire = MiniGameSprites.MakeSprite(px, w, h);
        return _fire;
    }
}
