using System.Collections.Generic;
using NUnit.Framework;

/// <summary>결정 11-05 코드 묶음: 사망 팁 순환 · 퀘스트 접기 직렬화</summary>
public class Dec1105Tests
{
    [Test]
    public void DeathTip_CyclesThroughAll()
    {
        var seen = new HashSet<string>();
        for (int i = 0; i < EventBanner.DeathTips.Length; i++)
        {
            string tip = EventBanner.NextDeathTip();
            Assert.IsNotEmpty(tip);
            seen.Add(tip);
        }
        Assert.AreEqual(EventBanner.DeathTips.Length, seen.Count);
    }

    [Test]
    public void FoldKey_SerializeRoundTrip()
    {
        string a = QuestFoldRules.Key(QuestListRules.Tab.Active, "- 숲 -");
        string b = QuestFoldRules.Key(QuestListRules.Tab.Done, "- 마을 -");
        Assert.AreNotEqual(a, b);
        var set = new HashSet<string> { a, b };
        var back = new HashSet<string>();
        QuestFoldRules.Deserialize(QuestFoldRules.Serialize(set), back);
        Assert.IsTrue(back.SetEquals(set));
        QuestFoldRules.Deserialize(null, back);
        Assert.AreEqual(0, back.Count);
    }
}
