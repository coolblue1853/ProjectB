// 퀘스트 테스트 (__KEY__ = open | accept | kill | report | state)
var board = UnityEngine.Object.FindFirstObjectByType<QuestBoard>();
var giver = board.GetComponent<PixelCrushers.QuestMachine.QuestGiver>();
var journal = PlayerController.instance.GetComponent<PixelCrushers.QuestMachine.QuestJournal>();
var k = "__KEY__";
if (k == "open") { PlayerController.instance.transform.position = board.transform.position + Vector3.right * 1.2f + Vector3.up; giver.StartDialogueWithPlayer(); return "dialogue opened"; }
if (k == "accept") { var q = giver.FindQuest("q_rabbits"); giver.GiveQuestToQuester(q, journal); return "accepted; journal=" + journal.questList.Count; }
if (k == "kill") { for (int i = 0; i < 10; i++) PixelCrushers.MessageSystem.SendMessage(null, "Kill", "Rabbit_BD"); }
if (k == "report") { giver.StartDialogueWithPlayer(); }
var sb = new System.Text.StringBuilder("money=" + DatabaseManager.money + " stones=" + InventoryManager.instance.CountItem("Enhance Stone") + " | journal: ");
foreach (var q in journal.questList) { var c = q.GetCounter("kills"); sb.Append(q.id + "=" + q.GetState() + (c != null ? " kills " + c.currentValue : "") + "; "); }
sb.Append("| board offers: ");
foreach (var q in giver.questList) sb.Append(q.id + "(" + q.GetState() + (q.canOffer ? ",offer" : "") + ") ");
return sb.ToString();
