using System.Text;

/// <summary>
/// PlayRecorder 용 JSON 한 줄 조립 — 순수 함수(엔진 비의존, EditMode 테스트 대상).
/// 숫자는 직접 자릿수를 써서 문자열 할당 없이 붙인다(StringBuilder.Append(float) 는 임시 문자열을 만든다).
/// </summary>
public static class PlayRecJson
{
    private static readonly long[] Pow10 = { 1L, 10L, 100L, 1000L, 10000L, 100000L, 1000000L };

    /// <summary>`{"t":0.000,"k":"종류","src":"human"` 까지 쓴다. 이어서 Key* 로 필드를 붙이고 End 로 닫는다.</summary>
    public static StringBuilder Begin(StringBuilder sb, float t, string kind, string src)
    {
        sb.Append("{\"t\":");
        AppendFixed(sb, t, 3);
        sb.Append(",\"k\":");
        AppendString(sb, kind);
        sb.Append(",\"src\":");
        AppendString(sb, src);
        return sb;
    }

    public static StringBuilder End(StringBuilder sb)
    {
        return sb.Append('}').Append('\n');
    }

    public static StringBuilder KeyStr(StringBuilder sb, string key, string value)
    {
        sb.Append(',');
        AppendString(sb, key);
        sb.Append(':');
        AppendString(sb, value);
        return sb;
    }

    public static StringBuilder KeyInt(StringBuilder sb, string key, long value)
    {
        sb.Append(',');
        AppendString(sb, key);
        sb.Append(':');
        AppendLong(sb, value);
        return sb;
    }

    public static StringBuilder KeyNum(StringBuilder sb, string key, double value, int decimals)
    {
        sb.Append(',');
        AppendString(sb, key);
        sb.Append(':');
        AppendFixed(sb, value, decimals);
        return sb;
    }

    /// <summary>따옴표·역슬래시·제어문자를 이스케이프한 JSON 문자열. null 은 빈 문자열.</summary>
    public static StringBuilder AppendString(StringBuilder sb, string s)
    {
        sb.Append('"');
        if (s != null)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '"': sb.Append('\\').Append('"'); break;
                    case '\\': sb.Append('\\').Append('\\'); break;
                    case '\n': sb.Append('\\').Append('n'); break;
                    case '\r': sb.Append('\\').Append('r'); break;
                    case '\t': sb.Append('\\').Append('t'); break;
                    default:
                        if (c < 0x20)
                        {
                            sb.Append('\\').Append('u').Append('0').Append('0');
                            sb.Append(HexDigit((c >> 4) & 0xF)).Append(HexDigit(c & 0xF));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
        }
        return sb.Append('"');
    }

    /// <summary>앞 max 글자만(서로게이트 쌍은 자르지 않는다) 이스케이프해 붙인다. 로그 메시지용.</summary>
    public static StringBuilder AppendTrimmed(StringBuilder sb, string s, int max)
    {
        if (s != null && s.Length > max)
        {
            int cut = max;
            if (cut > 0 && char.IsHighSurrogate(s[cut - 1]))
            {
                cut--;
            }
            s = s.Substring(0, cut);
        }
        return AppendString(sb, s);
    }

    public static StringBuilder AppendLong(StringBuilder sb, long v)
    {
        if (v == 0)
        {
            return sb.Append('0');
        }
        if (v < 0)
        {
            sb.Append('-');
            if (v == long.MinValue)
            {
                return sb.Append("9223372036854775808");
            }
            v = -v;
        }
        int start = sb.Length;
        while (v > 0)
        {
            sb.Append((char)('0' + (int)(v % 10)));
            v /= 10;
        }
        // 거꾸로 쓴 자릿수를 뒤집는다
        for (int i = start, j = sb.Length - 1; i < j; i++, j--)
        {
            char tmp = sb[i];
            sb[i] = sb[j];
            sb[j] = tmp;
        }
        return sb;
    }

    /// <summary>고정 소수점(0~6자리, 반올림). NaN/Infinity 는 0.</summary>
    public static StringBuilder AppendFixed(StringBuilder sb, double v, int decimals)
    {
        if (double.IsNaN(v) || double.IsInfinity(v))
        {
            v = 0;
        }
        if (decimals < 0)
        {
            decimals = 0;
        }
        if (decimals > 6)
        {
            decimals = 6;
        }
        bool neg = v < 0;
        if (neg)
        {
            v = -v;
        }
        double scaled = System.Math.Round(v * Pow10[decimals], System.MidpointRounding.AwayFromZero);
        if (scaled > 9e15)
        {
            scaled = 9e15;
        }
        long total = (long)scaled;
        if (neg && total != 0)
        {
            sb.Append('-');
        }
        long whole = total / Pow10[decimals];
        long frac = total % Pow10[decimals];
        AppendLong(sb, whole);
        if (decimals > 0)
        {
            sb.Append('.');
            for (int d = decimals - 1; d >= 0; d--)
            {
                sb.Append((char)('0' + (int)((frac / Pow10[d]) % 10)));
            }
        }
        return sb;
    }

    private static char HexDigit(int n)
    {
        return (char)(n < 10 ? '0' + n : 'a' + (n - 10));
    }
}

/// <summary>
/// "막힘" 판정: 이동 입력을 HoldSeconds 넘게 누르는데 위치가 MinMove 이상 움직이지 않으면 한 번 true.
/// 입력을 떼거나 움직이면(앵커 갱신) 다시 무장한다. 순수 로직 — 시간·위치는 호출자가 넘긴다.
/// </summary>
public sealed class PlayRecStuck
{
    public const float HoldSeconds = 2f;
    public const float MinMove = 0.2f;

    private bool _armed;
    private bool _fired;
    private float _t0;
    private float _x0;
    private float _y0;

    public void Reset()
    {
        _armed = false;
        _fired = false;
    }

    /// <summary>held = 이동 입력을 누르는 중(그리고 UI 잠금 등 정상 정지가 아님). true 가 돌아오면 막힘 한 건.</summary>
    public bool Feed(float now, bool held, float x, float y)
    {
        if (!held)
        {
            Reset();
            return false;
        }
        if (!_armed)
        {
            _armed = true;
            _fired = false;
            _t0 = now;
            _x0 = x;
            _y0 = y;
            return false;
        }
        float dx = x - _x0;
        float dy = y - _y0;
        if (dx * dx + dy * dy >= MinMove * MinMove)
        {
            _fired = false;
            _t0 = now;
            _x0 = x;
            _y0 = y;
            return false;
        }
        if (!_fired && now - _t0 > HoldSeconds)
        {
            _fired = true;
            return true;
        }
        return false;
    }
}
