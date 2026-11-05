using UnityEngine;

/// <summary>
/// 석궁 기믹 — AmmoMagazine 래퍼(WeaponTypes.md 2.2). 평타가 시작될 때 1발 소모, 재장전 진행.
/// 발사 차단·치명 확정 적용은 Weapon 게이트가 필요해 후속(현재는 상태·HUD 텍스트만). 그래픽 보류: 임시 텍스트.
/// </summary>
public class AmmoGimmick : WeaponGimmick
{
    private readonly AmmoMagazine _mag = new AmmoMagazine();

    public AmmoMagazine Magazine => _mag;
    /// <summary>방금 발사한 발이 치명 확정이었는가(후속 연결용)</summary>
    public bool LastShotGuaranteedCrit { get; private set; }

    public string HudText => _mag.Reloading ? $"재장전 {_mag.ReloadRemaining:0.0}s" : $"탄 {_mag.Ammo}/{AmmoMagazine.Capacity}";

    private void Update()
    {
        _mag.Tick(Time.deltaTime);
    }

    private void OnDisable()
    {
        _mag.Reset();
    }

    public override void OnAttackStart(Weapon weapon, int step)
    {
        bool crit;
        if (_mag.TryFire(out crit))
        {
            LastShotGuaranteedCrit = crit;
        }
    }

    private void OnGUI()
    {
        GimmickHud.Draw(HudText); // 임시 HUD(그래픽 보류, 확정 시 교체)
    }
}
