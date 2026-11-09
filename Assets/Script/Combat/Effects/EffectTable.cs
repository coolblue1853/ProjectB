using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Resources/EquipEffects.csv 로드·순수 파서 (EquipEffectDesign §2.8).
/// 열: id,entry,kind,selector,op,value,cond,group,rule,p1,p2,p3,nameKey,descKey,minGrade (쉼표 금지, 구분은 | ;).
/// 오류 행(헤더·타입 행 포함)은 건너뛰고 로드 실패로 만들지 않는다.
/// </summary>
public static class EffectTable
{
    static Dictionary<string, List<EffectEntry>> _byId;

    /// <summary>순수 파서. 잘못된 행은 건너뛴다</summary>
    public static List<EffectEntry> ParseEntries(string csv)
    {
        var list = new List<EffectEntry>();
        if (string.IsNullOrEmpty(csv))
        {
            return list;
        }
        string[] lines = csv.Replace("\r", "").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim('\uFEFF', ' ');
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }
            if (TryParseRow(line.Split(','), out EffectEntry e))
            {
                list.Add(e);
            }
        }
        return list;
    }

    static string Cell(string[] c, int i)
    {
        return i < c.Length ? c[i].Trim() : "";
    }

    static bool TryParseRow(string[] c, out EffectEntry e)
    {
        e = null;
        string id = Cell(c, 0);
        if (id.Length == 0 || !int.TryParse(Cell(c, 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int entryNo) || entryNo < 1)
        {
            return false;
        }
        if (!Enum.TryParse(Cell(c, 2), false, out EffectKind kind) || Cell(c, 3).Length == 0)
        {
            return false;
        }
        var r = new EffectEntry
        {
            id = id, entry = entryNo, kind = kind, selector = Cell(c, 3), cond = Cell(c, 6), group = Cell(c, 7),
            p1 = Cell(c, 9), p2 = Cell(c, 10), p3 = Cell(c, 11), nameKey = Cell(c, 12), descKey = Cell(c, 13), minGrade = Cell(c, 14),
        };
        if (kind == EffectKind.Mod)
        {
            if (!Enum.TryParse(Cell(c, 4), false, out r.op) || r.op == EffectOp.None
                || !float.TryParse(Cell(c, 5), NumberStyles.Float, CultureInfo.InvariantCulture, out r.value))
            {
                return false;
            }
        }
        else if (!Enum.TryParse(Cell(c, 8), false, out r.rule) || r.rule == EffectRule.None)
        {
            return false;
        }
        e = r;
        return true;
    }

    /// <summary>효과 id → 행 목록. 없으면 null</summary>
    public static List<EffectEntry> Get(string id)
    {
        EnsureLoaded();
        return id != null && _byId.TryGetValue(id, out List<EffectEntry> l) ? l : null;
    }

    public static void Load()
    {
        _byId = new Dictionary<string, List<EffectEntry>>();
        TextAsset ta = Resources.Load<TextAsset>("EquipEffects");
        if (ta == null)
        {
            return;
        }
        Build(ParseEntries(ta.text));
    }

    /// <summary>테스트·툴용: 행 목록으로 테이블을 직접 채운다</summary>
    public static void Build(List<EffectEntry> rows)
    {
        _byId = new Dictionary<string, List<EffectEntry>>();
        foreach (EffectEntry e in rows)
        {
            if (!_byId.TryGetValue(e.id, out List<EffectEntry> l))
            {
                l = new List<EffectEntry>();
                _byId[e.id] = l;
            }
            l.Add(e);
        }
    }

    /// <summary>테스트 정리용: 다음 조회 때 CSV 를 다시 로드</summary>
    public static void ResetCache()
    {
        _byId = null;
    }

    static void EnsureLoaded()
    {
        if (_byId == null)
        {
            Load();
        }
    }
}
