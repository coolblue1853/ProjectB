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
        public string rewardItem;
        public string requires;    // 선행 퀘스트 id (없으면 null)
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
            targets = new[] { "FlowerMole_BD", "LeftMole_BD" }, count = 8, gold = 300, stones = 2,
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

    public static Def Find(string id)
    {
        return Chain.Find(d => d.id == id);
    }
}
