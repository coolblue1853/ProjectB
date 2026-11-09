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

/// <summary>
/// 원거리(발사형) 몹 공격의 수직 시야 제한(U16). 풀 점프 높이 2.87(JumpReach) 안쪽이면 같은 발판 층 근처로 본다.
/// 1층계 발판 한 칸 차이는 약 4.0 이라 다른 층의 플레이어(밧줄 오르는 중 포함)는 쏘지 않는다.
/// </summary>
public static class RangedAttackRules
{
    public const float MaxVerticalGap = 2.5f;

    public static bool InReach(float shooterY, float targetY)
    {
        return Mathf.Abs(targetY - shooterY) <= MaxVerticalGap;
    }
}
