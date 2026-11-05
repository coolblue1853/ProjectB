using NUnit.Framework;

public class UiTextTests
{
    [Test] public void SameValue_NoSet() { Assert.IsFalse(UiText.NeedsSet("a", "a")); }
    [Test] public void Different_Set() { Assert.IsTrue(UiText.NeedsSet("a", "b")); }
    [Test] public void NullAndEmpty_Equal() { Assert.IsFalse(UiText.NeedsSet(null, "")); }
    [Test] public void CaseSensitive() { Assert.IsTrue(UiText.NeedsSet("a", "A")); }
}
