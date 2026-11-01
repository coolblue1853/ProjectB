using NUnit.Framework;
using UnityEngine;

public class TempSkillFxTests
{
    [TestCase("Jaw Shake", "Jaw_Shake")]
    [TestCase("Hunter's Stride", "Hunter's_Stride")]
    [TestCase("Slash Attck", "Slash_Attck")]
    [TestCase(null, "")]
    public void SkillIdPreservesExistingKeys(string name, string expected)
    {
        Assert.AreEqual(expected, TempSkillFxLibrary.SkillId(name));
    }

    [TestCase("Jaw_Shake.png", true, "Jaw_Shake", 1)]
    [TestCase("Jaw_Shake_f4.png", true, "Jaw_Shake", 4)]
    [TestCase("Jaw_Shake_f0.png", false, "", 0)]
    [TestCase("Jaw_Shake_f65.png", false, "", 0)]
    [TestCase("Jaw_Shake_fx.png", false, "", 0)]
    [TestCase("Jaw_Shake.jpg", false, "", 0)]
    public void FileNameRules(string path, bool valid, string expected, int count)
    {
        Assert.AreEqual(valid, TempSkillFxLibrary.TryParseFile(path, out string id, out int frames));
        if (valid)
        {
            Assert.AreEqual(expected, id);
            Assert.AreEqual(count, frames);
        }
    }

    [Test]
    public void MissingAndInvalidSlotsFallBackAndSheetSlicesLeftToRight()
    {
        var library = ScriptableObject.CreateInstance<TempSkillFxLibrary>();
        var texture = new Texture2D(16, 4);
        try
        {
            Assert.IsNull(library.GetClip("missing"));
            library.entries.Add(new TempSkillFxLibrary.Entry { skillId = "bad", texture = texture, frameCount = 3 });
            Assert.IsNull(library.GetClip("bad"));
            library.entries.Add(new TempSkillFxLibrary.Entry { skillId = "sheet", texture = texture, frameCount = 4 });
            PixelFxClip clip = library.GetClip("sheet");
            Assert.AreEqual(4, clip.frames.Length);
            Assert.AreEqual(new Rect(12, 0, 4, 4), clip.frames[3].rect);
            Assert.AreEqual(36f, clip.frames[0].pixelsPerUnit);
            Assert.AreEqual(0.2f, clip.Duration, 0.0001f);
            Assert.AreSame(clip, library.GetClip("sheet"));
        }
        finally
        {
            Object.DestroyImmediate(library);
            Object.DestroyImmediate(texture);
        }
    }
    [Test]
    public void MalformedCatalogFallsBackWithoutThrowing()
    {
        var library = ScriptableObject.CreateInstance<TempSkillFxLibrary>();
        var texture = new Texture2D(130, 4);
        try
        {
            Assert.IsNull(library.GetClip(null));
            library.entries.Add(null);
            Assert.IsNull(library.GetClip("missing"));
            library.entries.Add(new TempSkillFxLibrary.Entry { skillId = "tooMany", texture = texture, frameCount = 65 });
            Assert.IsNull(library.GetClip("tooMany"));
            library.entries.Add(new TempSkillFxLibrary.Entry { skillId = "duplicate", texture = texture, frameCount = 1 });
            library.entries.Add(new TempSkillFxLibrary.Entry { skillId = "duplicate", texture = texture, frameCount = 2 });
            Assert.IsNull(library.GetClip("duplicate"));
            library.entries = null;
            Assert.IsNull(library.GetClip("missing"));
        }
        finally
        {
            Object.DestroyImmediate(library);
            Object.DestroyImmediate(texture);
        }
    }
}
