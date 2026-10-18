// Q-415: 1층계 무기 피해를 구역 단계 T 공식으로 맞춘다 (R 평균 10+2.5T, U ×1.4, 레전더리 ×1.6, 범위 = 평균 ×0.8~1.2).
// 치명 특화(Bloodsting·Cutthroat)는 평균 ×0.9. 재실행 안전(절대값 지정). 표: TowerWorld 18절
string[] rows = {
  // T0 ① 샛길: R 8~12 / U 11~17 (C 상점·채집 무기 5~10 은 그대로)
  "ScareStaff,8,12","SacreGreatSword,8,12","DoubleDagger,8,12","PoisonSword,8,12","LeafStaff,8,12","LeafAdae,8,12",
  "RatCheifDagger,11,17","ScareSide,11,17","ScareClow,11,17",
  // T1 ② 일반 숲: R 10~15
  "FangDagger,10,15","TuskKnuckles,10,15","WispLantern,10,15","ShellShield,10,15",
  // T2 ③ 깊은 숲: R 12~18 / U 17~25
  "BearClaw,12,18","RootStaff,12,18","PhantomScythe,12,18","FaeWand,12,18","HeartwoodGreatsword,17,25",
  // T3 ④ 늪지: R 14~21 (치명 특화 13~19) / U 20~29
  "FrogTongueWhip,14,21","CrocJawAxe,14,21","PetalPincer,14,21","SlimeMace,14,21","BloodstingRapier,13,19","PrimordialGel,20,29",
  // T4 ⑤ 산적 성채: R 16~24 (치명 특화 14~22) / U 22~34
  "BanditShortbow,16,24","RaiderBuckler,16,24","CutthroatDagger,14,22","ChiefsWarBanner,22,34",
  // T5 ⑥ 탑 근교: R 18~27 / 레전더리 29~43
  "PebbleGauntlet,18,27","QuakestoneMaul,18,27","MonolithShield,18,27","GatekeepersOath,29,43",
};
var sb = new System.Text.StringBuilder();
foreach (var r in rows)
{
    var c = r.Split(',');
    string path = "Assets/Resources/Weapon/" + c[0] + ".prefab";
    var root = PrefabUtility.LoadPrefabContents(path);
    var w = root.GetComponent<Weapon>();
    if (w == null) { sb.Append(c[0] + " ?Weapon "); PrefabUtility.UnloadPrefabContents(root); continue; }
    string before = w.minDmg + "~" + w.maxDmg;
    w.minDmg = int.Parse(c[1]); w.maxDmg = int.Parse(c[2]);
    PrefabUtility.SaveAsPrefabAsset(root, path);
    PrefabUtility.UnloadPrefabContents(root);
    sb.Append(c[0] + " " + before + "->" + c[1] + "~" + c[2] + "\n");
}
AssetDatabase.SaveAssets();
return sb.ToString();
