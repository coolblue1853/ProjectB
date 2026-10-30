using UnityEngine;

/// <summary>미니게임 지점 표시용 코드 도트 스프라이트 도우미(빈 픽셀 배열·텍스처화). 채집 지점·발굴 유적이 같이 쓴다</summary>
public static class MiniGameSprites
{
    public static Color32[] Blank(int n)
    {
        var px = new Color32[n];
        var clear = new Color32(0, 0, 0, 0);
        for (int i = 0; i < n; i++)
        {
            px[i] = clear;
        }
        return px;
    }

    /// <summary>픽셀 배열 → 스프라이트(점 필터, 아래 가운데 기준, PPU 16)</summary>
    public static Sprite MakeSprite(Color32[] px, int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 16f);
    }
}
