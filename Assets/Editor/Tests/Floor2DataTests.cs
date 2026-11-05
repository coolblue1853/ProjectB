using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>AUTO-22: 2층계 로드 스위치 — OFF 불변, ON 시 초안 행 추가·ID 중복 0.</summary>
public class Floor2DataTests
{
    private static string Raw(string name)
    {
        return Resources.Load<TextAsset>(name).text;
    }

    private static int DataRows(string text)
    {
        return text.Replace("\r", "").Split('\n').Skip(1).Count(l => !string.IsNullOrWhiteSpace(l));
    }

    [SetUp]
    public void SetUp()
    {
        Floor2Data.Enabled = false;
    }

    [TearDown]
    public void TearDown()
    {
        Floor2Data.Enabled = false;
    }

    [Test]
    public void DefaultIsOff_MergeReturnsSameText()
    {
        Assert.IsFalse(Floor2Data.Enabled);
        string sheet = Raw("MonsterSheet");
        Assert.AreSame(sheet, Floor2Data.Merge(sheet, "Floor2Monsters"));
    }

    [Test]
    public void On_AddsDraftRows()
    {
        Floor2Data.Enabled = true;
        Assert.AreEqual(DataRows(Raw("MonsterSheet")) + 21, DataRows(Floor2Data.Merge(Raw("MonsterSheet"), "Floor2Monsters")));
        Assert.AreEqual(DataRows(Raw("ItemSheet")) + 45, DataRows(Floor2Data.Merge(Raw("ItemSheet"), "Floor2ItemSheet")));
        Assert.AreEqual(DataRows(Raw("SetItem")) + 12, DataRows(Floor2Data.Merge(Raw("SetItem"), "Floor2SetItem")));
        var off = ExtraDrops.Parse(Raw("MonsterExtraDrops")).Values.Sum(l => l.Count);
        var on = ExtraDrops.Parse(Floor2Data.Merge(Raw("MonsterExtraDrops"), "Floor2ExtraDrops")).Values.Sum(l => l.Count);
        Assert.AreEqual(off + 28, on);
    }

    [Test]
    public void On_NoDuplicateIds()
    {
        Floor2Data.Enabled = true;
        AssertUnique(Floor2Data.Merge(Raw("MonsterSheet"), "Floor2Monsters"), 0, 1);
        AssertUnique(Floor2Data.Merge(Raw("ItemSheet"), "Floor2ItemSheet"), 0, 2);
        AssertUnique(Floor2Data.Merge(Raw("ItemSheet"), "Floor2ItemSheet"), 1, 2);
        AssertUnique(Floor2Data.Merge(Raw("SetItem"), "Floor2SetItem"), 0, 2);
    }

    [Test]
    public void On_BossRowsHaveKillKey()
    {
        string draft = Resources.Load<TextAsset>("Floor2Preview/Floor2Monsters").text;
        foreach (string l in draft.Replace("\r", "").Split('\n').Skip(1).Where(x => x.Length > 0))
        {
            string[] c = l.Split(',');
            if (c[6] == "1")
            {
                Assert.AreEqual(c[0], c[15], l);
            }
        }
    }

    [Test]
    public void MergeRows_DropsDraftHeader_AndHandlesEmpty()
    {
        Assert.AreEqual("h\na\nb", Floor2Data.MergeRows("h\na\n", "\uFEFFh\r\nb\r\n"));
        Assert.AreEqual("h\na", Floor2Data.MergeRows("h\na", ""));
    }

    private static void AssertUnique(string text, int col, int skip)
    {
        var seen = new HashSet<string>();
        foreach (string l in text.Replace("\r", "").Split('\n').Skip(skip).Where(x => x.Length > 0))
        {
            string[] c = l.Split(',');
            Assert.IsTrue(seen.Add(c[col]), "중복: " + c[col]);
        }
    }
}
