using System;
using System.Collections.Generic;

/// <summary>ES3 저장 형식(키 weaponMastery). 무기 키 → 누적 숙련 EXP. 필드는 이름을 바꾸지 않는다</summary>
[Serializable]
public class WeaponMasterySave
{
    public string[] keys = new string[0];
    public int[] exps = new int[0];
}

/// <summary>무기 키별 누적 숙련 EXP 보관(순수, 저장은 Service 몫)</summary>
public sealed class WeaponMasteryData
{
    readonly Dictionary<string, int> _exp = new Dictionary<string, int>();

    public int Count { get { return _exp.Count; } }

    public bool Has(string key)
    {
        return !string.IsNullOrEmpty(key) && _exp.ContainsKey(key);
    }

    public int GetExp(string key)
    {
        int v;
        return !string.IsNullOrEmpty(key) && _exp.TryGetValue(key, out v) ? v : 0;
    }

    public int GetLevel(string key)
    {
        return WeaponMasteryRules.LevelFromExp(GetExp(key));
    }

    /// <summary>EXP 를 더한다. 반환 = 레벨 상승 여부</summary>
    public bool Add(string key, int amount)
    {
        if (string.IsNullOrEmpty(key) || amount <= 0)
        {
            return false;
        }
        int before = GetExp(key);
        int after = Math.Min(before + amount, WeaponMasteryRules.ExpCap);
        _exp[key] = after;
        return WeaponMasteryRules.LevelFromExp(after) > WeaponMasteryRules.LevelFromExp(before);
    }

    /// <summary>EXP 를 최소 minExp 로 올린다(구세이브 마이그레이션). 내리지는 않는다</summary>
    public void RaiseTo(string key, int minExp)
    {
        if (!string.IsNullOrEmpty(key) && GetExp(key) < minExp)
        {
            _exp[key] = Math.Min(minExp, WeaponMasteryRules.ExpCap);
        }
    }

    /// <summary>minLevel 이상이면서 match 를 만족하는 키가 하나라도 있는가(따라잡기 판정)</summary>
    public bool AnyAtLevel(int minLevel, Func<string, bool> match)
    {
        foreach (KeyValuePair<string, int> kv in _exp)
        {
            if (WeaponMasteryRules.LevelFromExp(kv.Value) >= minLevel && (match == null || match(kv.Key)))
            {
                return true;
            }
        }
        return false;
    }

    public WeaponMasterySave ToSave()
    {
        var s = new WeaponMasterySave { keys = new string[_exp.Count], exps = new int[_exp.Count] };
        int i = 0;
        foreach (KeyValuePair<string, int> kv in _exp)
        {
            s.keys[i] = kv.Key;
            s.exps[i] = kv.Value;
            i++;
        }
        return s;
    }

    public void LoadFrom(WeaponMasterySave s)
    {
        _exp.Clear();
        if (s == null || s.keys == null || s.exps == null)
        {
            return;
        }
        int n = Math.Min(s.keys.Length, s.exps.Length);
        for (int i = 0; i < n; i++)
        {
            if (!string.IsNullOrEmpty(s.keys[i]) && s.exps[i] > 0)
            {
                _exp[s.keys[i]] = Math.Min(s.exps[i], WeaponMasteryRules.ExpCap);
            }
        }
    }

    /// <summary>세이브 배열(인벤·창고 = [page,slot,{name..}], 장비 = [slot,{name..}])에 든 아이템 이름 중 isWeapon 인 것의 키 목록</summary>
    public static List<string> CollectWeaponKeys(string[,,] inven, string[,,] chest, string[,] gear, Func<string, bool> isWeapon)
    {
        var set = new HashSet<string>();
        AddCells(set, inven);
        AddCells(set, chest);
        if (gear != null)
        {
            for (int i = 0; i < gear.GetLength(0); i++)
            {
                AddName(set, gear[i, 0]);
            }
        }
        var result = new List<string>();
        foreach (string n in set)
        {
            if (isWeapon == null || isWeapon(n))
            {
                result.Add(n);
            }
        }
        return result;
    }

    static void AddCells(HashSet<string> set, string[,,] arr)
    {
        if (arr == null)
        {
            return;
        }
        for (int p = 0; p < arr.GetLength(0); p++)
        {
            for (int s = 0; s < arr.GetLength(1); s++)
            {
                AddName(set, arr[p, s, 0]);
            }
        }
    }

    static void AddName(HashSet<string> set, string name)
    {
        if (!string.IsNullOrEmpty(name) && name != "non")
        {
            set.Add(name);
        }
    }
}
