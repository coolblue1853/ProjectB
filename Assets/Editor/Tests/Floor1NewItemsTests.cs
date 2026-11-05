using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>AUTO-98: 1층계 신규 무기 313~315(SentryCrossbow·HuntersJavelin·FairysGrimoire) 데이터 정합. 현재 동작을 고정한다(읽기만).</summary>
public class Floor1NewItemsTests
{
    private static readonly string[] Names = { "SentryCrossbow", "HuntersJavelin", "FairysGrimoire" };
    private static readonly int[] Ids = { 313, 314, 315 };

    private static string[][] RowsAt(string path)
    {
        string text = File.ReadAllText(path).TrimStart((char)0xFEFF);
        return text.Replace("\r", "").Split('\n').Skip(1).Where(l => l.Length > 0).Select(l => l.Split(',')).ToArray();
    }

    private static string[][] Rows(string file)
    {
        return RowsAt(Path.Combine(Application.dataPath, "Resources/" + file));
    }

    private static string[] Item(int id)
    {
        return Rows("ItemSheet.csv").First(r => r[0].Trim() == id.ToString());
    }

    [Test]
    public void Ids_ExistWithExpectedNames()
    {
        for (int i = 0; i < Ids.Length; i++)
        {
            Assert.AreEqual(Names[i], Item(Ids[i])[1].Trim(), "id " + Ids[i]);
        }
    }

    [Test]
    public void Ids_AreUniqueInItemSheet()
    {
        var rows = Rows("ItemSheet.csv");
        foreach (int id in Ids)
        {
            Assert.AreEqual(1, rows.Count(r => r[0].Trim() == id.ToString()), "중복 id " + id);
        }
        foreach (string n in Names)
        {
            Assert.AreEqual(1, rows.Count(r => r[1].Trim() == n), "중복 이름 " + n);
        }
    }

    [Test]
    public void Ids_DoNotCollideWithFloor2Draft()
    {
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Floor2Data/Floor2ItemSheet_draft.csv"));
        var draft = RowsAt(path);
        var draftIds = new HashSet<string>(draft.Select(r => r[0].Trim()));
        var draftNames = new HashSet<string>(draft.Select(r => r[1].Trim()));
        for (int i = 0; i < Ids.Length; i++)
        {
            Assert.IsFalse(draftIds.Contains(Ids[i].ToString()), "Floor2 초안 id 충돌 " + Ids[i]);
            Assert.IsFalse(draftNames.Contains(Names[i]), "Floor2 초안 이름 충돌 " + Names[i]);
        }
    }

    [Test]
    public void Rows_AreEquipWeapons()
    {
        foreach (int id in Ids)
        {
            string[] r = Item(id);
            Assert.AreEqual("Equip", r[2].Trim(), "type " + id);
            Assert.AreEqual("Weapon", r[10].Trim(), "equipArea " + id);
            Assert.AreEqual("R", r[14].Trim(), "rarity " + id);
        }
    }

    [Test]
    public void WeaponTypeColumn_IsPinned()
    {
        // 현재 동작 고정: 열은 호스트 무기의 계열(Bow/Spear/Staff). 프리팹 _kind 는 None(TechDebt 등재)
        Assert.AreEqual("Bow", Item(313)[17].Trim());
        Assert.AreEqual("Spear", Item(314)[17].Trim());
        Assert.AreEqual("Staff", Item(315)[17].Trim());
    }

    [Test]
    public void HostPrefabs_LoadAndCarryGimmick()
    {
        var types = new System.Type[] { typeof(AmmoGimmick), typeof(ThrownSpearGimmick), typeof(TomePageGimmick) };
        for (int i = 0; i < Names.Length; i++)
        {
            GameObject go = Resources.Load<GameObject>("Weapon/" + Names[i]);
            Assert.IsNotNull(go, "프리팹 없음 " + Names[i]);
            Assert.IsNotNull(go.GetComponent<Weapon>(), "Weapon 없음 " + Names[i]);
            Assert.IsNotNull(go.GetComponent(types[i]), "기믹 " + types[i].Name + " 없음 " + Names[i]);
        }
    }

    [Test]
    public void HostPrefabs_KindIsNone_PinnedCurrentBehavior()
    {
        foreach (string n in Names)
        {
            Weapon w = Resources.Load<GameObject>("Weapon/" + n).GetComponent<Weapon>();
            Assert.AreEqual(WeaponKind.None, w.Kind, n);
        }
    }

    [Test]
    public void ExtraDrops_TwoPercentRowsExistForEachWeapon()
    {
        var drops = Rows("MonsterExtraDrops.csv");
        var expect = new Dictionary<string, string>
        {
            { "BanditSentry_BD", "SentryCrossbow" }, { "ForestHunter_BD", "HuntersJavelin" }, { "VineFairy_BD", "FairysGrimoire" },
        };
        foreach (var kv in expect)
        {
            var hit = drops.Where(r => r[0].Trim() == kv.Key && r[1].Trim() == kv.Value).ToArray();
            Assert.AreEqual(1, hit.Length, kv.Key + "→" + kv.Value);
            Assert.AreEqual(0.02f, float.Parse(hit[0][2].Trim(), System.Globalization.CultureInfo.InvariantCulture), 1e-6f, kv.Value);
            Assert.IsTrue(hit[0].Length < 4 || string.IsNullOrWhiteSpace(hit[0][3]), "조건 없는 일반 드롭: " + kv.Value);
        }
    }

    [Test]
    public void ExtraDrops_MonstersAndItemsExist()
    {
        var monsters = new HashSet<string>(Rows("MonsterSheet.csv").Select(r => r[0].Trim()));
        var items = new HashSet<string>(Rows("ItemSheet.csv").Select(r => r[1].Trim()));
        foreach (var r in Rows("MonsterExtraDrops.csv").Where(r => Names.Contains(r[1].Trim())))
        {
            Assert.IsTrue(monsters.Contains(r[0].Trim()), "몬스터 없음 " + r[0]);
            Assert.IsTrue(items.Contains(r[1].Trim()), "아이템 없음 " + r[1]);
        }
    }

    [Test]
    public void Weapons_BelongToNoSet_AndNoOtherSourceDrops()
    {
        var setNames = new HashSet<string>(Rows("SetItem.csv").Select(r => r[0].Trim()));
        foreach (int id in Ids)
        {
            string[] r = Item(id);
            Assert.AreEqual("", r[11].Trim(), "tfName(세트 키) 비어 있어야 함 " + id);
            Assert.AreEqual("", r[16].Trim(), "setName 비어 있어야 함 " + id);
            Assert.IsFalse(setNames.Contains(r[1].Trim()), "세트 이름으로 등록됨 " + r[1]);
        }
        var drops = Rows("MonsterExtraDrops.csv");
        foreach (string n in Names)
        {
            Assert.AreEqual(1, drops.Count(r => r[1].Trim() == n), "드롭 행은 1개: " + n);
        }
    }

    [Test]
    public void DropMonsters_MatchLineage_RangedHunterThemes()
    {
        // 계보: 석궁=밴딧 센트리(재장전 원거리), 투창=숲 사냥꾼(원거리 사냥꾼), 마도서=덩굴 요정(요정)
        var sheet = Rows("MonsterSheet.csv");
        Assert.IsTrue(sheet.First(r => r[0].Trim() == "BanditSentry_BD")[11].Contains("range"));
        Assert.IsTrue(sheet.First(r => r[0].Trim() == "ForestHunter_BD")[11].Contains("ranged"));
        Assert.IsTrue(sheet.First(r => r[0].Trim() == "VineFairy_BD")[1].Contains("Fairy"));
        foreach (string id in new[] { "BanditSentry_BD", "ForestHunter_BD", "VineFairy_BD" })
        {
            Assert.AreEqual("1", sheet.First(r => r[0].Trim() == id)[2].Trim(), id + " 은 1층계 몹");
        }
    }
}
