using System;

/// <summary>
/// 수량 선택 창 규칙(순수, REWORK-2 U14). 값 클램프·증감·숫자 직접 입력·길게 누르기 가속·구매 가능 최대치를 계산한다.
/// 화면(QuantityPopup)은 이 규칙만 호출하므로 EditMode 에서 그대로 테스트한다.
/// </summary>
public static class QuantityRules
{
    public const int SmallStep = 1;
    public const int BigStep = 10;

    /// <summary>길게 누르기 시작 후 첫 반복까지 기다리는 시간(초)</summary>
    public const float RepeatDelay = 0.4f;
    /// <summary>반복 간격(초)</summary>
    public const float RepeatInterval = 0.06f;

    /// <summary>값을 [min, max] 로 자른다. max 가 min 보다 작으면 min 으로 맞춘다</summary>
    public static int Clamp(int value, int min, int max)
    {
        if (max < min)
        {
            max = min;
        }
        return Math.Max(min, Math.Min(max, value));
    }

    /// <summary>delta 만큼 더하고 클램프한다(오버플로 방지)</summary>
    public static int Step(int value, int delta, int min, int max)
    {
        long next = (long)value + delta;
        if (next > int.MaxValue)
        {
            next = int.MaxValue;
        }
        else if (next < int.MinValue)
        {
            next = int.MinValue;
        }
        return Clamp((int)next, min, max);
    }

    /// <summary>
    /// 숫자 키 입력. 직접 입력을 막 시작했으면(fresh) 기존 값을 버리고 새로 쓰고, 이어 입력이면 뒤에 붙인다. 결과는 max 로 자른다.
    /// 0 으로 시작하는 입력은 min 으로 올린다.
    /// </summary>
    public static int AppendDigit(int value, int digit, bool fresh, int min, int max)
    {
        if (digit < 0 || digit > 9)
        {
            return value;
        }
        long next = fresh ? digit : (long)value * 10 + digit;
        if (next > max)
        {
            next = max;
        }
        return Clamp((int)next, min, max);
    }

    /// <summary>백스페이스: 한 자리 지운다. 한 자리만 남았으면 min</summary>
    public static int Backspace(int value, int min, int max)
    {
        return Clamp(value / 10, min, max);
    }

    /// <summary>길게 누른 시간(초)에 따른 한 번 반복당 배수. 0.4초 지나면 1, 1.2초 뒤 5, 2.4초 뒤 10 배</summary>
    public static int HoldMultiplier(float heldSeconds)
    {
        if (heldSeconds < 1.2f)
        {
            return 1;
        }
        return heldSeconds < 2.4f ? 5 : 10;
    }

    /// <summary>누르고 있는 동안 이번 프레임에 반복 입력이 몇 번 일어나는지. 누른 직후 첫 반복은 RepeatDelay 뒤</summary>
    public static int RepeatTicks(float heldBefore, float heldNow)
    {
        if (heldNow <= RepeatDelay)
        {
            return 0;
        }
        float from = Math.Max(heldBefore, RepeatDelay);
        int a = (int)((from - RepeatDelay) / RepeatInterval);
        int b = (int)((heldNow - RepeatDelay) / RepeatInterval);
        if (heldBefore <= RepeatDelay)
        {
            return b + 1;
        }
        return Math.Max(0, b - a);
    }

    /// <summary>골드·남은 칸·재고로 살 수 있는 최대 수량(0 이상)</summary>
    public static int MaxAffordable(int unitPrice, int gold, int bagLimit, int stockLimit)
    {
        int byGold = unitPrice <= 0 ? int.MaxValue : Math.Max(0, gold) / unitPrice;
        return Math.Max(0, Math.Min(byGold, Math.Min(bagLimit, stockLimit)));
    }

    /// <summary>합계 금액(오버플로 시 int.MaxValue)</summary>
    public static int Total(int unitPrice, int count)
    {
        long t = (long)unitPrice * count;
        return t > int.MaxValue ? int.MaxValue : (int)t;
    }
}
