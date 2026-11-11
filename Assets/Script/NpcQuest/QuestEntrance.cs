/// <summary>
/// 퀘스트 입구 (REWORK-5 P4): 의뢰 게시판·사냥 푯말이 같은 퀘스트 창(NpcQuestWindow)을 입구 모드로 연다.
/// 입구 = 필터(giver·source)일 뿐이고, 수락·보고·보상 규칙은 QuestService 가 3계통 공통으로 처리한다.
/// 호출 쪽은 `InteractArbiter.Contend(...)` 로 ↑ 를 중재한 뒤 ↑ 가 눌렸을 때 Open 을 부른다.
/// </summary>
public static class QuestEntrance
{
    /// <summary>의뢰 게시판: giver=questboard 퀘스트(받을 것 + 보고할 것)</summary>
    public static QuestFilter Board()
    {
        return new QuestFilter { Giver = QuestBoard.BoardNpcId, ReportToo = true };
    }

    /// <summary>사냥 푯말: 오늘의 현상 제안 + 진행 중 현상</summary>
    public static QuestFilter Kiosk()
    {
        return new QuestFilter { Giver = QuestBountyLink.Giver, Source = "bounty", ReportToo = true };
    }

    /// <summary>구역 보고 안내판(깊은 숲·늪·요새·탑 입구): 이브가 보고 대상인 진행 중 퀘스트를 일반 숲까지 돌아가지 않고 보고한다</summary>
    public static QuestFilter ZoneReport()
    {
        return new QuestFilter { Giver = ZoneReportNpc, ReportToo = true, ReportOnly = true };
    }

    /// <summary>구역 보고 안내판이 대신 받는 보고 NPC(일반 숲 입구의 이브)</summary>
    public const string ZoneReportNpc = "eve";

    /// <summary>입구 창을 연다. 현상은 열기 전에 오늘 목록을 보장한다. 이미 다른 UI 가 열려 있으면 false</summary>
    public static bool Open(QuestFilter filter)
    {
        if (filter != null && filter.Source == "bounty" && BountyBoard.Instance != null)
        {
            BountyBoard.Instance.EnsureOffers();
        }
        if (filter != null && !HasAnything(filter))
        {
            PickupLog.Notice(filter.ReportOnly ? "지금 보고할 퀘스트가 없습니다." : "지금 받을 수 있는 의뢰가 없습니다."); // 빈 창을 열지 않는다
            return false;
        }
        return NpcQuestWindow.OpenEntrance(filter);
    }

    /// <summary>이 입구에서 d 를 보고할 수 있는가 (보고 대상 NPC·입구가 필터 giver 와 같을 때)</summary>
    public static bool CanReportHere(NpcQuestDef d, QuestFilter filter)
    {
        return d != null && (filter == null || string.IsNullOrEmpty(filter.Giver) || d.ReportNpc == filter.Giver);
    }

    /// <summary>입구에 보여 줄 것(받을 수 있음·진행 중)이 하나라도 있는가</summary>
    public static bool HasAnything(QuestFilter filter)
    {
        QuestFilter a = new QuestFilter { Tab = QuestListRules.Tab.Available, Giver = filter.Giver, Source = filter.Source, Zone = filter.Zone, ReportToo = filter.ReportToo, ReportOnly = filter.ReportOnly };
        QuestFilter b = new QuestFilter { Tab = QuestListRules.Tab.Active, Giver = filter.Giver, Source = filter.Source, Zone = filter.Zone, ReportToo = filter.ReportToo, ReportOnly = filter.ReportOnly };
        return QuestService.Query(a).Items.Count > 0 || QuestService.Query(b).Items.Count > 0;
    }
}
