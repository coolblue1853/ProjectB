using System.Text;

/// <summary>
/// 지형 격자 인코딩 — 순수 함수(엔진 비의존, EditMode 테스트 대상).
/// 격자는 월드 정수 칸에 맞춘다(1칸 = 1유닛). 같은 자리·같은 지형이면 해시가 같아 세션 안에서 한 번만 전체를 쓴다.
/// 칸 순서: 행 0 = 맨 위(y = Oy + H - 1), 각 행은 왼쪽(x = Ox)부터. 칸 하나 = 문자 한 개('0'~'5').
/// </summary>
public static class PlayRecGrid
{
    public const int W = 24;
    public const int H = 14;
    /// <summary>플레이어 칸 왼쪽에 둘 칸 수(오른쪽은 W - Left - 1)</summary>
    public const int Left = 12;
    /// <summary>플레이어 칸 아래에 둘 칸 수(위는 H - Below - 1)</summary>
    public const int Below = 6;

    public const byte Empty = 0;
    public const byte Ground = 1;
    public const byte Platform = 2;
    public const byte Wall = 3;
    public const byte Ladder = 4;
    public const byte Hazard = 5;

    public static int OriginX(float px)
    {
        return FloorToInt(px) - Left;
    }

    public static int OriginY(float py)
    {
        return FloorToInt(py) - Below;
    }

    /// <summary>칸 (col,row) 의 중심 월드 좌표</summary>
    public static float CellCenterX(int ox, int col)
    {
        return ox + col + 0.5f;
    }

    public static float CellCenterY(int oy, int row)
    {
        return oy + (H - 1 - row) + 0.5f;
    }

    /// <summary>FNV-1a 32비트: 원점 + 칸 값. 0 은 "격자 없음"으로 쓰므로 피한다.</summary>
    public static uint Hash(byte[] cells, int ox, int oy)
    {
        uint h = 2166136261u;
        h = Mix(h, ox);
        h = Mix(h, oy);
        for (int i = 0; i < cells.Length; i++)
        {
            h ^= cells[i];
            h *= 16777619u;
        }
        return h == 0 ? 1u : h;
    }

    public static void AppendCells(StringBuilder sb, byte[] cells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            sb.Append((char)('0' + cells[i]));
        }
    }

    /// <summary>8자리 소문자 16진수</summary>
    public static void AppendHex(StringBuilder sb, uint h)
    {
        for (int s = 28; s >= 0; s -= 4)
        {
            int d = (int)((h >> s) & 0xF);
            sb.Append((char)(d < 10 ? '0' + d : 'a' + (d - 10)));
        }
    }

    private static uint Mix(uint h, int v)
    {
        for (int b = 0; b < 4; b++)
        {
            h ^= (uint)((v >> (b * 8)) & 0xFF);
            h *= 16777619u;
        }
        return h;
    }

    private static int FloorToInt(float v)
    {
        int i = (int)v;
        return v < i ? i - 1 : i;
    }
}
