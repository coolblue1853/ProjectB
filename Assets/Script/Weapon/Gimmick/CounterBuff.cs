using UnityEngine;

/// <summary>방패 기믹 — 반격 버프. 막기 성공 시 NotifyBlocked() 를 부르면 3초간 평타 피해 +20% (연결은 후속: 막기 판정 훅 미구현)</summary>
public class CounterBuff : WeaponGimmick
{
    private readonly CounterBuffRules _rules = new CounterBuffRules();

    public bool Active => _rules.Active;

    public void NotifyBlocked()
    {
        _rules.Arm();
    }

    private void Update()
    {
        _rules.Tick(Time.deltaTime);
    }

    private void OnDisable()
    {
        _rules.Clear();
    }

    public override void OnHitboxSpawned(Weapon weapon, int step, DamageObject hitbox)
    {
        if (_rules.Active)
        {
            hitbox.AddHitModifier(this);
        }
    }

    public override float HitMultiplier(DamageObject hitbox, EnemyHealth enemy, Collider2D enemyCollider)
    {
        return _rules.DamageMultiplier;
    }
}
