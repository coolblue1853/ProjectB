using System;
using System.Collections.Generic;

/// <summary>
/// 별도 콘텐츠(짝수 층계: 2 펫·4 룬·6 성물)의 해금 상태 (EQUIP-SLOT-REFORM v2). 장비 슬롯이 아니므로 세트 효과·EquipSlot 에 들어가지 않는다.
/// 해금 키·층계는 SlotUnlocks.csv(kind=content)가 정한다. ES3 키 "features.unlocked"(string[])는 추가만 한다.
/// </summary>
public static class ContentFeatures
{
    public const string SaveKey = "features.unlocked";
    public const string Pet = "Pet";
    public const string Rune = "Rune";
    public const string Relic = "Relic";

    static HashSet<string> _set;

    public static event Action<string> Unlocked;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _set = null;
        Unlocked = null;
    }

    static HashSet<string> Set
    {
        get
        {
            if (_set == null)
            {
                _set = new HashSet<string>();
                try
                {
                    string[] saved = SaveUtil.LoadOrThrow<string[]>(SaveKey, null);
                    if (saved != null)
                    {
                        _set.UnionWith(saved);
                    }
                }
                catch (Exception e)
                {
                    GameLog.Warn(GameLog.Cat.Item, "별도 콘텐츠 해금 세이브 읽기 실패, 잠금으로 시작: " + e.Message);
                }
            }
            return _set;
        }
    }

    public static bool IsUnlocked(string key)
    {
        return Set.Contains(key);
    }

    /// <summary>해금(영구). SlotUnlocks.csv 에 content 로 있는 키만. 새로 열렸으면 true</summary>
    public static bool Unlock(string key)
    {
        SlotUnlockRow row = SlotUnlockTable.Current.ForKey(key);
        if (row == null || row.IsSlot || !Set.Add(key))
        {
            return false;
        }
        Save();
        PickupLog.Notice(Loc.T("New feature unlocked: ") + Loc.T(key));
        Unlocked?.Invoke(key);
        return true;
    }

    /// <summary>메모리 상태를 비우고 다음 접근 때 세이브를 다시 읽는다(세이브 로드·삭제 때, 테스트)</summary>
    public static void ResetCache()
    {
        _set = null;
    }

    static void Save()
    {
        try
        {
            string[] arr = new string[Set.Count];
            Set.CopyTo(arr);
            ES3.Save(SaveKey, arr);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "별도 콘텐츠 해금 저장 실패: " + e.Message);
        }
    }
}
