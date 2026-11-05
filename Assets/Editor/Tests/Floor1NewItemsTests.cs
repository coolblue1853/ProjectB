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
        return RowsAt(ResPath(file));
    }

    private static string ResPath(string file)
    {
        return Path.Combine(Application.dataPath, "Resources/" + file);
    }

    private static readonly Dictionary<string, string[]> s_headers = new Dictionary<string, string[]>();

    /// <summary>헤더 행(앞 3줄 중 열 이름이 있는 줄)에서 열 이름의 위치를 찾는다. 없으면 명확한 메시지로 실패</summary>
    private static int Col(string path, string name)
    {
        if (!s_headers.TryGetValue(path, out string[] head))
        {
            string text = File.ReadAllText(path).TrimStart((char)0xFEFF);
            head = text.Replace("\r", "").Split('\n').Take(3).Select(l => l.Split(',')).FirstOrDefault(c => c.Any(x => x.Trim() == name));
            s_headers[path] = head;
        }
        int idx = head == null ? -1 : System.Array.FindIndex(head, x => x.Trim() == name);
        Assert.GreaterOrEqual(idx, 0, path + " 헤더에 열 '" + name + "' 없음");
        return idx;
    }

    /// <summary>ItemSheet.csv 한 행의 열 값(헤더명으로 조회)</summary>
    private static string I(string[] r, string col)
    {
        return r[Col(ResPath("ItemSheet.csv"), col)].Trim();
    }

    private static string M(string[] r, string col)
    {
        return r[Col(ResPath("MonsterSheet.csv"), col)].Trim();
    }

    private static string D(string[] r, string col)
    {
        string path = ResPath("MonsterExtraDrops.csv");
        int i = Col(path, col);
        return i < r.Length ? r[i].Trim() : "";
    }

    private static string[] Item(int id)
    {
        return Rows("ItemSheet.csv").First(r => I(r, "index") == id.ToString());
    }

    [Test]
    public void Ids_ExistWithExpectedNames()
    {
        for (int i = 0; i < Ids.Length; i++)
        {
            Assert.AreEqual(Names[i], I(Item(Ids[i]), "name"), "id " + Ids[i]);
        }
    }

    [Test]
    public void Ids_AreUniqueInItemSheet()
    {
        var rows = Rows("ItemSheet.csv");
        foreach (int id in Ids)
        {
            Assert.AreEqual(1, rows.Count(r => I(r, "index") == id.ToString()), "중복 id " + id);
        }
        foreach (string n in Names)
        {
            Assert.AreEqual(1, rows.Count(r => I(r, "name") == n), "중복 이름 " + n);
        }
    }

    [Test]
    public void Ids_DoNotCollideWithFloor2Draft()
    {
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Floor2Data/Floor2ItemSheet_draft.csv"));
        var draft = RowsAt(path);
        int idCol = Col(path, "index");
        int nameCol = Col(path, "name");
        var draftIds = new HashSet<string>(draft.Select(r => r[idCol].Trim()));
        var draftNames = new HashSet<string>(draft.Select(r => r[nameCol].Trim()));
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
            Assert.AreEqual("Equip", I(r, "type"), "type " + id);
            Assert.AreEqual("Weapon", I(r, "equipArea"), "equipArea " + id);
            Assert.AreEqual("R", I(r, "rarity"), "rarity " + id);
        }
    }

    [Test]
    public void WeaponTypeColumn_IsPinned()
    {
        // 현재 동작 고정: 열은 기믹 계열(Crossbow/Javelin/Tome)이며 프리팹 _kind 와 일치(AUTO-100 후속)
        Assert.AreEqual("Crossbow", I(Item(313), "WeaponType"));
        Assert.AreEqual("Javelin", I(Item(314), "WeaponType"));
        Assert.AreEqual("Tome", I(Item(315), "WeaponType"));
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
    public void HostPrefabs_KindMatchesWeaponTypeColumn()
    {
        var expect = new WeaponKind[] { WeaponKind.Crossbow, WeaponKind.Javelin, WeaponKind.Tome };
        for (int i = 0; i < Names.Length; i++)
        {
            Weapon w = Resources.Load<GameObject>("Weapon/" + Names[i]).GetComponent<Weapon>();
            Assert.AreEqual(expect[i], w.Kind, Names[i]);
            Assert.AreEqual(I(Item(Ids[i]), "WeaponType"), w.Kind.ToString(), Names[i]);
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
            var hit = drops.Where(r => D(r, "monsterId") == kv.Key && D(r, "item") == kv.Value).ToArray();
            Assert.AreEqual(1, hit.Length, kv.Key + "→" + kv.Value);
            Assert.AreEqual(0.02f, float.Parse(D(hit.First(), "probability"), System.Globalization.CultureInfo.InvariantCulture), 1e-6f, kv.Value);
            Assert.IsTrue(string.IsNullOrWhiteSpace(D(hit.First(), "condition")), "조건 없는 일반 드롭: " + kv.Value);
        }
    }

    [Test]
    public void ExtraDrops_MonstersAndItemsExist()
    {
        var monsters = new HashSet<string>(Rows("MonsterSheet.csv").Select(r => M(r, "id")));
        var items = new HashSet<string>(Rows("ItemSheet.csv").Select(r => I(r, "name")));
        foreach (var r in Rows("MonsterExtraDrops.csv").Where(r => Names.Contains(D(r, "item"))))
        {
            Assert.IsTrue(monsters.Contains(D(r, "monsterId")), "몬스터 없음 " + D(r, "monsterId"));
            Assert.IsTrue(items.Contains(D(r, "item")), "아이템 없음 " + D(r, "item"));
        }
    }

    [Test]
    public void Weapons_BelongToNoSet_AndNoOtherSourceDrops()
    {
        string setPath = ResPath("SetItem.csv");
        int setCol = Col(setPath, "setName");
        var setNames = new HashSet<string>(Rows("SetItem.csv").Select(r => r[setCol].Trim()));
        foreach (int id in Ids)
        {
            string[] r = Item(id);
            Assert.AreEqual("", I(r, "tfName"), "tfName(세트 키) 비어 있어야 함 " + id);
            Assert.AreEqual("", I(r, "setName"), "setName 비어 있어야 함 " + id);
            Assert.IsFalse(setNames.Contains(I(r, "name")), "세트 이름으로 등록됨 " + I(r, "name"));
        }
        var drops = Rows("MonsterExtraDrops.csv");
        foreach (string n in Names)
        {
            Assert.AreEqual(1, drops.Count(r => D(r, "item") == n), "드롭 행은 1개: " + n);
        }
    }

    [Test]
    public void DropMonsters_MatchLineage_RangedHunterThemes()
    {
        // 계보: 석궁=밴딧 센트리(재장전 원거리), 투창=숲 사냥꾼(원거리 사냥꾼), 마도서=덩굴 요정(요정)
        var sheet = Rows("MonsterSheet.csv");
        Assert.IsTrue(M(sheet.First(r => M(r, "id") == "BanditSentry_BD"), "note").Contains("range"));
        Assert.IsTrue(M(sheet.First(r => M(r, "id") == "ForestHunter_BD"), "note").Contains("ranged"));
        Assert.IsTrue(M(sheet.First(r => M(r, "id") == "VineFairy_BD"), "displayName").Contains("Fairy"));
        foreach (string id in new[] { "BanditSentry_BD", "ForestHunter_BD", "VineFairy_BD" })
        {
            Assert.AreEqual("1", M(sheet.First(r => M(r, "id") == id), "floor"), id + " 은 1층계 몹");
        }
    }
}
