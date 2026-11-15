/// <summary>판단 결과 한 틱(10Hz). 실행 층(PlayBot)이 진짜 입력으로 바꾼다</summary>
public struct BotIntent
{
    /// <summary>Focus 특수값(적 Uid 와 겹치지 않게 0·음수): 없음 · 돌아다님 · 드롭 · 창</summary>
    public const int FocusNone = 0;
    public const int FocusWander = -1;
    public const int FocusDrop = -2;
    public const int FocusUi = -3;

    /// <summary>-1 왼쪽 · 0 멈춤 · 1 오른쪽</summary>
    public int MoveX;
    /// <summary>-1 아래 · 0 · 1 위(로프·하향 점프·포털)</summary>
    public int MoveY;
    /// <summary>평타 리듬을 돌린다(실제 누름 간격은 BotAttackRhythm)</summary>
    public bool Attack;
    /// <summary>이번 틱 한 번 점프</summary>
    public bool Jump;
    /// <summary>열린 창을 닫는다(UI/Back 한 번)</summary>
    public bool Back;
    /// <summary>점프 누름 시간(초). 0 이면 사람 누름 분포(짧으면 가변 점프로 낮게 뜬다 — 이동 그래프 점프는 길게)</summary>
    public float JumpHold;
    /// <summary>공중·로프에서도 점프(2단 점프·로프에서 뛰어내리기)</summary>
    public bool AirJump;
    /// <summary>이번 틱 대시 한 번(방향은 MoveX 를 먼저 반영한 뒤)</summary>
    public bool Dash;
    /// <summary>스킬 칸 1~4(A·S·D·F), 0 = 없음</summary>
    public int Skill;
    /// <summary>퀵슬롯 1~4, 0 = 없음(포션)</summary>
    public int Quick;
    /// <summary>판단을 바꿀 때 반응 지연을 둘 기준(목표 바뀜 등). 같으면 지연 없이 갱신</summary>
    public int Focus;
    /// <summary>로그용 짧은 설명</summary>
    public string Note;

    public bool SameAction(BotIntent o)
    {
        return MoveX == o.MoveX && MoveY == o.MoveY && Attack == o.Attack && Focus == o.Focus;
    }
}

/// <summary>판단에 넘기는 맥락(인지 = 화면 수준 관측 PlayObservation)</summary>
public sealed class BotContext
{
    public PlayObservation Obs;
    public BotProfile Profile;
    public System.Random Rng;
    /// <summary>게임 시간(Time.time)</summary>
    public float Now;
    public string Goal;
    /// <summary>봇 시작 뒤 처치 수(EnemyHealth.Killed 구독 — 읽기 전용)</summary>
    public int Kills;
    /// <summary>사냥 순회 맵(비거나 1장이면 맵 이동 없음). HuntBrain 이 왕복한다</summary>
    public string[] Maps;
}

/// <summary>목표 층(B3 확장 지점): 퀘스트·귀환·판매 같은 상위 목표를 정하고 의도를 낸다. B1 은 HuntBrain 하나</summary>
public interface IBotBrain
{
    void Begin(BotContext ctx);
    BotIntent Decide(BotContext ctx);
}

/// <summary>이동 층(B2 확장 지점): 월드 목표 지점까지 가는 입력(좌우·점프·로프·포털)을 정한다. B1 DirectNavigator(직진 + 막히면 점프·반대), B2 GraphNavigator(이동 그래프)</summary>
public interface IBotNavigator
{
    void Reset();
    /// <summary>목표까지 dx, dy(플레이어 기준). intent 의 MoveX/MoveY/Jump 를 채운다. 진척이 없으면 false</summary>
    bool Steer(BotContext ctx, float dx, float dy, ref BotIntent intent);
    /// <summary>다른 맵으로 간다(포털 경로). 경로가 없거나 지원하지 않으면 false</summary>
    bool TravelTo(BotContext ctx, string map, ref BotIntent intent);
    /// <summary>dir 쪽 바로 앞이 막힌 끝(벽·턱)인가 — 돌아다닐 때 몸을 붙이지 않고 돌아선다. 모르면 false</summary>
    bool BlockedAhead(BotContext ctx, int dir);
}
