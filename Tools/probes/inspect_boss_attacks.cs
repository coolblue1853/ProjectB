// 보스(PassiveTerror) 공격 판정 프리팹 구조 조사
var sb = new System.Text.StringBuilder();
foreach (var p in new[]{ "Assets/Prefab/Enemy/EnemyDamge/ClowAttack.prefab", "Assets/Prefab/Enemy/EnemyDamge/PassiveTerrorAttackPrefab.prefab", "Assets/Resources/Weapon/DamageObject/ScareSide/SalshSlide.prefab", "Assets/Resources/Weapon/DamageObject/ScareSide/SSideDown.prefab", "Assets/Resources/Weapon/DamageObject/LeafStaff/HealGround.prefab" })
{
    var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
    sb.AppendLine("== " + p + " scale " + go.transform.localScale);
    foreach (var t in go.GetComponentsInChildren<Transform>(true))
    {
        sb.Append("  " + t.name + " pos" + t.localPosition + " sc" + t.localScale + ": ");
        foreach (var c in t.GetComponents<Component>())
        {
            sb.Append(c.GetType().Name);
            var sr = c as SpriteRenderer; if (sr != null) sb.Append("(" + (sr.sprite ? sr.sprite.name + " " + sr.sprite.rect.size + " ppu" + sr.sprite.pixelsPerUnit : "null") + " col" + sr.color + ")");
            var an = c as Animator; if (an != null) sb.Append("(" + (an.runtimeAnimatorController ? an.runtimeAnimatorController.name : "null") + ")");
            var bc = c as BoxCollider2D; if (bc != null) sb.Append("(" + bc.size + " off" + bc.offset + ")");
            var d = c as DamageObject; if (d != null) sb.Append("(ratio" + d.dmgRatio + " hold" + d.holdingTime + " dash" + d.isDashAttack + " inv" + d.isDashInvins + " spd" + d.dashSpeed + " hits" + d.hitCount + " max" + d.maxDamagedEnemies + " delMax" + d.isDeletByMaxHit + " heal" + d.isHealSkill + "/" + d.healPower + "x" + d.healCount + ")");
            sb.Append(", ");
        }
        sb.AppendLine();
    }
    var anim = go.GetComponentInChildren<Animator>(true);
    if (anim != null && anim.runtimeAnimatorController != null)
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            sb.Append("  clip " + clip.name + " len" + clip.length + " ");
            foreach (var b in AnimationUtility.GetObjectReferenceCurveBindings(clip)) sb.Append(b.path + "." + b.propertyName + " frames" + AnimationUtility.GetObjectReferenceCurve(clip, b).Length + " ");
            foreach (var b in AnimationUtility.GetCurveBindings(clip)) sb.Append("[" + b.path + "." + b.propertyName + "] ");
            sb.AppendLine();
        }
}
return sb.ToString();
