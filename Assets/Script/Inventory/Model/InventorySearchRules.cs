using System;
using System.Collections.Generic;

/// <summary>가방 이름 검색 순수 규칙 (InventoryRefactorDesign §9 S4). 표시만 거른다 — 슬롯 인덱스·세이브는 불변.</summary>
public static class InventorySearchRules
{
    public const int MaxLength = 16;

    /// <summary>공백으로 나눈 낱말이 모두 이름에 들어 있으면 true(대소문자 무시). 빈 검색어는 모두 통과</summary>
    public static bool Matches(string query, string itemName)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }
        if (string.IsNullOrEmpty(itemName))
        {
            return false;
        }
        foreach (string token in query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (itemName.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>글자를 뒤에 붙인다. 제어 문자·줄바꿈·길이 초과는 무시</summary>
    public static string Append(string query, char c)
    {
        string q = query ?? "";
        if (char.IsControl(c) || q.Length >= MaxLength)
        {
            return q;
        }
        return q + c;
    }

    public static string Backspace(string query)
    {
        return string.IsNullOrEmpty(query) ? "" : query.Substring(0, query.Length - 1);
    }

    /// <summary>
    /// 패드용(글자판 없음): 지금 가방 아이템 이름의 첫 글자를 정렬해 한 칸씩 넘긴다. 마지막 다음은 빈 검색어(해제).
    /// 현재 검색어가 한 글자가 아니거나 목록에 없으면 첫 글자부터 시작한다.
    /// </summary>
    public static string NextInitial(IEnumerable<string> names, string current)
    {
        var set = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string n in names)
        {
            if (!string.IsNullOrEmpty(n))
            {
                set.Add(n.Substring(0, 1));
            }
        }
        if (set.Count == 0)
        {
            return "";
        }
        var list = new List<string>(set);
        int at = (current != null && current.Length == 1) ? list.FindIndex(s => string.Equals(s, current, StringComparison.OrdinalIgnoreCase)) : -1;
        int next = at + 1;
        return next >= list.Count ? "" : list[next];
    }
}
