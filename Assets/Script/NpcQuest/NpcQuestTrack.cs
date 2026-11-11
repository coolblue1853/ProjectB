using System;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 추적 지정 (최대 5개, 11-02). 퀘스트 창에서 Z 로 켜고 끄면 우측 추적 HUD(NpcQuestTracker)가 이 목록만 보여준다.
/// 한 번도 지정하지 않았으면(기존 세이브 포함) 진행 중 퀘스트 앞에서 3개를 자동으로 추적한다. 지정한 뒤에는 수락한 새 퀘스트가
/// 자리가 남을 때만 뒤에 붙고, 완료한 퀘스트는 빠진다. 세이브는 ES3 새 키 "npcq.tracked" (추가만, NpcQuestLog.SaveHook 과 같은 시점에 기록).
/// </summary>
public static class NpcQuestTrack
{
    public const string SaveKey = "npcq.tracked";
    public const int Max = 5; // 11-11 P3: 3 → 5 (현상 포함, 저장 형식 불변이라 구 세이브 호환)

    public const string FoldKey = "quest.hudFold";

    private static List<string> s_explicit; // null = 아직 지정한 적 없음(자동)

    /// <summary>테스트에서 바꿔 끼운다. 저장 문자열(키 없음 = null)을 읽고 쓴다</summary>
    public static Func<string> Loader = DefaultLoad;
    public static Action<string> Saver = DefaultSave;
    public static Func<bool> FoldLoader = DefaultFoldLoad;
    public static Action<bool> FoldSaver = DefaultFoldSave;

    /// <summary>추적 HUD 를 헤더 한 줄로 접었는가 (ES3 quest.hudFold, 새 키)</summary>
    public static bool HudFolded { get; private set; }

    public static void SetHudFolded(bool folded)
    {
        if (HudFolded == folded)
        {
            return;
        }
        HudFolded = folded;
        FoldSaver(folded);
    }

    public static void LoadFold()
    {
        HudFolded = FoldLoader();
    }

    private static bool DefaultFoldLoad()
    {
        try
        {
            return ES3.KeyExists(FoldKey) && ES3.Load<bool>(FoldKey);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "추적 HUD 접힘 읽기 실패: " + e.Message);
            return false;
        }
    }

    private static void DefaultFoldSave(bool folded)
    {
        try
        {
            ES3.Save(FoldKey, folded);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "추적 HUD 접힘 저장 실패: " + e.Message);
        }
    }

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_explicit = null;
        RestoreDefaults();
        HudFolded = false;
    }

    /// <summary>테스트가 바꿔 끼운 읽기·쓰기를 기본(ES3)으로 되돌린다</summary>
    public static void RestoreDefaults()
    {
        Loader = DefaultLoad;
        Saver = DefaultSave;
        FoldLoader = DefaultFoldLoad;
        FoldSaver = DefaultFoldSave;
    }

    public static void Reset()
    {
        s_explicit = null;
    }

    /// <summary>화면에 추적할 퀘스트(진행 중 목록에 있는 것만, 최대 5)</summary>
    public static List<NpcQuestDef> Tracked(IList<NpcQuestDef> active)
    {
        var result = new List<NpcQuestDef>(Max);
        if (s_explicit == null)
        {
            for (int i = 0; i < active.Count && result.Count < Max; i++)
            {
                result.Add(active[i]);
            }
            return result;
        }
        foreach (string id in s_explicit)
        {
            for (int i = 0; i < active.Count && result.Count < Max; i++)
            {
                if (active[i].Id == id)
                {
                    result.Add(active[i]);
                    break;
                }
            }
        }
        return result;
    }

    public static bool IsTracked(string id, IList<NpcQuestDef> active)
    {
        foreach (NpcQuestDef d in Tracked(active))
        {
            if (d.Id == id)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>추적을 켜거나 끈다. 이미 3개가 추적 중이라 켤 수 없으면 false</summary>
    public static bool Toggle(string id, IList<NpcQuestDef> active)
    {
        List<NpcQuestDef> now = Tracked(active);
        s_explicit = new List<string>(Max);
        foreach (NpcQuestDef d in now)
        {
            s_explicit.Add(d.Id);
        }
        if (s_explicit.Remove(id))
        {
            return true;
        }
        if (s_explicit.Count >= Max)
        {
            return false;
        }
        s_explicit.Add(id);
        return true;
    }

    /// <summary>새 퀘스트를 수락했다 — 지정 모드에서 자리가 남았으면 뒤에 붙인다(자동 모드는 이미 앞 3개를 본다)</summary>
    public static void OnAccepted(string id, IList<NpcQuestDef> active = null)
    {
        if (s_explicit == null || s_explicit.Contains(id))
        {
            return;
        }
        if (s_explicit.Count >= Max && active != null)
        {
            Prune(active, id); // 만료·사라진 id 가 자리를 막지 않게(저장 형식 불변)
        }
        if (s_explicit.Count < Max)
        {
            s_explicit.Add(id);
        }
    }

    /// <summary>지정 목록에서 진행 중이 아닌 id(만료된 현상 등)를 뺀다. keep 은 항상 남긴다. 바뀌었으면 true</summary>
    public static bool Prune(IList<NpcQuestDef> active, string keep = null)
    {
        if (s_explicit == null || active == null)
        {
            return false;
        }
        int removed = s_explicit.RemoveAll(id =>
        {
            if (id == keep)
            {
                return false;
            }
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].Id == id)
                {
                    return false;
                }
            }
            return true;
        });
        return removed > 0;
    }

    /// <summary>퀘스트를 끝냈다 — 지정 목록에서 뺀다</summary>
    public static void OnCompleted(string id)
    {
        if (s_explicit != null)
        {
            s_explicit.Remove(id);
        }
    }

    public static string Serialize()
    {
        return s_explicit == null ? null : string.Join(";", s_explicit);
    }

    public static void Deserialize(string text)
    {
        if (text == null)
        {
            s_explicit = null;
            return;
        }
        s_explicit = new List<string>(Max);
        foreach (string part in text.Split(';'))
        {
            if (part.Length > 0 && (NpcQuestCatalog.Find(part) != null || part.StartsWith(QuestBountyLink.Prefix)) && !s_explicit.Contains(part) && s_explicit.Count < Max)
            {
                s_explicit.Add(part);
            }
        }
    }

    /// <summary>NpcQuestLog 가 저장할 때 같이 부른다</summary>
    public static void Save()
    {
        string text = Serialize();
        if (text != null)
        {
            Saver(text);
        }
    }

    public static void Load()
    {
        Deserialize(Loader());
    }

    private static string DefaultLoad()
    {
        try
        {
            return ES3.KeyExists(SaveKey) ? ES3.Load<string>(SaveKey) : null;
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "퀘스트 추적 읽기 실패, 자동으로: " + e.Message);
            return null;
        }
    }

    private static void DefaultSave(string text)
    {
        try
        {
            ES3.Save(SaveKey, text);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "퀘스트 추적 저장 실패: " + e.Message);
        }
    }
}
