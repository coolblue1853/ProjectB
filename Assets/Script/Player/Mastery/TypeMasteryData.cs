using System;
using System.Collections.Generic;

/// <summary>ES3 저장 형식(키 typeMastery). 타입 이름(WeaponKind) → 누적 EXP. 필드는 이름을 바꾸지 않는다</summary>
[Serializable]
public class TypeMasterySave
{
    public string[] kinds = new string[0];
    public int[] exps = new int[0];
}

/// <summary>타입별 누적 숙련 EXP 보관(순수, 저장은 Service 몫). 키 = WeaponKind 이름</summary>
public sealed class TypeMasteryData
{
    readonly Dictionary<string, int> _exp = new Dictionary<string, int>();

    public int Count { get { return _exp.Count; } }

    public static bool IsValidKind(string name)
    {
        WeaponKind k;
        return !string.IsNullOrEmpty(name) && Enum.TryParse(name, out k) && Enum.IsDefined(typeof(WeaponKind), k) && k != WeaponKind.None;
    }

    public int GetExp(string kind)
    {
        int v;
        return !string.IsNullOrEmpty(kind) && _exp.TryGetValue(kind, out v) ? v : 0;
    }

    public int GetLevel(string kind)
    {
        return TypeMasteryRules.LevelFromExp(GetExp(kind));
    }

    /// <summary>EXP 를 더한다. 반환 = 레벨 상승 여부</summary>
    public bool Add(string kind, int amount)
    {
        if (!IsValidKind(kind) || amount <= 0)
        {
            return false;
        }
        int before = GetExp(kind);
        int after = Math.Min(before + amount, TypeMasteryRules.ExpCap);
        _exp[kind] = after;
        return TypeMasteryRules.LevelFromExp(after) > TypeMasteryRules.LevelFromExp(before);
    }

    /// <summary>EXP 를 최소 minExp 로 올린다(이행). 내리지는 않는다</summary>
    public void RaiseTo(string kind, int minExp)
    {
        if (IsValidKind(kind) && GetExp(kind) < minExp)
        {
            _exp[kind] = Math.Min(minExp, TypeMasteryRules.ExpCap);
        }
    }

    /// <summary>
    /// 구 개별 무기 숙련(weaponMastery) → 타입 시드. 타입별 최대 EXP(합산 아님, 인플레 방지).
    /// 반환 = 타입을 못 찾은 키 수.
    /// </summary>
    public int SeedFromWeapons(string[] keys, int[] exps, Func<string, WeaponKind> kindOf)
    {
        int unresolved = 0;
        if (keys == null || exps == null)
        {
            return 0;
        }
        int n = Math.Min(keys.Length, exps.Length);
        for (int i = 0; i < n; i++)
        {
            WeaponKind k = kindOf(keys[i]);
            if (k == WeaponKind.None)
            {
                unresolved++;
                continue;
            }
            RaiseTo(k.ToString(), exps[i]);
        }
        return unresolved;
    }

    public TypeMasterySave ToSave()
    {
        var s = new TypeMasterySave { kinds = new string[_exp.Count], exps = new int[_exp.Count] };
        int i = 0;
        foreach (KeyValuePair<string, int> kv in _exp)
        {
            s.kinds[i] = kv.Key;
            s.exps[i] = kv.Value;
            i++;
        }
        return s;
    }

    public void LoadFrom(TypeMasterySave s)
    {
        _exp.Clear();
        if (s == null || s.kinds == null || s.exps == null)
        {
            return;
        }
        int n = Math.Min(s.kinds.Length, s.exps.Length);
        for (int i = 0; i < n; i++)
        {
            if (IsValidKind(s.kinds[i]) && s.exps[i] > 0)
            {
                _exp[s.kinds[i]] = Math.Min(s.exps[i], TypeMasteryRules.ExpCap);
            }
        }
    }
}
