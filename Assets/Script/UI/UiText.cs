using TMPro;

/// <summary>TMP 글자를 값이 바뀔 때만 대입한다(같은 값 재대입은 메시 재생성·할당을 부른다).</summary>
public static class UiText
{
    /// <summary>두 문자열이 달라 대입이 필요한지(TMP 없이 테스트 가능).</summary>
    public static bool NeedsSet(string current, string next)
    {
        return !string.Equals(current ?? "", next ?? "", System.StringComparison.Ordinal);
    }

    public static void SetIfChanged(this TMP_Text t, string next)
    {
        if (t != null && NeedsSet(t.text, next))
        {
            t.text = next;
        }
    }
}
