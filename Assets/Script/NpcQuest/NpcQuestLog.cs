using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>NPC 머리 위 표시 / 퀘스트 상태</summary>
public enum NpcQuestStatus
{
    Locked,     // 선행 미완료 — 표시 없음
    LevelLow,   // 선행은 끝났고 레벨이 모자람 — 회색 !
    Available,  // 수락 가능 — 노란 !
    Active,     // 진행 중
    Ready,      // 완료 가능 — 노란 ?
    Done,
}

/// <summary>
/// NPC 퀘스트 진행 상태·세이브 (ClassicMapleDesign.md 10절). 기존 QuestBoard(게시판 반복 의뢰)와 별개다.
/// 처치·제작은 MessageSystem "Kill"/"Craft" 메시지(NpcQuestDriver 가 수신), 수집은 인벤토리 개수, 방문은 VisitMarker.
/// 세이브: ES3 새 키 "npcq.state" 문자열 "id:상태:진행;..." (상태 1 = 진행 중, 2 = 완료). 기존 키는 건드리지 않는다.
/// </summary>
public static class NpcQuestLog
{
    public const string SaveKey = "npcq.state";

    private sealed class Rec
    {
        public int State;      // 1 진행, 2 완료
        public int Progress;   // 처치·제작·방문·대화 진행 (수집은 인벤토리에서 계산)
    }

    private static readonly Dictionary<string, Rec> s_recs = new Dictionary<string, Rec>();

    // 외부 의존 — 테스트에서 바꿔 끼운다
    public static Func<int> LevelGetter = () => LevelSystem.Level;
    public static Func<string, int> ItemCounter = DefaultItemCount;
    public static Action<string, int> ItemRemover = DefaultItemRemove;
    public static Func<int> RecordCounter = DefaultRecordCount;
    public static Action<NpcQuestDef> RewardApplier = NpcQuestRewards.Apply;
    public static Action SaveHook = DefaultSave;

    /// <summary>상태가 바뀜 (NPC 표시·퀘스트 창·추적 표시 갱신)</summary>
    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_recs.Clear();
        Changed = null;
        LevelGetter = () => LevelSystem.Level;
        ItemCounter = DefaultItemCount;
        ItemRemover = DefaultItemRemove;
        RecordCounter = DefaultRecordCount;
        RewardApplier = NpcQuestRewards.Apply;
        SaveHook = DefaultSave;
    }

    // ── 조회 ──

    public static bool IsDone(string id)
    {
        return s_recs.TryGetValue(id, out Rec r) && r.State == 2;
    }

    /// <summary>목표 진행량 (0..Count). 수집 = 인벤토리·기록 개수, 나머지 = 저장된 진행</summary>
    public static int Progress(NpcQuestDef d)
    {
        if (d == null)
        {
            return 0;
        }
        if (IsDone(d.Id))
        {
            return d.Count;
        }
        if (d.Objective == NpcQuestObjective.Collect)
        {
            int have = d.IsRecordsTarget ? RecordCounter() : ItemCounter(d.Target);
            return Mathf.Clamp(have, 0, d.Count);
        }
        return s_recs.TryGetValue(d.Id, out Rec r) ? Mathf.Clamp(r.Progress, 0, d.Count) : 0;
    }

    public static NpcQuestStatus StatusOf(NpcQuestDef d)
    {
        if (d == null)
        {
            return NpcQuestStatus.Locked;
        }
        if (s_recs.TryGetValue(d.Id, out Rec r))
        {
            if (r.State == 2)
            {
                return NpcQuestStatus.Done;
            }
            return Progress(d) >= d.Count ? NpcQuestStatus.Ready : NpcQuestStatus.Active;
        }
        if (!string.IsNullOrEmpty(d.Requires) && !IsDone(d.Requires))
        {
            return NpcQuestStatus.Locked;
        }
        return LevelGetter() >= d.MinLevel ? NpcQuestStatus.Available : NpcQuestStatus.LevelLow;
    }

    public static NpcQuestStatus StatusOf(string id) => StatusOf(NpcQuestCatalog.Find(id));

    /// <summary>NPC 한 명의 대표 상태: Ready > Available > LevelLow > Active > 없음. 해당 퀘스트도 돌려준다</summary>
    public static NpcQuestStatus BestFor(string npcId, out NpcQuestDef quest)
    {
        quest = null;
        NpcQuestStatus best = NpcQuestStatus.Locked;
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (d.Giver != npcId)
            {
                continue;
            }
            NpcQuestStatus s = StatusOf(d);
            if (Rank(s) > Rank(best))
            {
                best = s;
                quest = d;
            }
        }
        return best;
    }

    private static int Rank(NpcQuestStatus s)
    {
        switch (s)
        {
            case NpcQuestStatus.Ready: return 4;
            case NpcQuestStatus.Available: return 3;
            case NpcQuestStatus.LevelLow: return 2;
            case NpcQuestStatus.Active: return 1;
        }
        return 0;
    }

    /// <summary>진행 중(완료 가능 포함) 퀘스트 목록 — 퀘스트 창·추적 표시용 (표 순서)</summary>
    public static List<NpcQuestDef> ActiveQuests()
    {
        var list = new List<NpcQuestDef>();
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (s_recs.TryGetValue(d.Id, out Rec r) && r.State == 1)
            {
                list.Add(d);
            }
        }
        return list;
    }

    /// <summary>진행 중인 수집 퀘스트가 이 아이템을 아직 필요로 하는가 (퀘스트 전용 드롭 조건)</summary>
    public static bool WantsDrop(string itemKey)
    {
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (d.Objective == NpcQuestObjective.Collect && d.Target == itemKey
                && s_recs.TryGetValue(d.Id, out Rec r) && r.State == 1 && Progress(d) < d.Count)
            {
                return true;
            }
        }
        return false;
    }

    // ── 전이 ──

    /// <summary>수락. 수락 가능일 때만 성공. 목표가 "수락한 NPC와 대화"인 퀘스트는 곧바로 완료 가능이 된다</summary>
    public static bool Accept(string id)
    {
        NpcQuestDef d = NpcQuestCatalog.Find(id);
        if (d == null || StatusOf(d) != NpcQuestStatus.Available)
        {
            return false;
        }
        var rec = new Rec { State = 1 };
        if (d.Objective == NpcQuestObjective.Talk && d.Target == d.Giver)
        {
            rec.Progress = d.Count;
        }
        s_recs[id] = rec;
        Notify();
        GameLog.Info(GameLog.Cat.Item, "NPC 퀘스트 수락 " + id);
        return true;
    }

    /// <summary>완료 보고. 완료 가능일 때만 성공 — 수집 재료 회수 → 보상 지급 → 완료 기록</summary>
    public static bool Complete(string id)
    {
        NpcQuestDef d = NpcQuestCatalog.Find(id);
        if (d == null || StatusOf(d) != NpcQuestStatus.Ready)
        {
            return false;
        }
        if (d.Objective == NpcQuestObjective.Collect && !d.IsRecordsTarget)
        {
            ItemRemover(d.Target, d.Count);
        }
        s_recs[id] = new Rec { State = 2, Progress = d.Count };
        RewardApplier(d);
        Notify();
        GameLog.Info(GameLog.Cat.Item, "NPC 퀘스트 완료 " + id + " EXP " + d.Exp + " 골드 " + d.Gold);
        return true;
    }

    public static void OnKill(string monsterId)
    {
        Bump(NpcQuestObjective.Kill, monsterId);
    }

    public static void OnCraft(string itemName)
    {
        Bump(NpcQuestObjective.Craft, itemName);
    }

    public static void OnVisit(string markerId)
    {
        Bump(NpcQuestObjective.Visit, markerId);
    }

    /// <summary>NPC 와 대화 (대화 목표가 이 NPC 인 진행 중 퀘스트를 채운다)</summary>
    public static void OnTalk(string npcId)
    {
        Bump(NpcQuestObjective.Talk, npcId);
    }

    private static void Bump(NpcQuestObjective kind, string target)
    {
        bool changed = false;
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (d.Objective != kind || d.Target != target || !s_recs.TryGetValue(d.Id, out Rec r) || r.State != 1 || r.Progress >= d.Count)
            {
                continue;
            }
            r.Progress++;
            changed = true;
        }
        if (changed)
        {
            Notify();
        }
    }

    /// <summary>수집·기록처럼 외부 상태에서 계산되는 진행이 바뀌었을 수 있을 때 표시만 갱신 (저장 없음)</summary>
    public static void Refresh()
    {
        Changed?.Invoke();
    }

    private static void Notify()
    {
        Changed?.Invoke();
        SaveHook();
    }

    // ── 세이브 ──

    public static string Serialize()
    {
        var sb = new StringBuilder();
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (!s_recs.TryGetValue(d.Id, out Rec r))
            {
                continue;
            }
            if (sb.Length > 0)
            {
                sb.Append(';');
            }
            sb.Append(d.Id).Append(':').Append(r.State).Append(':').Append(r.Progress);
        }
        return sb.ToString();
    }

    /// <summary>문자열에서 복원 (알 수 없는 id·깨진 항목은 건너뜀)</summary>
    public static void Deserialize(string text)
    {
        s_recs.Clear();
        if (!string.IsNullOrEmpty(text))
        {
            foreach (string part in text.Split(';'))
            {
                string[] f = part.Split(':');
                if (f.Length != 3 || NpcQuestCatalog.Find(f[0]) == null
                    || !int.TryParse(f[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int st)
                    || !int.TryParse(f[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int pr)
                    || (st != 1 && st != 2))
                {
                    continue;
                }
                s_recs[f[0]] = new Rec { State = st, Progress = Mathf.Max(0, pr) };
            }
        }
        Changed?.Invoke();
    }

    public static void ClearAll()
    {
        s_recs.Clear();
        Changed?.Invoke();
    }

    private static void DefaultSave()
    {
        try
        {
            ES3.Save(SaveKey, Serialize());
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "NPC 퀘스트 저장 실패: " + e.Message);
        }
    }

    public static void LoadFromDisk()
    {
        try
        {
            Deserialize(ES3.KeyExists(SaveKey) ? ES3.Load<string>(SaveKey) : "");
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "NPC 퀘스트 읽기 실패, 비움: " + e.Message);
            ClearAll();
        }
    }

    // ── 기본 의존 구현 ──

    private static int DefaultItemCount(string item)
    {
        return InventoryManager.instance != null ? InventoryManager.instance.CountItem(item) : 0;
    }

    private static void DefaultItemRemove(string item, int count)
    {
        if (InventoryManager.instance == null)
        {
            return;
        }
        for (int i = 0; i < count; i++)
        {
            InventoryManager.instance.RemoveOne(item);
        }
    }

    private static int DefaultRecordCount()
    {
        int mask = FathersRecords.LoadMask();
        int n = 0;
        for (int i = 0; i < FathersRecords.All.Length; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                n++;
            }
        }
        return n;
    }
}
