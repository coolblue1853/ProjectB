using System.Collections.Generic;
using NUnit.Framework;

/// <summary>첫 경험 안내(11-02) 순수 규칙: 힌트 문구·본 목록 직렬화·레벨업 수치·퀘스트 보상 요약·사망 안내</summary>
public class FirstExperienceTests
{
    [Test]
    public void 힌트는_처음만_표시되고_직렬화로_복원된다()
    {
        var seen = new HashSet<string>();
        Assert.IsTrue(FirstHintRules.TryMarkSeen(seen, FirstHintRules.Kill));
        Assert.IsFalse(FirstHintRules.TryMarkSeen(seen, FirstHintRules.Kill));
        Assert.IsTrue(FirstHintRules.TryMarkSeen(seen, FirstHintRules.Portal));
        HashSet<string> back = FirstHintRules.Deserialize(FirstHintRules.Serialize(seen));
        Assert.AreEqual(2, back.Count);
        Assert.IsTrue(back.Contains("kill") && back.Contains("portal"));
        Assert.AreEqual(0, FirstHintRules.Deserialize(null).Count);
    }

    [Test]
    public void 모든_힌트에_문구가_있다()
    {
        foreach (string id in FirstHintRules.All)
        {
            Assert.IsNotEmpty(FirstHintRules.Text(id, k => "I"), id);
        }
    }

    [Test]
    public void 아이템_종류별_힌트()
    {
        Assert.AreEqual(FirstHintRules.Equip, FirstHintRules.HintForItemType("Equip"));
        Assert.AreEqual(FirstHintRules.Potion, FirstHintRules.HintForItemType("Consum"));
        Assert.IsNull(FirstHintRules.HintForItemType("Misc"));
    }

    [Test]
    public void 레벨업_수치_한_줄은_0을_뺀다()
    {
        Assert.AreEqual("HP +8", LevelUpBanner.GainText(8, 0, 0));
        Assert.AreEqual("", LevelUpBanner.GainText(0, 0, 0));
        StringAssert.Contains("+8", LevelUpBanner.GainText(8, 1, 1));
    }

    [Test]
    public void 퀘스트_보상_요약()
    {
        var d = new NpcQuestDef { Exp = 120, Gold = 300 };
        Assert.AreEqual("EXP +120   300 G", EventBanner.RewardText(d));
        Assert.AreEqual("", EventBanner.RewardText(new NpcQuestDef()));
    }

    [Test]
    public void 사망_안내는_손실_수치와_마을_부활을_담는다()
    {
        StringAssert.Contains("EXP -42", EventBanner.DeathSub(42));
        StringAssert.Contains("마을", EventBanner.DeathSub(42));
        StringAssert.Contains("손실 없음", EventBanner.DeathSub(0));
    }
}
