// Lv2 몬스터 일괄 생성 (Lv2ContentPlan 3절). MonsterBuilder가 bbox·발 위치를 자동 계산한다
var MB = typeof(ProjectB.EditorTools.MonsterBuilder);
var sb = new System.Text.StringBuilder();
System.Func<string, string, string, int, int, bool, bool, bool, string[], string> build = (name, tpl, folder, hp, def, armor, boss, flip, map) => {
    var S = new ProjectB.EditorTools.MonsterBuilder.Spec();
    S.Name = name; S.Template = tpl; S.SheetFolder = folder; S.MaxHp = hp; S.Def = def; S.SuperArmor = armor; S.Boss = boss; S.FlipX = flip;
    for (int i = 0; i + 1 < map.Length; i += 2) { if (map[i] == "death") S.DeathSheet = map[i + 1]; else S.ClipMap[map[i]] = map[i + 1]; }
    return ProjectB.EditorTools.MonsterBuilder.Build(S); };
var M = "Assets/ThirdParty/MonstersCreaturesFantasy/Sprites/";
sb.AppendLine(build("CliffGoblin_BD", "RatThief_BD", M + "Goblin", 150, 4, false, false, false, new[]{ "RatThiefStay","Idle","RatThiefWalk","Run","RatThiefHit","Take Hit","RatThiefAttack","Attack1","death","Death" }));
sb.AppendLine(build("SporeShroom_BD", "LeftMole_BD", M + "Mushroom", 160, 4, false, false, false, new[]{ "LeafMoleStay","Idle","LeafMoleWalk","Run","LeafMoleHit","Take Hit","LeafMoleAttack","Attack1","death","Death" }));
sb.AppendLine(build("CliffSkeleton_BD", "RatChief_BD", M + "Skeleton", 260, 12, false, false, false, new[]{ "RatChiefStay","Idle","RatChiefWalk","Walk","RatChiefHit","Take Hit","RatChiefAttack","Attack1","death","Death" }));
sb.AppendLine(build("GazeEye_BD", "FlowerMole_BD", M + "Flying eye", 120, 3, false, false, false, new[]{ "FlowerMoleStay","Flight","FlowerMoleWalk","Flight","FlowerMoleHit","Take Hit","FlowerMoleAttack","Attack1","death","Death" }));
var BS = "Bringer-of-Death-SpritSheet";
sb.AppendLine(build("DeathBringer_BD", "PassiveTerror_BD", "Assets/ThirdParty/BringerOfDeath/Sprite Sheet", 2500, 15, true, false, true, new[]{ "PT_Stay",BS+"#0-7","PT_Walk",BS+"#8-15","ScaryCrowHit",BS+"#51-53","PTAttackOne",BS+"#16-25","PTAttackTwo",BS+"#26-34","death",BS+"#54-63" }));
sb.AppendLine(build("AbyssWizard_BD", "PassiveTerror_BD", "Assets/ThirdParty/EvilWizard2/Sprites", 8000, 20, true, true, false, new[]{ "PT_Stay","Idle","PT_Walk","Run","ScaryCrowHit","Take hit","PTAttackOne","Attack1","PTAttackTwo","Attack2","death","Death" }));
return sb.ToString();
