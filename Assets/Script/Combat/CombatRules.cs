using UnityEngine;

/// <summary>스킬이 지금 나가지 못하는 이유(HUD 안내용). None 이면 발동 가능.</summary>
public enum SkillBlock { None, Cooldown, Stamina, Busy }

/// <summary>
/// 던파식 전투 규칙 수치 한 곳(11-02, CombatSpec 9절). 순수 함수라 EditMode 테스트로 고정한다.
/// 평타 후딜 캔슬·경직 연쇄 감소/저항·공중 콤보 중력·다운 공격 배율·스킬 막힘 이유.
/// </summary>
public static class CombatRules
{
    // ── 캔슬: 평타 후딜을 스킬로 끊기 ──
    /// <summary>평타 후딜(공격 주기)의 이만큼이 지나야 스킬로 끊을 수 있다. 타격 프레임 전에는 끊지 못한다(헛스윙 방지).</summary>
    public const float MeleeCancelFraction = 0.5f;
    /// <summary>판정이 나오는 시점(선딜) 뒤 이만큼 더 지나야 끊는다(초).</summary>
    public const float CancelHitMargin = 0.05f;

    /// <summary>평타 후딜 중 스킬로 끊을 수 있는가. elapsed = 평타 시작 후 시간, total = 후딜 총 시간, hitDelay = 판정이 나오는 시점.</summary>
    public static bool CanCancelMeleeRecovery(float elapsed, float total, float hitDelay)
    {
        if (total <= 0f)
        {
            return false;
        }
        float point = Mathf.Max(total * MeleeCancelFraction, hitDelay + CancelHitMargin);
        return elapsed >= point && elapsed < total;
    }

    // ── 조작감 CF-1·2·3·5: 평타 후딜 중 전진·점프/이동 캔슬·방향 전환 (CombatFeelPlan §3) ──
    /// <summary>마무리 타(3타 이상 무기의 마지막 타)는 전진을 절반으로, 이동 캔슬은 60% 에서.</summary>
    public const float FinisherDriftScale = 0.5f, PreHitDriftScale = 0.7f;
    public const float MoveCancelFraction = 0.4f, FinisherMoveCancelFraction = 0.6f;
    const int FinisherMinSteps = 3;

    /// <summary>step(1부터)이 3타 이상 콤보의 마지막 타인가.</summary>
    public static bool IsFinisher(int step, int totalSteps)
    {
        return totalSteps >= FinisherMinSteps && step == totalSteps;
    }

    /// <summary>후딜 중 전진 속도 배율. 타격 전(선딜)은 x0.7, 마무리 타는 x0.5.</summary>
    public static float DriftMul(float baseMul, int step, int totalSteps, float elapsed, float hitDelay)
    {
        float mul = Mathf.Clamp01(baseMul);
        if (IsFinisher(step, totalSteps))
        {
            mul *= FinisherDriftScale;
        }
        if (elapsed < hitDelay)
        {
            mul *= PreHitDriftScale;
        }
        return mul;
    }

    /// <summary>앞키(바라보는 방향과 같은 입력)일 때만 전진한다. 뒤키·무입력은 제자리.</summary>
    public static bool CanDrift(float inputDir, float facingDir)
    {
        return inputDir * facingDir > 0f;
    }

    /// <summary>판정이 나온 뒤(+여유)부터 후딜이 끝나기 전까지만 점프 캔슬·방향 전환이 된다. 타격 전에는 안 된다.</summary>
    public static bool CanJumpCancel(float elapsed, float total, float hitDelay)
    {
        return total > 0f && elapsed >= hitDelay + CancelHitMargin && elapsed < total;
    }

    /// <summary>후딜 중 이동 캔슬: 판정 뒤 + 후딜 40%(마무리 타 60%) 경과부터 후딜이 끝나기 전까지.</summary>
    public static bool CanMoveCancel(float elapsed, float total, float hitDelay, bool finisher)
    {
        if (total <= 0f)
        {
            return false;
        }
        float point = Mathf.Max(total * (finisher ? FinisherMoveCancelFraction : MoveCancelFraction), hitDelay + CancelHitMargin);
        return elapsed >= point && elapsed < total;
    }

    /// <summary>후딜 중 반대키로 몸 돌리기: 판정 뒤부터, 마무리 타는 돌지 않는다.</summary>
    public static bool CanTurnInRecovery(float elapsed, float total, float hitDelay, bool finisher)
    {
        return !finisher && CanJumpCancel(elapsed, total, hitDelay);
    }

    /// <summary>스킬이 막힌 이유. 우선순위: 쿨다운 → 스태미나 → 동작 중. 스태미나는 기존 규칙대로 '초과'여야 한다(같으면 부족).</summary>
    public static SkillBlock GetSkillBlock(bool cooling, int stamina, int cost, bool timingOk)
    {
        if (cooling)
        {
            return SkillBlock.Cooldown;
        }
        if (stamina <= cost)
        {
            return SkillBlock.Stamina;
        }
        return timingOk ? SkillBlock.None : SkillBlock.Busy;
    }

    // ── 경직: 연쇄 감소와 몹별 저항 ──
    /// <summary>이 시간 안에 이어 맞으면 연쇄로 본다(초).</summary>
    public const float StaggerChainWindow = 1.5f;
    static readonly float[] ChainScale = { 1f, 0.75f, 0.5f, 0.35f };
    public const float BossResist = 0.6f, SturdyResist = 0.5f, EliteResist = 0.3f;

    /// <summary>다음 연쇄 번호(1부터). 마지막 경직 후 창을 넘기면 1로 돌아간다.</summary>
    public static int NextChain(int chain, float sinceLast)
    {
        return chain <= 0 || sinceLast > StaggerChainWindow ? 1 : chain + 1;
    }

    /// <summary>경직 시간 배율 = 연쇄 배율(1, .75, .5, .35) × (1 - 저항). 연속 경직(스턴락) 방지.</summary>
    public static float StiffnessScale(int chain, float resist)
    {
        int idx = Mathf.Clamp(chain - 1, 0, ChainScale.Length - 1);
        return ChainScale[idx] * (1f - Mathf.Clamp01(resist));
    }

    /// <summary>몹 경직 저항(0~1). 보스·튼튼한 몹(trait sturdy)·정예 중 가장 큰 값. 슈퍼아머 몹은 경직 자체가 없다.</summary>
    public static float StaggerResist(bool boss, bool sturdy, bool elite)
    {
        float r = 0f;
        if (boss)
        {
            r = Mathf.Max(r, BossResist);
        }
        if (sturdy)
        {
            r = Mathf.Max(r, SturdyResist);
        }
        if (elite)
        {
            r = Mathf.Max(r, EliteResist);
        }
        return r;
    }

    // ── 공중 콤보(띄우기) ──
    /// <summary>공중 피격 중 중력 배율: 첫 공중 타 0.35, 타마다 +0.1(7타쯤 1.0). 무한 띄우기 방지.</summary>
    public const float AirGravityBase = 0.35f, AirGravityStep = 0.1f;
    /// <summary>공중 추가타가 주는 위쪽 속도(유닛/초): 4에서 타마다 0.7씩 줄어 0 이하는 0.</summary>
    public const float JuggleLiftBase = 4f, JuggleLiftStep = 0.7f;
    /// <summary>공중 추가타의 수평 넉백 배율(멀리 날아가 콤보가 끊기지 않게).</summary>
    public const float JuggleXMul = 0.6f;
    /// <summary>다운 시간과 다운 공격(↓+X) 피해 배율, 공중으로 인정하는 최소 체공(초).</summary>
    public const float DownTime = 0.8f, DownAttackMul = 1.3f, MinAirTime = 0.15f;
    /// <summary>이 위쪽 속도 이상으로 띄우면 '띄움'으로 본다(착지하면 다운).</summary>
    public const float LaunchSpeed = 3f;
    /// <summary>다운 대기가 무한히 길어지지 않게 하는 안전 한도(초).</summary>
    public const float MaxRecoverWait = 3f;

    public static float AirGravityMul(int airHits)
    {
        return Mathf.Clamp01(AirGravityBase + AirGravityStep * Mathf.Max(0, airHits - 1));
    }

    public static float JuggleLift(int airHits)
    {
        return Mathf.Max(0f, JuggleLiftBase - JuggleLiftStep * Mathf.Max(0, airHits - 1));
    }

    /// <summary>아티팩트 범위 피해 넉백 방향(11-02): 완전 횡스크롤이라 띄우지 않고 수평으로만 민다. 좌우는 EnemyHealth 가 맞은 쪽 기준으로 뒤집는다.</summary>
    public static Vector2 AreaBurstKnockDir()
    {
        return Vector2.right;
    }

    /// <summary>이 넉백(방향 x 힘)이 몹을 띄우는가(위쪽 속도가 LaunchSpeed 이상).</summary>
    public static bool IsLaunch(Vector2 dir, float force)
    {
        return dir.sqrMagnitude > 0f && dir.normalized.y * force >= LaunchSpeed;
    }

    /// <summary>다운된 적을 ↓ 를 누른 채 때리면(다운 공격) 피해 ×1.3. 그 외 1.</summary>
    public static float DownDamageMul(bool isDown, bool downAttack)
    {
        return isDown && downAttack ? DownAttackMul : 1f;
    }
}

/// <summary>던파식 히트 수 집계: 2초 안에 이어 때리면 쌓이고, 끊기거나 맞으면 0. 시간은 인자로 받아 테스트한다.</summary>
public sealed class HitComboCounter
{
    public const float Window = 2f;
    float _last = float.NegativeInfinity;

    public int Count { get; private set; }
    public long TotalDamage { get; private set; }
    public int Best { get; private set; }

    public void Hit(float now, int damage)
    {
        if (Count > 0 && now - _last > Window)
        {
            Reset();
        }
        Count++;
        TotalDamage += Mathf.Max(0, damage);
        _last = now;
        Best = Mathf.Max(Best, Count);
    }

    public bool Active(float now)
    {
        return Count > 0 && now - _last <= Window;
    }

    /// <summary>남은 유지 시간 비율(0~1) — 게이지용</summary>
    public float Remaining01(float now)
    {
        return Count > 0 ? Mathf.Clamp01(1f - (now - _last) / Window) : 0f;
    }

    public void Reset()
    {
        Count = 0;
        TotalDamage = 0;
        _last = float.NegativeInfinity;
    }
}
