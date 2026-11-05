var k = QuestFoldRules.Key(QuestListRules.Tab.Active, "PV9");
bool f0 = QuestFoldRules.IsFolded(k); QuestFoldRules.Toggle(k); bool f1 = QuestFoldRules.IsFolded(k);
string saved = ES3.KeyExists(QuestFoldRules.SaveKey) ? ES3.Load<string>(QuestFoldRules.SaveKey) : "(none)";
QuestFoldRules.Toggle(k); bool f2 = QuestFoldRules.IsFolded(k);
return "fold " + f0 + "->" + f1 + "->" + f2 + " saved='" + saved + "'";
