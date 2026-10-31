using NUnit.Framework;
using ProjectB.EditorTools;
using UnityEngine;

/// <summary>무기 계열 기믹 규칙·데이터 (WeaponTypes.md 1차 묶음)</summary>
public class WeaponGimmickRulesTests
{
    static readonly string[] NewWeapons = { "HuntersLongbow", "GnarledClub", "WoodcuttersAxe", "TwinFang", "VineLash" };

    [Test]
    public void Charge_TiersAndMultipliers()
    {
        Assert.AreEqual(0, WeaponGimmickRules.ChargeTier(0f));
        Assert.AreEqual(0, WeaponGimmickRules.ChargeTier(0.34f));
        Assert.AreEqual(1, WeaponGimmickRules.ChargeTier(0.35f));
        Assert.AreEqual(1, WeaponGimmickRules.ChargeTier(0.99f));
        Assert.AreEqual(2, WeaponGimmickRules.ChargeTier(1f));
        Assert.AreEqual(2, WeaponGimmickRules.ChargeTier(5f));
        Assert.AreEqual(1f, WeaponGimmickRules.ChargeDamageMul(0), 1e-5f);
        Assert.AreEqual(1.25f, WeaponGimmickRules.ChargeDamageMul(1), 1e-5f);
        Assert.AreEqual(1.6f, WeaponGimmickRules.ChargeDamageMul(2), 1e-5f);
        Assert.AreEqual(0, WeaponGimmickRules.ChargePierce(1));
        Assert.AreEqual(1, WeaponGimmickRules.ChargePierce(2));
    }

    [Test]
    public void Whip_TipAndNearBonus()
    {
        Assert.AreEqual(0.8f, WeaponGimmickRules.WhipMultiplier(0f), 1e-5f);
        Assert.AreEqual(0.8f, WeaponGimmickRules.WhipMultiplier(0.29f), 1e-5f);
        Assert.AreEqual(1f, WeaponGimmickRules.WhipMultiplier(0.3f), 1e-5f);
        Assert.AreEqual(1f, WeaponGimmickRules.WhipMultiplier(0.69f), 1e-5f);
        Assert.AreEqual(1.4f, WeaponGimmickRules.WhipMultiplier(0.7f), 1e-5f);
        Assert.AreEqual(1.4f, WeaponGimmickRules.WhipMultiplier(1f), 1e-5f);
    }

    [Test]
    public void Finisher_OnlyLastStepOfMultiHitCombo()
    {
        Assert.IsTrue(WeaponGimmickRules.IsFinisher(4, 4));
        Assert.IsFalse(WeaponGimmickRules.IsFinisher(3, 4));
        Assert.IsFalse(WeaponGimmickRules.IsFinisher(1, 1));
    }

    [Test]
    public void Axe_CleavesOnlyArmored()
    {
        Assert.AreEqual(1f, WeaponGimmickRules.CleaveMultiplier(14), 1e-5f);
        Assert.AreEqual(1.2f, WeaponGimmickRules.CleaveMultiplier(15), 1e-5f);
        Assert.AreEqual(1.2f, WeaponGimmickRules.CleaveMultiplier(27), 1e-5f);
    }

    [Test]
    public void Mace_IgnoreDefAddsAndClamps()
    {
        Assert.AreEqual(20, WeaponGimmickRules.AddIgnoreDef(0, 20));
        Assert.AreEqual(100, WeaponGimmickRules.AddIgnoreDef(90, 20));
    }

    [Test]
    public void ParseKind_AcceptsKnownRejectsTypo()
    {
        Assert.IsTrue(WeaponGimmickRules.TryParseKind("Bow", out WeaponKind k));
        Assert.AreEqual(WeaponKind.Bow, k);
        Assert.IsTrue(WeaponGimmickRules.TryParseKind("", out k));
        Assert.AreEqual(WeaponKind.None, k);
        Assert.IsFalse(WeaponGimmickRules.TryParseKind("bow", out k));
    }

    [Test]
    public void ExtraDrops_ParsesRows()
    {
        var t = ExtraDrops.Parse("monsterId,item,probability\r\nEnt_BD,GnarledClub,0.02\r\nbad line\r\nEnt_BD,Wood,1.5\r\n");
        Assert.AreEqual(2, t["Ent_BD"].Count);
        Assert.AreEqual(0.02f, t["Ent_BD"][0].Probability, 1e-5f);
        Assert.AreEqual(1f, t["Ent_BD"][1].Probability, 1e-5f);
    }

    [Test]
    public void ExtraDrops_FileItemsExistInItemSheet()
    {
        var items = ItemTable.Load();
        var t = ExtraDrops.Parse(Resources.Load<TextAsset>("MonsterExtraDrops").text);
        Assert.Greater(t.Count, 0);
        foreach (var kv in t)
        {
            foreach (var e in kv.Value)
            {
                Assert.IsNotNull(items.Rows.Find(r => r.Name == e.Item), "ItemSheet 에 없음: " + e.Item);
            }
        }
    }

    [Test]
    public void NewWeapons_ItemRowPrefabKindAndGimmick()
    {
        var items = ItemTable.Load();
        foreach (string n in NewWeapons)
        {
            var row = items.Rows.Find(r => r.Name == n);
            Assert.IsNotNull(row, "ItemSheet 행 없음: " + n);
            Assert.IsTrue(WeaponGimmickRules.TryParseKind(row.WeaponType, out WeaponKind kind) && kind != WeaponKind.None, n + " WeaponType");
            var pf = Resources.Load<GameObject>("Weapon/" + n);
            Assert.IsNotNull(pf, "프리팹 없음: " + n);
            Assert.AreEqual(kind, pf.GetComponent<Weapon>().Kind, n + " 프리팹 계열");
            Assert.IsNotNull(pf.GetComponent<WeaponGimmick>(), n + " 기믹 없음");
            Assert.IsNotNull(Resources.Load<Sprite>("Item/" + n), n + " 아이콘");
        }
    }

    [Test]
    public void ItemSheet_AllWeaponsHaveKind()
    {
        var report = ValidationReport.Build(ItemTable.Load(), SetTable.Load());
        StringAssert.DoesNotContain("WeaponType", report);
    }

    [Test]
    public void TwinFang_FourStepArraysConsistent()
    {
        var w = Resources.Load<GameObject>("Weapon/TwinFang").GetComponent<Weapon>();
        Assert.AreEqual(4, w.maxComboCount);
        Assert.AreEqual(4, w.attackPrefab.Length);
        Assert.AreEqual(4, w.attackPivot.Length);
        Assert.AreEqual(4, w.attckSpeed.Length);
        Assert.AreEqual(4, w.attackAnimName.Length);
        Assert.AreEqual(4, w.weaponSound.Length);
    }
}
