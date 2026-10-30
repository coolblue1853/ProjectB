using UnityEngine;

/// <summary>
/// 풍향 활공 순수 규칙(FloorCoreContent.md 2절). 상태 없음 — GlideMotor 가 매 물리 프레임 호출하고 EditMode 테스트가 직접 검증한다.
/// 수직 기류는 "그 값이 수직 속도"(활공 중) / "낙하 속도에 더해진다"(활공 아님)로 해석한다.
/// </summary>
public static class GlideRules
{
    public const float HoldToStart = 0.2f;
    public const float NormalFall = -5f;
    public const float GlideFall = -1.3f;
    public const float HorizontalMul = 0.85f;
    public const float MaxDuration = 12f;
    public const float RegenPerSecond = 3f;
    public const float ExitDecay = 0.4f;
    public const float WindWeak = 1.8f;
    public const float WindStrong = 3.5f;
    public const float WindDown = -3.0f;
    public const float WindSide = 1.5f;
    public const float LoadPenaltyMin = 0.10f;
    public const float LoadPenaltyMax = 0.20f;
    public const int LoadPenaltyFullDeficit = 50;

    /// <summary>활공 시작 조건: 공중·하강 중·점프 키를 0.2초 이상 누르고 있음·막힌 상태 아님</summary>
    public static bool CanStart(float heldSeconds, bool airborne, float velocityY, bool blocked, float remaining)
    {
        return !blocked && airborne && velocityY <= 0.01f && heldSeconds >= HoldToStart && remaining > 0f;
    }

    /// <summary>상승 부하 부족분에 따른 지속 시간 배율(부족 0 = 1, 부족 있으면 0.90 → 0.80)</summary>
    public static float DurationMul(int deficit)
    {
        if (deficit <= 0)
        {
            return 1f;
        }
        float t = Mathf.Clamp01(deficit / (float)LoadPenaltyFullDeficit);
        return 1f - Mathf.Lerp(LoadPenaltyMin, LoadPenaltyMax, t);
    }

    public static float MaxDurationFor(int deficit)
    {
        return MaxDuration * DurationMul(deficit);
    }

    /// <summary>남은 활공 시간 갱신. 활공 중 −1/s, 상승 기류 안 +3/s(둘 다면 합산), 상한 max</summary>
    public static float TickRemaining(float remaining, float dt, bool gliding, bool inUpdraft, float max)
    {
        float delta = (gliding ? -1f : 0f) + (inUpdraft ? RegenPerSecond : 0f);
        return Mathf.Clamp(remaining + delta * dt, 0f, max);
    }

    /// <summary>
    /// 수직 속도 결정. windVy = 기류 안이면 그 기류의 수직 속도(상승 +, 하강 −), hasWind 가 거짓이면 무시.
    /// exitRatio = 기류에서 막 벗어난 뒤 남은 감쇠 비율(1 → 0, 0.4초), lastUpVy = 벗어나기 직전 상승 속도.
    /// </summary>
    public static float VerticalSpeed(float vy, bool gliding, bool hasWind, float windVy, float exitRatio, float lastUpVy)
    {
        if (hasWind)
        {
            if (gliding)
            {
                return windVy;
            }
            if (windVy > 0f)
            {
                return Mathf.Max(vy, NormalFall + windVy);
            }
            return vy > 0f ? vy : Mathf.Max(vy + windVy * 0.2f, NormalFall + windVy);
        }
        float v = vy;
        if (gliding)
        {
            v = Mathf.Max(v, GlideFall);
        }
        if (lastUpVy > 0f && exitRatio > 0f)
        {
            v = Mathf.Max(v, lastUpVy * exitRatio);
        }
        return v;
    }

    /// <summary>수평 속도: 활공 중이면 85%, 순풍·역풍(부호 = 월드 방향)을 더한다</summary>
    public static float HorizontalSpeed(float vx, bool gliding, float windX)
    {
        return (gliding ? vx * HorizontalMul : vx) + (gliding ? windX : 0f);
    }

    public static float WindVy(AirflowKind kind)
    {
        switch (kind)
        {
            case AirflowKind.UpWeak: return WindWeak;
            case AirflowKind.UpStrong: return WindStrong;
            case AirflowKind.Down: return WindDown;
            default: return 0f;
        }
    }

    public static float WindVx(AirflowKind kind)
    {
        switch (kind)
        {
            case AirflowKind.Tail: return WindSide;
            case AirflowKind.Head: return -WindSide;
            default: return 0f;
        }
    }
}

public enum AirflowKind { UpWeak, UpStrong, Down, Tail, Head }
