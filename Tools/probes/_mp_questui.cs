// quest-ui 검증: 루엔 Q104→Q105 연속 수락 흐름 + Q 창 3탭 텍스트 → Logs/playtest/mapphys/questui.txt
var sb = new System.Text.StringBuilder();
NpcQuestWindow.Toggle(); NpcQuestLog.ClearAll(); NpcQuestLog.LevelGetter = () => 10;
System.Action<string> ensure = null;
ensure = id => { var d = NpcQuestCatalog.Find(id); if (d == null || NpcQuestLog.IsDone(id)) return; if (!string.IsNullOrEmpty(d.Requires)) ensure(d.Requires);
  NpcQuestLog.Accept(id); var dd = d; if (dd.Objective == NpcQuestObjective.Kill) for (int i = 0; i < dd.Count; i++) NpcQuestLog.OnKill(dd.Target);
  if (!NpcQuestLog.Complete(id)) sb.AppendLine("ensure " + id + " complete FAIL status=" + NpcQuestLog.StatusOf(id)); };
ensure("Q102");
sb.AppendLine("Q104=" + NpcQuestLog.StatusOf("Q104") + " Q105=" + NpcQuestLog.StatusOf("Q105"));
NpcQuestGiver ruen = null; foreach (var g in UnityEngine.Object.FindObjectsByType<NpcQuestGiver>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { var f = typeof(NpcQuestGiver).GetField("_npcId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); if ((string)f.GetValue(g) == "ruen") ruen = g; }
if (ruen == null) { sb.AppendLine("ruen NPC 없음(마을 밖?)"); }
else {
  System.Func<string, string> talk = label => { bool ok = NpcQuestGiver.TryTalk(ruen.gameObject); bool conv = PixelCrushers.DialogueSystem.DialogueManager.isConversationActive; string t = conv ? PixelCrushers.DialogueSystem.DialogueManager.CurrentConversationState?.subtitle?.formattedText?.text : ""; PixelCrushers.DialogueSystem.DialogueManager.StopConversation(); return label + ": TryTalk=" + ok + " conv=" + conv + " '" + (t ?? "") + "'"; };
  sb.AppendLine(talk("1 Q104 제안"));
  sb.AppendLine(talk("1b 같은 상태 재대화(묻지 않아야)"));
  NpcQuestLog.Accept("Q104"); for (int i = 0; i < 6; i++) NpcQuestLog.OnKill("Scarecrow_BD");
  sb.AppendLine("Q104=" + NpcQuestLog.StatusOf("Q104"));
  sb.AppendLine(talk("2 Q104 보고"));
  sb.AppendLine("complete=" + NpcQuestLog.Complete("Q104") + " Q105=" + NpcQuestLog.StatusOf("Q105"));
  sb.AppendLine(talk("3 Q105 제안(연속)"));
}
// Q 창
NpcQuestWindow.Toggle(); var w = UnityEngine.Object.FindFirstObjectByType<NpcQuestWindow>(FindObjectsInactive.Include);
sb.AppendLine("IsOpen=" + NpcQuestWindow.IsOpen);
if (w != null) foreach (var tm in w.GetComponentsInChildren<TMPro.TMP_Text>(true)) if (!string.IsNullOrEmpty(tm.text) && tm.gameObject.activeInHierarchy) sb.AppendLine("UI[" + tm.name + "]: " + tm.text.Replace("\n", " | "));
System.IO.Directory.CreateDirectory("Logs/playtest/mapphys"); System.IO.File.WriteAllText("Logs/playtest/mapphys/questui.txt", sb.ToString());
return sb.ToString();
