using System;
using System.Collections.Generic;

/// <summary>퀘스트 목록 필터 (null/빈칸 = 조건 없음). QuestSystemDesign §2-5</summary>
public sealed class QuestFilter
{
    public QuestListRules.Tab Tab = QuestListRules.Tab.Available;
    public string Giver;
    public string Source;
    public string Zone;
    /// <summary>true 면 giver 가 보고 대상(reportTo)인 퀘스트도 포함 — 입구 창이 "받을 것 + 보고할 것" 을 함께 보여 줄 때</summary>
    public bool ReportToo;
}

/// <summary>Query 결과: 필터한 탭의 정렬된 목록</summary>
public sealed class QuestView
{
    public QuestListRules.Tab Tab;
    public readonly List<NpcQuestDef> Items = new List<NpcQuestDef>();
}

/// <summary>
/// 3계통(NPC·게시판·사냥 현상) 단일 진입점 (REWORK-5 P0). UI·입구·HUD 는 이 클래스만 부른다.
/// 저장소는 NpcQuestLog(CSV 퀘스트)와 BountyBoard(현상, id 접두 "bounty:")가 그대로 맡는다 — 접두로 분기.
/// 테스트용 훅: BountyAvailable/BountyActive 로 현상 어댑터 목록을 대신 넣는다(null = QuestBountyLink 실제 값).
/// </summary>
public static class QuestService
{
    public static Func<List<NpcQuestDef>> BountyAvailable;
    public static Func<List<NpcQuestDef>> BountyActive;
    /// <summary>플레이어에게 알리는 경고 (테스트에서 비운다 — PickupLog 는 EditMode 에서 DontDestroyOnLoad 를 부른다)</summary>
    public static Action<string> Warn = msg => PickupLog.Warn(msg);

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        BountyAvailable = null;
        BountyActive = null;
        Warn = msg => PickupLog.Warn(msg);
    }

    public static bool IsBounty(NpcQuestDef d)
    {
        return QuestBountyLink.IsBounty(d);
    }

    /// <summary>CSV 퀘스트 + 오늘 현상(받을 수 있는 것·진행 중인 것)</summary>
    public static IReadOnlyList<NpcQuestDef> All()
    {
        var list = new List<NpcQuestDef>(NpcQuestCatalog.All);
        list.AddRange((BountyAvailable ?? QuestBountyLink.Available)());
        list.AddRange((BountyActive ?? QuestBountyLink.Active)());
        return list;
    }

    public static NpcQuestStatus StatusOf(NpcQuestDef d)
    {
        if (d == null)
        {
            return NpcQuestStatus.Locked;
        }
        if (!IsBounty(d))
        {
            return NpcQuestLog.StatusOf(d);
        }
        foreach (NpcQuestDef a in (BountyActive ?? QuestBountyLink.Active)())
        {
            if (a.Id == d.Id)
            {
                return QuestBountyLink.IsReady(d) ? NpcQuestStatus.Ready : NpcQuestStatus.Active;
            }
        }
        return NpcQuestLog.LevelGetter() >= d.MinLevel ? NpcQuestStatus.Available : NpcQuestStatus.LevelLow;
    }

    public static int Progress(NpcQuestDef d)
    {
        return IsBounty(d) ? QuestBountyLink.Progress(d) : NpcQuestLog.Progress(d);
    }

    /// <summary>탭·source·giver·zone 으로 거른 목록. 진행 중 탭은 완료 가능이 맨 위, 받을 수 있는 탭은 수락 가능이 레벨 부족보다 앞. 같은 묶음 안은 sort 후 표 순서</summary>
    public static QuestView Query(QuestFilter f)
    {
        f = f ?? new QuestFilter();
        var view = new QuestView { Tab = f.Tab };
        var pool = new List<KeyValuePair<int, NpcQuestDef>>();
        int n = 0;
        foreach (NpcQuestDef d in All())
        {
            if (Matches(d, f))
            {
                pool.Add(new KeyValuePair<int, NpcQuestDef>(n++, d));
            }
        }
        pool.Sort((a, b) => a.Value.Sort != b.Value.Sort ? a.Value.Sort.CompareTo(b.Value.Sort) : a.Key.CompareTo(b.Key));
        var sorted = new List<NpcQuestDef>();
        foreach (KeyValuePair<int, NpcQuestDef> kv in pool)
        {
            sorted.Add(kv.Value);
        }
        switch (f.Tab)
        {
            case QuestListRules.Tab.Available:
                view.Items.AddRange(QuestListRules.Available(sorted, StatusOf));
                break;
            case QuestListRules.Tab.Active:
                var active = new List<NpcQuestDef>();
                foreach (NpcQuestDef d in sorted)
                {
                    NpcQuestStatus s = StatusOf(d);
                    if (s == NpcQuestStatus.Active || s == NpcQuestStatus.Ready)
                    {
                        active.Add(d);
                    }
                }
                view.Items.AddRange(QuestListRules.Active(active, d => StatusOf(d) == NpcQuestStatus.Ready));
                break;
            default:
                foreach (NpcQuestDef d in sorted)
                {
                    if (StatusOf(d) == NpcQuestStatus.Done)
                    {
                        view.Items.Add(d);
                    }
                }
                break;
        }
        return view;
    }

    private static bool Matches(NpcQuestDef d, QuestFilter f)
    {
        if (!string.IsNullOrEmpty(f.Giver) && d.Giver != f.Giver && !(f.ReportToo && d.ReportNpc == f.Giver))
        {
            return false;
        }
        if (!string.IsNullOrEmpty(f.Source) && SourceOf(d) != f.Source)
        {
            return false;
        }
        return string.IsNullOrEmpty(f.Zone) || NpcQuestZone.Of(d) == f.Zone;
    }

    /// <summary>출처: CSV source 열(빈칸 = npc), 현상 어댑터 = bounty</summary>
    public static string SourceOf(NpcQuestDef d)
    {
        if (IsBounty(d))
        {
            return "bounty";
        }
        return string.IsNullOrEmpty(d.Source) ? "npc" : d.Source;
    }

    /// <summary>수락 가능 여부와, 안 되면 사유(창 안내용). 가방 공간은 보고 때 검사한다</summary>
    public static bool CanAccept(NpcQuestDef d, out string reason)
    {
        reason = "";
        if (d == null)
        {
            reason = "없는 퀘스트";
            return false;
        }
        switch (StatusOf(d))
        {
            case NpcQuestStatus.Available:
                if (IsBounty(d) && (BountyActive ?? QuestBountyLink.Active)().Count > 0)
                {
                    reason = "현상 의뢰는 동시에 1건만 받을 수 있습니다";
                    return false;
                }
                return true;
            case NpcQuestStatus.LevelLow: reason = "Lv " + d.MinLevel + " 필요"; return false;
            case NpcQuestStatus.Locked: reason = "먼저 완료해야 할 퀘스트가 있습니다"; return false;
            case NpcQuestStatus.Done: reason = "이미 완료한 퀘스트"; return false;
            default: reason = "이미 진행 중"; return false;
        }
    }

    public static bool Accept(string id)
    {
        if (IsBountyId(id))
        {
            BountyBoard b = BountyBoard.Instance;
            bool ok = b != null && b.Accept(id.Substring(QuestBountyLink.Prefix.Length));
            if (ok)
            {
                NpcQuestTrack.OnAccepted(id, ActiveAll()); // 현상도 자리가 남으면 추적에 붙인다
                NpcQuestLog.NotifyTrackingChanged();
            }
            return ok;
        }
        return NpcQuestLog.Accept(id);
    }

    /// <summary>포기: 진행 기록 삭제(현상은 Active 만 해제). 완료분·미수락은 false</summary>
    public static bool Abandon(string id)
    {
        if (IsBountyId(id))
        {
            BountyBoard b = BountyBoard.Instance;
            bool ok = b != null && b.Abandon(id.Substring(QuestBountyLink.Prefix.Length));
            if (ok)
            {
                NpcQuestTrack.OnCompleted(id);
                NpcQuestLog.NotifyTrackingChanged();
            }
            return ok;
        }
        return NpcQuestLog.Abandon(id);
    }

    /// <summary>완료 보고: 가방 공간이 모자라면 안내 후 보류(false). 성공하면 보상 지급</summary>
    public static bool Report(string id)
    {
        if (IsBountyId(id))
        {
            BountyBoard b = BountyBoard.Instance;
            bool ok = b != null && b.TryReport(id.Substring(QuestBountyLink.Prefix.Length));
            if (ok)
            {
                NpcQuestTrack.OnCompleted(id);
                NpcQuestLog.NotifyTrackingChanged();
            }
            return ok;
        }
        NpcQuestDef d = NpcQuestCatalog.Find(id);
        if (d != null && NpcQuestLog.StatusOf(d) == NpcQuestStatus.Ready && !NpcQuestLog.RewardCapacity(d))
        {
            Warn(NpcQuestRewards.InventoryFullHintFor(d.ReportNpc));
            return false;
        }
        return NpcQuestLog.Complete(id);
    }

    /// <summary>진행 중(수락함·완료 가능) 전부: CSV 퀘스트 + 진행 중 현상. 추적 HUD·창이 같은 목록을 쓴다</summary>
    public static List<NpcQuestDef> ActiveAll()
    {
        var list = new List<NpcQuestDef>(NpcQuestLog.ActiveQuests());
        list.AddRange((BountyActive ?? QuestBountyLink.Active)());
        return list;
    }

    public static bool IsReady(NpcQuestDef d)
    {
        return StatusOf(d) == NpcQuestStatus.Ready;
    }

    /// <summary>NPC·입구 id 의 가장 눈에 띄는 퀘스트 상태(머리 위 !/? 와 선택지 노출 조건이 같은 값을 쓴다)</summary>
    public static NpcQuestStatus BestFor(string giverId, out NpcQuestDef quest)
    {
        return NpcQuestLog.BestFor(giverId, out quest);
    }

    /// <summary>유형 한 글자: 메인 주 · 사이드 서 · 보상 보 · 현상 현</summary>
    public static string KindTag(NpcQuestDef d)
    {
        if (IsBounty(d))
        {
            return "현";
        }
        switch (d != null ? d.Kind : "")
        {
            case "main": return "주";
            case "side": return "서";
            default: return "보";
        }
    }

    /// <summary>구역 태그 (현상은 현상 정의의 zone, 그 외 NpcQuestZone)</summary>
    public static string ZoneOf(NpcQuestDef d)
    {
        return IsBounty(d) ? QuestBountyLink.Zone(d) : NpcQuestZone.Of(d);
    }

    private static bool IsBountyId(string id)
    {
        return id != null && id.StartsWith(QuestBountyLink.Prefix);
    }

    /// <summary>"대상 이름 + 행동 현재/필요" 한 줄 (예: "토끼 처치 4/10")</summary>
    public static string Objective(NpcQuestDef d)
    {
        int have = Progress(d);
        string what;
        switch (d.Objective)
        {
            case NpcQuestObjective.Kill: what = MonsterTable.DisplayName(d.FirstTarget) + " 처치"; break;
            case NpcQuestObjective.Collect: what = d.IsRecordsTarget ? "아버지의 기록 수집" : ItemText.DisplayName(d.Target) + " 수집"; break;
            case NpcQuestObjective.Craft: what = (d.Target == "Any" ? "장비" : ItemText.DisplayName(d.Target)) + " 제작"; break;
            case NpcQuestObjective.Visit: what = NpcQuestCatalog.VisitName(d.Target) + " 방문"; break;
            case NpcQuestObjective.KillElite: what = (d.Target == NpcQuestObjectiveRules.AnyElite ? "정예" : MonsterTable.DisplayName(d.Target)) + " 처치"; break;
            case NpcQuestObjective.Equip: what = EquipText(d.Target) + " 착용"; break;
            case NpcQuestObjective.Enhance: what = (d.Target == NpcQuestObjectiveRules.WeaponTarget ? "무기" : d.Target == NpcQuestObjectiveRules.ArmorTarget ? "방어구" : "장비") + " 강화 +" + d.Count; break;
            case NpcQuestObjective.Bestiary: what = "도감 몬스터 종류 처치"; break;
            default: what = NpcQuestCatalog.NpcName(d.Target) + "와(과) 대화"; break;
        }
        string line = what + " " + have + "/" + d.Count;
        if (!IsBounty(d) && NpcQuestLog.HasExtra(d))
        {
            line += " · " + (d.Target2 == NpcQuestObjectiveRules.AnyElite ? "정예" : MonsterTable.DisplayName(d.Target2)) + " 처치 " + NpcQuestLog.Progress2(d) + "/" + d.Count2;
        }
        return line;
    }

    private static string EquipText(string target)
    {
        if (target == NpcQuestObjectiveRules.ArmorTarget)
        {
            return "방어구 부위";
        }
        return NpcQuestObjectiveRules.TryParseSets(target, out int min) ? "세트 " + min + "부위 이상 세트 종류" : target + " 세트 부위";
    }

    /// <summary>완료 보고를 받는 NPC·입구의 표시 이름</summary>
    public static string ReportTarget(NpcQuestDef d)
    {
        if (d == null)
        {
            return "";
        }
        return IsBounty(d) ? NpcQuestCatalog.NpcName(QuestBountyLink.Giver) : NpcQuestCatalog.NpcName(d.ReportNpc);
    }
}
