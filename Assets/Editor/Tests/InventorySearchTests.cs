using NUnit.Framework;

/// <summary>가방 이름 검색 순수 규칙 테스트 (InventoryRefactorDesign S4)</summary>
public class InventorySearchTests
{
    [Test]
    public void Matches_EmptyQueryPassesEverything()
    {
        Assert.IsTrue(InventorySearchRules.Matches("", "철검"));
        Assert.IsTrue(InventorySearchRules.Matches("  ", null));
    }

    [Test]
    public void Matches_SubstringCaseInsensitive()
    {
        Assert.IsTrue(InventorySearchRules.Matches("sword", "Iron Sword"));
        Assert.IsTrue(InventorySearchRules.Matches("철", "낡은 철검"));
        Assert.IsFalse(InventorySearchRules.Matches("활", "낡은 철검"));
        Assert.IsFalse(InventorySearchRules.Matches("검", null));
    }

    [Test]
    public void Matches_AllTokensRequired()
    {
        Assert.IsTrue(InventorySearchRules.Matches("낡은 검", "낡은 철검"));
        Assert.IsFalse(InventorySearchRules.Matches("낡은 활", "낡은 철검"));
    }

    [Test]
    public void AppendAndBackspace_EditQuery()
    {
        Assert.AreEqual("a", InventorySearchRules.Append("", 'a'));
        Assert.AreEqual("a", InventorySearchRules.Append("a", '\n'));
        Assert.AreEqual("", InventorySearchRules.Backspace("a"));
        Assert.AreEqual("", InventorySearchRules.Backspace(""));
        string full = new string('x', InventorySearchRules.MaxLength);
        Assert.AreEqual(full, InventorySearchRules.Append(full, 'y'));
    }

    [Test]
    public void NextInitial_CyclesSortedFirstLettersThenClears()
    {
        string[] names = { "철검", "나무 활", "철갑", "가죽" };
        string a = InventorySearchRules.NextInitial(names, "");
        Assert.AreEqual("가", a);
        string b = InventorySearchRules.NextInitial(names, a);
        Assert.AreEqual("나", b);
        string c = InventorySearchRules.NextInitial(names, b);
        Assert.AreEqual("철", c);
        Assert.AreEqual("", InventorySearchRules.NextInitial(names, c));
        Assert.AreEqual("", InventorySearchRules.NextInitial(new string[0], ""));
    }
}
