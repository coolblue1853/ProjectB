using UnityEngine;

/// <summary>도끼 기믹 — 쪼개기. 방어력이 높은 몹(기사·방패병·골렘)에게 피해 +20% (WeaponTypes.md 2.4)</summary>
public class ArmorCleave : WeaponGimmick
{
    public override void OnHitboxSpawned(Weapon weapon, int step, DamageObject hitbox)
    {
        hitbox.AddHitModifier(this);
    }

    public override float HitMultiplier(DamageObject hitbox, EnemyHealth enemy, Collider2D enemyCollider)
    {
        float mul = WeaponGimmickRules.CleaveMultiplier(enemy.enemyDef);
        if (mul > 1f)
        {
            GameLog.Info(GameLog.Cat.Combat, "쪼개기 " + enemy.name + " 방어 " + enemy.enemyDef + " x" + mul.ToString("F2"), this);
        }
        return mul;
    }
}
