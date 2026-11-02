using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>UI 기능 2차(11-02) 순수 규칙: 퀘스트 추적·구역 이름·목록 정렬, 조작 안내 표, HUD 숫자, 알림 중복 방지, 미니맵 NPC 점.</summary>
public class UiFunc2Tests
{
    private List<NpcQuestDef> _active;

    [SetUp]
    public void SetUp()
    {
        NpcQuestTrack.Reset();
        _active = new List<NpcQuestDef>();
        for (int i = 0; i < 5; i++)
        {
            _active.Add(NpcQuestCatalog.All[i]);
        }
    }

    [TearDown]
    public void TearDown()
    {
        NpcQuestTrack.Reset();
    }

    // ── 퀘스트 추적 ──

    [Test]
    public void 추적_지정_전에는_진행_중_앞_3개를_자동_추적한다()
    {
        List<NpcQuestDef> t = NpcQuestTrack.Tracked(_active);
        Assert.AreEqual(3, t.Count);
        Assert.AreEqual(_active[0].Id, t[0].Id);
        Assert.AreEqual(_active[2].Id, t[2].Id);
    }

    [Test]
    public void 추적을_끄면_지정_모드가_되고_빈_자리에_다른_퀘스트를_켤_수_있다()
    {
        Assert.IsTrue(NpcQuestTrack.Toggle(_active[0].Id, _active)); // 켜져 있던 것을 끔
        Assert.IsFalse(NpcQuestTrack.IsTracked(_active[0].Id, _active));
        Assert.AreEqual(2, NpcQuestTrack.Tracked(_active).Count);
        Assert.IsTrue(NpcQuestTrack.Toggle(_active[4].Id, _active));
        Assert.IsTrue(NpcQuestTrack.IsTracked(_active[4].Id, _active));
        Assert.AreEqual(3, NpcQuestTrack.Tracked(_active).Count);
    }

    [Test]
    public void 추적은_3개까지만_켤_수_있다()
    {
        Assert.IsFalse(NpcQuestTrack.Toggle(_active[3].Id, _active));
        Assert.IsFalse(NpcQuestTrack.IsTracked(_active[3].Id, _active));
        Assert.AreEqual(3, NpcQuestTrack.Tracked(_active).Count);
    }

    [Test]
    public void 새로_수락한_퀘스트는_지정_모드에서_자리가_있을_때만_붙는다()
    {
        NpcQuestTrack.Toggle(_active[0].Id, _active);
        NpcQuestTrack.OnAccepted(_active[3].Id);
        Assert.IsTrue(NpcQuestTrack.IsTracked(_active[3].Id, _active));
        NpcQuestTrack.OnAccepted(_active[4].Id); // 이미 3개
        Assert.IsFalse(NpcQuestTrack.IsTracked(_active[4].Id, _active));
    }

    [Test]
    public void 완료한_퀘스트는_추적에서_빠지고_진행_중이_아니면_보이지_않는다()
    {
        NpcQuestTrack.Toggle(_active[0].Id, _active);
        NpcQuestTrack.Toggle(_active[0].Id, _active); // 다시 켬(지정 모드)
        NpcQuestTrack.OnCompleted(_active[0].Id);
        Assert.IsFalse(NpcQuestTrack.IsTracked(_active[0].Id, _active));
        var onlyOthers = new List<NpcQuestDef> { _active[1], _active[2] };
        Assert.AreEqual(2, NpcQuestTrack.Tracked(onlyOthers).Count);
    }

    [Test]
    public void 추적_저장_문자열_왕복과_자동_모드_복원()
    {
        Assert.IsNull(NpcQuestTrack.Serialize()); // 자동 모드는 저장하지 않는다
        NpcQuestTrack.Toggle(_active[1].Id, _active);
        string text = NpcQuestTrack.Serialize();
        Assert.AreEqual(_active[0].Id + ";" + _active[2].Id, text);
        NpcQuestTrack.Reset();
        NpcQuestTrack.Deserialize(text);
        Assert.AreEqual(2, NpcQuestTrack.Tracked(_active).Count);
        Assert.IsFalse(NpcQuestTrack.IsTracked(_active[1].Id, _active));
        NpcQuestTrack.Deserialize(null);
        Assert.AreEqual(3, NpcQuestTrack.Tracked(_active).Count); // 키 없음 = 자동
        NpcQuestTrack.Deserialize("nope;" + _active[2].Id + ";" + _active[2].Id);
        Assert.AreEqual(1, NpcQuestTrack.Tracked(_active).Count); // 모르는 id·중복은 버린다
    }

    [Test]
    public void 저장_훅이_바뀌어도_빈_지정은_빈_문자열로_저장된다()
    {
        string saved = null;
        NpcQuestTrack.Saver = t => saved = t;
        try
        {
            NpcQuestTrack.Toggle(_active[0].Id, _active);
            NpcQuestTrack.Toggle(_active[1].Id, _active);
            NpcQuestTrack.Toggle(_active[2].Id, _active);
            NpcQuestTrack.Save();
            Assert.AreEqual("", saved);
            NpcQuestTrack.Deserialize(saved);
            Assert.AreEqual(0, NpcQuestTrack.Tracked(_active).Count);
        }
        finally
        {
            NpcQuestTrack.RestoreDefaults();
        }
    }

    // ── 구역 이름·목록 ──

    [Test]
    public void 퀘스트_목표_구역_이름이_처치_방문_대화_수집마다_나온다()
    {
        NpcQuestDef kill = NpcQuestCatalog.Find("Q102");
        Assert.AreEqual(Loc.Region("Forest Side Road"), NpcQuestZone.Of(kill));
        NpcQuestDef visit = NpcQuestCatalog.Find("Q601");
        Assert.AreEqual(NpcQuestCatalog.VisitName(visit.Target), NpcQuestZone.Of(visit));
        NpcQuestDef talk = NpcQuestCatalog.Find("Q101");
        Assert.AreEqual(Loc.IsKorean ? "마을" : "Village", NpcQuestZone.Of(talk));
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            Assert.IsNotEmpty(NpcQuestZone.Of(d), d.Id + " 구역 이름이 비었다");
        }
    }

    [Test]
    public void 완료_가능이_맨_위_나머지는_표_순서_완료_탭은_최근_것이_위()
    {
        var ready = new HashSet<string> { _active[2].Id, _active[4].Id };
        List<NpcQuestDef> list = QuestListRules.Active(_active, d => ready.Contains(d.Id));
        Assert.AreEqual(_active[2].Id, list[0].Id);
        Assert.AreEqual(_active[4].Id, list[1].Id);
        Assert.AreEqual(_active[0].Id, list[2].Id);
        Assert.AreEqual(_active[1].Id, list[3].Id);
        Assert.AreEqual(_active[3].Id, list[4].Id);
        var done = new HashSet<string> { _active[0].Id, _active[1].Id };
        List<NpcQuestDef> doneList = QuestListRules.Done(_active, done.Contains);
        Assert.AreEqual(2, doneList.Count);
        Assert.AreEqual(_active[1].Id, doneList[0].Id);
    }

    // ── 조작 안내 ──

    [Test]
    public void 조작_안내_표에_재지정_가능한_액션이_모두_들어_있다()
    {
        var text = new System.Text.StringBuilder();
        foreach (ControlsGuide.Section sec in ControlsGuide.Sections)
        {
            foreach (ControlsGuide.Row row in sec.Rows)
            {
                text.Append(row.Keyboard).Append(row.Pad);
            }
        }
        foreach (string name in InputBindings.Rebindable)
        {
            Assert.IsTrue(text.ToString().Contains("{" + name + "}"), name + " 가 조작 안내에 없다");
        }
    }

    [Test]
    public void 조작_안내_자리표시가_모두_채워진다()
    {
        foreach (ControlsGuide.Section sec in ControlsGuide.Sections)
        {
            foreach (ControlsGuide.Row row in sec.Rows)
            {
                Assert.IsNotEmpty(row.Label);
                foreach (bool pad in new[] { false, true })
                {
                    string filled = ControlsGuide.Fill(pad ? row.Pad : row.Keyboard, pad, (n, g) => "<" + n + ">");
                    Assert.IsFalse(filled.Contains("{") || filled.Contains("}"), row.Label + ": " + filled);
                }
            }
        }
        Assert.AreEqual("-", ControlsGuide.Fill("", false, (n, g) => n));
        Assert.AreEqual("Z+C / x", ControlsGuide.Fill("{A}+{B} / x", false, (n, g) => n == "A" ? "Z" : "C"));
    }

    // ── HUD 숫자·알림·미니맵 ──

    [Test]
    public void HUD_막대_숫자_현재_슬래시_최대()
    {
        Assert.AreEqual("80 / 100", HudBarText.Format(80, 100));
        Assert.AreEqual("0 / 100", HudBarText.Format(-5, 100));
        Assert.AreEqual("100 / 100", HudBarText.Format(120, 100));
        Assert.AreEqual("", HudBarText.Format(5, 0));
    }

    [Test]
    public void 같은_알림_문구는_2초_안에_다시_띄우지_않고_다른_문구는_바로_띄운다()
    {
        Assert.IsTrue(PickupLogModel.ShouldShowNotice(null, -99f, "가방이 가득", 0f, 2f));
        Assert.IsFalse(PickupLogModel.ShouldShowNotice("가방이 가득", 0f, "가방이 가득", 1.9f, 2f));
        Assert.IsTrue(PickupLogModel.ShouldShowNotice("가방이 가득", 0f, "가방이 가득", 2f, 2f));
        Assert.IsTrue(PickupLogModel.ShouldShowNotice("가방이 가득", 0f, "골드 부족", 0.1f, 2f));
    }

    [Test]
    public void 미니맵_NPC_점은_완료_가능이_가장_크고_금색이다()
    {
        Assert.Greater(MinimapMath.NpcDotSize(NpcQuestStatus.Ready), MinimapMath.NpcDotSize(NpcQuestStatus.Available));
        Assert.Greater(MinimapMath.NpcDotSize(NpcQuestStatus.Available), MinimapMath.NpcDotSize(NpcQuestStatus.Active));
        Assert.AreNotEqual(MinimapMath.NpcDotColor(NpcQuestStatus.Ready), MinimapMath.NpcDotColor(NpcQuestStatus.Locked));
        Assert.AreEqual(MinimapMath.NpcDotColor(NpcQuestStatus.Active), MinimapMath.NpcDotColor(NpcQuestStatus.Locked)); // 평소 초록
    }
}
