using System.Text;
using NUnit.Framework;

public class PlayRecorderTests
{
    private static string Line(string kind, System.Action<StringBuilder> body)
    {
        StringBuilder sb = new StringBuilder();
        PlayRecJson.Begin(sb, 12.3456f, kind, "human");
        body(sb);
        PlayRecJson.End(sb);
        return sb.ToString();
    }

    [Test]
    public void Begin_WritesCommonFields_OneLine()
    {
        string s = Line("ev", sb => PlayRecJson.KeyStr(sb, "e", "kill"));
        Assert.AreEqual("{\"t\":12.346,\"k\":\"ev\",\"src\":\"human\",\"e\":\"kill\"}\n", s);
    }

    [Test]
    public void String_EscapesQuotesBackslashNewlineAndControl()
    {
        StringBuilder sb = new StringBuilder();
        PlayRecJson.AppendString(sb, "a\"b\\c\nd\te\u0001");
        Assert.AreEqual("\"a\\\"b\\\\c\\nd\\te\\u0001\"", sb.ToString());
    }

    [Test]
    public void String_NullIsEmpty()
    {
        StringBuilder sb = new StringBuilder();
        PlayRecJson.AppendString(sb, null);
        Assert.AreEqual("\"\"", sb.ToString());
    }

    [Test]
    public void Trimmed_CutsAndKeepsSurrogatePairWhole()
    {
        StringBuilder sb = new StringBuilder();
        PlayRecJson.AppendTrimmed(sb, "ab😀cd", 3); // 3번째 글자가 서로게이트 앞쪽 → 뺀다
        Assert.AreEqual("\"ab\"", sb.ToString());
    }

    [Test]
    public void Numbers_FixedAndNegativeAndNonFinite()
    {
        StringBuilder sb = new StringBuilder();
        PlayRecJson.AppendFixed(sb, -1.25, 1).Append('|');
        PlayRecJson.AppendFixed(sb, 0.0004, 3).Append('|');
        PlayRecJson.AppendFixed(sb, -0.0004, 3).Append('|');
        PlayRecJson.AppendFixed(sb, double.NaN, 2).Append('|');
        PlayRecJson.AppendFixed(sb, 7, 0).Append('|');
        PlayRecJson.AppendLong(sb, -120).Append('|');
        PlayRecJson.AppendLong(sb, 0);
        Assert.AreEqual("-1.3|0.000|0.000|0.00|7|-120|0", sb.ToString());
    }

    [Test]
    public void Line_HasNoRawNewlineInside_AndBracesBalance()
    {
        string s = Line("log", sb =>
        {
            sb.Append(",\"msg\":");
            PlayRecJson.AppendTrimmed(sb, "한글\n두 줄 \"인용\"", 50);
        });
        Assert.AreEqual(1, s.Split('\n').Length - 1);
        Assert.IsTrue(s.StartsWith("{") && s.EndsWith("}\n"));
    }

    [Test]
    public void Stuck_FiresOnceAfterHoldWithoutMovement()
    {
        PlayRecStuck st = new PlayRecStuck();
        Assert.IsFalse(st.Feed(0f, true, 0f, 0f));
        Assert.IsFalse(st.Feed(1.9f, true, 0.05f, 0f));
        Assert.IsTrue(st.Feed(2.1f, true, 0.1f, 0f));
        Assert.IsFalse(st.Feed(5f, true, 0.1f, 0f), "한 번만");
    }

    [Test]
    public void Stuck_MovementOrRelease_Rearms()
    {
        PlayRecStuck st = new PlayRecStuck();
        st.Feed(0f, true, 0f, 0f);
        Assert.IsFalse(st.Feed(1.5f, true, 1f, 0f), "움직이면 앵커 갱신");
        Assert.IsFalse(st.Feed(3f, true, 1f, 0f), "앵커 갱신 뒤 1.5초");
        Assert.IsTrue(st.Feed(3.6f, true, 1f, 0f));
        Assert.IsFalse(st.Feed(4f, false, 1f, 0f), "입력 뗌 = 리셋");
        Assert.IsFalse(st.Feed(4.1f, true, 1f, 0f));
        Assert.IsTrue(st.Feed(6.2f, true, 1f, 0f));
    }
}
