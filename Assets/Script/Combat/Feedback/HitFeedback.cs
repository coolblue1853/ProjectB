using Com.LuisPedroFonseca.ProCamera2D;
using DamageNumbersPro;
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

    public enum NumberKind { Normal, Crit, Extra, Poison, Bleed }

    const int MaxCritNumbersPerFrame = 4;
    static int critFrame = -1, critCountThisFrame;

    /// <summary>
    /// 데미지 숫자 스폰을 한 곳으로 모은다. 종류별로 색·크기를 매번 명시한다(풀링 재사용 시 이전 색이 남지 않도록).
    /// 치명타는 프레임당 4개까지만 강조하고 나머지는 일반 숫자로 낸다(가독성).
    /// </summary>
    public static void SpawnNumber(DamageNumber prefab, Vector3 position, int value, NumberKind kind)
    {
        if (prefab == null || value <= 0)
        {
            return;
        }
        if (kind == NumberKind.Crit)
        {
            if (critFrame != Time.frameCount)
            {
                critFrame = Time.frameCount;
                critCountThisFrame = 0;
            }
            if (++critCountThisFrame > MaxCritNumbersPerFrame)
            {
                kind = NumberKind.Normal;
            }
        }

        float baseScale = prefab.transform.localScale.x;
        Color color;
        float scale;
        Vector3 offset = Vector3.zero;
        switch (kind)
        {
            case NumberKind.Crit: color = new Color(1f, 0.55f, 0.1f); scale = 1.45f; break;
            case NumberKind.Extra: color = new Color(0.45f, 0.85f, 1f); scale = 0.8f; offset = new Vector3(0.15f, 0.25f, 0f); break;
            case NumberKind.Poison: color = new Color(0.45f, 1f, 0.35f); scale = 0.7f; break;
            case NumberKind.Bleed: color = new Color(0.95f, 0.15f, 0.2f); scale = 0.7f; break;
            default: color = Color.white; scale = 1f; break;
        }
        DamageNumber number = prefab.Spawn(position + offset, value);
        if (number != null)
        {
            number.SetColor(color);
            number.SetScale(baseScale * scale);
        }
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
