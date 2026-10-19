// 촌장 스토리 퀘스트 체인 테스트 (__KEY__ = state | step1 | step2 | step3). 세이브를 바꾸므로 테스트 뒤 백업 복원.
var board = QuestBoard.ElderBoard;
if (board == null) return "no elder board";
var giver = board.GetComponent<PixelCrushers.QuestMachine.QuestGiver>();
var journal = PlayerController.instance.GetComponent<PixelCrushers.QuestMachine.QuestJournal>();
var k = "__KEY__";
string[] ids = { "st_father1", "st_father2", "st_father3" };
string[] tgt = { "AncientEnt_BD", "GolemMedium_BD", "Gatekeeper_BD" };
int[] cnt = { 1, 12, 1 };
for (int s = 0; s < 3; s++)
{
    if (k != "step" + (s + 1)) continue;
    var q = giver.FindQuest(ids[s]);
    if (q == null) return ids[s] + " missing";
    if (!q.canOffer) return ids[s] + " not offerable (" + q.GetState() + ")";
    giver.GiveQuestToQuester(q, journal);
    for (int i = 0; i < cnt[s]; i++) PixelCrushers.MessageSystem.SendMessage(null, "Kill", tgt[s]);
    var jq = journal.FindQuest(ids[s]);
    var c = jq.GetCounter("kills");
    string before = "kills " + c.currentValue + " node hunt=" + jq.GetNode("hunt").GetState();
    jq.SetState(PixelCrushers.QuestMachine.QuestState.Successful);

    return ids[s] + " " + before + " → done; title=" + jq.title;
}
var sb = new System.Text.StringBuilder("elder offers: ");
foreach (var q in giver.questList) sb.Append(q.id + "(" + q.GetState() + (q.canOffer ? ",offer" : "") + ") ");
return sb.ToString();
