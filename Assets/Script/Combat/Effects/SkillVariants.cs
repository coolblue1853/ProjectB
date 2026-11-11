using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Replace 규칙(EquipEffectDesign §2.3, P5): 스킬의 판정 프리팹을 변형판(Resources/Weapon/Variants/{p1})으로 바꾼다.
/// 공유 원본 프리팹은 절대 수정하지 않는다(복제본만 로드). 효과가 없거나 변형이 없으면 원본 그대로 돌려준다.
/// </summary>
public static class SkillVariants
{
    public const string Folder = "Weapon/Variants/";
    static readonly Dictionary<string, GameObject> Cache = new Dictionary<string, GameObject>();
    static readonly HashSet<string> Warned = new HashSet<string>();

    /// <summary>적용된 Replace 규칙의 변형 키(p1). 없으면 null</summary>
    public static string KeyOf(in SkillMods m)
    {
        EffectEntry[] rules = m.rules;
        if (rules == null)
        {
            return null;
        }
        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i].rule == EffectRule.Replace && !string.IsNullOrEmpty(rules[i].p1))
            {
                return rules[i].p1;
            }
        }
        return null;
    }

    /// <summary>변형 키로 프리팹 로드(캐시). DamageObject 루트가 아니거나 없으면 null + 1회 경고</summary>
    public static GameObject Load(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }
        if (Cache.TryGetValue(key, out GameObject g))
        {
            return g;
        }
        g = Resources.Load<GameObject>(Folder + key);
        if (g != null && !g.TryGetComponent(out DamageObject _))
        {
            g = null;
        }
        if (g == null && Warned.Add(key))
        {
            GameLog.Warn(GameLog.Cat.Combat, "장비 효과 Replace: 변형 프리팹 '" + key + "' 없음 또는 DamageObject 아님 → 원본 사용");
        }
        Cache[key] = g;
        return g;
    }

    /// <summary>원본 프리팹과 질의 결과 → 실제로 생성할 프리팹</summary>
    public static GameObject Resolve(GameObject basePrefab, in SkillMods m)
    {
        string key = KeyOf(m);
        if (key == null)
        {
            return basePrefab;
        }
        GameObject v = Load(key);
        return v != null ? v : basePrefab;
    }
}
