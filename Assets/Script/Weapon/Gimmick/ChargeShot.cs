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
    private int _releasedTier;
    private SpriteRenderer[] _tintTargets;
    private Color[] _baseColors;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_validUntil = 0f;
        s_mul = 1f;
        s_pierce = 0;
    }

    /// <summary>지금 키를 누르고 충전 중인가</summary>
    public bool Holding => _holding;

    /// <summary>방금 놓은 충전 단계(다음 평타가 소비)</summary>
    public int ReleasedTier => _releasedTier;

    /// <summary>충전 시작 (평타 키를 누른 순간)</summary>
    public void BeginHold(float now)
    {
        _holding = true;
        _holdStart = now;
        _releasedTier = 0;
    }

    /// <summary>키를 놓았다 — 단계를 확정해 돌려준다</summary>
    public int EndHold(float now)
    {
        _releasedTier = _holding ? WeaponGimmickRules.ChargeTier(now - _holdStart) : 0;
        _holding = false;
        RestoreTint();
        return _releasedTier;
    }

    public void CancelHold()
    {
        _holding = false;
        _releasedTier = 0;
        RestoreTint();
    }

    public override void OnAttackStart(Weapon weapon, int step)
    {
        // 평타가 실제로 시작될 때 놓은 단계를 화살에 넘기고 비운다
        s_mul = WeaponGimmickRules.ChargeDamageMul(_releasedTier);
        s_pierce = WeaponGimmickRules.ChargePierce(_releasedTier);
        s_validUntil = Time.time + WeaponGimmickRules.ChargeValidTime;
        GameLog.Info(GameLog.Cat.Combat, "차지 샷 단계 " + _releasedTier + " x" + s_mul.ToString("F2") + " 관통+" + s_pierce, this);
        _releasedTier = 0;
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
            _tintTargets = w != null && w.RightWeqponSprite != null ? w.RightWeqponSprite.GetComponentsInChildren<SpriteRenderer>(true) : new SpriteRenderer[0];
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
    }
}
