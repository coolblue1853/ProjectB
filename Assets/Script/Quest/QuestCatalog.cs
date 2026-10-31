using System.Collections.Generic;

/// <summary>
/// 메이플식 사냥 퀘스트 체인 정의 (G11, ProgressionDesign 0절 "사냥터 순차 진행").
/// 처치 대상은 몬스터 프리팹 이름(Clone 제외) — EnemyHealth 가 처치 시 MessageSystem "Kill" 메시지로 보낸다.
/// 앞 퀘스트를 완료해야 다음 퀘스트가 게시판에 뜬다.
/// </summary>
public static class QuestCatalog
{
    public class Def
    {
        public string id, title, offer, journal;
        public string[] targets;   // 하나라도 맞으면 카운트
        public int count;
        public int gold, stones;
        public int exp;                 // 보상 EXP (LevelSystem.GainExp)
        public string rewardItem;
        public string requires;    // 선행 퀘스트 id (없으면 null)
        public string message = "Kill"; // 카운트할 MessageSystem 메시지 (Kill / Pickup / Craft)
        public ArtifactId artifact;     // 완료 시 해금 아티팩트
        public string report;           // 보고 대사(없으면 기본 문구)
    }

    public static readonly List<Def> Chain = new List<Def>
    {
        new Def
        {
            id = "q_rabbits", title = "Rabbit Trouble",
            offer = "The rabbits by the forest road are eating our crops. Drive off 10 of them!",
            journal = "Hunt rabbits on the forest road.",
            targets = new[] { "Rabbit_BD", "StoneRabbit_BD" }, count = 10, gold = 150, stones = 1,
        },
        new Def
        {
            id = "q_moles", title = "Mole Menace",
            offer = "Moles are digging under the side road. Take down 8 of them.",
            journal = "Hunt moles on the forest side road.",
            targets = new[] { "FlowerMole_BD", "LeftMole_BD" }, count = 8, gold = 300, stones = 2, artifact = ArtifactId.StarBurst,
            requires = "q_rabbits",
        },
        new Def
        {
            id = "q_terror", title = "The Passive Terror",
            offer = "Something lurks on the side road at night. Defeat the Passive Terror.",
            journal = "Defeat the Passive Terror at night.",
            targets = new[] { "PassiveTerror_BD" }, count = 1, gold = 600, stones = 3,
            requires = "q_moles",
        },
        new Def
        {
            id = "q_goblins", title = "Into the Cliffs",
            offer = "Goblins have taken the Magic Cliffs. Clear out 10 of them.",
            journal = "Hunt Cliff Goblins at the Magic Cliffs.",
            targets = new[] { "CliffGoblin_BD" }, count = 10, gold = 1000, stones = 4,
            requires = "q_terror",
        },
        new Def
        {
            id = "q_wizard", title = "The Abyss Wizard",
            offer = "An Abyss Wizard commands the cliffs at night. End his reign.",
            journal = "Defeat the Abyss Wizard at the cliff altar.",
            targets = new[] { "AbyssWizard_BD" }, count = 1, gold = 3000, stones = 8,
            requires = "q_goblins",
        },
    };

    /// <summary>체인과 별개로 처음부터 받을 수 있는 부가 퀘스트 (수집·제작)</summary>
    public static readonly List<Def> Side = new List<Def>
    {
        new Def
        {
            id = "s_wood", title = "Firewood", message = "Pickup",
            offer = "Winter is coming. Bring me 10 pieces of wood from the forest.",
            journal = "Pick up wood in the forest.",
            targets = new[] { "Wood" }, count = 10, gold = 120, stones = 0,
        },
        new Def
        {
            id = "s_herb", title = "Healer's Request", message = "Pickup",
            offer = "Gather 6 green herbs for the village healer.",
            journal = "Pick up green herbs.",
            targets = new[] { "GreenHurb" }, count = 6, gold = 150, stones = 1,
        },
        new Def
        {
            id = "s_craft", title = "Smith's Apprentice", message = "Craft",
            offer = "Show me you can work a forge — craft any 2 pieces of gear at the smiths.",
            journal = "Craft 2 items at the smiths.",
            targets = new[] { "Any" }, count = 2, gold = 200, stones = 2,
        },
    };

    /// <summary>촌장이 대화로 주는 스토리 퀘스트 (TowerWorld 2절 — 아버지의 흔적). 게시판에는 뜨지 않는다</summary>
    public static readonly List<Def> Story = new List<Def>
    {
        new Def
        {
            id = "st_father1", title = "Father's Traces I",
            offer = "Hunters found strange marks carved on the trees of the Deep Forest — the same crest as your brooch. " +
                    "An Ancient Ent has taken root over that grove. Defeat it, and see what your father left behind.",
            journal = "Defeat the Ancient Ent in the Deep Forest.",
            targets = new[] { "AncientEnt_BD" }, count = 1, gold = 800, stones = 3,
            report = "The crest was carved deep into its heartwood... He passed that grove alive, and kept climbing. " +
                     "His trail leads to the Tower Outskirts. Rest first, child.",
        },
        new Def
        {
            id = "st_father2", title = "Father's Traces II",
            offer = "Beyond the swamp lie the Tower Outskirts. The stones there walk, they say — golems that guard the old ruins. " +
                    "If your father carved his crest on the trees, he surely marked the stones too. Break 12 golems and read what they carry.",
            journal = "Defeat 12 golems in the Tower Outskirts.",
            targets = new[] { "GolemSmall_BD", "GolemMedium_BD", "GolemLarge_BD" }, count = 12, gold = 1500, stones = 5,
            requires = "st_father1",
            report = "The same crest, worn into the golems' cores... as if they were made to guard something he swore to protect. " +
                     "Every mark points to the Tower gate. Something waits there, child. Something old.",
        },
        new Def
        {
            id = "st_father3", title = "Father's Traces III",
            offer = "An armored giant stands before the Tower gate. No one who faced it has returned to tell us its face. " +
                    "If you mean to follow him upward, you must pass it.",
            journal = "Defeat the Gatekeeper of the Tower.",
            targets = new[] { "Gatekeeper_BD" }, count = 1, gold = 3000, stones = 8,
            requires = "st_father2",
            report = "It wore his crest on its chest...? No — I will not guess at what I cannot know. " +
                     "The gate is open now. Whatever is up there, your brooch will bring you home. Go, and come back.",
        },
    };

    public static IEnumerable<Def> All()
    {
        foreach (Def d in Chain) yield return d;
        foreach (Def d in Side) yield return d;
    }

    public static Def Find(string id)
    {
        Def d = Chain.Find(x => x.id == id) ?? Side.Find(x => x.id == id);
        return d ?? Story.Find(x => x.id == id);
    }
}
