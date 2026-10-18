using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

/// <summary>
/// 타격감 연출 단일 진입점 (설계: .claude-rules/Game/GameFeelPlan.md).
/// 적중 정보를 받아 플래시·국소 히트스톱·카메라 흔들림을 낸다.
/// 많이 때리는 게임이므로 흔들림은 0.1초 창에서 가장 센 1회만 낸다.
/// </summary>
public static class HitFeedback
{
    public enum ShakeTier { None = 0, Light = 1, Medium = 2, Heavy = 3, Impact = 4, PlayerHit = 5 }

    /// <summary>접근성 옵션: 흔들림 배율(0~1), 플래시 사용 여부</summary>
    public static float ShakeScale = 1f;
    public static bool FlashEnabled = true;

    // 튜닝 값 (GameFeelPlan 4~5절)
    const float FlashBasic = 0.06f, FlashSkill = 0.08f, FlashCritBonus = 0.02f;
    const float StopBasic = 0.04f, StopSkill = 0.06f, StopCritBonus = 0.03f;
    const float ShakeWindow = 0.10f;

    static float shakeWindowEnd;
    static ShakeTier shakeInWindow;
    static ProCamera2DShake cachedShake;

    /// <summary>디버그·프로브용 카운터</summary>
    public static int ShakeCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        shakeWindowEnd = 0f;
        shakeInWindow = ShakeTier.None;
        cachedShake = null;
        ShakeCount = 0;
    }

    /// <summary>플레이어 공격이 몬스터에 맞았을 때</summary>
    public static void OnEnemyHit(EnemyHealth target, bool isCrit, bool isSkill, bool killed)
    {
        if (target == null)
        {
            return;
        }
        SpriteHitFx fx = GetFx(target);
        if (fx != null && !killed)
        {
            float flash = (isSkill ? FlashSkill : FlashBasic) + (isCrit ? FlashCritBonus : 0f);
            float stop = (isSkill ? StopSkill : StopBasic) + (isCrit ? StopCritBonus : 0f);
            fx.Flash(flash, target.isBossMob ? 0.6f : 1f, isCrit ? 3f : 1f);
            fx.HitStop(stop);
        }

        ShakeTier tier = killed || isCrit ? ShakeTier.Heavy : isSkill ? ShakeTier.Medium : ShakeTier.Light;
        if (killed && target.isBossMob)
        {
            tier = ShakeTier.Impact;
        }
        Shake(tier);
    }

    /// <summary>독·출혈 틱: 약한 색 펄스만</summary>
    public static void OnDotTick(EnemyHealth target, bool isBleeding)
    {
        if (target == null)
        {
            return;
        }
        SpriteHitFx fx = GetFx(target);
        if (fx != null)
        {
            fx.Flash(0.04f, 0.35f, 1f, isBleeding ? new Color(1f, 0.15f, 0.15f) : new Color(0.3f, 1f, 0.3f));
        }
    }

    public static void OnPlayerHit()
    {
        Shake(ShakeTier.PlayerHit);
    }

    public static void Shake(ShakeTier tier)
    {
        if (tier == ShakeTier.None || ShakeScale <= 0f)
        {
            return;
        }
        float now = Time.time;
        if (now < shakeWindowEnd && tier <= shakeInWindow)
        {
            return; // 창 안에서 같거나 약한 흔들림은 버린다
        }
        shakeWindowEnd = now + ShakeWindow;
        shakeInWindow = tier;

        float duration;
        Vector2 strength;
        int vibrato;
        switch (tier)
        {
            case ShakeTier.Light: duration = 0.06f; strength = new Vector2(0.04f, 0.03f); vibrato = 2; break;
            case ShakeTier.Medium: duration = 0.10f; strength = new Vector2(0.09f, 0.07f); vibrato = 4; break;
            case ShakeTier.Heavy: duration = 0.14f; strength = new Vector2(0.16f, 0.12f); vibrato = 5; break;
            case ShakeTier.Impact: duration = 0.25f; strength = new Vector2(0.30f, 0.25f); vibrato = 7; break;
            default: duration = 0.12f; strength = new Vector2(0.10f, 0.10f); vibrato = 3; break;
        }
        if (cachedShake == null)
        {
            cachedShake = Object.FindFirstObjectByType<ProCamera2DShake>();
            if (cachedShake == null)
            {
                return;
            }
        }
        cachedShake.Shake(duration, strength * ShakeScale, vibrato, 0.3f, -1f, Vector3.zero, 0.05f);
        ShakeCount++;
    }

    static SpriteHitFx GetFx(EnemyHealth target)
    {
        if (!target.TryGetComponent(out SpriteHitFx fx))
        {
            fx = target.gameObject.AddComponent<SpriteHitFx>();
        }
        return fx;
    }
}
