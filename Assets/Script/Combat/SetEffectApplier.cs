using System.Collections.Generic;

/// <summary>
/// 세트 효과 문자열을 해석해 플레이어 스탯에 적용/해제한다 (스펙: .claude-rules/Game/ItemSpec.md 3절).
/// 문법: "(+)값 스탯 [스킬_이름]"을 "/"로 연결. 예) "(+)10 Def/(+)2 bulletCount Scattering_Leaves"
/// 적용과 해제가 같은 코드 경로(부호만 반대)라서 비대칭 버그가 생기지 않는다.
/// </summary>
public static class SetEffectApplier
{
    public struct Entry
    {
        public int value;      // 부호 포함
        public string stat;
        public string skill;   // 스킬 대상 스탯만 (공백 복원됨)
    }

    static readonly HashSet<string> SkillStats = new HashSet<string> { "skillHitCount", "bulletCount", "coolDown" };

    /// <summary>파싱만 (테스트·툴팁용). 잘못된 항목은 건너뛴다.</summary>
    public static List<Entry> Parse(string effect)
    {
        var list = new List<Entry>();
        if (string.IsNullOrEmpty(effect))
        {
            return list;
        }
        foreach (string raw in effect.Split('/'))
        {
            string part = raw.Trim();
            int close = part.IndexOf(')');
            if (!part.StartsWith("(") || close < 2)
            {
                continue;
            }
            char sign = part[1];
            string[] tokens = part.Substring(close + 1).Trim().Split(' ');
            if (tokens.Length < 2 || !int.TryParse(tokens[0], out int v))
            {
                continue;
            }
            var e = new Entry { value = sign == '-' ? -v : v, stat = tokens[1] };
            if (SkillStats.Contains(e.stat))
            {
                if (tokens.Length < 3)
                {
                    continue;
                }
                e.skill = tokens[2].Replace("_", " ");
            }
            list.Add(e);
        }
        return list;
    }

    /// <summary>
    /// apply=true면 적용, false면 같은 양을 되돌린다.
    /// group(예: "Set:RabbitArmor_2")이 있으면 정수 스탯은 그 그룹의 모디파이어로 넣고 해제는 RemoveByGroup 한 번(PlayerStateDesign Phase 10b).
    /// HP·스킬별 수치는 아직 기존 경로(10d·10e).
    /// </summary>
    public static void Apply(string effect, bool apply, string group = null)
    {
        GameLog.Info(GameLog.Cat.Item, (apply ? "세트 효과 적용: " : "세트 효과 해제: ") + effect);
        if (group != null)
        {
            PlayerStats.RemoveByGroup(group); // 적용이면 중복 방지, 해제면 정수 스탯 전부 해제
        }
        foreach (Entry e in Parse(effect))
        {
            if (group != null && StatOf(e.stat, out StatId id))
            {
                if (apply)
                {
                    PlayerStats.Add(new StatModifier(id, ModType.Add, e.value) { Group = group, Owner = group });
                }
                continue;
            }
            ApplyEntry(e, apply ? e.value : -e.value);
        }
    }

    /// <summary>세트 CSV 스탯 키 → StatId (HP·스킬 키는 false)</summary>
    static bool StatOf(string stat, out StatId id)
    {
        switch (stat)
        {
            case "Def": id = StatId.Def; return true;
            case "Crit": id = StatId.CritRate; return true;
            case "CritDmg": id = StatId.CritDmg; return true;
            case "IncDmg": id = StatId.IncDmg; return true;
            case "BasicDmg": id = StatId.BasicDmg; return true;
            case "Poison": id = StatId.PoisonDmg; return true;
            case "BleedDmg": id = StatId.BleedIncDmg; return true;
            case "AtkSpeed": id = StatId.AttackSpeed; return true;
            case "MoveSpeed": id = StatId.MoveSpeed; return true;
            case "DropRate": id = StatId.DropRate; return true;
            default: id = StatId.Def; return false;
        }
    }

    static void ApplyEntry(Entry e, int delta)
    {
        switch (e.stat)
        {
            case "Def": DatabaseManager.playerDef += delta; break;
            case "Hp":
                if (PlayerHealthManager.Instance != null)
                {
                    if (delta > 0) PlayerHealthManager.Instance.EquipmentActiveTrue(delta);
                    else PlayerHealthManager.Instance.EquipmentActiveFalse(-delta);
                }
                break;
            case "Crit": DatabaseManager.playerCritRate += delta; break;
            case "CritDmg": DatabaseManager.playerCritDmgRate += delta; break;
            case "IncDmg": DatabaseManager.incDmg += delta; break;
            case "BasicDmg": DatabaseManager.addbasicDmg += delta; break;
            case "Poison": DatabaseManager.addPoisonDmg += delta; break;
            case "BleedDmg": DatabaseManager.bleedingAddDmg += delta; break;
            case "AtkSpeed": DatabaseManager.attackSpeedBuff += delta; break;
            case "MoveSpeed": DatabaseManager.SpeedBuff += delta; break;
            case "DropRate": DatabaseManager.playerDropRate += delta; break;
            case "skillHitCount": AddSkill(DatabaseManager.skillHitCount, e.skill, delta); break;
            case "bulletCount": AddSkill(DatabaseManager.skillBulletCount, e.skill, delta); break;
            case "coolDown": AddSkill(DatabaseManager.skillCoolDown, e.skill, delta); break;
            default: UnityEngine.Debug.LogWarning("알 수 없는 세트 효과 스탯: " + e.stat); break;
        }
    }

    static void AddSkill(Dictionary<string, int> dict, string skill, int delta)
    {
        dict.TryGetValue(skill, out int now);
        now += delta;
        if (now == 0)
        {
            dict.Remove(skill);
        }
        else
        {
            dict[skill] = now;
        }
    }
}
