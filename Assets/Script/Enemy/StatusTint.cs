using UnityEngine;

/// <summary>
/// 상태이상 틴트: 독(초록)·출혈(붉은색) 효과 오브젝트가 자식에 있는 동안 몸 색이 은은하게 맥동한다.
/// 피격 플래시(셰이더 HITEFFECT)와 겹치지 않게 SpriteRenderer.color 를 쓴다. 효과가 끝나거나 풀로 돌아가면 원래 색으로.
/// 둔화·빙결 효과는 아직 게임에 없다(AssetIntegrationPlan 6절 3).
/// </summary>
public class StatusTint : MonoBehaviour
{
    static readonly Color PoisonColor = new Color(0.55f, 1f, 0.45f);
    static readonly Color BleedColor = new Color(1f, 0.45f, 0.45f);
    const float CheckInterval = 0.2f;

    private SpriteRenderer _sr;
    private Color _base;
    private bool _tinting;
    private float _nextCheck;
    private Color _target;

    public static void Ensure(GameObject enemy)
    {
        if (!enemy.TryGetComponent(out StatusTint tint))
        {
            tint = enemy.AddComponent<StatusTint>();
        }
        tint._nextCheck = 0f;
    }

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (_sr == null)
        {
            return;
        }
        if (Time.time >= _nextCheck)
        {
            _nextCheck = Time.time + CheckInterval;
            bool poison = GetComponentInChildren<PosionAttack>() != null;
            bool bleed = GetComponentInChildren<BleedingAttack>() != null;
            if (poison || bleed)
            {
                if (!_tinting)
                {
                    _base = _sr.color;
                    _tinting = true;
                }
                _target = poison ? PoisonColor : BleedColor;
            }
            else if (_tinting)
            {
                Restore();
            }
        }
        if (_tinting)
        {
            float k = 0.45f + 0.2f * Mathf.Sin(Time.time * 6f);
            Color c = Color.Lerp(_base, _base * _target, k);
            c.a = _base.a;
            _sr.color = c;
        }
    }

    void Restore()
    {
        _tinting = false;
        if (_sr != null)
        {
            _sr.color = _base;
        }
    }

    void OnDisable()
    {
        if (_tinting)
        {
            Restore();
        }
    }
}
