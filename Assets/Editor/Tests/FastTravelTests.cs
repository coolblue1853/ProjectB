using System.Collections.Generic;
using NUnit.Framework;

/// <summary>빠른 이동(FastTravel.md) 순수 규칙·상태·표 테스트</summary>
public class FastTravelTests
{
    private const string Csv =
        "id,order,floor,displayName,anchorX,anchorY,questId,npcName,alwaysOpen\n" +
        "Town,0,1,마을,-393,,,coach_town,1\n" +
        "Side,1,1,샛길,-421.5,,,coach_side,1\n" +
        "Deep,3,1,깊은 숲,-421.5,,FT_Q_Deep,coach_deep,\n" +
        "Cliffs,7,2,절벽,-413,-179.4,FT_Q_Cliffs,coach_cliffs,\n";

    private int _saves;

    [SetUp]
    public void SetUp()
    {
        FastTravelStops.SetTableForTests(Csv);
        FastTravelState.Reset();
        _saves = 0;
        FastTravelState.SaveHook = () => _saves++;
    }

    [TearDown]
    public void TearDown()
    {
        FastTravelStops.SetTableForTests(null);
        FastTravelState.Reset();
        FastTravelState.ResetSaveHook();
    }

    [Test] public void 요금은_50에_순번차_25를_더하고_2층계는_두배()
    {
        Assert.AreEqual(150, FastTravelRules.Fee(0, 4, 1)); // 마을→늪
        Assert.AreEqual(75, FastTravelRules.Fee(3, 2, 1));
        Assert.AreEqual(450, FastTravelRules.Fee(0, 7, 2)); // 마을→절벽
        Assert.AreEqual(100, FastTravelRules.Fee(5, 5, 2), "같은 순번도 기본요금 (x2 적용)");
    }

    [Test] public void 이동_금지_사유는_사망_잠금_보스_전투_골드_순()
    {
        Assert.AreEqual(FastTravelRules.Block.Dead, FastTravelRules.Check(true, true, 0f, false, 0, 100));
        Assert.AreEqual(FastTravelRules.Block.Locked, FastTravelRules.Check(false, true, 0f, false, 0, 100));
        Assert.AreEqual(FastTravelRules.Block.Boss, FastTravelRules.Check(false, true, 0f, true, 0, 100));
        Assert.AreEqual(FastTravelRules.Block.Combat, FastTravelRules.Check(false, false, 2.9f, true, 0, 100));
        Assert.AreEqual(FastTravelRules.Block.NoGold, FastTravelRules.Check(false, false, 3f, true, 99, 100));
        Assert.AreEqual(FastTravelRules.Block.None, FastTravelRules.Check(false, false, -1f, true, 100, 100));
    }

    [Test] public void 표_파싱_앵커와_기본_열림()
    {
        Assert.AreEqual(4, FastTravelStops.All.Count);
        FastTravelStop cl = FastTravelStops.Find("Cliffs");
        Assert.AreEqual(7, cl.Order);
        Assert.AreEqual(2, cl.Floor);
        Assert.AreEqual(-413f, cl.AnchorX.Value, 0.001f);
        Assert.AreEqual(-179.4f, cl.AnchorY.Value, 0.001f);
        Assert.IsFalse(FastTravelStops.Find("Town").AnchorY.HasValue);
        Assert.IsTrue(FastTravelStops.Find("Town").AlwaysOpen);
        Assert.IsFalse(cl.AlwaysOpen);
    }

    [Test] public void 기본_정류장은_열려_있고_나머지는_잠겨_있다()
    {
        Assert.IsTrue(FastTravelState.IsOpen("Town"));
        Assert.IsTrue(FastTravelState.IsOpen("Side"));
        Assert.IsFalse(FastTravelState.IsOpen("Deep"));
        Assert.IsFalse(FastTravelState.IsOpen("없는맵"));
    }

    [Test] public void 해금은_한_번만_저장하고_중복_해금은_무시한다()
    {
        Assert.IsTrue(FastTravelState.Unlock("Deep"));
        Assert.IsFalse(FastTravelState.Unlock("Deep"));
        Assert.IsFalse(FastTravelState.Unlock("Town"), "항상 열린 곳은 저장하지 않는다");
        Assert.IsFalse(FastTravelState.Unlock("없는맵"));
        Assert.AreEqual(1, _saves);
        Assert.AreEqual("Deep", FastTravelState.Serialize());
    }

    [Test] public void 직렬화_왕복과_중복_공백_제거()
    {
        FastTravelState.Deserialize(" Deep ;Cliffs;Deep;;");
        Assert.AreEqual("Deep;Cliffs", FastTravelState.Serialize());
        Assert.IsTrue(FastTravelState.IsOpen("Cliffs"));
        FastTravelState.Deserialize("");
        Assert.IsFalse(FastTravelState.IsOpen("Cliffs"));
        Assert.AreEqual("", FastTravelState.Serialize());
    }

    [Test] public void 구세이브는_완료한_퀘스트로_자동_해금된다()
    {
        var done = new HashSet<string> { "FT_Q_Cliffs" };
        Assert.AreEqual(1, FastTravelState.AutoUnlock(done.Contains));
        Assert.IsTrue(FastTravelState.IsOpen("Cliffs"));
        Assert.IsFalse(FastTravelState.IsOpen("Deep"));
        Assert.AreEqual(0, FastTravelState.AutoUnlock(done.Contains), "두 번째는 새로 열린 곳 없음");
    }

    [Test] public void 실제_표는_정류장마다_맵이_고유하고_해금_퀘스트가_실존한다()
    {
        FastTravelStops.SetTableForTests(null);
        var seen = new HashSet<string>();
        foreach (FastTravelStop s in FastTravelStops.All)
        {
            Assert.IsTrue(seen.Add(s.Id), "맵 중복 " + s.Id);
            if (!s.AlwaysOpen)
            {
                NpcQuestDef q = NpcQuestCatalog.Find(s.QuestId);
                Assert.IsNotNull(q, s.Id + " 해금 퀘스트 없음: " + s.QuestId);
                Assert.AreEqual(s.NpcId, q.Giver, s.QuestId + " giver 는 그 정류장 마차 NPC");
            }
        }
        Assert.GreaterOrEqual(seen.Count, 9);
    }

    [Test]
    public void InCooldown_BlocksRapidSecondTravel()
    {
        Assert.IsTrue(FastTravelRules.InCooldown(10.5f, 10f), "0.5초 뒤 재진입 차단");
        Assert.IsFalse(FastTravelRules.InCooldown(12f, 10f), "가드 시간 경과 후 허용");
        Assert.IsFalse(FastTravelRules.InCooldown(0f, -999f), "첫 이동은 허용");
    }
}
