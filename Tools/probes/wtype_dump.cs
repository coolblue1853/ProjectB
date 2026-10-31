// 무기 프리팹 요약(편집 모드): 이름 목록은 __N__ 쉼표 구분
var sb = new System.Text.StringBuilder();
foreach (var n in "__N__".Split(','))
{
    var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Weapon/" + n + ".prefab");
    if (go == null) { sb.Append("[없음 " + n + "]\n"); continue; }
    var w = go.GetComponent<Weapon>();
    sb.Append("== " + n + " combo" + w.maxComboCount + " dmg" + w.minDmg + "~" + w.maxDmg + " crit" + w.critPer + "/" + w.critDmg + " stop" + w.isWeaponStopMove + " over" + w.isOverHand + " swing" + w.swingVfx + " x" + w.swingVfxScale + "\n");
    for (int i = 0; i < w.attackPrefab.Length; i++)
    {
        var ap = w.attackPrefab[i]; var d = ap != null ? ap.GetComponent<DamageObject>() : null; var bc = ap != null ? ap.GetComponent<BoxCollider2D>() : null;
        sb.Append("  atk" + (i + 1) + " " + (ap != null ? AssetDatabase.GetAssetPath(ap) : "null") + " spd" + (i < w.attckSpeed.Length ? w.attckSpeed[i].ToString() : "?") + " anim=" + (i < w.attackAnimName.Length ? w.attackAnimName[i] : "?") + " rev=" + (i < w.spriteReverse.Length && w.spriteReverse[i]) + " pivot=" + (i < w.attackPivot.Length && w.attackPivot[i] != null ? w.attackPivot[i].name : "null"));
        if (d != null) sb.Append(" ratio" + d.dmgRatio + " hit" + d.hitCount + " maxE" + d.maxDamagedEnemies + " hold" + d.holdingTime + " stiff" + d.stiffnessTime + " knock" + d.knockForce + " box" + (bc != null ? bc.size.ToString() + "@" + bc.offset.ToString() + " sc" + ap.transform.localScale : "-"));
        sb.Append("\n");
    }
    foreach (var s in go.GetComponentsInChildren<Skill>(true)) sb.Append("  skill " + s.name + "=" + s.skillName + " cd" + s.SkillCoolTime + " prefabs" + s.skillprefab.Length + (s.skillprefab.Length > 0 && s.skillprefab[0] != null ? "(" + s.skillprefab[0].name + ")" : "") + " hold" + s.isHoldSkill + " anim=" + s.skillAnim + "\n");
}
return sb.ToString();
