string s = "";
foreach (var e in EnemyHealth.Active)
{
    if (e.name.StartsWith("BanditArcher"))
    {
        typeof(EnemyHealth).GetMethod("ToggleObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(e, null);
        e.hpBar.setHpBar(e); // 스폰 직후 상태(라벨 켬)
        s += "after setHpBar";
    }
}
return s;
