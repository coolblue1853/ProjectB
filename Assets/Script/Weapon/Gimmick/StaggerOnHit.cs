using UnityEngine;

/// <summary>둔기 기믹 — 충격. 평타 판정에 경직 시간을 더하고 방어 무시를 붙인다 (WeaponTypes.md 2.3)</summary>
public class StaggerOnHit : WeaponGimmick
{
    [SerializeField] private float _extraStagger = WeaponGimmickRules.MaceStagger;
    [SerializeField] private int _ignoreDef = WeaponGimmickRules.MaceIgnoreDef;

    public override void OnHitboxSpawned(Weapon weapon, int step, DamageObject hitbox)
    {
        hitbox.stiffnessTime += _extraStagger;
        // 피해 배열은 무기가 공유하므로 복사해서 이 판정에만 방어 무시를 더한다
        int[] copy = (int[])hitbox.damageArr.Clone();
        copy[5] = WeaponGimmickRules.AddIgnoreDef(copy[5], _ignoreDef);
        hitbox.SetDamge(copy);
    }
}
