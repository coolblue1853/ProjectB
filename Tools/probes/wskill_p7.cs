// WSKILL-V2-2 P7 검증(플레이 모드): 장비 HP 변화 시 트리 최대 HP 재계산, 전투 중 K 창 차단
var sb = new System.Text.StringBuilder();
var phm = PlayerHealthManager.Instance;
if (phm == null) { return "no PHM"; }
DatabaseManager.money = 1000000;
SkillTreeService.Respec(SkillTreeRules.CommonTree);
var d = SkillTreeService.Data;
for (int i = 0; i < 10; i++) { d.Invest("Common", "C5", 999); }
SkillTreeEffects.Refresh(SkillTreeEffects.CurrentKind());
SkillTreeEffects.SyncMaxHp();
int h1 = phm.fullHP;
phm.AddLevelMaxHp(100); // 기준 HP +100 (장비 착용 유사)
SkillTreeEffects.SyncMaxHp();
int h2 = phm.fullHP;
sb.AppendLine("[HP] 트리 20% 적용 후 " + h1 + " -> 기준+100 후 " + h2 + " (기대 +120)");
phm.AddLevelMaxHp(-100);
SkillTreeEffects.SyncMaxHp();
sb.AppendLine("[HP] 원복 " + phm.fullHP + " (기대 " + h1 + ")");
d.ClearForRespec("Common"); SkillTreeEffects.Refresh(SkillTreeEffects.CurrentKind()); SkillTreeEffects.SyncMaxHp();
CombatState.Mark(UnityEngine.Time.unscaledTime);
SkillTreeWindow.Toggle();
sb.AppendLine("[전투 중] 창 열림=" + SkillTreeWindow.IsOpen + " (기대 False)");
CombatState.Mark(float.NegativeInfinity);
SkillTreeWindow.Toggle();
sb.AppendLine("[평시] 창 열림=" + SkillTreeWindow.IsOpen + " (기대 True)");
SkillTreeWindow.Toggle();
return sb.ToString();
