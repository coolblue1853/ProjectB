using System.Linq;
using NUnit.Framework;

/// <summary>LP-1: 번역 표 신규 키 존재, 영어 키 중복 없음, 한국어 키 금지.</summary>
public class LocUiKeysTests
{
    private static readonly string[] Required =
    {
        "Craft cost : ", "Explorer Seal acquired ({0}/{1})", "Locked: first-clear a Floor 2 boss",
        "Locked: first-clear a Floor 4 boss", "New equipment slot unlocked: ", "Trial readiness", "Other side",
    };

    [Test]
    public void RequiredKeys_Present_WithKorean()
    {
        foreach (string k in Required)
        {
            var e = LocUi.Entries.FirstOrDefault(x => x.en == k);
            Assert.IsNotNull(e.en, "번역 없음: " + k);
            Assert.IsNotEmpty(e.ko, k);
        }
    }

    [Test]
    public void EnglishKeys_NoDuplicates_NoHangul()
    {
        var dup = LocUi.Entries.GroupBy(x => x.en).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.IsEmpty(dup, "중복 키: " + string.Join(" | ", dup));
        Assert.IsFalse(LocUi.Entries.Any(x => x.en.Any(ch => ch >= '가' && ch <= '힣')), "한국어 키");
    }
}
