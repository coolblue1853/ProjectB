var w = Weapon.Equipped; if (w == null) return "no weapon";
string r = w.name + ":";
var a = w.GetComponent<AmmoGimmick>(); if (a != null) r += " ammo=[" + a.HudText + "]";
var s = w.GetComponent<ThrownSpearGimmick>(); if (s != null) r += " spear=[" + s.HudText + "]";
var t = w.GetComponent<TomePageGimmick>(); if (t != null) r += " tome=[" + t.HudText + "]";
return r;
