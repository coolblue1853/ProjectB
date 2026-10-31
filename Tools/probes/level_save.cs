// 현재 레벨 저장 → 디스크 값 확인 → 상태를 Lv1 로 지우고 → 디스크에서 다시 읽어 복원되는지
var sm = SaveManager.instance;
sm.DataSave();
var d = new Datas();
ES3.LoadInto("Datas", d);
string disk = "disk level=" + d.level + " exp=" + d.exp;
LevelSystem.SetState(1, 0);
string cleared = "cleared Lv" + LevelSystem.Level + " exp=" + LevelSystem.Exp;
LevelSystem.LoadFrom(d);
return disk + " | " + cleared + " | reloaded Lv" + LevelSystem.Level + " exp=" + LevelSystem.Exp + " fullHP=" + PlayerHealthManager.Instance.fullHP;
