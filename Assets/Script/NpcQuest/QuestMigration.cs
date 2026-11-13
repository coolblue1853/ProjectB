using System;
using System.Collections.Generic;
using PixelCrushers.QuestMachine;

/// <summary>
/// 게시판·촌장 Quest Machine 퀘스트(qm.*) → NPC 퀘스트 저장(npcq.state) 이행 (REWORK-5 P1, QuestSystemDesign §2-6).
/// qm.* 키는 건드리지 않고 남겨 롤백이 가능하다. 새 키 quest.migrated(int 버전) 만 추가한다 — 지우면 재이행.
/// 이행 규칙: 완료(Successful) → "id:2", 진행 중(Active) → "id:1:처치 카운터값". 이미 npcq 기록이 있으면 덮지 않는다.
/// 수집류(s_wood·s_herb)는 진행이 가방 보유량 실시간이라 진행값 0 으로 넣는다.
/// </summary>
public static class QuestMigration
{
    public const string SaveKey = "quest.migrated";
    public const int Version = 1;

    /// <summary>구 Quest Machine 저장 키 — 읽기 전용으로만 남긴다(삭제·이름 변경 금지, 롤백용)</summary>
    public const string LegacyBoardKey = "qm.board";
    public const string LegacyElderKey = "qm.elder";
    public const string LegacyJournalKey = "qm.journal";

    /// <summary>이행 대상 구 퀘스트 id (구 QuestCatalog 의 Chain·Side·Story — 정의는 P5 에서 삭제, 기준값은 QuestSystemTests.LegacySnapshot)</summary>
    public static readonly string[] LegacyIds =
    {
        "q_rabbits", "q_moles", "q_terror", "q_goblins", "q_wizard",
        "s_wood", "s_herb", "s_craft",
        "st_father1", "st_father2", "st_father3",
    };

    public enum LegacyState { None, Active, Done }

    public struct Legacy
    {
        public string Id;
        public LegacyState State;
        public int Progress;
    }

    public static Func<int> VersionGetter = () => SaveUtil.LoadOr(SaveKey, 0);
    public static Action<int> VersionSetter = v => ES3.Save(SaveKey, v);

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        VersionGetter = () => SaveUtil.LoadOr(SaveKey, 0);
        VersionSetter = v => ES3.Save(SaveKey, v);
    }

    public static bool Needed()
    {
        return VersionGetter() < Version;
    }

    /// <summary>구 상태 목록을 npcq 기록으로 옮긴다. 새로 넣은 건수를 돌려준다 (순수 — 테스트 대상)</summary>
    public static int Apply(IEnumerable<Legacy> legacy)
    {
        int n = 0;
        foreach (Legacy l in legacy)
        {
            if (l.State == LegacyState.None)
            {
                continue;
            }
            bool done = l.State == LegacyState.Done;
            NpcQuestDef d = NpcQuestCatalog.Find(l.Id);
            if (d == null)
            {
                continue;
            }
            int progress = done ? d.Count : Math.Min(Math.Max(0, l.Progress), d.Count);
            if (d.Objective == NpcQuestObjective.Collect)
            {
                progress = done ? d.Count : 0;
            }
            if (NpcQuestLog.SeedRecord(l.Id, done ? 2 : 1, progress))
            {
                n++;
            }
        }
        return n;
    }

    /// <summary>Quest Machine 저널을 읽어 구 상태 목록으로 (저널이 ApplyData 를 마친 뒤에 부른다)</summary>
    public static List<Legacy> ReadJournal(QuestJournal journal)
    {
        var list = new List<Legacy>();
        if (journal == null)
        {
            return list;
        }
        foreach (string id in LegacyIds)
        {
            Quest q = journal.FindQuest(id);
            if (q == null)
            {
                continue;
            }
            QuestState st = q.GetState();
            var l = new Legacy { Id = id };
            if (st == QuestState.Successful)
            {
                l.State = LegacyState.Done;
            }
            else if (st == QuestState.Active)
            {
                l.State = LegacyState.Active;
                QuestCounter c = q.GetCounter("kills");
                l.Progress = c != null ? c.currentValue : 0;
            }
            list.Add(l);
        }
        return list;
    }

    /// <summary>
    /// 한 번만 이행한다. 이행한 구 퀘스트는 저널에서 메모리상으로만 지워(HUD 가운데 글씨 방지) 디스크의 qm.* 는 그대로 둔다.
    /// 실패하면 로그만 남기고 다음 시작에 다시 시도한다.
    /// </summary>
    public static void RunOnce(QuestJournal journal)
    {
        try
        {
            if (!Needed() || journal == null)
            {
                return; // 저널이 없으면 구 기록을 읽을 수 없으니 이행했다고 표시하지 않는다
            }
            List<Legacy> legacy = ReadJournal(journal);
            int n = Apply(legacy);
            if (n > 0)
            {
                NpcQuestLog.NotifyTrackingChanged(); // 저장 + 표시 갱신
            }
            if (journal != null)
            {
                foreach (Legacy l in legacy)
                {
                    Quest q = journal.FindQuest(l.Id);
                    if (q != null)
                    {
                        journal.DeleteQuest(q);
                    }
                }
            }
            VersionSetter(Version);
            GameLog.Info(GameLog.Cat.Save, "[Quest] 게시판 퀘스트 이행 " + n + "건 (구 기록 " + legacy.Count + ")");
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "[PB][Quest] 이행 실패, 다음 시작에 재시도: " + e.Message);
        }
    }
}
