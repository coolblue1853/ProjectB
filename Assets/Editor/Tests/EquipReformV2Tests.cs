using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>EQUIP-SLOT-REFORM v2: 해금 표(SlotUnlocks.csv)·구 세이브 이행 규칙·펫 물약 규칙·데이터 정합</summary>
public class EquipReformV2Tests
{
    const string Csv = "key,kind,floor,hookId\nCharm,slot,1,boss:1;bestiary:B04\nPet,content,2,boss:2\nBelt,slot,5,boss:5\nbad,slot,x,\n";

    [Test]
    public void UnlockTable_Parse_SlotsAndContent()
    {
        SlotUnlockTable t = SlotUnlockTable.Parse(Csv);
        Assert.AreEqual(3, t.Rows.Count, "잘못된 줄(floor 숫자 아님)은 건너뛴다");
        Assert.IsTrue(t.TrySlotForFloor(1, out EquipSlot s));
        Assert.AreEqual(EquipSlot.Charm, s);
        Assert.IsFalse(t.TrySlotForFloor(2, out _), "펫은 슬롯이 아니다");
        Assert.AreEqual("Pet", t.ForFloor(2).Key);
        Assert.AreEqual("Charm", t.ForHook("bestiary:B04").Key);
        Assert.IsNull(t.ForHook("boss:9"));
    }

    [Test]
    public void UnlockTable_ShippedCsv_OddSlotsEvenContent()
    {
        SlotUnlockTable.Reload();
        SlotUnlockTable t = SlotUnlockTable.Current;
        string[] want = { "Charm", "Pet", "Bracelet", "Rune", "Belt", "Relic", "Stone" };
        for (int floor = 1; floor <= 7; floor++)
        {
            SlotUnlockRow r = t.ForFloor(floor);
            Assert.IsNotNull(r, "층계 " + floor);
            Assert.AreEqual(want[floor - 1], r.Key);
            Assert.AreEqual(floor % 2 == 1, r.IsSlot, "홀수 층계만 장비 슬롯: " + floor);
            Assert.IsNotEmpty(r.Hooks);
        }
    }

    [Test]
    public void Legacy_Decide_PrefersBoxThenExtraThenBag()
    {
        var T = LegacyGearMigration.Target.Box;
        Assert.AreEqual(T, LegacyGearMigration.Decide("Ring", true, false, true));
        Assert.AreEqual(LegacyGearMigration.Target.Bag, LegacyGearMigration.Decide("Ring", false, false, true), "반지 칸이 차 있으면 가방");
        Assert.AreEqual(LegacyGearMigration.Target.Extra, LegacyGearMigration.Decide("Earring", false, true, true));
        Assert.AreEqual(LegacyGearMigration.Target.Bag, LegacyGearMigration.Decide("Charm", false, false, true), "잠금 슬롯이면 가방");
        Assert.AreEqual(LegacyGearMigration.Target.Keep, LegacyGearMigration.Decide("Charm", false, false, false), "공간이 없으면 구 칸에 둔다(소실 금지)");
        Assert.AreEqual(LegacyGearMigration.Target.Bag, LegacyGearMigration.Decide("Head", false, false, true), "아직 옛 부위면 가방");
    }

    [Test]
    public void Legacy_CollectAndClear_OnlyLegacyIndexes()
    {
        var gear = new string[10, 3];
        gear[1, 0] = "Ring1";
        gear[2, 0] = "Cap";
        gear[2, 1] = "1";
        gear[2, 2] = "3";
        gear[9, 0] = "Shoes";
        var flags = new int[10];
        flags[2] = 5;
        var list = LegacyGearMigration.Collect(gear, flags);
        Assert.AreEqual(new[] { 2, 9 }, list.Select(e => e.Index).ToArray());
        Assert.AreEqual(5, list[0].Flags);
        LegacyGearMigration.Clear(gear, flags, 2);
        Assert.IsNull(gear[2, 0]);
        Assert.AreEqual(0, flags[2]);
        Assert.AreEqual("Ring1", gear[1, 0], "다른 칸은 건드리지 않는다");
    }

    [Test]
    public void PetPotion_NeedsPotion_Thresholds()
    {
        Assert.IsTrue(PetPotionRules.NeedsPotion(40, 100, 40));
        Assert.IsFalse(PetPotionRules.NeedsPotion(41, 100, 40));
        Assert.IsFalse(PetPotionRules.NeedsPotion(0, 100, 40), "죽었으면 마시지 않는다");
        Assert.IsFalse(PetPotionRules.NeedsPotion(10, 0, 40));
    }

    [Test]
    public void PetPotion_NextPercent_CyclesAndOnlySimpleRestore()
    {
        Assert.AreEqual(30, PetPotionRules.NextPercent(20));
        Assert.AreEqual(20, PetPotionRules.NextPercent(80), "끝에서 처음으로");
        Assert.AreEqual(50, PetPotionRules.NextPercent(45));
        Assert.IsTrue(PetPotionRules.IsSimpleRestore("hp +", "hp"));
        Assert.IsFalse(PetPotionRules.IsSimpleRestore("fullness +/hp +/stemina +", "hp"), "음식(여러 효과)은 자동으로 먹지 않는다");
        Assert.IsFalse(PetPotionRules.IsSimpleRestore("stemina +", "hp"));
    }

    static string[][] SheetRows()
    {
        string text = File.ReadAllText(Path.Combine(Application.dataPath, "Resources/ItemSheet.csv"));
        return text.Replace("\r", "").Split('\n').Skip(2).Where(l => l.Length > 0).Select(SplitCsv).Where(c => c.Length > 16).ToArray();
    }

    /// <summary>따옴표 안의 쉼표를 지키는 한 줄 CSV 분리</summary>
    static string[] SplitCsv(string line)
    {
        var fields = new System.Collections.Generic.List<string>();
        var sb = new System.Text.StringBuilder();
        bool quoted = false;
        foreach (char ch in line)
        {
            if (ch == '"')
            {
                quoted = !quoted;
            }
            else if (ch == ',' && !quoted)
            {
                fields.Add(sb.ToString());
                sb.Length = 0;
            }
            else
            {
                sb.Append(ch);
            }
        }
        fields.Add(sb.ToString());
        return fields.ToArray();
    }

    [Test]
    public void ItemSheet_NoRetiredAreas_ConvertedAndBeltExists()
    {
        string[][] rows = SheetRows();
        foreach (string[] c in rows)
        {
            Assert.IsFalse(SlotRules.IsRetiredArea(c[10]), c[1] + " 가 폐지 부위 " + c[10]);
        }
        string[] cap = rows.First(c => c[1] == "Cap");
        Assert.AreEqual("Earring", cap[10]);
        Assert.AreEqual("RabbitArmor", cap[11], "세트 키 유지");
        Assert.AreEqual("Ring", rows.First(c => c[1] == "HandArmor")[10]);
        Assert.AreEqual("Necklace", rows.First(c => c[1] == "LegArmor")[10]);
        string[] shoes = rows.First(c => c[1] == "Shoes");
        Assert.AreEqual("Charm", shoes[10]);
        Assert.IsEmpty(shoes[11], "보조장비는 세트에 세지 않는다");
        Assert.AreEqual("Belt", rows.First(c => c[1] == "BeltPlain")[10]);
    }

    [Test]
    public void SetItem_NoSixthTier_FiveMax()
    {
        string text = File.ReadAllText(Path.Combine(Application.dataPath, "Resources/SetItem.csv"));
        string[] keys = text.Replace("\r", "").Split('\n').Skip(2).Where(l => l.Length > 0).Select(l => l.Split(',')[0]).ToArray();
        Assert.IsFalse(keys.Any(k => k.EndsWith("_6")), "6셋 단계는 5칸 구조에서 도달 불가");
        Assert.Contains("WolfArmor_5", keys);
        Assert.Contains("CliffArmor_4", keys);
    }

    [Test]
    public void Features_ContentUnlockKeys_ComeFromTable()
    {
        SlotUnlockTable.Reload();
        foreach (string key in new[] { ContentFeatures.Pet, ContentFeatures.Rune, ContentFeatures.Relic })
        {
            SlotUnlockRow r = SlotUnlockTable.Current.ForKey(key);
            Assert.IsNotNull(r, key);
            Assert.IsFalse(r.IsSlot, key + " 는 장비 슬롯이 아니다");
        }
    }
}
