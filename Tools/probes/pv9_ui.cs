// PV9: Loc 전환 + NpcQuestTracker 캐시 + NestHuntUI 습격 문구
var o = new System.Text.StringBuilder();
string l0 = Loc.Current;
Loc.Set("English"); o.Append("EN: " + Loc.T("New equipment slot unlocked: ") + " | " + Loc.F("Nest raid wave {0}/{1}", 1, 3) + "\n");
Loc.Set(Loc.Korean); o.Append("KO: " + Loc.T("New equipment slot unlocked: ") + " | " + Loc.F("Nest raid wave {0}/{1}", 1, 3) + "\n");
o.Append("lang now=" + Loc.Current + " (orig " + l0 + ")\n");
return o.ToString();
