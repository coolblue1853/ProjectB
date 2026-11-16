using NUnit.Framework;

/// <summary>봇 스킬 투자·Lv 사냥터 순수 로직</summary>
public class PlayBotSkillTests
{
    private static readonly string[] Maps = { "Lv1_NearForest_SideRoad", "Lv1_NormalForest", "Lv1_DeepForest" };

    [Test]
    public void LevelMap_ByLevel()
    {
        Assert.AreEqual("Lv1_NearForest_SideRoad", BotQuestRules.LevelMap(1, Maps));
        Assert.AreEqual("Lv1_NearForest_SideRoad", BotQuestRules.LevelMap(3, Maps));
        Assert.AreEqual("Lv1_NormalForest", BotQuestRules.LevelMap(4, Maps));
        Assert.AreEqual("Lv1_DeepForest", BotQuestRules.LevelMap(8, Maps));
        Assert.IsNull(BotQuestRules.LevelMap(8, new[] { "Lv1_NormalForest" }));
    }

    [Test]
    public void Pick_NoPoints_NotFound()
    {
        Assert.IsFalse(BotSkillRules.Pick("Sword", 0, 0, (t, i) => 0).Found);
    }

    [Test]
    public void Pick_TypeFirst_ThenCommon()
    {
        BotSkillPick p = BotSkillRules.Pick("Sword", 2, 3, (t, i) => 0);
        Assert.AreEqual("P1", p.NodeId);
        Assert.AreEqual(SkillTab.Type, p.Tab);
        Assert.AreEqual(0, p.Row);
        p = BotSkillRules.Pick("Sword", 0, 3, (t, i) => 0);
        Assert.AreEqual("C5", p.NodeId);
        Assert.AreEqual(SkillTab.Combat, p.Tab);
    }

    [Test]
    public void Pick_SkipsMaxedAndPrereq()
    {
        BotSkillPick p = BotSkillRules.Pick("Sword", 1, 0, (t, i) => i == "P1" ? 5 : 0);
        Assert.AreEqual("A1", p.NodeId);
        p = BotSkillRules.Pick("Sword", 1, 0, (t, i) => i == "P1" ? 5 : i == "A1" ? 3 : i == "M1" ? 3 : 0);
        Assert.AreEqual("P2", p.NodeId); // P2 선행 P1 3
    }

    [Test]
    public void Step_TabThenRowThenConfirm()
    {
        Assert.AreEqual(BotKey.Back, BotSkillRules.Step(false, SkillTab.Type, 0, SkillTab.Type, 0, 8));
        Assert.AreEqual(BotKey.Right, BotSkillRules.Step(true, SkillTab.Type, 0, SkillTab.Combat, 1, 5));
        Assert.AreEqual(BotKey.Down, BotSkillRules.Step(true, SkillTab.Combat, 0, SkillTab.Combat, 1, 5));
        Assert.AreEqual(BotKey.Up, BotSkillRules.Step(true, SkillTab.Combat, 0, SkillTab.Combat, 4, 5));
        Assert.AreEqual(BotKey.Confirm, BotSkillRules.Step(true, SkillTab.Combat, 2, SkillTab.Combat, 2, 5));
    }
}
