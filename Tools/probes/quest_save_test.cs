// 퀘스트 저장/복원 테스트: 수락 → 처치 4회 → 기록 → 저널 비우기 → ES3 에서 복원 → 카운터 유지 확인
var board = UnityEngine.Object.FindFirstObjectByType<QuestBoard>();
var giver = board.GetComponent<PixelCrushers.QuestMachine.QuestGiver>();
var journal = PlayerController.instance.GetComponent<PixelCrushers.QuestMachine.QuestJournal>();
var q = giver.FindQuest("q_rabbits"); if (q == null) return "no quest on board";
giver.GiveQuestToQuester(q, journal);
for (int i = 0; i < 4; i++) PixelCrushers.MessageSystem.SendMessage(null, "Kill", "Rabbit_BD");
var saved = ES3.KeyExists("qm.journal") ? ES3.Load<string>("qm.journal") : "";
journal.DeleteQuest(new PixelCrushers.StringField("q_rabbits"));
int afterDelete = journal.questList.Count;
journal.ApplyData(saved);
var r = journal.FindQuest("q_rabbits");
return "savedLen=" + saved.Length + " afterDelete=" + afterDelete + " restored=" + (r != null ? r.GetState() + " kills=" + r.GetCounter("kills").currentValue : "null");
