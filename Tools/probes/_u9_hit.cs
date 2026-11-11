string s = "";
foreach (var e in EnemyHealth.Active)
{
    if (e.name.StartsWith("BanditArcher")) { e.Damage(50); s += "hit " + e.name + " bar=" + e.hpBar.hpBarRect.gameObject.activeInHierarchy; }
}
return s;
