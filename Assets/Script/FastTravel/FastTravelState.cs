using System;
using System.Collections.Generic;

/// <summary>
/// 열린 정류장 상태 (FastTravel.md §7). ES3 키 ft.stops(문자열 "맵;맵;…", Town·SideRoad 처럼 항상 열린 곳은 저장 안 함)·ft.last(마지막 이용 정류장).
/// 해금 판정은 이 클래스 한 곳에서만 읽고 쓴다. 퀘스트 완료(NpcQuestLog.IsDone)는 AutoUnlock 이 반영 — 구세이브 자동 해금(Q-FT4)도 같은 경로.
/// </summary>
public static class FastTravelState
{
    public const string StopsKey = "ft.stops";
    public const string LastKey = "ft.last";

    private static readonly List<string> s_open = new List<string>();
    public static Action SaveHook = DefaultSave;

    /// <summary>마지막으로 이용한 정류장 id (목록 기본 선택용, 없으면 빈칸)</summary>
    public static string Last { get; set; } = "";

    public static bool IsOpen(FastTravelStop stop)
    {
        return stop != null && (stop.AlwaysOpen || s_open.Contains(stop.Id));
    }

    public static bool IsOpen(string mapId) => IsOpen(FastTravelStops.Find(mapId));

    /// <summary>정류장을 연다. 새로 열렸을 때만 true (저장 포함). 이미 열렸거나 모르는 맵이면 false</summary>
    public static bool Unlock(string mapId)
    {
        FastTravelStop stop = FastTravelStops.Find(mapId);
        if (stop == null || IsOpen(stop))
        {
            return false;
        }
        s_open.Add(mapId);
        SaveHook();
        return true;
    }

    /// <summary>해금 퀘스트를 끝낸 정류장을 연다. 새로 열린 수를 돌려준다</summary>
    public static int AutoUnlock(Func<string, bool> questDone)
    {
        int n = 0;
        foreach (FastTravelStop s in FastTravelStops.All)
        {
            if (!string.IsNullOrEmpty(s.QuestId) && questDone(s.QuestId) && Unlock(s.Id))
            {
                n++;
            }
        }
        return n;
    }

    public static string Serialize() => FastTravelRules.Join(s_open);

    public static void Deserialize(string text)
    {
        s_open.Clear();
        s_open.AddRange(FastTravelRules.Split(text));
    }

    public static void Reset()
    {
        s_open.Clear();
        Last = "";
    }

    public static void LoadFromDisk()
    {
        try
        {
            Deserialize(SaveUtil.LoadOr(StopsKey, ""));
            Last = SaveUtil.LoadOr(LastKey, "");
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "빠른 이동 읽기 실패, 비움: " + e.Message);
            Reset();
        }
    }

    public static void ResetSaveHook()
    {
        SaveHook = DefaultSave;
    }

    public static void Save()
    {
        SaveHook();
    }

    private static void DefaultSave()
    {
        try
        {
            ES3.Save(StopsKey, Serialize());
            ES3.Save(LastKey, Last ?? "");
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "빠른 이동 저장 실패: " + e.Message);
        }
    }
}
