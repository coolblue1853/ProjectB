using UnityEngine;

/// <summary>공격 중 이동 모드(CombatInputRedesign §3.1). 무기 타입 화이트리스트(WeaponMoveProfiles)에 있는 무기만 쓴다.</summary>
public enum AttackMoveMode
{
    /// <summary>앞키 무시 — 가로 속도 0 고정</summary>
    InPlace = 0,
    /// <summary>앞키를 누르는 동안만 모션 시작 후 LungeTime 동안 고정 거리 전진</summary>
    LungeOnForward = 1,
    /// <summary>후딜 중 이동 가능(현행 경로 유지)</summary>
    Free = 2,
}

/// <summary>무기 무게 등급 — 이동 캔슬·점프 캔슬·평타 선행 소비 시점이 등급별로 다르다(§3.4·§5.2)</summary>
public enum MoveWeightClass { Light = 0, Standard = 1, Heavy = 2, Cast = 3, Free = 4 }

/// <summary>
/// 공격 중 이동 규칙 순수 함수(CombatInputRedesign CI-P0). 수치는 전부 임시값(Q-CI1~8) — 상수 한 곳에서 조정한다.
/// </summary>
public static class AttackMoveRules
{
    // ── Lunge (§3.3) ──
    public const float LungeExtraAfterHit = 0.12f, LungeTimeMin = 0.12f, LungeTimeMax = 0.30f;
    /// <summary>속도 곡선: 앞 60% 일정, 뒤 40% 선형 감속</summary>
    public const float LungeConstFraction = 0.6f;

    /// <summary>전진 시간 = clamp(hitDelay + 0.12, 0.12, 0.30)초</summary>
    public static float LungeTime(float hitDelay)
    {
        return Mathf.Clamp(hitDelay + LungeExtraAfterHit, LungeTimeMin, LungeTimeMax);
    }

    /// <summary>곡선 적분(= 0.6T + 0.4T/2 = 0.8T)이 LungeDist 와 같도록 한 최고 속도(u/s). 설계서 예시 속도(dist/T)는 감속을 무시한 값이라 총 이동이 모자란다.</summary>
    public static float LungePeakSpeed(float dist, float lungeTime)
    {
        return lungeTime > 0f ? dist / (lungeTime * (LungeConstFraction + (1f - LungeConstFraction) * 0.5f)) : 0f;
    }

    /// <summary>
    /// 모션 시작 후 elapsed 초의 가로 전진 속도(크기, 방향은 호출 쪽). 앞키를 안 누르거나 모드·거리가 0 이거나
    /// LungeTime 이 지나면 0 — 마지막 프레임 0 이 보장된다(§8).
    /// </summary>
    public static float LungeVelocity(AttackMoveMode mode, float dist, float elapsed, float hitDelay, bool forwardHeld)
    {
        if (mode != AttackMoveMode.LungeOnForward || !forwardHeld || dist <= 0f || elapsed < 0f)
        {
            return 0f;
        }
        float time = LungeTime(hitDelay);
        if (elapsed >= time)
        {
            return 0f;
        }
        float peak = LungePeakSpeed(dist, time);
        float t = elapsed / time;
        if (t <= LungeConstFraction)
        {
            return peak;
        }
        return peak * Mathf.Clamp01((1f - t) / (1f - LungeConstFraction));
    }

    // ── 이동·점프 캔슬 (§3.4) ──
    /// <summary>등급별 이동 캔슬 시작 후딜 비율(층계 배율 이전의 기준값, 기본 칼 = Standard 0.40 = 현행)</summary>
    public static float MoveCancelFraction(MoveWeightClass cls)
    {
        switch (cls)
        {
            case MoveWeightClass.Light: return 0.30f;
            case MoveWeightClass.Heavy: return 0.55f;
            case MoveWeightClass.Cast: return 0.50f;
            default: return 0.40f;
        }
    }

    /// <summary>등급별 점프 캔슬 여유(판정 뒤 초). 중량만 0.10</summary>
    public static float JumpCancelMargin(MoveWeightClass cls)
    {
        return cls == MoveWeightClass.Heavy ? 0.10f : CombatRules.CancelHitMargin;
    }

    public static bool CanJumpCancel(MoveWeightClass cls, float elapsed, float total, float hitDelay)
    {
        return total > 0f && elapsed >= hitDelay + JumpCancelMargin(cls) && elapsed < total;
    }

    /// <summary>이동 캔슬: 층계 비율(MovementUnlockRules.CancelStartRatio)에 등급 비율을 배율로 곱한다(CombatRules.WeaponCancelFraction 재사용)</summary>
    public static bool CanMoveCancel(MoveWeightClass cls, float floorRatio, float elapsed, float total, float hitDelay, bool finisher)
    {
        float frac = CombatRules.WeaponCancelFraction(floorRatio, MoveCancelFraction(cls));
        return CombatRules.CanMoveCancel(elapsed, total, hitDelay, finisher, frac);
    }

    // ── 평타→평타 선행 소비 (§5.2, CI-P7 에서 연결) ──
    /// <summary>다음 타를 소비할 수 있는 후딜 경과 비율. 경량 0.75·표준 0.90·그 외 1.0</summary>
    public static float ComboConsumeFraction(MoveWeightClass cls)
    {
        switch (cls)
        {
            case MoveWeightClass.Light: return 0.75f;
            case MoveWeightClass.Standard: return 0.90f;
            default: return 1f;
        }
    }

    // ── 공중 (§3.5) ──
    public const float InPlaceAirInputMul = 0.3f;

    /// <summary>InPlace 무기의 공중 평타 입력 반영 배율(나머지는 CombatRules.AirAttackInputMul 0.7)</summary>
    public static float AirInputMul(AttackMoveMode mode)
    {
        return mode == AttackMoveMode.InPlace ? InPlaceAirInputMul : CombatRules.AirAttackInputMul;
    }

    /// <summary>CombatRules.AirAttackVx 와 같되 입력 반영 배율을 모드별로 받는다</summary>
    public static float AirAttackVx(AttackMoveMode mode, float currentVx, float inputDir, float maxSpeed, float dt)
    {
        if (Mathf.Abs(inputDir) > 0.01f)
        {
            return Mathf.Sign(inputDir) * maxSpeed * AirInputMul(mode);
        }
        return Mathf.MoveTowards(currentVx, 0f, maxSpeed / CombatRules.AirAttackDecelTime * dt);
    }
}
