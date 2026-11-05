using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 신규 비외형 슬롯(귀걸이·팔찌·부적·마법석) 아이템 보조 스탯 (EquipSlotExpansion.md 3절). 프리팹 없이 Resources/ExtraEquipStats.csv 로 정의.
/// 컬럼: name,critical,criticalDmg,attSpeed,moveSpeed,dropRate,def,incDmg. 세트 판정(SetBonusEvaluator)과 무관.
/// </summary>
public static class ExtraEquipStats
{
    public const string CsvName = "ExtraEquipStats";
    static Dictionary<string, (StatId stat, float value)[]> _table;

    static readonly StatId[] Columns = { StatId.CritRate, StatId.CritDmg, StatId.AttackSpeed, StatId.MoveSpeed, StatId.DropRate, StatId.Def, StatId.IncDmg };

    /// <summary>CSV 텍스트 파싱 (순수). 헤더 1줄, 0 값은 제외</summary>
    public static Dictionary<string, (StatId stat, float value)[]> Parse(string csv)
    {
        var table = new Dictionary<string, (StatId, float)[]>();
        if (string.IsNullOrEmpty(csv))
        {
            return table;
        }
        string[] lines = csv.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 2 || c[0].Length == 0)
            {
                continue;
            }
            var list = new List<(StatId, float)>();
            for (int k = 0; k < Columns.Length && k + 1 < c.Length; k++)
            {
                if (float.TryParse(c[k + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) && v != 0f)
                {
                    list.Add((Columns[k], v));
                }
            }
            table[c[0]] = list.ToArray();
        }
        return table;
    }

    public static (StatId stat, float value)[] For(string itemName)
    {
        if (_table == null)
        {
            TextAsset ta = Resources.Load<TextAsset>(CsvName);
            _table = Parse(ta != null ? ta.text : "");
        }
        return itemName != null && _table.TryGetValue(itemName, out var s) ? s : System.Array.Empty<(StatId, float)>();
    }

    /// <summary>슬롯 하나가 장착한 아이템의 스탯을 PlayerStats 에 반영. source 로 이전분을 지우고 다시 건다</summary>
    public static void Apply(object source, string itemName)
    {
        PlayerStats.RemoveBySource(source);
        foreach (var (stat, value) in For(itemName))
        {
            PlayerStats.Add(new StatModifier(stat, ModType.Add, value) { Source = source, Owner = "Extra:" + itemName });
        }
    }
}
