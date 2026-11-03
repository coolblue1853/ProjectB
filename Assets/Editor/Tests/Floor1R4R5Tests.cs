using NUnit.Framework;
using UnityEngine;

public class Floor1R4R5Tests
{
    [Test]
    public void WeaponTrial_CsvParsesAndItemsExist()
    {
        var defs = WeaponTrialRules.Parse(Resources.Load<TextAsset>("Floor1WeaponTrials").text);
        Assert.AreEqual(2, defs.Count);
        Assert.AreEqual(5, defs[0].MinLevel);
        Assert.AreEqual(10, defs[1].MinLevel);
        string sheet = Resources.Load<TextAsset>("ItemSheet").text;
        foreach (WeaponTrialDef d in defs)
        {
            Assert.AreEqual(3, d.Weapons.Length);
            foreach (string w in d.Weapons)
            {
                StringAssert.Contains("," + w + ",Equip,", sheet, w);
            }
        }
    }

    [Test]
    public void WeaponTrial_NextByLevelAndState()
    {
        var defs = WeaponTrialRules.Parse(Resources.Load<TextAsset>("Floor1WeaponTrials").text);
        Assert.IsNull(WeaponTrialRules.Next(defs, 4, s => 0));
        Assert.AreEqual(1, WeaponTrialRules.Next(defs, 5, s => 0).Stage);
        Assert.AreEqual(1, WeaponTrialRules.Next(defs, 12, s => 0).Stage);
        Assert.AreEqual(2, WeaponTrialRules.Next(defs, 12, s => s == 1 ? WeaponTrialRules.Done : 0).Stage);
        Assert.IsNull(WeaponTrialRules.Next(defs, 12, s => WeaponTrialRules.Done));
        Assert.IsNull(WeaponTrialRules.Next(defs, 9, s => s == 1 ? WeaponTrialRules.Done : 0));
    }

    [Test]
    public void WeaponTrial_PickBlockedOnlyForOtherRental()
    {
        var d = WeaponTrialRules.Parse(Resources.Load<TextAsset>("Floor1WeaponTrials").text)[0];
        Assert.IsTrue(WeaponTrialRules.PickBlocked(d, "TwinFang", "GnarledClub"));
        Assert.IsFalse(WeaponTrialRules.PickBlocked(d, "TwinFang", "TwinFang"));
        Assert.IsFalse(WeaponTrialRules.PickBlocked(d, "TwinFang", null));
        Assert.IsFalse(WeaponTrialRules.PickBlocked(d, "TwinFang", "FangDagger"));
    }

    [Test]
    public void NestWave_StepSequence()
    {
        Assert.AreEqual(NestWaveStep.Spawn, NestHuntRules.Step(1, 0, 0, 0f));
        Assert.AreEqual(NestWaveStep.Wait, NestHuntRules.Step(1, NestHuntRules.PerWave, 2, 10f));
        Assert.AreEqual(NestWaveStep.Wait, NestHuntRules.Step(1, NestHuntRules.PerWave, 0, 0.5f));
        Assert.AreEqual(NestWaveStep.NextWave, NestHuntRules.Step(1, NestHuntRules.PerWave, 0, 5f));
        Assert.AreEqual(NestWaveStep.Elite, NestHuntRules.Step(NestHuntRules.Waves, NestHuntRules.PerWave, 0, 5f));
    }

    [Test]
    public void NestChest_ScalesWithLevel()
    {
        NestHuntRules.ChestReward(5, out int g5, out int e5);
        NestHuntRules.ChestReward(13, out int g13, out int e13);
        Assert.Greater(g13, g5);
        Assert.Greater(e13, e5);
        NestHuntRules.ChestReward(0, out int g0, out _);
        Assert.Greater(g0, 0);
    }
}
