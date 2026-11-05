/// <summary>
/// 2층계 몹 행동 규칙 (Floor2Plan.md 5.2~5.4, AUTO-24). 상태·거리·타이머 입력 → 다음 행동 출력의 순수 함수.
/// 반사 피해 계산은 MobReflectRules, 둔화·화상은 MobDebuffRules 가 맡는다 — 여기서는 "언제 시작하나"만 정한다.
/// 임시 가정(수치): 깨어남 6, 반사 자세 2s/주기 6s, 덩굴 사거리 3·쿨 8s, 사격 사거리 9·후퇴 4·쿨 2.5s. 프리팹·AI 연결 없음.
/// </summary>
public static class Floor2MobBehaviorRules
{
    public const float WakeRange = 6f;          // 석상: 플레이어가 이 거리 이하(포함)면 깨어남
    public const float StanceSeconds = 2f;      // 천칭 석상: 반사 자세 지속 (MobReflectRules.Begin 에 넘김)
    public const float StanceCycle = 6f;        // 자세 시작 간격 (지속 + 대기)
    public const float VineRange = 3f;          // 정원사 골렘: 덩굴 묶기 최대 사거리
    public const float VineCooldown = 8f;
    public const float ShootRange = 9f;         // 궁수형: 사격 최대 거리
    public const float RetreatRange = 4f;       // 이 거리 미만이면 후퇴(포함 아님)
    public const float ShootCooldown = 2.5f;

    public enum StatueState { Dormant, Awake }
    public enum RangedAction { Approach, Shoot, Hold, Retreat }

    /// <summary>석상 정지→깨어남. 한번 깨면 되돌아가지 않는다. 피격으로도 깬다.</summary>
    public static StatueState StatueNext(StatueState cur, float distToPlayer, bool wasHit)
    {
        if (cur == StatueState.Awake || wasHit)
        {
            return StatueState.Awake;
        }
        return distToPlayer <= WakeRange ? StatueState.Awake : StatueState.Dormant;
    }

    /// <summary>
    /// 천칭 반사 자세 시작 여부. sinceStanceStart = 지난 자세 시작 뒤 경과(처음이면 큰 값). 자세 중/비활성(깨기 전)이면 false.
    /// 주기 = StanceCycle (자세 2s + 대기 4s).
    /// </summary>
    public static bool ShouldStartStance(bool awake, bool inStance, float sinceStanceStart)
    {
        return awake && !inStance && sinceStanceStart >= StanceCycle;
    }

    /// <summary>덩굴 묶기 시전 가능: 사거리 안(포함)이고 쿨 끝(남은 시간 0 이하)</summary>
    public static bool CanVineBind(float distToPlayer, float cooldownRemain)
    {
        return distToPlayer <= VineRange && cooldownRemain <= 0f;
    }

    /// <summary>궁수형 행동: 너무 가까우면 후퇴, 사거리 밖이면 접근, 사거리 안이면 쿨 끝났을 때 사격 아니면 대기</summary>
    public static RangedAction RangedNext(float distToPlayer, float shootCooldownRemain)
    {
        if (distToPlayer < RetreatRange)
        {
            return RangedAction.Retreat;
        }
        if (distToPlayer > ShootRange)
        {
            return RangedAction.Approach;
        }
        return shootCooldownRemain <= 0f ? RangedAction.Shoot : RangedAction.Hold;
    }
}
