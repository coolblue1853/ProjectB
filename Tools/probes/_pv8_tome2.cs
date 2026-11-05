var g = Weapon.Equipped.GetComponent<TomePageGimmick>(); string s = g.HudText; bool a = g.Tome.TrySwitch(); bool b = g.Tome.TrySwitch(); return s + " | first=" + a + " second=" + b + " " + g.HudText;
