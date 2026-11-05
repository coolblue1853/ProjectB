var w = Weapon.Equipped; if (w == null) return "no weapon";
var g = w.GetComponent<ThrownSpearGimmick>(); if (g == null) return w.name + " no gimmick";
string b = g.HudText; string ex2 = "";
try { w.MeleeAttack(); } catch (System.Exception ex) { ex2 = " EX:" + ex.Message; }
return w.name + " before:" + b + " after:" + g.HudText + ex2;
