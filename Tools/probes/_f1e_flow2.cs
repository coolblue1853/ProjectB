// 전 구간 퀘스트 흐름(Q1xx~Q7xx, 선행 체인 포함, 보고는 Q3xx 이후만 출력) 논리 점검 + 구역 안내판 필터(ZoneReport) 경로: 저장·보상 스텁, 선행 잠금→수락→목표 충족→Ready→보고→Done. 저장 접촉 없음(에디트 모드).
var origLv = NpcQuestLog.LevelGetter; var origItem = NpcQuestLog.ItemCounter; var origRem = NpcQuestLog.ItemRemover; var origSave = NpcQuestLog.SaveHook; var origRw = NpcQuestLog.RewardApplier; var origCap = NpcQuestLog.RewardCapacity; var origEq = NpcQuestLog.EquipCounter; var origBest = NpcQuestLog.BestiaryCounter; var origRec = NpcQuestLog.RecordCounter; var origSeed = NpcQuestLog.EnhanceSeed;
var have = new System.Collections.Generic.Dictionary<string, int>(); int equipped = 0; int lv = 1; int bestKinds = 0; int recCount = 0;
var sb = new System.Text.StringBuilder(); int bad = 0;
try {
 NpcQuestLog.LevelGetter = () => lv; NpcQuestLog.ItemCounter = k => have.ContainsKey(k) ? have[k] : 0; NpcQuestLog.ItemRemover = (k, n) => { have[k] = System.Math.Max(0, (have.ContainsKey(k) ? have[k] : 0) - n); };
 NpcQuestLog.SaveHook = () => { }; NpcQuestLog.RewardApplier = d => { }; NpcQuestLog.RewardCapacity = d => true; NpcQuestLog.EquipCounter = k => equipped; NpcQuestLog.BestiaryCounter = () => bestKinds; NpcQuestLog.RecordCounter = () => recCount; NpcQuestLog.EnhanceSeed = k => 0;
 NpcQuestCatalog.SetTableForTests(null); NpcQuestLog.ClearAll();
 var all = new System.Collections.Generic.List<NpcQuestDef>();
 foreach (var d in NpcQuestCatalog.All) if (d.Id.Length == 4 && d.Id[0] == 'Q' && d.Id[1] >= '1' && d.Id[1] <= '7' && d.Source != "bounty") all.Add(d);
 var done = new System.Collections.Generic.HashSet<string>(); int guard = 0;
 while (done.Count < all.Count && guard++ < 50) {
  bool prog = false;
  foreach (var d in all) {
   if (done.Contains(d.Id)) continue;
   if (!string.IsNullOrEmpty(d.Requires) && !done.Contains(d.Requires) && all.Exists(x => x.Id == d.Requires)) continue;
   lv = 1; var st0 = NpcQuestLog.StatusOf(d); if (d.MinLevel > 1 && st0 != NpcQuestStatus.LevelLow) { sb.AppendLine("BAD " + d.Id + " lv1 status=" + st0); bad++; }
   lv = d.MinLevel; var st1 = NpcQuestLog.StatusOf(d);
   if (st1 != NpcQuestStatus.Available) { sb.AppendLine("BAD " + d.Id + " not Available at Lv" + lv + " = " + st1 + " req=" + d.Requires); bad++; done.Add(d.Id); continue; }
   if (!NpcQuestLog.Accept(d.Id)) { sb.AppendLine("BAD " + d.Id + " accept fail"); bad++; done.Add(d.Id); continue; }
   string t = d.FirstTarget;
   switch (d.Objective) {
    case NpcQuestObjective.Kill: for (int i = 0; i < d.Count; i++) NpcQuestLog.OnKill(t); break;
    case NpcQuestObjective.Collect: if (d.IsRecordsTarget) recCount = d.Count; else have[d.Target] = d.Count; break;
    case NpcQuestObjective.Talk: for (int i = 0; i < d.Count; i++) NpcQuestLog.OnTalk(t); break;
    case NpcQuestObjective.Visit: NpcQuestLog.OnVisit(d.Target); break;
    case NpcQuestObjective.Craft: for (int i = 0; i < d.Count; i++) NpcQuestLog.OnCraft(d.Target); break;
    case NpcQuestObjective.Equip: equipped = d.Count; break;
    case NpcQuestObjective.Bestiary: bestKinds = d.Count; break;
    case NpcQuestObjective.KillElite: for (int i = 0; i < d.Count; i++) NpcQuestLog.OnKillElite(d.Target); break;
    case NpcQuestObjective.Enhance: NpcQuestLog.OnEnhance(d.Target == "weapon" ? "Weapon" : (d.Target == "armor" ? "Chest" : d.Target), d.Count); break;
   }
   if (NpcQuestLog.HasExtra(d)) for (int i = 0; i < d.Count2; i++) NpcQuestLog.OnKillElite(d.Target2);
   var st2 = NpcQuestLog.StatusOf(d);
   bool ready = st2 == NpcQuestStatus.Ready;
   var zv = QuestService.Query(new QuestFilter { Tab = QuestListRules.Tab.Active, Giver = QuestEntrance.ZoneReportNpc, ReportToo = true, ReportOnly = true });
   bool inSign = zv.Items.Exists(x => x.Id == d.Id); bool wantSign = d.ReportNpc == "eve";
   bool signOk = inSign == wantSign;
   bool ok = ready && signOk && (wantSign ? QuestService.Report(d.Id) : NpcQuestLog.Complete(d.Id)) && NpcQuestLog.StatusOf(d) == NpcQuestStatus.Done;
   if (!ok || d.Id[1] >= '3') sb.AppendLine((ok ? "ok  " : "BAD ") + d.Id + " obj=" + d.Objective + " tgt=" + d.Target + "x" + d.Count + (NpcQuestLog.HasExtra(d) ? "+" + d.Target2 + "x" + d.Count2 : "") + " giver=" + d.Giver + " report=" + d.ReportNpc + " sign=" + inSign + (signOk ? "" : "(BAD)") + (ok ? "" : " st=" + st2));
   if (!ok) bad++; equipped = 0; bestKinds = 0; recCount = 0; done.Add(d.Id); prog = true;
  }
  if (!prog) break;
 }
 sb.AppendLine("done=" + done.Count + "/" + all.Count + " bad=" + bad);
} finally {
 NpcQuestLog.ClearAll(); NpcQuestLog.LevelGetter = origLv; NpcQuestLog.ItemCounter = origItem; NpcQuestLog.ItemRemover = origRem; NpcQuestLog.SaveHook = origSave; NpcQuestLog.RewardApplier = origRw; NpcQuestLog.RewardCapacity = origCap; NpcQuestLog.EquipCounter = origEq; NpcQuestLog.BestiaryCounter = origBest; NpcQuestLog.RecordCounter = origRec; NpcQuestLog.EnhanceSeed = origSeed;
}
return sb.ToString();
