using UnityEngine;

/// <summary>
/// 투창 기믹 — ThrownSpear 래퍼(WeaponTypes.md 2.10). 평타로 던지면 Throw, 일정 시간 뒤 Land,
/// 박힌 창은 TrapDuration 동안 함정, Retrieve 로 회수. 실제 박힘 오브젝트·함정 판정은 후속(그래픽 보류, 임시 텍스트).
/// </summary>
public class ThrownSpearGimmick : WeaponGimmick
{
    [SerializeField] private float _throwRange = 6f;   // 최대 사거리
    [SerializeField] private float _throwSpeed = 18f;  // 비행 속도(유닛/초)

    private readonly ThrownSpear _spear = new ThrownSpear();
    private float _flight;

    public ThrownSpear Spear => _spear;
    public string HudText => _spear.Current == ThrownSpear.State.InHand ? Loc.T("Spear held") : _spear.Current == ThrownSpear.State.InFlight ? Loc.T("Spear in flight") : (_spear.TrapActive ? Loc.T("Spear planted (trap)") : Loc.T("Spear planted"));

    private void Update()
    {
        _spear.Tick(Time.deltaTime);
        if (_spear.Current == ThrownSpear.State.InFlight)
        {
            _flight -= Time.deltaTime;
            if (_flight <= 0f)
            {
                _spear.Land();
            }
        }
    }

    public override void OnAttackStart(Weapon weapon, int step)
    {
        if (_spear.Throw())
        {
            _flight = ThrownSpear.FlightTime(ClearDistance(), _throwRange, _throwSpeed);
        }
    }

    /// <summary>던지는 방향으로 Ground/Wall 에 막힐 때까지의 거리(없으면 사거리)</summary>
    private float ClearDistance()
    {
        Transform player = transform.root;
        Vector2 dir = player.localScale.x < 0f ? Vector2.left : Vector2.right;
        int mask = LayerMask.GetMask("Ground", "Wall");
        RaycastHit2D hit = Physics2D.Raycast(player.position, dir, _throwRange, mask);
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
