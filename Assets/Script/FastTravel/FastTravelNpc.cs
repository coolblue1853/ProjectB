using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마차 NPC 표지 (FastTravel.md §3~5). TalkNpc·NpcQuestGiver 와 같은 오브젝트에 붙는다.
/// NpcInteraction 이 이 컴포넌트를 보고 NPC 선택지에 "마차로 이동" 을 넣는다. 목적지 선택은 NpcMenu 행 모드, 확인은 ConfirmPopup.
/// </summary>
public sealed class FastTravelNpc : MonoBehaviour
{
    [SerializeField] private string _stopId;

    public string StopId => _stopId;

    public void Init(string stopId)
    {
        _stopId = stopId;
    }

    /// <summary>목적지 목록 창을 연다 (현재 정류장은 숨김)</summary>
    public void OpenDestinations()
    {
        FastTravelFlow.Open(_stopId);
    }
}

/// <summary>목적지 선택 → 확인 → 요금 차감·이동 흐름. 입력은 NpcMenu(마우스·키보드·패드)·ConfirmPopup 이 맡는다</summary>
public static class FastTravelFlow
{
    private const string Title = "마차: 어디로 갈까요?";

    /// <summary>프로브·테스트용: 마지막으로 이동한 목적지 id</summary>
    public static string LastTravelled { get; private set; } = "";

    private static float _lastTravelAt = -999f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        LastTravelled = "";
        _lastTravelAt = -999f;
    }

    public static void Open(string fromStopId)
    {
        FastTravelStop from = FastTravelStops.Find(fromStopId);
        if (from == null)
        {
            PickupLog.Warn("이 마차는 출발 정류장이 정해지지 않았습니다");
            return;
        }
        var dests = new List<FastTravelStop>();
        var labels = new List<string>();
        var reasons = new List<string>();
        foreach (FastTravelStop s in FastTravelStops.All)
        {
            if (s.Id == fromStopId)
            {
                continue;
            }
            dests.Add(s);
            bool open = FastTravelState.IsOpen(s);
            int fee = from != null ? FastTravelRules.Fee(from.Order, s.Order, s.Floor) : 0;
            FastTravelRules.Block b = Evaluate(s, fee);
            labels.Add(open ? s.DisplayName + "  " + fee.ToString("N0") + "G" : s.DisplayName + "  (잠김)");
            reasons.Add(b == FastTravelRules.Block.None ? null : Reason(b, s));
        }
        NpcMenu.OpenRows(Title, labels, reasons, i => Confirm(from, dests[i]));
    }

    /// <summary>지금 상태로 이동 가능한지(사유 반환). 플레이어가 없으면 Dead 로 본다</summary>
    public static FastTravelRules.Block Evaluate(FastTravelStop dest, int fee)
    {
        PlayerController pc = PlayerController.instance;
        PlayerHealthManager hp = PlayerHealthManager.Instance;
        bool dead = pc == null || (hp != null && hp.IsDead);
        bool boss = pc != null && RecallRules.BlockedByBoss(RecallCast.NearestBossDistance(pc.transform.position));
        return FastTravelRules.Check(dead, boss, FastTravelHost.SecondsSinceHurt, FastTravelState.IsOpen(dest), DatabaseManager.money, fee);
    }

    private static string Reason(FastTravelRules.Block b, FastTravelStop dest)
    {
        return b == FastTravelRules.Block.Locked ? dest.DisplayName + " 마차 의뢰(구역 처치)를 먼저 깨세요" : FastTravelRules.Reason(b);
    }

    private static void Confirm(FastTravelStop from, FastTravelStop dest)
    {
        int fee = from != null ? FastTravelRules.Fee(from.Order, dest.Order, dest.Floor) : 0;
        ConfirmPopup.Open(new ConfirmRequest
        {
            Title = "마차 이동",
            Message = dest.DisplayName + "(으)로 이동합니다.\n요금 " + fee.ToString("N0") + "G",
            YesLabel = "이동",
            NoLabel = "취소",
            DefaultYes = true,
            OnYes = () => Travel(dest, fee),
        });
    }

    /// <summary>요금을 내고 이동한다. 확인 사이에 상태가 바뀌었을 수 있어 다시 판정한다. 성공하면 true</summary>
    public static bool Travel(FastTravelStop dest, int fee)
    {
        if (FastTravelRules.InCooldown(Time.unscaledTime, _lastTravelAt))
        {
            return false; // 이동 직후 이중 입력 무시
        }
        FastTravelRules.Block b = Evaluate(dest, fee);
        if (b != FastTravelRules.Block.None)
        {
            PickupLog.Warn(Reason(b, dest));
            return false;
        }
        if (!MapInfo.GoToMap(dest.Id, PlayerController.instance.gameObject))
        {
            PickupLog.Warn("이동할 수 없는 지역입니다");
            return false;
        }
        DatabaseManager.money -= fee;
        FastTravelState.Last = dest.Id;
        FastTravelState.Save();
        LastTravelled = dest.Id;
        _lastTravelAt = Time.unscaledTime;
        Sfx.Play(SfxId.Money);
        GameLog.Info(GameLog.Cat.Map, "마차 이동: " + dest.Id + " (" + fee + "G)");
        return true;
    }
}
