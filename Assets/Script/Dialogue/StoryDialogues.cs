using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

/// <summary>
/// 대사 데이터베이스를 코드로 만든다 (Dialogue System 에디터 미사용 — 퀘스트처럼 코드 생성).
/// 스토리: TowerWorld.md 2절(아버지를 찾는 소녀, 브로치). 문구는 픽셀 폰트에 한글이 없어 영어(I2 도입 시 키로 이전, Q-407).
/// </summary>
public static class StoryDialogues
{
    public const string Opening = "Story/Opening";
    public const string Elder = "Village/Elder";
    public const string Smith = "Village/Smith";
    public const string Merchant = "Village/Merchant";
    public const string Gatekeeper = "Story/Gatekeeper";

    const int PlayerId = 1, InnerId = 2, ElderId = 3, SmithId = 4, MerchantId = 5, GatekeeperId = 6;

    static Template template;
    static DialogueDatabase db;

    public static DialogueDatabase Build()
    {
        template = Template.FromDefault();
        db = ScriptableObject.CreateInstance<DialogueDatabase>();
        db.name = "ProjectB Story";
        db.actors.Add(template.CreateActor(PlayerId, "Explorer", true));
        db.actors.Add(template.CreateActor(InnerId, "You", false));
        db.actors.Add(template.CreateActor(ElderId, "Village Elder", false));
        db.actors.Add(template.CreateActor(SmithId, "Smith Brann", false));
        db.actors.Add(template.CreateActor(MerchantId, "Merchant Ilsa", false));
        db.actors.Add(template.CreateActor(GatekeeperId, "Gatekeeper of the Tower", false));

        BuildOpening(1);
        BuildElder(2);
        BuildSmith(3);
        BuildMerchant(4);
        BuildGatekeeper(5);
        return db;
    }

    public static void SetPortrait(string actorName, Sprite sprite)
    {
        Actor a = DialogueManager.masterDatabase != null ? DialogueManager.masterDatabase.GetActor(actorName) : null;
        if (a != null)
        {
            a.spritePortrait = sprite;
        }
    }

    // ── 대화 빌더 ──

    sealed class Conv
    {
        public Conversation c;
        public int next = 1;
    }

    static Conv NewConv(int id, string title, int actor, int conversant)
    {
        Conversation c = template.CreateConversation(id, title);
        c.ActorID = actor;
        c.ConversantID = conversant;
        DialogueEntry start = template.CreateDialogueEntry(0, id, "START");
        start.ActorID = actor;
        start.ConversantID = conversant;
        start.Sequence = "None()";
        c.dialogueEntries.Add(start);
        db.conversations.Add(c);
        return new Conv { c = c };
    }

    static DialogueEntry Line(Conv v, int speaker, string text)
    {
        DialogueEntry e = template.CreateDialogueEntry(v.next++, v.c.id, "");
        e.ActorID = speaker;
        e.ConversantID = speaker == PlayerId ? v.c.ConversantID : PlayerId;
        e.DialogueText = text;
        v.c.dialogueEntries.Add(e);
        return e;
    }

    static void Link(Conv v, DialogueEntry from, DialogueEntry to)
    {
        from.outgoingLinks.Add(new Link(v.c.id, from.id, v.c.id, to.id));
    }

    /// <summary>순서대로 잇는다. 마지막 항목을 돌려준다</summary>
    static DialogueEntry Chain(Conv v, DialogueEntry from, int speaker, params string[] lines)
    {
        DialogueEntry prev = from;
        foreach (string t in lines)
        {
            DialogueEntry e = Line(v, speaker, t);
            Link(v, prev, e);
            prev = e;
        }
        return prev;
    }

    static DialogueEntry Start(Conv v) => v.c.dialogueEntries[0];

    /// <summary>NPC 질문 메뉴: 각 선택지 → 답변 줄들 → 다시 메뉴. 마지막 선택지는 작별(끝)</summary>
    static void Menu(Conv v, DialogueEntry hub, int npc, List<(string ask, string[] answer)> topics, string bye, string byeReply)
    {
        foreach (var (ask, answer) in topics)
        {
            DialogueEntry q = Line(v, PlayerId, ask);
            Link(v, hub, q);
            DialogueEntry last = Chain(v, q, npc, answer);
            Link(v, last, hub);
        }
        DialogueEntry leave = Line(v, PlayerId, bye);
        Link(v, hub, leave);
        Chain(v, leave, npc, byeReply);
    }

    static void BuildOpening(int id)
    {
        Conv v = NewConv(id, Opening, PlayerId, InnerId);
        Chain(v, Start(v), InnerId,
            "The Tower. It swallows the sky above the village.",
            "Father climbed it to find a cure for me... and never came back down.",
            "This brooch was his. If I fall up there, it will carry me home.",
            "Whatever waits on the higher floors - I'm going to find him.");
    }

    static void BuildElder(int id)
    {
        Conv v = NewConv(id, Elder, PlayerId, ElderId);
        DialogueEntry hub = Chain(v, Start(v), ElderId,
            "Ah... you wear his brooch. So you are the child he spoke of.");
        hub = Chain(v, hub, ElderId, "Ask what you must, explorer.");
        // 퀘스트 연동: 이 줄이 끝나면(대화 종료) 촌장 퀘스트 창(Quest Machine)이 열린다 — DialogueBoot.OpenElderQuests
        DialogueEntry askWork = Line(v, PlayerId, "Is there anything I can do?");
        Link(v, hub, askWork);
        DialogueEntry work = Chain(v, askWork, ElderId, "There is. Let me tell you what the hunters found.");
        work.userScript = "OpenElderQuests()";
        Menu(v, hub, ElderId, new List<(string, string[])>
        {
            ("About my father...", new[]
            {
                "He passed through here many winters ago, set on reaching the top.",
                "Rumor says the Tower changed him. That he now guards one of its gates.",
            }),
            ("Why is climbing so hard?", new[]
            {
                "The Tower presses on those who climb. We call it the ascent load.",
                "Each floor weighs more. Earn higher ranks as an explorer and you will bear it.",
                "Going down is always free. Coming back up is the test.",
            }),
            ("What does the brooch do?", new[]
            {
                "If you fall, it pulls you back to this village.",
                "What you wear stays with you. Loose spoils... the Tower may keep.",
            }),
        }, "I should go.", "Return alive. That is all an old man asks.");
    }

    static void BuildSmith(int id)
    {
        Conv v = NewConv(id, Smith, PlayerId, SmithId);
        DialogueEntry hub = Chain(v, Start(v), SmithId,
            "Steel's hot. What do you need?");
        Menu(v, hub, SmithId, new List<(string, string[])>
        {
            ("How do I get stronger gear?", new[]
            {
                "Bring me Floor Crystals. I can temper your gear up to +10.",
                "Past +5 the metal fights back. Failures happen - don't cry about it.",
            }),
            ("Tell me about gear sets.", new[]
            {
                "Pieces cut from the same monster resonate when worn together.",
                "Two, three, four pieces - each wakes a stronger bonus. Hunt for the full set.",
            }),
        }, "Thanks, Brann.", "Don't break it on the first floor.");
    }

    static void BuildMerchant(int id)
    {
        Conv v = NewConv(id, Merchant, PlayerId, MerchantId);
        DialogueEntry hub = Chain(v, Start(v), MerchantId,
            "Welcome! Potions, food, odds and ends - fresh from the lower floors.");
        Menu(v, hub, MerchantId, new List<(string, string[])>
        {
            ("Any advice for the forest?", new[]
            {
                "The outskirts are calm by day. At night the rats come out, big ones.",
                "Carry stew. A full belly has saved more explorers than any sword.",
            }),
            ("What sells well?", new[]
            {
                "Relics from higher floors fetch a fortune. Monster parts, not so much.",
            }),
        }, "See you later.", "Come back with coin!");
    }

    /// <summary>1층계 보스 첫 조우(BossEncounterTalk). 아버지 = 문지기 설정(TowerWorld 2절)은 암시만 한다 — 확정하지 않는다</summary>
    static void BuildGatekeeper(int id)
    {
        Conv v = NewConv(id, Gatekeeper, PlayerId, GatekeeperId);
        DialogueEntry e = Chain(v, Start(v), GatekeeperId,
            "...None... shall pass... the gate.");
        e = Chain(v, e, InnerId,
            "A giant in armor, older than the ruins around it.",
            "That crest on its chest... it's the same pattern as my brooch.",
            "...Father?");
        e = Chain(v, e, GatekeeperId,
            "...Turn... back...");
        Chain(v, e, InnerId,
            "No. Whatever you are, I'm going up.");
    }
}
