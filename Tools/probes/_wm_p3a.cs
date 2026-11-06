var tc = TimeChange.instance; if (!tc.isDaytime) { float target = (tc.endNighttime + 0.05f) * tc.totalTime; float now = tc.value * tc.totalTime; float add = target - now; if (add < 0) add += tc.totalTime; tc.AddTime(add); }
WeaponMasteryService.Data.LoadFrom(new WeaponMasterySave());
var w = UnityEngine.Object.FindFirstObjectByType<AttackManager>().equipWeapon;
string s = "";
for (int i = 0; i < 4; i++) s += i + ":" + (w.skill[i] != null ? (WeaponMasteryService.IsSlotUnlocked(w, i) ? "open" : "LOCK@" + WeaponMasteryService.UnlockLevelFor(w, i)) : "-") + " ";
return w.MasteryKey + " day=" + tc.isDaytime + " " + s + " | " + WeaponMasteryTooltip.Build(w).Replace("\n", " / ");
