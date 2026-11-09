using System;
using System.Collections.Generic;

/// <summary>
/// 활성 효과 출처 → 스킬별 SkillMods 질의 (EquipEffectDesign §2.4).
/// 레거시 Dictionary(skillCoolDown·skillHitCount·skillBulletCount)는 저장소로 남기고, 최종 질의는 여기서 상한을 통과시킨다.
/// 효과 id 는 CSV 에서 파생되므로 ES3 저장 없음(§6). Attach/Detach 는 출처 키 단위 멱등.
/// </summary>
public static class SkillEffectService
{
    static readonly Dictionary<string, List<ActiveEffect>> Sources = new Dictionary<string, List<ActiveEffect>>();
    static readonly List<ActiveEffect> Active = new List<ActiveEffect>();
    static int _order;

    /// <summary>출처의 효과 id 들을 건다(같은 키 재호출 = 교체). ids 가 비면 Detach 와 같다. 모르는 id 는 경고 후 무시</summary>
    public static void Attach(string sourceKey, IEnumerable<string> effectIds, string grade = null)
    {
        if (string.IsNullOrEmpty(sourceKey))
        {
            return;
        }
        var rows = new List<ActiveEffect>();
        if (effectIds != null)
        {
            int rank = EffectResolver.GradeRank(grade);
            var seen = new HashSet<string>();
            foreach (string id in effectIds)
            {
                if (!seen.Add(id))
                {
                    continue; // 같은 출처 내 중복 id 는 한 번만(이중 합산 방지)
                }
                List<EffectEntry> entries = EffectTable.Get(id);
                if (entries == null)
                {
                    GameLog.Warn(GameLog.Cat.Item, "알 수 없는 효과 id 무시: " + id);
                    continue;
                }
                foreach (EffectEntry e in entries)
                {
                    rows.Add(new ActiveEffect { entry = e, gradeRank = rank, order = ++_order });
                }
            }
        }
        if (rows.Count == 0)
        {
            Detach(sourceKey);
            return;
        }
        Sources[sourceKey] = rows;
        Recompute();
    }

    public static void Detach(string sourceKey)
    {
        if (sourceKey != null && Sources.Remove(sourceKey))
        {
            Recompute();
        }
    }

    /// <summary>활성 목록 재구성(장착·해제·세트 변동·로드 직후만 호출)</summary>
    public static void Recompute()
    {
        Active.Clear();
        foreach (List<ActiveEffect> l in Sources.Values)
        {
            Active.AddRange(l);
        }
    }

    public static void ClearAll()
    {
        Sources.Clear();
        Active.Clear();
    }

    public static int ActiveCount => Active.Count;

    /// <summary>스킬 효과 질의. 레거시 Dictionary 값을 장비 버킷에 더해 상한 적용. 시전 시점에만 호출(Update 금지)</summary>
    public static SkillMods Query(string skillName, WeaponKind kind)
    {
        DatabaseManager.skillCoolDown.TryGetValue(skillName ?? "", out int cd);
        DatabaseManager.skillHitCount.TryGetValue(skillName ?? "", out int hit);
        DatabaseManager.skillBulletCount.TryGetValue(skillName ?? "", out int bullet);
        return EffectResolver.Resolve(Active, skillName, kind == WeaponKind.None ? null : kind.ToString(), cd, hit, bullet, SkillTagTable.HasTag, null);
    }
}
