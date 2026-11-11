using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>SlotUnlocks.csv 한 줄: 층계별 해금 항목(slot=장비 슬롯, content=별도 콘텐츠). hookId 는 ";" 로 여러 개(스토리 진행 호출 훅 이름)</summary>
public sealed class SlotUnlockRow
{
    public string Key;
    public bool IsSlot;
    public int Floor;
    public string[] Hooks;
}

/// <summary>
/// 층계 → 해금 표 (Assets/Resources/SlotUnlocks.csv: key,kind,floor,hookId). 순수 파서 + Resources 로더.
/// 홀수 층계 = 장비 슬롯(key 가 EquipSlot 이름), 짝수 층계 = 별도 콘텐츠(Pet·Rune·Relic). 해금 호출처는 훅 이름(예: "boss:1")으로 TryHook 한다.
/// </summary>
public sealed class SlotUnlockTable
{
    public const string ResourceName = "SlotUnlocks";

    /// <summary>CSV 가 없을 때의 기본값(파일과 같은 내용)</summary>
    const string DefaultCsv = "key,kind,floor,hookId\n"
        + "Charm,slot,1,boss:1;bestiary:B04\nPet,content,2,boss:2\nBracelet,slot,3,boss:3\nRune,content,4,boss:4\n"
        + "Belt,slot,5,boss:5\nRelic,content,6,boss:6\nStone,slot,7,boss:7\n";

    static SlotUnlockTable _current;

    public readonly List<SlotUnlockRow> Rows = new List<SlotUnlockRow>();

    public static SlotUnlockTable Current => _current ??= Load();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _current = null;
    }

    /// <summary>테스트용: 캐시를 비워 다시 읽게 한다</summary>
    public static void Reload()
    {
        _current = null;
    }

    static SlotUnlockTable Load()
    {
        TextAsset ta = Resources.Load<TextAsset>(ResourceName);
        SlotUnlockTable t = Parse(ta != null ? ta.text : DefaultCsv);
        return t.Rows.Count > 0 ? t : Parse(DefaultCsv);
    }

    public static SlotUnlockTable Parse(string csv)
    {
        var t = new SlotUnlockTable();
        if (string.IsNullOrEmpty(csv))
        {
            return t;
        }
        string[] lines = csv.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++) // 0 = 헤더
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }
            string[] f = line.Split(',');
            if (f.Length < 3 || !int.TryParse(f[2].Trim(), out int floor))
            {
                continue;
            }
            t.Rows.Add(new SlotUnlockRow
            {
                Key = f[0].Trim(),
                IsSlot = f[1].Trim() == "slot",
                Floor = floor,
                Hooks = f.Length > 3 && f[3].Trim().Length > 0 ? f[3].Trim().Split(';') : new string[0],
            });
        }
        return t;
    }

    public SlotUnlockRow ForFloor(int floor)
    {
        foreach (SlotUnlockRow r in Rows)
        {
            if (r.Floor == floor)
            {
                return r;
            }
        }
        return null;
    }

    public SlotUnlockRow ForHook(string hookId)
    {
        foreach (SlotUnlockRow r in Rows)
        {
            if (Array.IndexOf(r.Hooks, hookId) >= 0)
            {
                return r;
            }
        }
        return null;
    }

    public SlotUnlockRow ForKey(string key)
    {
        foreach (SlotUnlockRow r in Rows)
        {
            if (r.Key == key)
            {
                return r;
            }
        }
        return null;
    }

    /// <summary>층계의 장비 슬롯. 별도 콘텐츠 층계·표에 없으면 false</summary>
    public bool TrySlotForFloor(int floor, out EquipSlot slot)
    {
        slot = default;
        SlotUnlockRow r = ForFloor(floor);
        return r != null && r.IsSlot && Enum.TryParse(r.Key, out slot) && SlotRules.IsExtra(slot);
    }
}
