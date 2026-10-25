using UnityEngine;

/// <summary>무기 테마 색 램프(어둠 → 흰 심 5색). PixelSlash2D 셰이더가 회색조 이펙트를 이 색으로 칠한다 (GameFeelPlan 10.7).</summary>
public enum PixelRampId
{
    White, Gold, Blue, Red, Green, Purple,
}

public static class PixelRamp
{
    // PixelVfxBaker.Ramps 와 같은 값(에디터 전용 쪽을 런타임에서도 쓰도록 옮겨 둔 사본)
    private static readonly Color[][] Ramps =
    {
        Hex(0x3C3C50, 0x828CAA, 0xC8D2E6, 0xEBF0FA, 0xFFFFFF),
        Hex(0x5A1E0A, 0xC8501A, 0xF5A032, 0xFADF9F, 0xFFFAEB),
        Hex(0x141E5A, 0x285AC8, 0x50AAFF, 0xAAE1FF, 0xF0FAFF),
        Hex(0x460A14, 0xAA1E28, 0xF0503C, 0xFFAA8C, 0xFFF0E6),
        Hex(0x0A3214, 0x1E8228, 0x5AD250, 0xBEFFA0, 0xF5FFEB),
        Hex(0x28104A, 0x6A28B4, 0xB45AF0, 0xE1AAFF, 0xFAF0FF),
    };

    public static Color[] Get(PixelRampId id)
    {
        return Ramps[(int)id];
    }

    /// <summary>무기의 기존 평타 이펙트 색을 그대로 테마로 쓴다 — 계보 무기 데이터를 고치지 않아도 색이 이어진다</summary>
    public static PixelRampId FromSwing(VfxId swing)
    {
        switch (swing)
        {
            case VfxId.SlashRed:
            case VfxId.CrackRed:
            case VfxId.BurstRed:
                return PixelRampId.Red;
            case VfxId.SlashBlue:
            case VfxId.BurstBlue:
                return PixelRampId.Blue;
            case VfxId.SlashGold:
            case VfxId.SlashGoldWide:
            case VfxId.BurstGold:
                return PixelRampId.Gold;
            case VfxId.SlashGreenBig:
            case VfxId.BurstGreen:
                return PixelRampId.Green;
            case VfxId.BurstPurple:
            case VfxId.SmokePurple:
                return PixelRampId.Purple;
            default:
                return PixelRampId.White;
        }
    }

    private static Color[] Hex(params int[] rgb)
    {
        var c = new Color[rgb.Length];
        for (int i = 0; i < rgb.Length; i++)
        {
            c[i] = new Color(((rgb[i] >> 16) & 0xFF) / 255f, ((rgb[i] >> 8) & 0xFF) / 255f, (rgb[i] & 0xFF) / 255f, 1f);
        }
        return c;
    }
}
