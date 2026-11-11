string s = "";
foreach (var e in EnemyHealth.Active)
{
    if (e.name.StartsWith("BanditArcher")) { e.hpCanvas.SetActive(true); s += "bar on pos=" + e.hpBar.hpBarRect.position + " mob=" + e.transform.position; }
}
return s;
