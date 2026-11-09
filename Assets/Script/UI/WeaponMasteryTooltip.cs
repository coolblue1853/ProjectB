using System.Text;

/// <summary>
/// 무기 숙련 툴팁 문자열 헬퍼 (WeaponSkillProgression.md 6절). InventoryManager 를 키우지 않으려고 문자열만 만든다.
/// 숙련 데이터가 아직 없거나 스킬이 없는 무기는 빈 문자열.
/// </summary>
public static class WeaponMasteryTooltip
{
    /// <summary>숙련 Lv 3 (1200/3000) 줄과 다음 해금 줄을 줄바꿈으로 이어 붙인 문자열. 없으면 빈 문자열</summary>
    public static string Build(Weapon w)
    {
        if (w == null || w.skill == null || WeaponMasteryService.Data == null)
        {
            return string.Empty;
        }
        int count = 0;
        for (int i = 0; i < w.skill.Length; i++)
        {
            if (w.skill[i] != null)
            {
                count++;
            }
        }
        if (count == 0)
        {
            return string.Empty;
        }
        bool perWeapon = WeaponMasteryRules.PerWeaponMastery;
        int exp = WeaponMasteryService.EffectiveExp(w);
        int lv = WeaponMasteryService.EffectiveLevel(w);
        int maxLv = perWeapon ? WeaponMasteryRules.MaxLevel : TypeMasteryRules.MaxLevel;
        var sb = new StringBuilder();
        sb.Append('\n');
        if (lv >= maxLv)
        {
            sb.Append(Loc.F("Weapon mastery Lv {0} (MAX)", lv));
        }
        else
        {
            int inLv = perWeapon ? WeaponMasteryRules.ExpInLevel(exp) : TypeMasteryRules.ExpInLevel(exp);
            int toNext = perWeapon ? WeaponMasteryRules.ExpToNext(lv) : TypeMasteryRules.ExpToNext(lv);
            sb.Append(Loc.F("Weapon mastery Lv {0} ({1}/{2})", lv, inLv, toNext));
            int bestLv = int.MaxValue;
            string bestName = null;
            for (int i = 0; i < w.skill.Length; i++)
            {
                if (w.skill[i] == null)
                {
                    continue;
                }
                int need = WeaponMasteryService.UnlockLevelFor(w, i);
                if (need > lv && need < bestLv)
                {
                    bestLv = need;
                    bestName = Loc.T(w.skill[i].skillName);
                }
            }
            if (bestName != null)
            {
                sb.Append('\n').Append(Loc.F("Next unlock: {0} (Lv {1})", bestName, bestLv));
            }
        }
        return sb.ToString();
    }
}
