using UnityEngine;

/// <summary>
/// 가지뿔 여우 특성(trait=dash): 플레이어가 앞 2~5.5칸에 있으면 웅크렸다가(0.35초) 짧게 돌진(0.3초·약 3칸)한다. 쿨 3.5초.
/// 웅크림·돌진 동안 추적·순찰 이동은 멈춘다(EnemyHealth.MoveSpeedMul 이 0). 앞에 땅이 없거나 벽이면 돌진을 멈춘다.
/// 몸박 피해는 평소 그대로(EnemyContact). 그림은 TempMobMotion 이 CrouchAmount 로 낮춘다.
/// </summary>
public sealed class TempMobDash : MonoBehaviour
{
    enum State { Idle, Crouch, Dash }

    EnemyHealth _enemy;
    State _state;
    float _stateUntil, _readyAt, _dir;
    int _groundMask, _wallMask;

    /// <summary>추적·순찰 이동 배율: 웅크림·돌진 중 0</summary>
    public float ChaseMul => _state == State.Idle ? 1f : 0f;

    /// <summary>웅크린 정도 0~1(그림 낮추기용)</summary>
    public float CrouchAmount { get; private set; }

    public bool IsDashing => _state == State.Dash;

    void Awake()
    {
        _enemy = GetComponent<EnemyHealth>();
        _groundMask = LayerMask.GetMask("Ground");
        _wallMask = LayerMask.GetMask("Wall");
    }

    void OnEnable()
    {
        _state = State.Idle;
        CrouchAmount = 0f;
        _readyAt = Time.time + 0.3f; // 막 나타나자마자 달려들지 않는다
    }

    void Update()
    {
        if (_enemy == null || !_enemy.CanContact || _enemy.isStun)
        {
            _state = State.Idle;
            CrouchAmount = 0f;
            return;
        }
        switch (_state)
        {
            case State.Idle:
                TryStart();
                break;
            case State.Crouch:
                CrouchAmount = Mathf.MoveTowards(CrouchAmount, 1f, Time.deltaTime / 0.12f);
                if (Time.time >= _stateUntil)
                {
                    _state = State.Dash;
                    _stateUntil = Time.time + TempMobTraits.DashTime;
                }
                break;
            case State.Dash:
                CrouchAmount = Mathf.MoveTowards(CrouchAmount, 0f, Time.deltaTime / 0.1f);
                StepDash();
                break;
        }
    }

    void TryStart()
    {
        PlayerController player = PlayerController.instance;
        if (player == null || Time.time < _readyAt)
        {
            return;
        }
        Vector3 d = player.transform.position - transform.position;
        if (!TempMobTraits.DashInRange(d.x, d.y))
        {
            return;
        }
        _dir = Mathf.Sign(d.x);
        _state = State.Crouch;
        _stateUntil = Time.time + TempMobTraits.CrouchTime;
        _readyAt = Time.time + TempMobTraits.CrouchTime + TempMobTraits.DashTime + TempMobTraits.DashCooldown;
    }

    void StepDash()
    {
        float step = TempMobTraits.DashSpeed * Time.deltaTime;
        Vector2 pos = transform.position;
        bool groundAhead = Physics2D.Raycast(pos + new Vector2(_dir * 0.6f, 0.3f), Vector2.down, 1.6f, _groundMask);
        bool wallAhead = Physics2D.Raycast(pos + Vector2.up * 0.3f, new Vector2(_dir, 0f), 0.6f + step, _wallMask);
        if (Time.time >= _stateUntil || !groundAhead || wallAhead)
        {
            _state = State.Idle;
            return;
        }
        Vector3 p = transform.position;
        p.x += _dir * step;
        transform.position = p;
    }
}
