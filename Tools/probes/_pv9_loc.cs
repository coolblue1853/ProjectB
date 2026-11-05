// PV9: LocUi.Entries 전체가 한국어 모드에서 ko 로 나오는지, 영어 모드에서 en 인지. 언어는 원복
string l0 = Loc.Current; int n = 0, badKo = 0, badEn = 0; var bad = new System.Text.StringBuilder();
Loc.Set(Loc.Korean);
foreach (var e in LocUi.Entries) { n++; if (Loc.T(e.en) != e.ko) { badKo++; if (badKo <= 5) bad.Append("[" + e.en + "->" + Loc.T(e.en) + "] "); } }
Loc.Set("English");
foreach (var e in LocUi.Entries) { if (Loc.T(e.en) != e.en) badEn++; }
Loc.Set(l0);
return "entries=" + n + " KO불일치=" + badKo + " EN불일치=" + badEn + " " + bad + " 원복=" + Loc.Current + "(orig " + l0 + ")";
