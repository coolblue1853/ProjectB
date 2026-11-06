var w = UnityEngine.Object.FindFirstObjectByType<AttackManager>().equipWeapon;
WeaponMasteryService.AddExp(w.MasteryKey, 1500);
return "mExp=" + WeaponMasteryService.Data.GetExp(w.MasteryKey) + " lv=" + WeaponMasteryService.LevelOf(w.MasteryKey);
