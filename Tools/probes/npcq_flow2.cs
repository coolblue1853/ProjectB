var sb = new System.Text.StringBuilder();
for (int i = 0; i < 8; i++) PixelCrushers.MessageSystem.SendMessage(null, "Kill", "FlowerMole_BD");
var q = NpcQuestCatalog.Find("Q102");
sb.AppendLine("Q102 " + NpcQuestLog.StatusOf(q) + " " + NpcQuestWindow.ObjectiveText(q) + " ruen mark=" + GameObject.Find("NPC_ruen").GetComponent<NpcQuestGiver>().MarkState);
bool t = NpcQuestGiver.TryTalk(GameObject.Find("NPC_ruen"));
sb.AppendLine("report talk=" + t + " conv=" + PixelCrushers.DialogueSystem.DialogueManager.lastConversationStarted);
PixelCrushers.DialogueSystem.DialogueManager.StopConversation();
int e0 = LevelSystem.Exp;
PixelCrushers.DialogueSystem.Lua.Run("NpcQuestComplete('Q102')");
sb.AppendLine("Q102 done=" + NpcQuestLog.IsDone("Q102") + " exp " + e0 + "->" + LevelSystem.Exp + " Lv" + LevelSystem.Level);
sb.AppendLine("elder mark=" + GameObject.Find("NPC_Elder").GetComponent<NpcQuestGiver>().MarkState + " (Q201 Lv3 > Lv" + LevelSystem.Level + ")");
// 드롭 조건
var dm = UnityEngine.Object.FindObjectsByType<DropManager>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
sb.AppendLine("wantsDrop before=" + NpcQuestLog.WantsDrop("StarBarkFragment") + " dms=" + dm.Length);
return sb.ToString();
