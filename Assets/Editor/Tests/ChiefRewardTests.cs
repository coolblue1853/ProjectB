using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>산적 대장 보상(Floor1Completion 15절): 데이터 행·드롭·퀘스트 보상 + 약탈·오기 규칙</summary>
public class ChiefRewardTests
{
    static string[] Lines(string resource)
    {
        TextAsset t = Resources.Load<TextAsset>(resource);
        Assert.IsNotNull(t, resource + ".csv 없음");
        return t.text.Replace("\r", "").Split('\n');
    }

    static string[] ItemRow(string name)
    {
        foreach (string line in Lines("ItemSheet"))
        {
            string[] c = line.Split(',');
            if (c.Length > 1 && c[1] == name)
            {
                return c;
            }
        }
        return null;
    }

    [Test]
    public void ItemSheet_BothRows_Parse()
    {
        string[] medal = ItemRow(AccessoryGimmick.PlunderMedal);
        string[] ring = ItemRow(AccessoryGimmick.LastStandRing);
        Assert.IsNotNull(medal);
        Assert.IsNotNull(ring);
        Assert.AreEqual(19, medal.Length);
        Assert.AreEqual(19, ring.Length);
        Assert.AreEqual("Necklace", medal[10]);
        Assert.AreEqual("Ring", ring[10]);
        Assert.AreEqual("U", medal[14]);
        Assert.AreEqual("U", ring[14]);
        Assert.AreEqual("258", medal[0]);
        Assert.AreEqual("259", ring[0]);
        Assert.AreEqual("Bandit Plunder Medal", medal[12]);
        Assert.AreEqual("Chief's Last Stand Ring", ring[12]);
    }

    [Test]
    public void ExtraDrops_Chief_DropsMedal15Percent()
    {
        List<ExtraDrops.Entry> list = ExtraDrops.Parse(Resources.Load<TextAsset>("MonsterExtraDrops").text)["BanditChief_BD"];
        ExtraDrops.Entry e = list.Find(x => x.Item == AccessoryGimmick.PlunderMedal);
        Assert.AreEqual(AccessoryGimmick.PlunderMedal, e.Item);
        Assert.AreEqual(0.15f, e.Probability, 1e-5f);
        Assert.IsFalse(e.QuestOnly);
    }

    [Test]
    public void Q504_Rewards_LastStandRing()
    {
        string row = null;
        foreach (string line in Lines("QuestsFloor1"))
        {
            if (line.TrimStart('﻿').StartsWith("Q504,"))
            {
                row = line;
            }
        }
        Assert.IsNotNull(row);
        string[] c = row.Split(',');
        Assert.AreEqual(AccessoryGimmick.LastStandRing, c[10]);
        Assert.AreEqual("1", c[11]);
    }

    [Test]
    public void Loc_KoreanNames()
    {
        Assert.AreEqual("약탈의 훈장", System.Array.Find(LocItems.Entries, e => e.en == "Bandit Plunder Medal").ko);
        Assert.AreEqual("대장의 오기 반지", System.Array.Find(LocItems.Entries, e => e.en == "Chief's Last Stand Ring").ko);
    }

    [Test]
    public void Plunder_ChanceAndMultiplier()
    {
        Assert.IsTrue(PlunderRules.Roll(0.079f));
        Assert.IsFalse(PlunderRules.Roll(0.08f));
        Assert.AreEqual(300, PlunderRules.Apply(100, true, 0.01f));
        Assert.AreEqual(100, PlunderRules.Apply(100, true, 0.5f));
        Assert.AreEqual(100, PlunderRules.Apply(100, false, 0.01f));
        Assert.AreEqual(0, PlunderRules.Apply(0, true, 0.01f));
    }

    [Test]
    public void LastStand_RescuesOnceThenCooldown()
    {
        var r = new LastStandRules();
        int dmg = 50;
        Assert.IsFalse(r.TryRescue(100, ref dmg, 0f));
        Assert.AreEqual(50, dmg);
        dmg = 100;
        Assert.IsTrue(r.TryRescue(100, ref dmg, 10f));
        Assert.AreEqual(99, dmg);
        Assert.AreEqual(180f, r.Remaining(10f), 1e-4f);
        dmg = 500;
        Assert.IsFalse(r.TryRescue(1, ref dmg, 100f));
        Assert.AreEqual(500, dmg);
        Assert.IsTrue(r.TryRescue(1, ref dmg, 190f));
        Assert.AreEqual(0, dmg);
    }

    [Test]
    public void LastStand_DeadHpNeverTriggers_ResetClears()
    {
        var r = new LastStandRules();
        int dmg = 10;
        Assert.IsFalse(r.TryRescue(0, ref dmg, 0f));
        Assert.IsTrue(r.TryRescue(5, ref dmg, 0f));
        r.Reset();
        dmg = 99;
        Assert.IsTrue(r.TryRescue(5, ref dmg, 1f));
    }

    [Test]
    public void Gimmick_IsEquippedScansSlots()
    {
        var gear = new string[10, 3];
        Assert.IsFalse(AccessoryGimmick.IsEquipped(gear, AccessoryGimmick.PlunderMedal));
        gear[3, 0] = AccessoryGimmick.PlunderMedal;
        Assert.IsTrue(AccessoryGimmick.IsEquipped(gear, AccessoryGimmick.PlunderMedal));
        Assert.IsFalse(AccessoryGimmick.IsEquipped(gear, AccessoryGimmick.LastStandRing));
        Assert.IsFalse(AccessoryGimmick.IsEquipped(null, "x"));
    }
}
