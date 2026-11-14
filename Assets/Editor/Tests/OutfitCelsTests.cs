using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>의상 한 벌 통짜 셀 묶음(OutfitCels, 11-15): 의상 → 셀 묶음 id 선택·클립 단위 교체·코트 폴백</summary>
public class OutfitCelsTests
{
    private readonly List<Object> _made = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        foreach (Object o in _made)
        {
            Object.DestroyImmediate(o);
        }
        _made.Clear();
    }

    private Sprite MakeSprite(string name)
    {
        var tex = new Texture2D(2, 2);
        Sprite s = Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.zero);
        s.name = name;
        _made.Add(tex);
        _made.Add(s);
        return s;
    }

    private CelClip Coat(params string[] pngs)
    {
        var sprites = new Sprite[pngs.Length];
        for (int i = 0; i < pngs.Length; i++)
        {
            sprites[i] = MakeSprite(pngs[i]);
        }
        return new CelClip
        {
            name = "Stab",
            frame = 0.083f,
            loop = false,
            strike = 2,
            sprites = sprites,
            fists = new Sprite[pngs.Length],
            hand = new Vector2[pngs.Length],
            angles = new float[pngs.Length],
            times = new float[pngs.Length],
        };
    }

    [TestCase("ScareArmor", "scarecrow")]
    [TestCase("LeafArmor", "leaf")]
    [TestCase("WispArmor", "wisp")]
    [TestCase("WolfArmor", "wolf")]
    [TestCase("RatArmor", "rat")]
    [TestCase("Armor", null)]
    [TestCase("", null)]
    [TestCase(null, null)]
    public void IdFor_MapsFloor1Outfits(string item, string id)
    {
        Assert.AreEqual(id, OutfitCels.IdFor(item));
    }

    [Test]
    public void Swap_AllFramesPresent_ReplacesArtKeepsTiming()
    {
        CelClip coat = Coat("ds_stab_0", "ds_stab_1");
        var outfit = new Dictionary<string, Sprite> { { "ds_stab_0", MakeSprite("ds_stab_0") }, { "ds_stab_1", MakeSprite("ds_stab_1") } };
        CelClip swapped = OutfitCels.Swap(coat, outfit);
        Assert.IsNotNull(swapped);
        Assert.AreSame(outfit["ds_stab_0"], swapped.sprites[0]);
        Assert.AreSame(outfit["ds_stab_1"], swapped.sprites[1]);
        Assert.IsTrue(swapped.outfit, "의상 셀은 부위 레이어 합성을 건너뛴다");
        Assert.AreEqual(coat.name, swapped.name);
        Assert.AreEqual(coat.strike, swapped.strike, "3타 타격 프레임 보존");
        Assert.AreEqual(coat.frame, swapped.frame);
        Assert.AreSame(coat.hand, swapped.hand, "손 좌표는 코트 표 재사용");
        Assert.AreSame(coat.fists, swapped.fists, "주먹은 코트 것");
        Assert.AreNotSame(coat.sprites[0], swapped.sprites[0]);
        Assert.IsFalse(coat.outfit, "원본 코트 클립은 그대로");
    }

    [Test]
    public void Swap_MissingFrame_FallsBackToCoat()
    {
        CelClip coat = Coat("ds_side_0", "ds_side_1");
        var outfit = new Dictionary<string, Sprite> { { "ds_side_0", MakeSprite("ds_side_0") } };
        Assert.IsNull(OutfitCels.Swap(coat, outfit), "프레임 일부만 있으면 섞지 않고 코트");
    }

    [Test]
    public void Swap_NoOutfit_FallsBackToCoat()
    {
        CelClip coat = Coat("ds_idle_0");
        Assert.IsNull(OutfitCels.Swap(coat, null));
        Assert.IsNull(OutfitCels.Swap(coat, new Dictionary<string, Sprite>()));
    }

    /// <summary>구운 주인공 프리팹: 의상 4벌은 기본·공격 셀 전부, 쥐는 기본만(공격은 코트 폴백)</summary>
    [Test]
    public void AdventurerPrefab_OutfitSetsCoverBasicAndAttack()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/PixelRig/Adventurer.prefab");
        Assert.IsNotNull(prefab);
        var counts = new Dictionary<string, int>();
        using (var so = new SerializedObject(prefab.GetComponentInChildren<PixelRig>(true)))
        {
            SerializedProperty sets = so.FindProperty("_outfitCels");
            for (int i = 0; i < sets.arraySize; i++)
            {
                SerializedProperty e = sets.GetArrayElementAtIndex(i);
                counts[e.FindPropertyRelative("id").stringValue] = e.FindPropertyRelative("sprites").arraySize;
            }
        }
        foreach (string id in new[] { "scarecrow", "leaf", "wisp", "wolf" })
        {
            Assert.IsTrue(counts.TryGetValue(id, out int n), id);
            Assert.AreEqual(169, n, id + ": 기본 31 + 공격 136 + 회복 2");
        }
        Assert.AreEqual(31, counts["rat"], "쥐: 기본 4종만");
    }
}
