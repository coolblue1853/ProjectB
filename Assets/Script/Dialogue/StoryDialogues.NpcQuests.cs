using PixelCrushers.DialogueSystem;

/// <summary>
/// 1층계 NPC 퀘스트 대화 (ClassicMapleDesign.md 10절). 퀘스트마다 수락 대화(설명 한 줄 → 수락/나중에)와 보고 대화를 CSV 에서 만든다.
/// 수락·보고는 Lua 함수 NpcQuestAccept / NpcQuestComplete (DialogueBoot 이 등록 → NpcQuestLog).
/// 퀘스트 문구는 CSV 의 한국어 원문 그대로 쓴다(번역 표 없음).
/// </summary>
public static partial class StoryDialogues
{
    public const string RuenTalk = "Village/Ruen";
    public const string SenaTalk = "Village/Sena";
    public const string EveTalk = "Village/Eve";

    private const int RuenId = 7, SenaId = 8, EveId = 9;
    private const int QuestConvBase = 100;

    public static string OfferConversation(string questId) => "NpcQ/" + questId + "/offer";

    public static string ReportConversation(string questId) => "NpcQ/" + questId + "/report";

    private static void AddNpcQuestActors()
    {
        db.actors.Add(template.CreateActor(RuenId, "사냥꾼 루엔", false));
        db.actors.Add(template.CreateActor(SenaId, "약초꾼 세나", false));
        db.actors.Add(template.CreateActor(EveId, "떠돌이 탐험가 이브", false));
    }

    private static int ActorOf(string npcId)
    {
        switch (npcId)
        {
            case "elder": return ElderId;
            case "smith": return SmithId;
            case "merchant": return MerchantId;
            case "ruen": return RuenId;
            case "sena": return SenaId;
            case "eve": return EveId;
        }
        return ElderId;
    }

    private static void BuildNpcQuests()
    {
        BuildGreeting(90, RuenTalk, RuenId, "조용히 걸어. 발소리가 크면 사냥감이 먼저 도망가거든.");
        BuildGreeting(91, SenaTalk, SenaId, "약초는 뿌리가 마르기 전에 캐야 해요. 해가 지면 늪 근처는 위험하니 조심해요.");
        BuildGreeting(92, EveTalk, EveId, "길은 발이 먼저 알아. 어디든 한 번은 직접 가 봐야지.");

        int id = QuestConvBase;
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            int npc = ActorOf(d.Giver);
            BuildOffer(id++, d, npc);
            BuildReport(id++, d, npc);
        }
    }

    private static void BuildGreeting(int id, string title, int npc, string line)
    {
        Conv v = NewConv(id, title, PlayerId, npc);
        Chain(v, Start(v), npc, line);
    }

    /// <summary>수락 대화: NPC 설명 → [수락한다 / 나중에]. "수락한 NPC 와 대화" 목표(Q101)는 수락과 동시에 보고까지 끝낸다</summary>
    private static void BuildOffer(int convId, NpcQuestDef d, int npc)
    {
        Conv v = NewConv(convId, OfferConversation(d.Id), PlayerId, npc);
        DialogueEntry hub = Chain(v, Start(v), npc, d.Description);
        DialogueEntry yes = Line(v, PlayerId, "수락한다");
        DialogueEntry no = Line(v, PlayerId, "나중에");
        Link(v, hub, yes);
        Link(v, hub, no);
        bool instant = d.Objective == NpcQuestObjective.Talk && d.Target == d.Giver;
        yes.userScript = instant
            ? "NpcQuestAccept('" + d.Id + "'); NpcQuestComplete('" + d.Id + "')"
            : "NpcQuestAccept('" + d.Id + "')";
        Chain(v, yes, npc, instant ? "그래, 확인했네. 약속한 보상일세." : "부탁하지. 일이 끝나면 다시 오게.");
        Chain(v, no, npc, "그래, 준비되면 다시 오게.");
    }

    private static void BuildReport(int convId, NpcQuestDef d, int npc)
    {
        Conv v = NewConv(convId, ReportConversation(d.Id), PlayerId, npc);
        DialogueEntry e = Chain(v, Start(v), npc, "해냈군. 수고했네. 약속한 보상일세.");
        e.userScript = "NpcQuestComplete('" + d.Id + "')";
    }
}
