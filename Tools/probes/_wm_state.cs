var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
var w = am != null ? am.equipWeapon : null;
var sb = new System.Text.StringBuilder();
sb.Append("Data=" + (WeaponMasteryService.Data != null) + " weapon=" + (w != null ? w.MasteryKey : "null") + " lvPlayer=" + LevelSystem.Level + " exp=" + LevelSystem.Exp);
if (WeaponMasteryService.Data != null && w != null)
{
    sb.Append(" mExp=" + WeaponMasteryService.Data.GetExp(w.MasteryKey) + " mLv=" + WeaponMasteryService.LevelOf(w.MasteryKey));
    for (int i = 0; i < 4; i++) { sb.Append(" s" + i + "=" + (w.skill[i] != null ? (WeaponMasteryService.IsSlotUnlocked(w, i) ? "open" : "LOCK@" + WeaponMasteryService.UnlockLevelFor(w, i)) : "-")); }
}
sb.Append(" ES3key=" + ES3.KeyExists("weaponMastery") + " migrated=" + (ES3.KeyExists("weaponMasteryMigrated") && ES3.Load<bool>("weaponMasteryMigrated")));
return sb.ToString();
