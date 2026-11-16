// 시험 준비(봇 행동 아님, 봇 실행 전에만): 진행 중 퀘스트를 전부 포기해 비운다(퀘스트 받기 경로 시험). 세이브는 bot_run.sh 가 백업·원복한다
if (PlayBot.IsRunning) return "bot running - refused";
int n = 0;
foreach (NpcQuestDef d in new System.Collections.Generic.List<NpcQuestDef>(QuestService.ActiveAll()))
{
    if (QuestService.Abandon(d.Id)) { n++; }
}
return "abandoned=" + n + " active=" + QuestService.ActiveAll().Count;
