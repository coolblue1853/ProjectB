using System.Collections.Generic;
using NUnit.Framework;

/// <summary>퀘스트 창 3탭(받을 수 있는/진행 중/완료) 순수 규칙, NPC 대화 재제안 규칙(NPC_BLOCK 수정)</summary>
public class QuestListTabsTests
{
    private static NpcQuestDef Q(string id) => new NpcQuestDef { Id = id, Title = id };

    [Test]
    public void 탭은_전체_받을_수_있는_진행_중_완료_순으로_순환한다()
    {
        Assert.AreEqual(QuestListRules.Tab.Active, QuestListRules.Step(QuestListRules.Tab.Available, 1));
        Assert.AreEqual(QuestListRules.Tab.Done, QuestListRules.Step(QuestListRules.Tab.Active, 1));
        Assert.AreEqual(QuestListRules.Tab.All, QuestListRules.Step(QuestListRules.Tab.Done, 1), "U6: 전체 탭이 맨 앞");
        Assert.AreEqual(QuestListRules.Tab.All, QuestListRules.Step(QuestListRules.Tab.Available, -1));
    }

    [Test]
    public void 받을_수_있는_목록은_수락_가능이_먼저_레벨_부족이_뒤_나머지는_빠진다()
    {
        var all = new List<NpcQuestDef> { Q("a"), Q("b"), Q("c"), Q("d"), Q("e") };
        var st = new Dictionary<string, NpcQuestStatus>
        {
            { "a", NpcQuestStatus.LevelLow }, { "b", NpcQuestStatus.Available }, { "c", NpcQuestStatus.Active },
            { "d", NpcQuestStatus.Available }, { "e", NpcQuestStatus.Done },
        };
        List<NpcQuestDef> res = QuestListRules.Available(all, d => st[d.Id]);
        Assert.AreEqual(new[] { "b", "d", "a" }, res.ConvertAll(d => d.Id).ToArray());
    }

    [Test]
    public void 구역별_묶음은_구역_첫_등장_순서와_구역_안_원래_순서를_지킨다()
    {
        var list = new List<NpcQuestDef> { Q("a1"), Q("b1"), Q("a2"), Q("c1"), Q("b2") };
        List<NpcQuestDef> res = QuestListRules.GroupByZone(list, d => d.Id.Substring(0, 1));
        Assert.AreEqual(new[] { "a1", "a2", "b1", "b2", "c1" }, res.ConvertAll(d => d.Id).ToArray());
    }

    [Test]
    public void 실제_표의_모든_퀘스트가_구역_묶음에_빠짐없이_들어간다()
    {
        List<NpcQuestDef> res = QuestListRules.GroupByZone(new List<NpcQuestDef>(NpcQuestCatalog.All), NpcQuestZone.Of);
        Assert.AreEqual(NpcQuestCatalog.All.Count, res.Count);
    }
}
