typeof(Portal).GetMethod("GrantArrivalInvincible", System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,null);
return "inv now=" + PlayerStatus.Has(StatusFlag.Invincible) + " const=" + Portal.ArrivalInvincible;
