using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 제작 재료 추적(핀) 모델(순수, BasicFeatures.md 7절): 고정한 레시피 하나의 결과·재료·필요 수량.
/// 같은 재료가 여러 줄이면 합친다. 세이브 문자열 "결과|재료*수|재료*수".
/// </summary>
public sealed class CraftPinModel
{
    private const char Sep = '|';
    private const char CountSep = '*';

    private readonly List<string> _ids = new List<string>();
    private readonly List<int> _needs = new List<int>();

    public string Result { get; private set; }
    public bool IsEmpty => Result == null;
    public int Count => _ids.Count;
    public string IdAt(int i) => _ids[i];
    public int NeedAt(int i) => _needs[i];

    /// <summary>레시피를 고정한다. 결과가 비었거나 재료가 없으면 비운다</summary>
    public void Set(string result, IList<string> ids, IList<int> counts)
    {
        Clear();
        if (string.IsNullOrEmpty(result) || ids == null || counts == null)
        {
            return;
        }
        for (int i = 0; i < ids.Count && i < counts.Count; i++)
        {
            string id = ids[i];
            if (string.IsNullOrEmpty(id) || counts[i] <= 0)
            {
                continue;
            }
            int at = _ids.IndexOf(id);
            if (at >= 0)
            {
                _needs[at] += counts[i];
            }
            else
            {
                _ids.Add(id);
                _needs.Add(counts[i]);
            }
        }
        Result = _ids.Count > 0 ? result : null;
        if (Result == null)
        {
            Clear();
        }
    }

    public void Clear()
    {
        Result = null;
        _ids.Clear();
        _needs.Clear();
    }

    /// <summary>모든 재료를 갖췄는지(have: 재료 이름 → 보유 수)</summary>
    public bool IsReady(Func<string, int> have)
    {
        if (IsEmpty || have == null)
        {
            return false;
        }
        for (int i = 0; i < _ids.Count; i++)
        {
            if (have(_ids[i]) < _needs[i])
            {
                return false;
            }
        }
        return true;
    }

    public string Serialize()
    {
        if (IsEmpty)
        {
            return "";
        }
        var sb = new StringBuilder(Result);
        for (int i = 0; i < _ids.Count; i++)
        {
            sb.Append(Sep).Append(_ids[i]).Append(CountSep).Append(_needs[i]);
        }
        return sb.ToString();
    }

    /// <summary>세이브 문자열을 읽는다. 깨진 항목은 건너뛰고, 재료가 하나도 없으면 빈 핀</summary>
    public static CraftPinModel Parse(string s)
    {
        var m = new CraftPinModel();
        if (string.IsNullOrEmpty(s))
        {
            return m;
        }
        string[] parts = s.Split(Sep);
        var ids = new List<string>();
        var counts = new List<int>();
        for (int i = 1; i < parts.Length; i++)
        {
            int star = parts[i].LastIndexOf(CountSep);
            if (star <= 0 || !int.TryParse(parts[i].Substring(star + 1), out int n))
            {
                continue;
            }
            ids.Add(parts[i].Substring(0, star));
            counts.Add(n);
        }
        m.Set(parts[0], ids, counts);
        return m;
    }
}
