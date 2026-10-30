using UnityEngine;

/// <summary>
/// 물가 낚시터(MiniGames.md "층계 낚시 구현"). 공용 틀(MiniGameSpot)에 낚시 세션만 얹는다:
/// 머리 위 "UP  FISH", ↑ 를 누르고 있다가 떼면 캐스팅. 잠금 UiLock "Fishing". 포획 성공 때만 GiveReward(AddSource.Fishing).
/// </summary>
public class FishingSpot : MiniGameSpot
{
    public const string Lock = "Fishing";

    [SerializeField] private int _floor = 1;

    /// <summary>지금 낚시 중인 낚시터(없으면 null). FishingUI 가 읽는다</summary>
    public static FishingSpot Current => Active as FishingSpot;

    public FishingSession Session { get; private set; }

    public int Floor
    {
        get => _floor;
        set => _floor = value;
    }

    protected override string PromptKey => "UP  FISH";
    protected override string LockOwner => Lock;
    protected override float ResultHold => 1.6f;
    protected override bool HasSession => Session != null;
    protected override bool SessionDone => Session.Phase == FishingPhase.Result;
    protected override bool SessionCancelled => Session.Result == FishingResult.Cancelled;
    protected override float ResultElapsed => Session.PhaseTime;

    protected override void StartSession()
    {
        Session = new FishingSession(FishingRules.Rod(FishingSave.LoadRod()), FishingRules.TableFor(_floor), RandomValue);
    }

    protected override void TickSession(float dt, bool held, bool pressed)
    {
        Session.Tick(dt, held, pressed);
    }

    protected override void CancelSession()
    {
        Session.Cancel();
    }

    protected override void ClearSession()
    {
        Session = null;
    }

    protected override void OnResult()
    {
        if (Session.Result == FishingResult.Caught)
        {
            GiveReward(Session.Fish.Id, Session.Count, AddSource.Fishing, Session.Fish.Rarity);
        }
    }
}
