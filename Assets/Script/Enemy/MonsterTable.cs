using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터 스탯 데이터 (Resources/MonsterSheet.csv, QualityRoadmap C5). 프리팹 이름이 키. EnemyHealth 가 Awake 에서 행이 있으면 적용한다.
/// 컬럼: id,displayName,floor,zone,maxHP,enemyDef,isBoss,superArmor,hpHeight,moveSpeed,dropRate,note,level,exp,contactDamage — 빈 값은 프리팹 값 유지.
/// 컬럼 추가는 맨 뒤에만(기존 행 호환).
/// </summary>
public static class MonsterTable
{
    public class Row
    {
        public string id, displayName, zone;
        public int floor;
        public int? maxHP, enemyDef;
        public bool? isBoss, superArmor;
        public float? hpHeight;
        public int? level, exp, contactDamage; // 클래식 메이플 레벨·처치 EXP·몸박 공격력(몸박 구현은 다음 단계)
    }

    static Dictionary<string, Row> rows;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        rows = null;
    }

    public static Row Get(string prefabName)
    {
        if (rows == null)
        {
            Load();
        }
        string key = prefabName.Replace("(Clone)", "").Trim();
        return rows.TryGetValue(key, out Row r) ? r : null;
    }

    /// <summary>배너·로그용 표시 이름: MonsterSheet displayName, 없으면 프리팹 이름에서 (Clone)·_BD 를 뗀 것.</summary>
    public static string DisplayName(string prefabName)
    {
        Row r = Get(prefabName);
        if (r != null && !string.IsNullOrEmpty(r.displayName))
        {
            return Loc.T(r.displayName); // 몬스터 이름 현지화(LocUi)
        }
        return prefabName.Replace("(Clone)", "").Replace("_BD", "").Trim();
    }

    public static IEnumerable<Row> All()
    {
        if (rows == null)
        {
            Load();
        }
        return rows.Values;
    }

    static void Load()
    {
        rows = new Dictionary<string, Row>();
        var text = Resources.Load<TextAsset>("MonsterSheet");
        if (text == null)
        {
            GameLog.Warn(GameLog.Cat.AI, "Resources/MonsterSheet.csv 없음 — 프리팹 값 사용");
            return;
        }
        string[] lines = text.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            string[] c = line.Split(',');
            if (c.Length < 9 || string.IsNullOrEmpty(c[0]))
            {
                continue;
            }
            var r = new Row { id = c[0], displayName = c[1], zone = c[3] };
            int.TryParse(c[2], out r.floor);
            r.maxHP = ParseInt(c[4]);
            r.enemyDef = ParseInt(c[5]);
            r.isBoss = ParseBool(c[6]);
            r.superArmor = ParseBool(c[7]);
            r.hpHeight = ParseFloat(c[8]);
            if (c.Length > 14)
            {
                r.level = ParseInt(c[12]);
                r.exp = ParseInt(c[13]);
                r.contactDamage = ParseInt(c[14]);
            }
            rows[r.id] = r;
        }
        GameLog.Info(GameLog.Cat.AI, "MonsterSheet 로드: " + rows.Count + "행");
    }

    static int? ParseInt(string s) => int.TryParse(s, out int v) ? v : (int?)null;
    static float? ParseFloat(string s) => float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : (float?)null;
    static bool? ParseBool(string s) => s == "1" ? true : s == "0" ? false : (bool?)null;

    /// <summary>프리팹 값 위에 CSV 값을 덮어쓴다 (행이 없으면 그대로)</summary>
    public static bool Apply(EnemyHealth enemy)
    {
        Row r = Get(enemy.name);
        if (r == null)
        {
            return false;
        }
        if (r.maxHP.HasValue) enemy.maxHP = r.maxHP.Value;
        if (r.enemyDef.HasValue) enemy.enemyDef = r.enemyDef.Value;
        if (r.isBoss.HasValue) enemy.isBossMob = r.isBoss.Value;
        if (r.superArmor.HasValue) enemy.isSuperArmor = r.superArmor.Value;
        if (r.hpHeight.HasValue) enemy.hpHeight = r.hpHeight.Value;
        return true;
    }
}
