using System;

/// <summary>
/// 퀵슬롯 등록 상태(순수 모델, BasicFeatures.md 3절). 칸마다 소비 아이템 이름 하나를 기억한다.
/// 같은 이름은 한 칸에만 둔다(새 칸에 등록하면 이전 칸은 비운다). 세이브는 string[SlotCount], 빈칸은 "".
/// </summary>
public sealed class QuickSlotModel
{
    public const int SlotCount = 4;

    private readonly string[] _ids = new string[SlotCount];

    public event Action Changed;

    /// <summary>slot 칸의 아이템 이름. 비었으면 null</summary>
    public string this[int slot] => slot >= 0 && slot < SlotCount ? _ids[slot] : null;

    /// <summary>slot 칸에 id 를 등록한다(null·빈 문자열이면 비움). 다른 칸의 같은 id 는 비운다. 바뀌었으면 true</summary>
    public bool Assign(int slot, string id)
    {
        if (slot < 0 || slot >= SlotCount)
        {
            return false;
        }
        string value = string.IsNullOrEmpty(id) ? null : id;
        if (_ids[slot] == value)
        {
            return false;
        }
        if (value != null)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (i != slot && _ids[i] == value)
                {
                    _ids[i] = null;
                }
            }
        }
        _ids[slot] = value;
        Changed?.Invoke();
        return true;
    }

    /// <summary>id 가 등록된 칸 번호. 없으면 -1</summary>
    public int IndexOf(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return -1;
        }
        for (int i = 0; i < SlotCount; i++)
        {
            if (_ids[i] == id)
            {
                return i;
            }
        }
        return -1;
    }

    public string[] ToSaveArray()
    {
        var a = new string[SlotCount];
        for (int i = 0; i < SlotCount; i++)
        {
            a[i] = _ids[i] ?? "";
        }
        return a;
    }

    /// <summary>세이브 배열을 읽는다. null·짧은 배열·중복은 관대하게(앞 칸 우선) 처리한다</summary>
    public void LoadFrom(string[] saved)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            _ids[i] = null;
        }
        if (saved != null)
        {
            for (int i = 0; i < SlotCount && i < saved.Length; i++)
            {
                string v = string.IsNullOrEmpty(saved[i]) ? null : saved[i];
                if (v != null && IndexOf(v) < 0)
                {
                    _ids[i] = v;
                }
            }
        }
        Changed?.Invoke();
    }
}
