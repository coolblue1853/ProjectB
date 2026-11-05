using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>ES3 세이브 호환 (CodeStyle.md 3절, FloorTransition.md 6절): 키 이름 고정 + 구세이브(키 없음) 기본값 로드. Cache 위치만 써서 디스크에 쓰지 않는다.</summary>
public class SaveCompatTests
{
    // 키 목록(이름 변경 금지, 추가만). 접두어형은 뒤에 보스 이름이 붙는다.
    static readonly string[] FixedKeys =
    {
        "explorer.rank", "explorer.seals", "glide.unlocked", "forge.unlock", "forge.pity",
        "bestiary.kills", "qm.board", "qm.elder", "qm.journal",
    };
    static readonly string[] PrefixKeys = { "bossFirstKill.", "bossSinceUnique." };

    static string AllSource()
    {
        var sb = new System.Text.StringBuilder();
        foreach (string f in Directory.GetFiles(Path.Combine(Application.dataPath, "Script"), "*.cs", SearchOption.AllDirectories))
        {
            sb.Append(File.ReadAllText(f));
        }
        return sb.ToString();
    }

    [Test]
    public void KeyNames_StillInSource()
    {
        string src = AllSource();
        foreach (string k in FixedKeys)
        {
            Assert.IsTrue(src.Contains("\"" + k + "\""), "세이브 키가 소스에서 사라짐/이름 변경: " + k);
        }
        foreach (string k in PrefixKeys)
        {
            Assert.IsTrue(src.Contains("\"" + k), "세이브 키 접두어가 소스에서 사라짐/이름 변경: " + k);
        }
        Assert.AreEqual("explorer.seals", ExplorerSeal.Key);
    }

    [Test]
    public void OldSave_MissingKeys_LoadDefaults()
    {
        var st = new ES3Settings("savecompat_test.es3", ES3.Location.Cache);
        try
        {
            ES3.Save("legacy.only", 7, st); // 구세이브: 신규 키 없음
            foreach (string k in FixedKeys)
            {
                Assert.IsFalse(ES3.KeyExists(k, st), k);
                Assert.AreEqual(0, ES3.Load(k, 0, st), k); // 기본값
            }
            Assert.AreEqual(7, ES3.Load("legacy.only", 0, st));
        }
        finally
        {
            ES3.DeleteFile(st);
        }
    }

    [Test]
    public void OldSave_DefaultsAreSafe()
    {
        Assert.AreEqual(0, ExplorerSeal.BonusOf(0)); // 인장 없음 = 저항 보너스 0
        Assert.AreEqual(25, BagExpansionRules.Clamp(0)); // 가방 키 없음 = 시작 칸
    }
}
