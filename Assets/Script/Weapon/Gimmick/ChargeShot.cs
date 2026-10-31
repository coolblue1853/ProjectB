using UnityEngine;

/// <summary>
/// 활 기믹 — 차지 샷. X 를 길게 누르면 충전, 놓으면 그 단계만큼 평타 화살 피해 배율·관통이 붙는다 (WeaponTypes.md 2.1).
/// 평타는 키를 놓을 때 나간다(짧게 눌러도 놓는 순간 쏜다). 평타 화살(StraightArrow)이 TryConsume 으로 받아 간다.
/// </summary>
public class ChargeShot : WeaponGimmick
{
    private static float s_validUntil;
    private static float s_mul = 1f;
    private static int s_pierce;

    private bool _holding;
    private float _holdStart;
    private int _pendingTier;
    private float _pendingAt = -100f;
    private const float PendingKeepTime = 1.0f;
    private static ChargeShot s_owner;
    private SpriteRenderer[] _tintTargets;
    private Color[] _baseColors;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_validUntil = 0f;
        s_mul = 1f;
        s_pierce = 0;
        s_owner = null;
    }

    /// <summary>지금 키를 누르고 충전 중인가</summary>
    public bool Holding => _holding;

    /// <summary>방금 놓은 충전 단계(다음 평타가 소비)</summary>
    public int ReleasedTier => _pendingTier;

    /// <summary>충전 시작 (평타 키를 누른 순간)</summary>
    public void BeginHold(float now)
    {
        // 놓은 단계(_pendingTier)는 건드리지 않는다 - 릴리스~발사 사이에 재입력해도 단계 유지
        _holding = true;
        _holdStart = now;
    }

    /// <summary>키를 놓았다 — 단계를 확정해 돌려준다</summary>
    public int EndHold(float now)
    {
        int tier = _holding ? WeaponGimmickRules.ChargeTier(now - _holdStart) : 0;
        _holding = false;
        RestoreTint();
        // 아직 발사되지 않은 더 높은 단계가 있으면 유지한다
        if (Time.time - _pendingAt > PendingKeepTime || tier > _pendingTier)
        {
            _pendingTier = tier;
        }
        _pendingAt = Time.time;
        return tier;
    }

    public void CancelHold()
    {
        _holding = false;
        _pendingTier = 0;
        RestoreTint();
    }

    public override void OnAttackStart(Weapon weapon, int step)
    {
        // 평타가 실제로 시작될 때 놓은 단계를 화살에 넘기고 비운다
        int tier = Time.time - _pendingAt <= PendingKeepTime ? _pendingTier : 0;
        s_mul = WeaponGimmickRules.ChargeDamageMul(tier);
        s_pierce = WeaponGimmickRules.ChargePierce(tier);
        // 유효 시간은 화살이 생성되는 시점 기준 - 선딜(delayTime)만큼 더 준다
        float preDelay = weapon != null && weapon.isHasDelayTime && weapon.delayTime != null && weapon.delayTime.Length >= step ? weapon.delayTime[step - 1] / StatMath.AttackSpeedMul : 0f;
        s_validUntil = Time.time + preDelay + WeaponGimmickRules.ChargeValidTime;
        s_owner = (s_mul > 1f || s_pierce > 0) ? this : null;
        GameLog.Info(GameLog.Cat.Combat, "차지 샷 단계 " + tier + " x" + s_mul.ToString("F2") + " 관통+" + s_pierce, this);
        _pendingTier = 0;
    }

    /// <summary>평타 화살이 생길 때 한 번 소비한다. 차지가 없으면 false(배율 1)</summary>
    public static bool TryConsume(out float damageMul, out int extraPierce)
    {
        if (Time.time <= s_validUntil && (s_mul > 1f || s_pierce > 0))
        {
            damageMul = s_mul;
            extraPierce = s_pierce;
            s_validUntil = 0f;
            s_mul = 1f;
            s_pierce = 0;
            return true;
        }
        damageMul = 1f;
        extraPierce = 0;
        return false;
    }

    private void Update()
    {
        if (!_holding)
        {
            return;
        }
        // 충전 표시: 무기 그림이 단계가 오를수록 노랗게 밝아진다
        Tint(WeaponGimmickRules.ChargeTier(Time.time - _holdStart));
    }

    private void Tint(int tier)
    {
        if (_tintTargets == null)
        {
            Weapon w = GetComponent<Weapon>();
            if (w == null || w.RightWeqponSprite == null)
            {
                return; // 아직 무기 그림이 없다 - 빈 배열로 고정하지 않고 다음에 다시 시도
            }
            _tintTargets = w.RightWeqponSprite.GetComponentsInChildren<SpriteRenderer>(true);
            _baseColors = new Color[_tintTargets.Length];
            for (int i = 0; i < _tintTargets.Length; i++)
            {
                _baseColors[i] = _tintTargets[i].color;
            }
        }
        for (int i = 0; i < _tintTargets.Length; i++)
        {
            if (_tintTargets[i] != null)
            {
                _tintTargets[i].color = tier <= 0 ? _baseColors[i] : Color.Lerp(_baseColors[i], new Color(1f, 0.9f, 0.4f, _baseColors[i].a), tier * 0.3f);
            }
        }
    }

    private void RestoreTint()
    {
        if (_tintTargets == null)
        {
            return;
        }
        for (int i = 0; i < _tintTargets.Length; i++)
        {
            if (_tintTargets[i] != null)
            {
                _tintTargets[i].color = _baseColors[i];
            }
        }
    }

    private void OnDisable()
    {
        _holding = false;
        RestoreTint();
        if (s_owner == this)
        {
            // 무기를 바꾸면 남은 충전을 버린다(다른 무기 화살이 소비하지 못하게)
            s_validUntil = 0f;
            s_mul = 1f;
            s_pierce = 0;
            s_owner = null;
        }
    }
}
