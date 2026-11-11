using NUnit.Framework;
using UnityEngine;

/// <summary>숨은 포털 표 파싱·검증 테스트 (FastTravel P4)</summary>
public class HiddenPortalTests
{
    [Test]
    public void Parse_ReadsValidRows()
    {
        var rows = HiddenPortalTable.Parse("map,id,ax,ay,bx,by,note\nM,p1,-230,18.4,-355,2.5,메모\n");
        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual("M", rows[0].Map);
        Assert.AreEqual(new Vector2(-355f, 2.5f), rows[0].B);
        Assert.AreEqual("메모", rows[0].Note);
    }

    [Test]
    public void Parse_SkipsBadNumbersTooCloseAndDuplicates()
    {
        string csv = "map,id,ax,ay,bx,by,note\nM,bad,x,1,2,3,\nM,near,0,0,3,0,\nM,ok,0,0,20,0,\nM,ok,0,0,30,0,\nN,ok,0,0,30,0,\n";
        var rows = HiddenPortalTable.Parse(csv);
        Assert.AreEqual(2, rows.Count);
        Assert.AreEqual("M", rows[0].Map);
        Assert.AreEqual("N", rows[1].Map);
    }

    [Test]
    public void ShippedTable_IsAllValid()
    {
        TextAsset ta = Resources.Load<TextAsset>("HiddenPortals");
        Assert.NotNull(ta);
        var rows = HiddenPortalTable.Parse(ta.text);
        Assert.GreaterOrEqual(rows.Count, 1);
    }
}
