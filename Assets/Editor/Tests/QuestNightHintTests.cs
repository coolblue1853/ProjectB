using NUnit.Framework;

public class QuestNightHintTests
{
    // TimeChange 실값: 하루 1200초, 밤 0.5~0.9 (600~1080초)
    private const float Total = 1200f, Start = 0.5f, End = 0.9f;

    [TestCase(0f, 600f)]
    [TestCase(0.25f, 300f)]
    [TestCase(0.5f, 0f)]
    [TestCase(0.9f, 720f)]
    [TestCase(0.95f, 660f)]
    [TestCase(1f, 600f)]
    public void 낮이면_밤까지_남은_초를_센다(float value, float expected)
    {
        Assert.AreEqual(expected, QuestNightHint.SecondsUntilNight(value, Total, Start, End), 0.01f);
    }

    [TestCase(0.6f)]
    [TestCase(0.89f)]
    public void 밤이면_음수(float value)
    {
        Assert.Less(QuestNightHint.SecondsUntilNight(value, Total, Start, End), 0f);
    }

    [Test]
    public void 잘못된_설정은_음수()
    {
        Assert.Less(QuestNightHint.SecondsUntilNight(0.2f, 0f, Start, End), 0f);
        Assert.Less(QuestNightHint.SecondsUntilNight(0.2f, Total, 0.9f, 0.5f), 0f);
    }

    [TestCase("Q104", true)]
    [TestCase("Q205", true)]
    [TestCase("Q106", false)]
    [TestCase("Q102", false)]
    [TestCase("Q204", false)]
    public void 밤_전용_목표_판정(string id, bool expected)
    {
        Assert.AreEqual(expected, QuestNightHint.IsNightOnly(NpcQuestCatalog.Find(id)), id);
    }

    [Test]
    public void 안내_문구는_올림_분이고_1분_미만도_1분()
    {
        NpcQuestDef d = NpcQuestCatalog.Find("Q104");
        Assert.AreEqual("밤까지 약 5분", QuestNightHint.Line(d, false, 300f));
        Assert.AreEqual("밤까지 약 6분", QuestNightHint.Line(d, false, 301f));
        Assert.AreEqual("밤까지 약 1분", QuestNightHint.Line(d, false, 5f));
    }

    [Test]
    public void 완료_가능_밤_낮고정_비밤전용은_빈_문자열()
    {
        NpcQuestDef night = NpcQuestCatalog.Find("Q104");
        Assert.AreEqual("", QuestNightHint.Line(night, true, 300f));
        Assert.AreEqual("", QuestNightHint.Line(night, false, -1f));
        Assert.AreEqual("", QuestNightHint.Line(NpcQuestCatalog.Find("Q102"), false, 300f));
        Assert.AreEqual("", QuestNightHint.Line(null, false, 300f));
    }

    [Test]
    public void 밤_퀘스트_설명에_밤에만_표기가_있다()
    {
        foreach (string id in new[] { "Q104", "Q205", "Q403", "Q311", "Q411" })
        {
            Assert.IsTrue(NpcQuestCatalog.Find(id).Description.Contains("밤에만"), id);
        }
    }

    [Test]
    public void 낮_샛길_쥐_도둑은_토끼_호스트에서도_RatThief_처치_키를_보낸다()
    {
        // Q106 이 낮에도 진행되도록 낮 스포너(Rabbit2·Rabbit3)에 RatThief_BD 프로필을 섞는다. killKey 가 비면 호스트(토끼) id 로 처치가 세어져 퀘스트가 안 오른다.
        Assert.AreEqual("RatThief_BD", MonsterTable.Get("RatThief_BD").killKey);
        var rows = HuntingSpawnTable.Parse(UnityEngine.Resources.Load<UnityEngine.TextAsset>("HuntingSpawnsFloor1").text);
        Assert.AreEqual("RatThief_BD", rows["Lv1_NearForest_SideRoad/Rabbit2"].Profile);
        Assert.AreEqual("RatThief_BD", rows["Lv1_NearForest_SideRoad/Rabbit3"].Profile);
        Assert.Less(rows["Lv1_NearForest_SideRoad/Rabbit3"].ProfileRatio, 0.5f);
    }
}
