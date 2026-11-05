var w = Weapon.Equipped; if (w == null) return "no weapon";
var g = w.GetComponent<AmmoGimmick>(); if (g == null) return w.name + " no gimmick";
string b = g.HudText;
try { w.MeleeAttack(); } catch (System.Exception ex) { b += " EX:" + ex.Message; }
return w.name + " before:" + b + " after:" + g.HudText + " wait=" + w.isAttackWait;
