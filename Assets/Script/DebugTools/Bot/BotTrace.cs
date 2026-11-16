/// <summary>
/// 봇 실행 기록용 한 줄 흔적: 마지막 판단(Decide)이 따라가던 이동 간선 종류·목표 구간·Focus.
/// PlayBot.Decide 가 매번 BeginDecide 로 비우고, 길찾기(GraphNavigator)·HuntBrain 이 채운다.
/// PlayRecorder 가 src=bot 일 때 tk 행에 새 필드(be·bn·bf)로 덧붙인다. 읽기 전용 관측이라 봇 동작에는 영향이 없다.
/// </summary>
public static class BotTrace
{
    public const string None = "none";

    /// <summary>walk · direct · wander · drop(낙하 간선) · jump · jumphigh · down(아래 점프) · rope · portal · none</summary>
    public static string Edge { get; private set; } = None;
    /// <summary>길찾기 목표 구간 번호(없으면 -1)</summary>
    public static int Node { get; private set; } = -1;
    /// <summary>BotIntent.Focus: 몹 Uid(&gt;0) · 0 없음 · -1 돌아다님 · -2 드롭 · -3 창</summary>
    public static int Focus { get; private set; }

    public static void BeginDecide()
    {
        Edge = None;
        Node = -1;
        Focus = 0;
    }

    public static void MarkEdge(string edge, int node)
    {
        Edge = edge;
        Node = node;
    }

    public static void SetFocus(int focus)
    {
        Focus = focus;
    }

    public static void Reset()
    {
        BeginDecide();
    }
}
