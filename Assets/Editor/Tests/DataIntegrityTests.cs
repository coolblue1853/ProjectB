using NUnit.Framework;
using ProjectB.EditorTools;

/// <summary>기획 데이터 무결성: CSV 파싱·이름 중복·세트 키 (ProjectBDataWindow의 검증 리포트 재사용)</summary>
public class DataIntegrityTests
{
    [Test]
    public void ItemSheet_NoDuplicateNamesOrCommas()
    {
        var items = ItemTable.Load();
        var report = ValidationReport.Build(items, SetTable.Load());
        StringAssert.DoesNotContain("이름 중복", report);
        StringAssert.DoesNotContain("쉼표 포함", report);
    }

    [Test]
    public void SetItem_AllKeysMatchItemsAndParse()
    {
        var report = ValidationReport.Build(ItemTable.Load(), SetTable.Load());
        StringAssert.DoesNotContain("❌ 세트", report);
    }

    [Test]
    public void ItemSheet_RoundTripPreservesRows()
    {
        var items = ItemTable.Load();
        Assert.Greater(items.Rows.Count, 80);
        foreach (var r in items.Rows)
        {
            Assert.AreEqual(18, r.ToCsv().Split(',').Length, "컬럼 수: " + r.Name);
        }
    }
}
