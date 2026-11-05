using UnityEngine;

/// <summary>
/// 투창 기믹 — ThrownSpear 래퍼(WeaponTypes.md 2.10). 평타로 던지면 Throw, 일정 시간 뒤 Land,
/// 박힌 창은 TrapDuration 동안 함정, Retrieve 로 회수. 실제 박힘 오브젝트·함정 판정은 후속(그래픽 보류, 임시 텍스트).
/// </summary>
public class ThrownSpearGimmick : WeaponGimmick
{
    [SerializeField] private float _flightTime = 0.4f; // 임시 비행 시간(착지 판정 후속)

    private readonly ThrownSpear _spear = new ThrownSpear();
    private float _flight;

    public ThrownSpear Spear => _spear;
    public string HudText => _spear.Current == ThrownSpear.State.InHand ? "창 보유" : _spear.Current == ThrownSpear.State.InFlight ? "투창 비행" : (_spear.TrapActive ? "창 박힘(함정)" : "창 박힘");

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
            _flight = _flightTime;
        }
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
