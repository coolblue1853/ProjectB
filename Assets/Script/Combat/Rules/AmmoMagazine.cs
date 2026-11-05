using UnityEngine;

/// <summary>석궁 장전 규칙(WeaponTypes.md 2.2). 순수 로직: 3발 후 1.2초 재장전, 재장전 직후 첫 발 치명 확정.</summary>
public sealed class AmmoMagazine
{
    public const int Capacity = 3;
    public const float ReloadTime = 1.2f;

    private int _ammo = Capacity;
    private CountdownTimer _reload;
    private bool _critReady;

    public int Ammo => _ammo;
    public bool Reloading => _reload.Active;
    public bool CanFire => !Reloading && _ammo > 0;
    public float ReloadRemaining => _reload.Remaining;

    /// <summary>발사 시도. 성공하면 true, out crit 는 이 발이 치명 확정인지.</summary>
    public bool TryFire(out bool guaranteedCrit)
    {
        guaranteedCrit = false;
        if (!CanFire)
        {
            return false;
        }
        guaranteedCrit = _critReady;
        _critReady = false;
        _ammo--;
        if (_ammo <= 0)
        {
            _reload.Start(ReloadTime);
        }
        return true;
    }

    public void Tick(float dt)
    {
        if (!_reload.Active)
        {
            return;
        }
        _reload.Tick(dt);
        if (!_reload.Active)
        {
            _ammo = Capacity;
            _critReady = true;
        }
    }

    public void Reset()
    {
        _ammo = Capacity;
        _reload.Clear();
        _critReady = false;
    }
}
