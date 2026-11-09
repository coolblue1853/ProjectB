using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Resources/SkillTags.csv (skillName,tags; tags 는 ; 구분). 태그 미등록 스킬은 오류 아님 (EquipEffectDesign §2.1)</summary>
public static class SkillTagTable
{
    static Dictionary<string, HashSet<string>> _tags;

    public static string Norm(string skillName)
    {
        return string.IsNullOrEmpty(skillName) ? "" : skillName.Replace('_', ' ').Trim();
    }

    /// <summary>순수 파서. 오류·빈 행은 건너뛴다</summary>
    public static Dictionary<string, HashSet<string>> Parse(string csv)
    {
        var d = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        if (string.IsNullOrEmpty(csv))
        {
            return d;
        }
        foreach (string raw in csv.Replace("\r", "").Split('\n'))
        {
            string line = raw.Trim('\uFEFF', ' ');
            int comma = line.IndexOf(',');
            if (line.Length == 0 || line[0] == '#' || comma < 1)
            {
                continue;
            }
            string name = Norm(line.Substring(0, comma));
            if (name == "skillName" || name == "string")
            {
                continue;
            }
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (string t in line.Substring(comma + 1).Split(';'))
            {
                if (t.Trim().Length > 0)
                {
                    set.Add(t.Trim());
                }
            }
            d[name] = set;
        }
        return d;
    }

    public static void Build(Dictionary<string, HashSet<string>> tags)
    {
        _tags = tags;
    }

    public static void ResetCache()
    {
        _tags = null;
    }

    public static bool HasTag(string skillName, string tag)
    {
        if (_tags == null)
        {
            TextAsset ta = Resources.Load<TextAsset>("SkillTags");
            _tags = Parse(ta == null ? null : ta.text);
        }
        return _tags.TryGetValue(Norm(skillName), out HashSet<string> s) && s.Contains(tag);
    }
}
