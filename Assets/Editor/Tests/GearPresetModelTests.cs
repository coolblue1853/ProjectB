using System.Collections.Generic;
using NUnit.Framework;

public class GearPresetModelTests
{
    private static Dictionary<string, GearPresetModel.Entry> Two()
    {
        return new Dictionary<string, GearPresetModel.Entry>
        {
            { "Weapon", new GearPresetModel.Entry { Name = "RustedSword", Tier = 0, Upgrade = 3 } },
            { "Head", new GearPresetModel.Entry { Name = "Chief's Cap: Old", Tier = 1, Upgrade = 0 } },
        };
    }

    [Test]
    public void SaveRoundTrip_KeepsTierUpgradeAndColonNames()
    {
        var m = new GearPresetModel();
        m.Set(1, Two());
        var back = new GearPresetModel();
        back.LoadFrom(m.ToSaveArray());
        Assert.IsTrue(back.IsEmpty(0));
        Assert.AreEqual(3, back.Get(1)["Weapon"].Upgrade);
        Assert.AreEqual("Chief's Cap: Old", back.Get(1)["Head"].Name);
        Assert.AreEqual(1, back.Get(1)["Head"].Tier);
    }

    [Test]
    public void LoadFrom_ToleratesNullShortAndBroken()
    {
        var m = new GearPresetModel();
        m.LoadFrom(null);
        Assert.IsTrue(m.IsEmpty(0));
        m.LoadFrom(new[] { "Weapon=Sword:0:1;broken;Head=:0:0;Leg=Pants:x:1" });
        Assert.AreEqual(1, m.Get(0).Count);
        Assert.IsTrue(m.IsEmpty(2));
    }

    [Test]
    public void Set_OutOfRangeIgnored_EmptyNameSkipped()
    {
        var m = new GearPresetModel();
        m.Set(5, Two());
        var d = Two();
        d["Leg"] = new GearPresetModel.Entry { Name = "" };
        m.Set(0, d);
        Assert.AreEqual(2, m.Get(0).Count);
        Assert.IsNull(m.Get(5));
    }
}
