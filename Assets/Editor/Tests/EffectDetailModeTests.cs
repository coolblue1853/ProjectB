using System.Linq;
using NUnit.Framework;

/// <summary>REWORK-12 P6: 효과 상세 토글 문구와 세트 창 Loc 키.</summary>
public class EffectDetailModeTests
{
    [TearDown]
    public void Off()
    {
        if (EffectDetailMode.On)
        {
            EffectDetailMode.Toggle();
        }
    }

    [Test]
    public void Toggle_FlipsState()
    {
        Assert.IsFalse(EffectDetailMode.On);
        Assert.IsTrue(EffectDetailMode.Toggle());
        Assert.IsFalse(EffectDetailMode.Toggle());
    }

    [Test]
    public void DetailLine_ModAndRule_StatusReason()
    {
        var mod = new EffectEntry { kind = EffectKind.Mod, selector = "all" };
        var rule = new EffectEntry { kind = EffectKind.Rule, selector = "all" };
        string m = EffectTooltip.DetailLine(mod, " (capped)", s => s);
        StringAssert.Contains("Modifier", m);
        StringAssert.Contains("Capped", m);
        string r = EffectTooltip.DetailLine(rule, " (inactive)", s => s);
        StringAssert.Contains("Rule", r);
        StringAssert.Contains("Off: skill rule slots are full", r);
        StringAssert.DoesNotContain(" / Capped", EffectTooltip.DetailLine(mod, "", s => s));
    }

    [Test]
    public void SetWindowKeys_HaveKorean()
    {
        string[] keys = { "Worn Set Effects", "No set worn", "(active tiers {0})", "Next tier ({0}): {1} more pieces", "All tiers active", "Worn set effects (set-colored tier = active)", "Tab / Y: show details", "Tab / Y: hide details" };
        foreach (string k in keys)
        {
            Assert.IsTrue(LocUi.Entries.Any(x => x.en == k && x.ko.Length > 0), k);
        }
    }
}
