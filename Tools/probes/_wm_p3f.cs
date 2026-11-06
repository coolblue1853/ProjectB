var w = UnityEngine.Object.FindFirstObjectByType<AttackManager>().equipWeapon;
WeaponMasteryService.AddExp(w.MasteryKey, 1600);
UnityEngine.ScreenCapture.CaptureScreenshot("C:/Users/user/ProjectB/Logs/playtest/wsp3_unlock_toast.png");
return "lv=" + WeaponMasteryService.LevelOf(w.MasteryKey) + " notice=" + CombatHud.NoticeVisible + " lab1=" + SkillCooldown.instance.LabelText(1);
