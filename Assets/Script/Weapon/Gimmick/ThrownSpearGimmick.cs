using UnityEngine;

/// <summary>
/// 투창 기믹 — ThrownSpear 래퍼(WeaponTypes.md 2.10). 평타로 던지면 Throw, 일정 시간 뒤 Land,
/// 착지 시 SpearTrap(판정 전용)을 만든다. 박힌 창 스프라이트는 그래픽 후속(임시 텍스트).
/// </summary>
public class ThrownSpearGimmick : WeaponGimmick
{
    [SerializeField] private float _throwRange = 6f;   // 최대 사거리
    [SerializeField] private float _throwSpeed = 18f;  // 비행 속도(유닛/초)
    [SerializeField] private float _trapDuration = 4f;       // 착지 함정 유지 시간(초)
    [SerializeField] private float _trapInterval = 0.5f;     // 틱 간격(초)
    [SerializeField] private float _trapDamageRatio = 0.3f;  // 틱 피해 = 무기 평균 공격력 × 비율
    [SerializeField] private float _trapRadius = 1f;         // 판정 반경

    private static bool s_layerWarned; // Ground/Wall 레이어 누락 경고를 한 번만
    private int _groundWallMask;
    private float _throwDir = 1f;
    private float _throwDist;
    private int _trapTickDamage;

    private readonly ThrownSpear _spear = new ThrownSpear();
    private float _flight;

    public ThrownSpear Spear => _spear;
    public string HudText => _spear.Current == ThrownSpear.State.InHand ? Loc.T("Spear held") : _spear.Current == ThrownSpear.State.InFlight ? Loc.T("Spear in flight") : (_spear.TrapActive ? Loc.T("Spear planted (trap)") : Loc.T("Spear planted"));

    private void Awake()
    {
        _groundWallMask = LayerMask.GetMask("Ground", "Wall");
        if (!s_layerWarned && (LayerMask.NameToLayer("Ground") < 0 || LayerMask.NameToLayer("Wall") < 0))
        {
            s_layerWarned = true;
            Debug.LogWarning("[ThrownSpearGimmick] Ground/Wall 레이어가 없어 투창이 벽을 뚫는다.");
        }
    }

    private void Update()
    {
        _spear.Tick(Time.deltaTime);
        if (_spear.Current == ThrownSpear.State.InFlight)
        {
            _flight -= Time.deltaTime;
            if (_flight <= 0f)
            {
                _spear.Land();
                SpawnTrap();
            }
        }
    }

    public override void OnAttackStart(Weapon weapon, int step)
    {
        if (_spear.Throw())
        {
            _throwDist = ClearDistance();
            _flight = ThrownSpear.FlightTime(_throwDist, _throwRange, _throwSpeed);
            _throwDir = transform.root.localScale.x < 0f ? -1f : 1f;
            int[] dmg = weapon != null ? weapon.damgeArray : null;
            float avg = dmg != null && dmg.Length > 1 ? (dmg[0] + dmg[1]) * 0.5f : 0f;
            _trapTickDamage = Mathf.Max(1, Mathf.RoundToInt(avg * _trapDamageRatio));
        }
    }

    /// <summary>착지 지점(던진 위치 + 방향 × min(막힘 거리, 사거리))에 판정 전용 함정 생성. 지면이 아래에 있으면 높이를 맞춘다</summary>
    private void SpawnTrap()
    {
        Vector3 pos = transform.root.position + new Vector3(_throwDir * Mathf.Min(_throwDist, _throwRange), 0f, 0f);
        RaycastHit2D down = Physics2D.Raycast(pos, Vector2.down, 6f, _groundWallMask);
        if (down.collider != null)
        {
            pos.y = down.point.y;
        }
        SpearTrap.Spawn(pos, _trapRadius, _trapTickDamage, _trapDuration, _trapInterval);
    }

    /// <summary>던지는 방향으로 Ground/Wall 에 막힐 때까지의 거리(없으면 사거리)</summary>
    private float ClearDistance()
    {
        Transform player = transform.root;
        Vector2 dir = player.localScale.x < 0f ? Vector2.left : Vector2.right;
                RaycastHit2D hit = Physics2D.Raycast(player.position, dir, _throwRange, _groundWallMask);
        return hit.collider != null ? hit.distance : _throwRange;
    }

    /// <summary>줍기·소환 회수</summary>
    public bool Retrieve()
    {
        return _spear.Retrieve();
    }

    private void OnGUI()
    {
        GimmickHud.Draw(HudText); // 임시 HUD(그래픽 보류, 확정 시 교체)
    }
}
