// PV9: 슬롯별 신규 키 읽기(+쓰기). 마커 __MASK__/__FOLD__ 가 -1 이면 읽기만
int slot = SaveSlots.Current; var o = new System.Text.StringBuilder();
o.Append("slot=" + slot + " path=" + ES3Settings.defaultSettings.path + "\n");
Func<string> rd = () => "mask=" + (ES3.KeyExists(SlotRules.SaveKeyUnlock) ? ES3.Load<int>(SlotRules.SaveKeyUnlock).ToString() : "(none)") + " fold='" + (ES3.KeyExists(QuestFoldRules.SaveKey) ? ES3.Load<string>(QuestFoldRules.SaveKey) : "(none)") + "' bind=" + (ES3.KeyExists(InputBindings.SaveKey, SaveSlots.SharedFile) ? ES3.Load<string>(InputBindings.SaveKey, SaveSlots.SharedFile).GetHashCode().ToString() : "(none)");
o.Append("before: " + rd() + "\n");
if (__MASK__ >= 0) { ES3.Save(SlotRules.SaveKeyUnlock, __MASK__); ES3.Save(QuestFoldRules.SaveKey, "__FOLD__"); o.Append("wrote mask=__MASK__ fold=__FOLD__\nafter: " + rd() + "\n"); }
return o.ToString();
