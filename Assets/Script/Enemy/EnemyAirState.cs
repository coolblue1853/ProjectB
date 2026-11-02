using UnityEngine;

/// <summary>
/// 몹 공중 콤보·다운 상태(던파식, CombatSpec 9절). EnemyHealth 가 피격 때 알려주고, 이 컴포넌트가 중력·착지·다운을 관리한다.
/// - 공중에서 맞으면 중력을 줄여(타마다 점점 원래대로) 추가타가 이어지게 하고, 위로 살짝 띄운다.
/// - 띄워진 몹은 경직이 끝나도 착지할 때까지 행동 트리를 되살리지 않고, 착지하면 0.8초 다운(누워 있음).
/// 중력 0(비행) 몹과 슈퍼아머 몹은 건드리지 않는다. 풀 재사용 때 OnDisable 에서 모두 원복.
/// </summary>
[DisallowMultipleComponent]
public class EnemyAirState : MonoBehaviour
{
    EnemyHealth _enemy;
    Rigidbody2D _rb;
    Collider2D _col;
    float _baseGravity = -1f;
    int _airHits;
    bool _launched;       // 띄워진 적 — 착지하면 다운
    float _airSince = -1f;
    float _downUntil = -1f;
    float _recoverDeadline = -1f;
    System.Action _recover; // 경직이 끝났지만 착지·다운 때문에 미룬 복귀
    static int s_groundMask = -1;

    /// <summary>지금 다운(누워 있는) 상태인가</summary>
    public bool IsDown => _downUntil > Time.time;
    /// <summary>공중 피격으로 중력이 줄어든 상태인가</summary>
    public bool Juggling => _baseGravity >= 0f && _rb != null && !Mathf.Approximately(_rb.gravityScale, _baseGravity);
    public int AirHits => _airHits;
    public bool Launched => _launched;
    /// <summary>디버그·테스트용: 지금 땅에 닿아 있는가</summary>
    public bool Grounded => IsGrounded();

    void Awake()
    {
        Cache();
    }

    void Cache()
    {
        if (_enemy == null)
        {
            _enemy = GetComponent<EnemyHealth>();
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
        }
        if (_rb != null && _baseGravity < 0f)
        {
            _baseGravity = _rb.gravityScale;
        }
    }

    bool IsGrounded()
    {
        if (_rb == null)
        {
            return true;
        }
        if (s_groundMask < 0)
        {
            s_groundMask = LayerMask.GetMask("Ground");
        }
        Bounds b = _col != null ? _col.bounds : new Bounds(transform.position, Vector3.one * 0.5f);
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.05f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.2f, s_groundMask);
        return hit.collider != null && _rb.linearVelocity.y <= 0.05f;
    }

    /// <summary>
    /// 피격 직전 판정: 이 타격이 공중 추가타인가(중력 있는 몹이 땅에서 떨어져 있음). 맞으면 타수를 올리고 중력을 줄인다.
    /// 지상 타격이면 타수를 0으로 되돌린다. 반환 = 공중 추가타 여부.
    /// </summary>
    public bool RegisterHit()
    {
        Cache();
        if (_rb == null || _baseGravity <= 0f)
        {
            return false;
        }
        bool air = !IsGrounded();
        if (air)
        {
            _airHits++;
            _rb.gravityScale = _baseGravity * CombatRules.AirGravityMul(_airHits);
            _launched = true;
            if (_airSince < 0f)
            {
                _airSince = Time.time;
            }
        }
        else
        {
            _airHits = 0;
        }
        return air;
    }

    /// <summary>넉백으로 위로 띄워졌다면(속도 기준) 띄움으로 표시 — 착지하면 다운. EnemyHealth.KnockbackActive 가 부른다.</summary>
    public void NoteKnockVelocity(Vector2 velocity)
    {
        if (velocity.y >= CombatRules.LaunchSpeed && _baseGravity > 0f)
        {
            _launched = true;
            _airSince = _airSince < 0f ? Time.time : _airSince;
        }
    }

    /// <summary>공중 추가타의 위쪽 속도·수평 감쇠를 적용(넉백 직후 호출).</summary>
    public void ApplyJuggle()
    {
        if (_rb == null || _airHits <= 0)
        {
            return;
        }
        Vector2 v = _rb.linearVelocity;
        v.x *= CombatRules.JuggleXMul;
        v.y = Mathf.Max(v.y, CombatRules.JuggleLift(_airHits));
        _rb.linearVelocity = v;
    }

    /// <summary>
    /// 경직이 끝났을 때 EnemyHealth 가 부른다. 띄워진 채 공중이거나 다운 중이면 복귀(recover)를 미루고 true.
    /// 착지 + 다운 시간 뒤(또는 안전 한도 3초) 에 recover 를 부른다.
    /// </summary>
    public bool TryDeferRecovery(System.Action recover)
    {
        Cache();
        if (!_launched || _baseGravity <= 0f)
        {
            RestoreGravity();
            return false;
        }
        if (IsGrounded() && !IsDown && _airSince < 0f)
        {
            RestoreGravity();
            return false;
        }
        _recover = recover;
        _recoverDeadline = Time.time + CombatRules.MaxRecoverWait;
        return true;
    }

    void Update()
    {
        if (_baseGravity < 0f)
        {
            return;
        }
        if (_launched && _airSince >= 0f && Time.time - _airSince >= CombatRules.MinAirTime && IsGrounded())
        {
            // 착지: 중력 원복 후 다운
            _airSince = -1f;
            _airHits = 0;
            RestoreGravity();
            _downUntil = Time.time + CombatRules.DownTime;
            GameLog.Info(GameLog.Cat.Combat, name + " 다운 " + CombatRules.DownTime + "초", this);
        }
        if (_recover != null)
        {
            bool landed = _airSince < 0f && !IsDown;
            if (landed || Time.time >= _recoverDeadline)
            {
                System.Action r = _recover;
                _recover = null;
                Clear();
                r();
            }
        }
        else if (_launched && _airSince < 0f && !IsDown)
        {
            Clear(); // 다운이 끝나면 띄움 표시 해제
        }
    }

    /// <summary>다운 중 다운 공격을 맞으면 누워 있는 시간을 늘린다(맞을 때마다 0.3초, 상한은 복귀 안전 한도 3초).</summary>
    public void ExtendDown(float seconds)
    {
        if (IsDown)
        {
            _downUntil += seconds;
        }
    }

    void RestoreGravity()
    {
        if (_rb != null && _baseGravity >= 0f)
        {
            _rb.gravityScale = _baseGravity;
        }
    }

    void Clear()
    {
        _launched = false;
        _airSince = -1f;
        _airHits = 0;
        _downUntil = -1f;
        RestoreGravity();
    }

    void OnDisable()
    {
        _recover = null;
        Clear();
    }
}
