// 부가 퀘스트: 게시판 제안 목록 → s_wood 수락 → Pickup Wood 10 → 상태, s_craft 수락 → Craft Any 2 → 상태
var board = UnityEngine.Object.FindFirstObjectByType<QuestBoard>();
var giver = board.GetComponent<PixelCrushers.QuestMachine.QuestGiver>();
var journal = PlayerController.instance.GetComponent<PixelCrushers.QuestMachine.QuestJournal>();
var sb = new System.Text.StringBuilder("offers: ");
foreach (var q in giver.questList) if (q.canOffer) sb.Append(q.id + " ");
giver.GiveQuestToQuester(giver.FindQuest("s_wood"), journal);
giver.GiveQuestToQuester(giver.FindQuest("s_craft"), journal);
for (int i = 0; i < 10; i++) PixelCrushers.MessageSystem.SendMessage(null, "Pickup", "Wood");
for (int i = 0; i < 2; i++) PixelCrushers.MessageSystem.SendMessage(null, "Craft", "Any");
sb.Append("| ");
foreach (var q in journal.questList) sb.Append(q.id + "=" + q.GetState() + " " + q.GetCounter("kills").currentValue + "; ");
return sb.ToString();
