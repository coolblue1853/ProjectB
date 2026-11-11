using System.Linq;
using NUnit.Framework;

/// <summary>퀘스트 대사 CSV 이행(QuestDesign Q-QD2): CSV 열 우선·빈 열은 기존 대사·형식 상한</summary>
public class QuestVoiceCsvTests
{
    [SetUp]
    public void SetUp()
    {
        NpcQuestCatalog.SetTableForTests(null);
    }

    [TestCase("Q204")]
    [TestCase("Q301")]
    [TestCase("Q403")]
    [TestCase("Q502")]
    [TestCase("Q601")]
    [TestCase("Q604")]
    public void 이행된_퀘스트는_CSV_열_대사를_쓴다(string id)
    {
        NpcQuestDef d = NpcQuestCatalog.Find(id);
        Assert.IsNotEmpty(d.OfferLine, id);
        Assert.IsNotEmpty(d.ProgressLine, id);
        Assert.IsNotEmpty(d.DoneLine, id);
        Assert.AreEqual(d.OfferLine.Replace('^', '\n').Split('|'), NpcQuestVoice.Offer(d));
        Assert.AreEqual(d.ProgressLine.Replace('^', '\n'), NpcQuestVoice.Progress(d));
        Assert.AreEqual(d.DoneLine.Replace('^', '\n'), NpcQuestVoice.Report(d));
    }

    [Test]
    public void 열이_비어_있으면_기존_코드_대사를_쓴다()
    {
        NpcQuestDef d = NpcQuestCatalog.Find("Q202");
        Assert.IsEmpty(d.OfferLine);
        Assert.IsEmpty(d.ProgressLine);
        Assert.IsEmpty(d.DoneLine);
        Assert.IsTrue(NpcQuestVoice.Offer(d)[0].Contains("멧돼지"));
        Assert.IsTrue(NpcQuestVoice.Progress(d).Contains("멧돼지"));
        Assert.IsTrue(NpcQuestVoice.Report(d).Contains("단검"));
    }

    [Test]
    public void 모든_퀘스트의_대사가_비어_있지_않다()
    {
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            Assert.IsTrue(NpcQuestVoice.Offer(d).All(p => !string.IsNullOrWhiteSpace(p)), d.Id + " offer");
            Assert.IsNotEmpty(NpcQuestVoice.Progress(d), d.Id + " progress");
            Assert.IsNotEmpty(NpcQuestVoice.Report(d), d.Id + " report");
        }
    }

    [Test]
    public void CSV_대사_열은_형식_상한을_지킨다()
    {
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (!string.IsNullOrEmpty(d.OfferLine))
            {
                string[] pages = d.OfferLine.Split('|');
                Assert.LessOrEqual(pages.Length, 2, d.Id + " 수락 2페이지 이내");
                foreach (string page in pages)
                {
                    Assert.LessOrEqual(page.Count(c => c == '^'), 1, d.Id + " 한 페이지 2줄 이내");
                }
            }
            // 진행·완료는 한 덩어리 문자열로 나가므로 페이지 구분 `|` 는 쓰지 않는다
            foreach (string line in new[] { d.ProgressLine, d.DoneLine })
            {
                if (!string.IsNullOrEmpty(line))
                {
                    Assert.IsFalse(line.Contains("|"), d.Id + " 진행·완료는 1페이지");
                    Assert.LessOrEqual(line.Count(c => c == '^'), 1, d.Id + " 2줄 이내");
                }
            }
        }
    }
}
