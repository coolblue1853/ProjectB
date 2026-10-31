using UnityEngine;

/// <summary>쌍검 기믹 — 교차 마무리. 콤보 마지막 타의 피해 x1.5, 범위 x1.2 (WeaponTypes.md 2.7)</summary>
public class FinisherBonus : WeaponGimmick
{
    [SerializeField] private float _damageMul = WeaponGimmickRules.FinisherDamageMul;
    [SerializeField] private float _sizeMul = WeaponGimmickRules.FinisherSizeMul;

    public override void OnHitboxSpawned(Weapon weapon, int step, DamageObject hitbox)
    {
        if (!WeaponGimmickRules.IsFinisher(step, weapon.maxComboCount))
        {
            return;
        }
        hitbox.dmgRatio *= _damageMul;
        hitbox.transform.localScale *= _sizeMul;
        GameLog.Info(GameLog.Cat.Combat, "교차 마무리 x" + _damageMul.ToString("F2") + " 범위 x" + _sizeMul.ToString("F2"), this);
    }
}
