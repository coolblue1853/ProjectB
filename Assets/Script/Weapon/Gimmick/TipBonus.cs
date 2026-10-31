using UnityEngine;

/// <summary>채찍 기믹 — 끝 판정. 판정 상자 끝 30% 에 맞으면 피해 +40%, 근접 30% 는 -20% (WeaponTypes.md 2.5)</summary>
public class TipBonus : WeaponGimmick
{
    private DamageObject _cachedHitbox;
    private BoxCollider2D _cachedBox;

    public override void OnHitboxSpawned(Weapon weapon, int step, DamageObject hitbox)
    {
        hitbox.AddHitModifier(this);
    }

    public override float HitMultiplier(DamageObject hitbox, EnemyHealth enemy, Collider2D enemyCollider)
    {
        if (_cachedHitbox != hitbox)
        {
            _cachedHitbox = hitbox;
            _cachedBox = hitbox.GetComponent<BoxCollider2D>();
        }
        BoxCollider2D box = _cachedBox;
        if (box == null || enemyCollider == null)
        {
            return 1f;
        }
        Bounds b = box.bounds;
        float length = b.size.x;
        if (length <= 0.001f)
        {
            return 1f;
        }
        // 상자에서 플레이어에 가까운 쪽 끝 → 적 몸에서 그 끝에 가장 가까운 점까지의 비율
        float faceSign = hitbox.player != null ? hitbox.player.transform.localScale.x : hitbox.transform.lossyScale.x;
        bool facingRight = faceSign >= 0f;
        float nearEdge = facingRight ? b.min.x : b.max.x;
        Vector2 p = enemyCollider.ClosestPoint(new Vector2(nearEdge, b.center.y));
        float t = Mathf.Clamp01(Mathf.Abs(p.x - nearEdge) / length);
        float mul = WeaponGimmickRules.WhipMultiplier(t);
        GameLog.Info(GameLog.Cat.Combat, "채찍 끝 판정 t=" + t.ToString("F2") + " x" + mul.ToString("F2"), this);
        return mul;
    }
}
