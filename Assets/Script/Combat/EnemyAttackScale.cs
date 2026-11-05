using UnityEngine;

/// <summary>몹 공격 판정 피해 배율(정예 ×1.5 · 보스 페이즈 격노). 순수 규칙.</summary>
public static class EnemyAttackScale
{
    public static int Apply(int baseDamage, bool elite, float phaseMul)
    {
        float v = baseDamage;
        if (elite)
        {
            v *= EnemyHealth.EliteAtkMul;
        }
        int dealt = Mathf.RoundToInt(v);
        return Mathf.RoundToInt(dealt * phaseMul);
    }
}
