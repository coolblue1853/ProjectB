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
    /// <summary>-1 아래 · 0 · 1 위(로프·포털은 B2)</summary>
    public int MoveY;
    /// <summary>평타 리듬을 돌린다(실제 누름 간격은 BotAttackRhythm)</summary>
    public bool Attack;
    /// <summary>이번 틱 한 번 점프</summary>
    public bool Jump;
    /// <summary>열린 창을 닫는다(UI/Back 한 번)</summary>
    public bool Back;
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
}

/// <summary>목표 층(B3 확장 지점): 퀘스트·귀환·판매 같은 상위 목표를 정하고 의도를 낸다. B1 은 HuntBrain 하나</summary>
public interface IBotBrain
{
    void Begin(BotContext ctx);
    BotIntent Decide(BotContext ctx);
}

/// <summary>이동 층(B2 확장 지점): 월드 목표 지점까지 가는 입력(좌우·점프·로프·포털)을 정한다. B1 은 DirectNavigator(직진 + 막히면 점프·반대)</summary>
public interface IBotNavigator
{
    void Reset();
    /// <summary>목표까지 dx, dy(플레이어 기준). intent 의 MoveX/MoveY/Jump 를 채운다. 진척이 없으면 false</summary>
    bool Steer(BotContext ctx, float dx, float dy, ref BotIntent intent);
}
