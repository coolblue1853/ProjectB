// WSKILL-V2 P1/P2 검증: 구세이브 이행·타입 EXP 공유(검A→검B)·재로드 유지·플래그 분기·트리 투자/선행/리스펙/자동 초기화. 플레이 모드, 세이브는 호출자가 백업/복원
var sb = new System.Text.StringBuilder();
var am = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
int money0 = DatabaseManager.money; var origW = am.equipWeapon;
Weapon w1 = null; Weapon w2 = null; Weapon w3 = null;
System.Action<string> eq = null;
// 1) 구세이브 상태 재현: 옛 weaponMastery 만 있고 신규 키는 없음
ES3.DeleteKey("typeMastery"); ES3.DeleteKey("typeMasteryMigrated"); ES3.DeleteKey("skillTree");
ES3.Save("weaponMastery", new WeaponMasterySave { keys = new[] { "RustedSword", "PoisonSword", "HeartwoodGreatsword" }, exps = new[] { 12000, 400, 3000 } });
ES3.Save("weaponMasteryMigrated", true);
SaveManager.instance.DataLoad();
sb.AppendLine("[이행] Sword exp=" + TypeMasteryService.Data.GetExp("Sword") + " (기대 12000, 합산 아님) Greatsword=" + TypeMasteryService.Data.GetExp("Greatsword") + " (기대 3000) ready=" + TypeMasteryService.Ready + " migratedKey=" + ES3.Load<bool>("typeMasteryMigrated") + " typeKey=" + ES3.KeyExists("typeMastery"));
// 2) 검 A → 검 B 같은 Lv 이어짐 (더미 Weapon 으로 장착 흉내)
var g1 = new GameObject("RustedSword"); w1 = g1.AddComponent<Weapon>();
var g2 = new GameObject("PoisonSword"); w2 = g2.AddComponent<Weapon>();
var g3 = new GameObject("HeartwoodGreatsword"); w3 = g3.AddComponent<Weapon>();
sb.AppendLine("[Lv] flag=" + WeaponMasteryRules.PerWeaponMastery + " RustedSword=" + WeaponMasteryService.EffectiveLevel(w1) + " PoisonSword=" + WeaponMasteryService.EffectiveLevel(w2) + " Greatsword=" + WeaponMasteryService.EffectiveLevel(w3) + " (기대 5/5/3)");
am.equipWeapon = w1;
WeaponMasteryService.AddForEquipped(500);
am.equipWeapon = w2;
WeaponMasteryService.AddForEquipped(500);
sb.AppendLine("[공유] 검 타입 exp=" + TypeMasteryService.Data.GetExp("Sword") + " (기대 13000) 키별 Rusted=" + WeaponMasteryService.Data.GetExp("RustedSword") + " Poison=" + WeaponMasteryService.Data.GetExp("PoisonSword") + " (기대 12500/900, 키별 누적 유지, 따라잡기·StartMastery 없음)");
sb.AppendLine("[보너스] flag=false DamageBonus(Rusted)=" + WeaponMasteryService.DamageBonus("RustedSword") + " (기대 1)");
WeaponMasteryRules.PerWeaponMastery = true;
sb.AppendLine("[플래그 true] Lv Rusted=" + WeaponMasteryService.EffectiveLevel(w1) + " Poison=" + WeaponMasteryService.EffectiveLevel(w2) + " (기대 5/2) Bonus(Rusted)=" + WeaponMasteryService.DamageBonus("RustedSword") + " (기대 1.05)");
WeaponMasteryRules.PerWeaponMastery = false;
// 타입 레벨업 이벤트
int lvEvt = 0; System.Action<string, int> h = (k, l) => { lvEvt = l; }; WeaponMasteryService.LevelUp += h;
TypeMasteryService.Data.RaiseTo("Sword", TypeMasteryRules.TotalExpForLevel(6) - 100);
WeaponMasteryService.AddForEquipped(150);
WeaponMasteryService.LevelUp -= h;
sb.AppendLine("[이벤트] 타입 Lv 상승 알림=" + lvEvt + " (기대 6) swordLv=" + TypeMasteryService.LevelOf(WeaponKind.Sword));
// 3) 저장 → 디스크 → 재로드 유지
SaveManager.instance.DataSave();
int expBefore = TypeMasteryService.Data.GetExp("Sword");
SaveManager.instance.DataLoad();
sb.AppendLine("[재로드] Sword exp 전=" + expBefore + " 후=" + TypeMasteryService.Data.GetExp("Sword") + " 이행 재실행 없음(migrated=" + ES3.Load<bool>("typeMasteryMigrated") + ")");
// 4) 트리: 검 Lv6 → earned 5
string T = "Sword";
sb.AppendLine("[트리] earned=" + SkillTreeService.EarnedPoints(T) + " avail=" + SkillTreeService.Available(T) + " (기대 5)");
sb.AppendLine("  P2 선행 거부=" + SkillTreeService.Invest(T, "P2") + " (기대 PrereqUnmet)");
for (int i = 0; i < 3; i++) { SkillTreeService.Invest(T, "P1"); }
sb.AppendLine("  P1x3 후 avail=" + SkillTreeService.Available(T) + " (기대 2) P2=" + SkillTreeService.Invest(T, "P2") + " P1 Lv=" + SkillTreeService.LevelOf(T, "P1"));
sb.AppendLine("  Mod AttackDamage(Sword)=" + SkillTreeService.Mod(TreeStat.AttackDamage, WeaponKind.Sword) + " (기대 6) Bow=" + SkillTreeService.Mod(TreeStat.AttackDamage, WeaponKind.Bow) + " (기대 0) SkillDamage=" + SkillTreeService.Mod(TreeStat.SkillDamage, WeaponKind.Sword) + " (기대 3)");
sb.AppendLine("  공용 earned=" + SkillTreeService.EarnedPoints(SkillTreeRules.CommonTree) + " (플레이어 Lv" + LevelSystem.Level + " - 1)");
if (SkillTreeService.Available(SkillTreeRules.CommonTree) > 0) { sb.AppendLine("  C1 투자=" + SkillTreeService.Invest(SkillTreeRules.CommonTree, "C1") + " Mod CritChance(Bow)=" + SkillTreeService.Mod(TreeStat.CritChance, WeaponKind.Bow)); }
// 로드 후 유지
SaveManager.instance.DataLoad();
sb.AppendLine("  [재로드] P1 Lv=" + SkillTreeService.LevelOf(T, "P1") + " P2 Lv=" + SkillTreeService.LevelOf(T, "P2") + " spent=" + SkillTreeService.Spent(T) + " key=" + ES3.KeyExists("skillTree"));
// 리스펙: 첫 1회 무료 → 두 번째는 150*투입*1
DatabaseManager.money = 100000;
sb.AppendLine("  리스펙 비용(첫)=" + SkillTreeService.RespecCost(T) + " (기대 0) 결과=" + SkillTreeService.Respec(T) + " avail=" + SkillTreeService.Available(T) + " money=" + DatabaseManager.money);
SkillTreeService.Invest(T, "P1"); SkillTreeService.Invest(T, "P1");
int c2 = SkillTreeService.RespecCost(T);
DatabaseManager.money = c2 - 1;
var rNo = SkillTreeService.Respec(T);
DatabaseManager.money = 100000;
var rOk = SkillTreeService.Respec(T);
sb.AppendLine("  두 번째 비용=" + c2 + " (기대 300) 골드 부족=" + rNo + " 충분=" + rOk + " money=" + DatabaseManager.money + " (기대 99700)");
// 5) spent > earned 자동 초기화: 디스크에 P1x5 + A1x5 강제 → 로드
ES3.Save("skillTree", new SkillTreeSave { nodeIds = new[] { "Sword.P1", "Sword.A1", "Bow.P1" }, levels = new[] { 5, 5, 1 }, respecTrees = new string[0], respecCounts = new int[0] });
SaveManager.instance.DataLoad();
sb.AppendLine("[안전망] Sword spent=" + SkillTreeService.Spent("Sword") + " (earned 5 < 10 → 기대 0) Bow spent=" + SkillTreeService.Spent("Bow") + " (Bow earned 0 < 1 → 기대 0)");
// 정리
SaveManager.instance.DataSave();
UnityEngine.Object.Destroy(g1); UnityEngine.Object.Destroy(g2); UnityEngine.Object.Destroy(g3);
am.equipWeapon = origW;
DatabaseManager.money = money0;
return sb.ToString();
