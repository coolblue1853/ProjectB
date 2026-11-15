using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 옛 장비 재발 방지(LegacyGearAudit, 11-15): 게임에서 얻을 수 있는 모든 장비 경로(몹 드롭표·추가 드롭·미니보스·보스·상점·제작·퀘스트·도감·보물·습격·의뢰·무기 시연)가
/// 시트에 있는 현행 아이템만 가리키는지 검사한다. ItemSheet retired=1 행은 세이브 호환으로 남겨 두지만 획득 경로에 있으면 안 된다.
/// </summary>
public class LegacyGearIntegrityTests
{
    private static readonly string[] RetiredMust =
    {
        "Shoes", "LeafShoes", "RatShoes", "ScareShoes", "CliffShoes", "WolfShoes", "WispShoes", "GroveShoes", "FaeShoes",
        "BogShoes", "GloomShoes", "OutlawShoes", "RaiderShoes", "StoneShoes", "RampartShoes", "SkyShoes",
    };

    private sealed class Sheet
    {
        public readonly HashSet<string> All = new HashSet<string>();
        public readonly HashSet<string> Equip = new HashSet<string>();
        public readonly HashSet<string> Retired = new HashSet<string>();
        public readonly Dictionary<string, string[]> Rows = new Dictionary<string, string[]>();
        public int AreaCol, NameTCol;
    }

    private static Sheet LoadSheet()
    {
        var s = new Sheet();
        string[] lines = Resources.Load<TextAsset>("ItemSheet").text.Replace("\r", "").Split('\n');
        string[] head = lines[1].Split(',');
        int rc = System.Array.IndexOf(head, "retired");
        Assert.GreaterOrEqual(rc, 0, "ItemSheet 에 retired 열이 없다");
        s.AreaCol = System.Array.IndexOf(head, "equipArea");
        s.NameTCol = System.Array.IndexOf(head, "itemNameT");
        for (int i = 2; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length <= rc)
            {
                continue;
            }
            string name = c[1].Trim();
            s.All.Add(name);
            s.Rows[name] = c;
            if (c[2].Trim() == "Equip")
            {
                s.Equip.Add(name);
            }
            if (c[rc].Trim() == "1")
            {
                s.Retired.Add(name);
            }
        }
        return s;
    }

    /// <summary>CSV 의 지정 열(머리글 이름) 값. sep 이 있으면 그 문자로 쪼개고 ':'·'*' 뒤 수량은 버린다</summary>
    private static IEnumerable<string> Column(string resource, string col, char sep = '\0')
    {
        string[] lines = Resources.Load<TextAsset>(resource).text.Replace("\r", "").TrimStart('\uFEFF').Split('\n');
        int ci = System.Array.IndexOf(lines[0].Split(','), col);
        Assert.GreaterOrEqual(ci, 0, resource + " 에 열 " + col + " 없음");
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim().Length == 0 || lines[i].StartsWith("#"))
            {
                continue;
            }
            string[] c = lines[i].Split(',');
            if (c.Length <= ci || c[ci].Trim().Length == 0)
            {
                continue;
            }
            foreach (string part in sep == '\0' ? new[] { c[ci] } : c[ci].Split(sep))
            {
                string v = part.Split(':')[0].Split('*')[0].Trim();
                if (v.Length > 0)
                {
                    yield return v;
                }
            }
        }
    }

    /// <summary>씬·프리팹 YAML 의 DropManager.dropItems 이름(문서 단위로 dropItems 가 있는 것만)</summary>
    private static IEnumerable<(string file, string item)> YamlDrops(string path)
    {
        string text = File.ReadAllText(path);
        foreach (string doc in Regex.Split(text, "\n--- !u!"))
        {
            int at = doc.IndexOf("dropItems:");
            if (at < 0)
            {
                continue;
            }
            foreach (Match m in Regex.Matches(doc.Substring(at), @"- itemName: (.+)"))
            {
                yield return (Path.GetFileName(path), m.Groups[1].Value.Trim());
            }
        }
    }

    private static List<(string where, string item)> AllSources()
    {
        var list = new List<(string, string)>();
        void Add(string where, IEnumerable<string> items)
        {
            foreach (string i in items)
            {
                list.Add((where, i));
            }
        }
        Add("ShopStock", Column("ShopStock", "ItemId"));
        Add("ArmorCraftRecipes", Column("ArmorCraftRecipes", "result"));
        Add("SelfCraftRecipes", Column("SelfCraftRecipes", "result"));
        Add("MonsterExtraDrops", Column("MonsterExtraDrops", "item"));
        Add("MiniBosses", Column("MiniBosses", "guaranteed", '|'));
        Add("QuestsFloor1", Column("QuestsFloor1", "rewardItem"));
        Add("Floor1BestiaryRewards", Column("Floor1BestiaryRewards", "item"));
        Add("Floor1Treasures", Column("Floor1Treasures", "item"));
        Add("Floor1Raids", Column("Floor1Raids", "item"));
        Add("Floor1Bounties", Column("Floor1Bounties", "item"));
        Add("Floor1WeaponTrials", Column("Floor1WeaponTrials", "weapons", '|'));
        foreach (BossReward r in Resources.LoadAll<BossReward>("Boss"))
        {
            Add("Boss/" + r.name, (r.setPieces ?? new string[0]).Concat(string.IsNullOrEmpty(r.uniqueItem) ? new string[0] : new[] { r.uniqueItem }));
        }
        var yaml = new List<string> { "Assets/Scenes/SampleScene.unity" };
        yaml.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab", "Assets/Resources" }).Select(AssetDatabase.GUIDToAssetPath));
        foreach (string p in yaml)
        {
            string full = Path.GetFullPath(p);
            if (File.Exists(full))
            {
                foreach (var d in YamlDrops(full))
                {
                    list.Add((d.file, d.item));
                }
            }
        }
        return list;
    }

    [Test]
    public void RetiredRows_KeptForSaves_AndMarked()
    {
        Sheet s = LoadSheet();
        foreach (string n in RetiredMust)
        {
            Assert.IsTrue(s.All.Contains(n), "세이브 호환: 행을 지우면 안 된다 " + n);
            Assert.IsTrue(s.Retired.Contains(n), "옛 신발(폐기 슬롯)은 retired=1 이어야 한다 " + n);
        }
    }

    [Test]
    public void AcquisitionSources_HaveNoRetiredGear()
    {
        Sheet s = LoadSheet();
        var bad = AllSources().Where(x => s.Retired.Contains(x.item)).Select(x => x.where + ":" + x.item).Distinct().ToList();
        Assert.IsEmpty(bad, "획득 경로에 폐기 장비: " + string.Join(", ", bad));
    }

    [Test]
    public void AcquisitionSources_ReferExistingItems()
    {
        Sheet s = LoadSheet();
        var missing = AllSources().Where(x => !s.All.Contains(x.item)).Select(x => x.where + ":" + x.item).Distinct().ToList();
        Assert.IsEmpty(missing, "시트에 없는 아이템: " + string.Join(", ", missing));
    }

    [Test]
    public void ObtainableSetAccessories_UseNewSlotNames()
    {
        // 옛 투구·하의·장갑(키 *Cap/*LegArmor/*HandArmor)은 귀걸이·목걸이·반지가 됐다 — 표시 이름에 옛 부위 낱말이 남으면 안 된다
        Sheet s = LoadSheet();
        string[] oldWords = { "Cap", "Hood", "Helm", "Cowl", "Greaves", "Leggings", "Breeches", "Gloves", "Gauntlets", "LegArmor", "HandArmor", "Boots" };
        var obtainable = new HashSet<string>(AllSources().Select(x => x.item));
        foreach (string n in s.Equip.Where(obtainable.Contains))
        {
            string[] c = s.Rows[n];
            string area = c[s.AreaCol].Trim();
            if (area != "Earring" && area != "Ring" && area != "Necklace")
            {
                continue;
            }
            string shown = c[s.NameTCol].Trim();
            Assert.IsFalse(oldWords.Any(w => shown.EndsWith(w)), n + " 표시 이름이 옛 부위: " + shown);
        }
    }

    [Test]
    public void RetiredDef_CannotBeEquipped()
    {
        var def = new ItemDef { Id = "WolfShoes", Type = "Equip", EquipArea = "Charm", Retired = true };
        Assert.IsFalse(EquipmentModel.TryGetSlot(def, out _));
        def.Retired = false;
        Assert.IsTrue(EquipmentModel.TryGetSlot(def, out EquipSlot slot));
        Assert.AreEqual(EquipSlot.Charm, slot);
    }
}
