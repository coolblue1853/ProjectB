using UnityEngine;

/// <summary>
/// 임시 몹 특성(MonsterSheet trait 열, '|' 로 여러 개): 이동·넉백·도약 배율과 돌진 수치. 순수 규칙이라 테스트한다.
/// slow = 이동 0.65배, heavy = 넉백 0.5배, dash = 웅크렸다 짧은 돌진(TempMobDash), lowhop = 도약 높이 0.8배, spore = 포자 구름.
/// 같은 호스트 프리팹을 쓰는 기존 몹(@0.5 섞기)은 특성이 없어 모두 1배다.
/// </summary>
public static class TempMobTraits
{
    public const float SlowMoveMul = 0.65f;
    public const float HeavyKnockbackMul = 0.5f;
    public const float LowHopMul = 0.8f;

    // 가지뿔 여우 돌진: 플레이어가 앞 2~5.5칸, 높이 차 1.2 이내면 0.35초 웅크린 뒤 0.3초 동안 초당 10칸(약 3칸)
    public const float DashMinRange = 2f, DashMaxRange = 5.5f, DashMaxDy = 1.2f; // 추적이 초당 약 3칸이라 창이 좁으면 돌진 전에 붙어 버린다(11-01 캡처)
    public const float CrouchTime = 0.35f, DashTime = 0.3f, DashSpeed = 10f, DashCooldown = 3.5f;

    public static bool Has(string traits, string trait)
    {
        if (string.IsNullOrEmpty(traits))
        {
            return false;
        }
        foreach (string t in traits.Split('|'))
        {
            if (t.Trim() == trait)
            {
                return true;
            }
        }
        return false;
    }

    public static float MoveMul(string traits) => Has(traits, "slow") ? SlowMoveMul : 1f;
    public static float KnockbackMul(string traits) => Has(traits, "heavy") ? HeavyKnockbackMul : 1f;
    public static float JumpMul(string traits) => Has(traits, "lowhop") ? LowHopMul : 1f;

    /// <summary>돌진을 시작할 거리인가(dx = 플레이어 x - 몹 x)</summary>
    public static bool DashInRange(float dx, float dy)
    {
        float ax = Mathf.Abs(dx);
        return ax >= DashMinRange && ax <= DashMaxRange && Mathf.Abs(dy) <= DashMaxDy;
    }
}
