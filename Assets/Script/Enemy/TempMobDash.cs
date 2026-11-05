using DG.Tweening;
using UnityEngine;

/// <summary>
/// 가지뿔 여우 특성(trait=dash): 플레이어가 앞 2~5.5칸에 있으면 웅크렸다가(0.35초) 짧게 돌진(0.3초·약 3칸)한다. 쿨 3.5초.
/// 웅크림·돌진 동안 추적은 멈추고(EnemyHealth.MoveSpeedMul 이 0) 순찰 트윈(BehaviorTree.sequence)은 일시 정지한다. 앞에 땅이 없거나 벽이면 돌진을 멈춘다.
/// 몸박 피해는 평소 그대로(EnemyContact). 그림은 TempMobMotion 이 CrouchAmount 로 낮춘다.
/// </summary>
public sealed class TempMobDash : MonoBehaviour
{
    enum State { Idle, Crouch, Dash }

    private EnemyHealth _enemy;
    private BehaviorDesigner.Runtime.BehaviorTree _bt;
    private State _state;
    private Cooldown _stateUntil, _readyAt;
    private float _dir;
    private int _groundMask, _wallMask;

    /// <summary>추적·순찰 이동 배율: 웅크림·돌진 중 0</summary>
    public float ChaseMul => _state == State.Idle ? 1f : 0f;

    /// <summary>웅크린 정도 0~1(그림 낮추기용)</summary>
    public float CrouchAmount { get; private set; }

    public bool IsDashing => _state == State.Dash;

    void Awake()
    {
        _enemy = GetComponent<EnemyHealth>();
        _bt = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        _groundMask = LayerMask.GetMask("Ground");
        _wallMask = LayerMask.GetMask("Wall");
    }

    void OnEnable()
    {
        _state = State.Idle;
        _stateUntil = default; // 풀링 재사용 시 이전 대시 종료 시각이 남지 않게
        CrouchAmount = 0f;
        _readyAt.Arm(Time.time, 0.3f); // 막 나타나자마자 달려들지 않는다
    }

    void Update()
    {
        if (_enemy == null || !_enemy.CanContact || _enemy.isStun)
        {
            EndDash();
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
                if (_stateUntil.Ready(Time.time))
                {
                    _state = State.Dash;
                    _stateUntil.Arm(Time.time, TempMobTraits.DashTime);
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
        if (player == null || !_readyAt.Ready(Time.time))
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
        if (_bt != null && _bt.sequence != null)
        {
            _bt.sequence.Pause(); // 순찰 트윈이 돌진과 겹쳐 미끄러지지 않게
        }
        _stateUntil.Arm(Time.time, TempMobTraits.CrouchTime);
        _readyAt.Arm(Time.time, TempMobTraits.CrouchTime + TempMobTraits.DashTime + TempMobTraits.DashCooldown);
    }

    /// <summary>돌진 끝·중단: 멈춰 둔 순찰 트윈을 다시 돌린다(돌진으로 위치가 바뀌어도 순찰이 다음 지점에서 새로 잡는다)</summary>
    void EndDash()
    {
        if (_state != State.Idle)
        {
            if (_bt != null && _bt.sequence != null)
            {
                _bt.sequence.Play();
            }
        }
        _state = State.Idle;
    }

    void StepDash()
    {
        float step = TempMobTraits.DashSpeed * Time.deltaTime;
        Vector2 pos = transform.position;
        bool groundAhead = Physics2D.Raycast(pos + new Vector2(_dir * 0.6f, 0.3f), Vector2.down, 1.6f, _groundMask);
        bool wallAhead = Physics2D.Raycast(pos + Vector2.up * 0.3f, new Vector2(_dir, 0f), 0.6f + step, _wallMask);
        if (_stateUntil.Ready(Time.time) || !groundAhead || wallAhead)
        {
            EndDash();
            return;
        }
        Vector3 p = transform.position;
        p.x += _dir * step;
        transform.position = p;
    }
}
