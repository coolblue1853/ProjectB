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
    public void SetItems_TfNameLinksToSetItemKeys()
    {
        // 세트 소속 아이템의 tfName 이 SetItem 키(tfName_2/_4/_6)와 이어지는지 (MigrationPlan §2 b)
        var keys = new HashSet<string>(Resources.Load<TextAsset>("Floor2Preview/Floor2SetItem").text
            .Replace("\r", "").Split('\n').Skip(1).Where(x => x.Length > 0).Select(x => x.Split(',')[0]));
        int checkedSets = 0;
        var sets = new HashSet<string>();
        foreach (string l in Resources.Load<TextAsset>("Floor2Preview/Floor2ItemSheet").text.Replace("\r", "").Split('\n').Skip(1).Where(x => x.Length > 0))
        {
            string[] c = l.Split(',');
            if (c[16].Length == 0)
            {
                continue;
            }
            Assert.IsNotEmpty(c[11], "세트 아이템은 tfName 필요: " + l);
            sets.Add(c[11]);
        }
        foreach (string tf in sets)
        {
            foreach (string n in new[] { "_2", "_4", "_6" })
            {
                Assert.IsTrue(keys.Contains(tf + n), "SetItem 키 없음: " + tf + n);
            }
            checkedSets++;
        }
        Assert.AreEqual(4, checkedSets, "세트 4종");
        Assert.AreEqual(12, keys.Count, "SetItem 12행");
    }

    [Test]
    public void MergeRows_DropsDraftHeader_AndHandlesEmpty()
    {
        Assert.AreEqual("h\na\nb", Floor2Data.MergeRows("h\na\n", "\uFEFFh\r\nb\r\n"));
        Assert.AreEqual("h\na", Floor2Data.MergeRows("h\na", ""));
    }

    [Test]
    public void MergeRows_UnifiesEol_ToBaseStyle()
    {
        Assert.AreEqual("h\r\na\r\nb\r\nc", Floor2Data.MergeRows("h\r\na\r\n", "h\nb\nc\n"));
        Assert.AreEqual("h\na\nb", Floor2Data.MergeRows("h\na", "h\r\nb"));
    }

    [Test]
    public void MergeRows_RejectsMismatchedHeaderOrder()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("헤더 열 순서 불일치"));
        Assert.AreEqual("a,b\n1,2", Floor2Data.MergeRows("a,b\n1,2", "b,a\n3,4"));
    }

    [Test]
    public void MergeRows_AcceptsHeaderBehindTypeRow()
    {
        Assert.AreEqual("int,string\nid,name\n1,x\n2,y", Floor2Data.MergeRows("int,string\nid,name\n1,x", "id,name\n2,y"));
    }

    [Test]
    public void RealHeaders_MatchBase()
    {
        foreach (var (b, p) in new[] { ("MonsterSheet", "Floor2Monsters"), ("ItemSheet", "Floor2ItemSheet"), ("SetItem", "Floor2SetItem"), ("MonsterExtraDrops", "Floor2ExtraDrops") })
        {
            string head = Resources.Load<TextAsset>("Floor2Preview/" + p).text.TrimStart('﻿').Replace("\r", "").Split('\n')[0];
            Assert.IsTrue(Floor2Data.HeaderMatches(Raw(b), head), b);
        }
    }

    [Test]
    public void ProductionCode_NeverEnablesSwitch()
    {
        foreach (string f in System.IO.Directory.GetFiles(Application.dataPath + "/Script", "*.cs", System.IO.SearchOption.AllDirectories))
        {
            if (System.IO.Path.GetFileName(f) != "Floor2Data.cs")
            {
                StringAssert.DoesNotContain("Floor2Data.Enabled = true", System.IO.File.ReadAllText(f), f);
            }
        }
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
