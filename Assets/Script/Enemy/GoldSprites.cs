using UnityEngine;

/// <summary>골드 더미 스프라이트 3단계(절차 생성, 점 필터). 코인 1개 / 코인 더미 / 자루. PPU 20 → 0.8·1.2·1.6 유닛(드롭 프리팹 스케일 0.5 적용 전)</summary>
public static class GoldSprites
{
    private const float Ppu = 20f;
    private static readonly int[] Sizes = { 16, 24, 32 };
    private static Sprite[] cache;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cache = null;
    }

    public static Sprite Get(int tier)
    {
        tier = Mathf.Clamp(tier, 0, 2);
        if (cache == null)
        {
            cache = new Sprite[3];
        }
        if (cache[tier] == null)
        {
            cache[tier] = Build(tier);
        }
        return cache[tier];
    }

    private static Sprite Build(int tier)
    {
        int s = Sizes[tier];
        var px = new Color32[s * s];
        switch (tier)
        {
            case 0:
                Coin(px, s, 8, 5, 5);
                break;
            case 1:
                Coin(px, s, 7, 5, 5);
                Coin(px, s, 16, 5, 5);
                Coin(px, s, 11, 10, 5);
                break;
            default:
                Bag(px, s);
                Coin(px, s, 6, 4, 4);
                Coin(px, s, 26, 4, 4);
                break;
        }
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "GoldTier" + tier };
        tex.SetPixels32(px);
        tex.Apply(false, true);
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.1f), Ppu);
    }

    private static readonly Color32 Outline = new Color32(70, 40, 10, 255);
    private static readonly Color32 Dark = new Color32(190, 130, 20, 255);
    private static readonly Color32 Mid = new Color32(250, 200, 50, 255);
    private static readonly Color32 Light = new Color32(255, 240, 150, 255);

    private static void Put(Color32[] px, int s, int x, int y, Color32 c)
    {
        if (x >= 0 && y >= 0 && x < s && y < s)
        {
            px[y * s + x] = c;
        }
    }

    /// <summary>cx, cy = 중심, r = 반지름. 외곽선 + 어두운 아랫면 + 밝은 윗 하이라이트</summary>
    private static void Coin(Color32[] px, int s, int cx, int cy, int r)
    {
        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                float d = Mathf.Sqrt(x * x + y * y * 1.15f);
                if (d > r + 0.3f)
                {
                    continue;
                }
                Color32 c = d > r - 0.8f ? Outline : (y < -r / 2 ? Dark : (y > r / 2 && x < 0 ? Light : Mid));
                Put(px, s, cx + x, cy + y, c);
            }
        }
    }

    private static void Bag(Color32[] px, int s)
    {
        // 자루 몸통(타원) + 목 + 끈
        for (int y = 1; y < 22; y++)
        {
            float t = (y - 1) / 21f;
            float half = 4f + 9f * Mathf.Sin(Mathf.Clamp01(t * 1.15f) * Mathf.PI * 0.95f);
            if (y > 17)
            {
                half = 3.5f - (y - 17) * 0.2f;
            }
            for (int x = -Mathf.CeilToInt(half); x <= Mathf.CeilToInt(half); x++)
            {
                float ax = Mathf.Abs(x);
                Color32 c = ax > half - 1f ? Outline : (x > half * 0.35f ? Dark : (x < -half * 0.4f && y > 7 ? Light : Mid));
                Put(px, s, 16 + x, y, c);
            }
        }
        for (int x = -5; x <= 5; x++)
        {
            Put(px, s, 16 + x, 22, Outline);
            Put(px, s, 16 + x, 23, Ax(x) < 4 ? new Color32(150, 80, 40, 255) : Outline);
        }
    }

    private static int Ax(int x) => x < 0 ? -x : x;
}
