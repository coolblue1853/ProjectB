using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>L7 신규 몹 6종(Floor1Completion §7) 데이터 무결성: id 유일, 스폰 참조, 드롭 아이템 실존, 무기 드롭 이전.</summary>
public class L7MobDataTests
{
    static readonly string[] Ids =
    {
        "ForestHunter_BD", "VineFairy_BD", "ForestOgre_BD", "BanditSentry_BD", "BanditWoodcutter_BD", "RuinWarden_BD",
    };

    static string[] Lines(string resource)
    {
        TextAsset t = Resources.Load<TextAsset>(resource);
        Assert.IsNotNull(t, resource);
        return t.text.Replace("\r", "").Split('\n');
    }

    [Test]
    public void MonsterSheet_IdsUnique()
    {
        var seen = new HashSet<string>();
        string[] lines = Lines("MonsterSheet");
        for (int i = 1; i < lines.Length; i++)
        {
            string id = lines[i].Split(',')[0].Trim();
            if (id.Length > 0)
            {
                Assert.IsTrue(seen.Add(id), "중복 id " + id);
            }
        }
    }

    [Test]
    public void NewMobs_RowsValid()
    {
        foreach (string id in Ids)
        {
            MonsterTable.Row r = MonsterTable.Get(id);
            Assert.IsNotNull(r, id);
            Assert.AreEqual(id, r.killKey, id);
            Assert.IsFalse(r.isBoss ?? false, id);
            Assert.IsTrue(r.tint.HasValue, id + " 임시 틴트");
            Assert.IsTrue((r.maxHP ?? 0) > 0 && (r.level ?? 0) > 0 && (r.exp ?? 0) > 0 && (r.contactDamage ?? 0) > 0, id);
        }
    }

    [Test]
    public void NewMobs_ReferencedBySpawnAndHostPrefabExists()
    {
        string spawns = string.Join("\n", Lines("HuntingSpawnsFloor1"));
        foreach (string id in Ids)
        {
            Assert.IsTrue(spawns.Contains(id + "@"), id + " 스폰 행 없음");
        }
        foreach (string host in new[] { "ForestWolf_BD", "WildBoar_BD", "Ent_BD", "BanditArcher_BD", "BanditCutthroat_BD", "GolemSmall_BD" })
        {
            Assert.IsNotNull(MonsterTable.Get(host), host + " 호스트 행");
#if UNITY_EDITOR
            Assert.IsNotEmpty(UnityEditor.AssetDatabase.FindAssets(host + " t:Prefab"), host + " 호스트 프리팹");
#endif
        }
    }

    [Test]
    public void WeaponDrops_MovedToNewMobs()
    {
        var items = new HashSet<string>(Lines("ItemSheet").Skip(2).Select(l => l.Split(',')).Where(c => c.Length > 1).Select(c => c[1]));
        var moves = new Dictionary<string, string>
        {
            { "ForestHunter_BD", "HuntersLongbow" }, { "VineFairy_BD", "VineLash" },
            { "ForestOgre_BD", "GnarledClub" }, { "BanditWoodcutter_BD", "WoodcuttersAxe" },
        };
        foreach (KeyValuePair<string, string> kv in moves)
        {
            Assert.IsTrue(ExtraDrops.For(kv.Key).Any(e => e.Item == kv.Value), kv.Key);
            foreach (string old in new[] { "BanditArcher_BD", "Ent_BD", "BanditCutthroat_BD", "Fairy_BD" })
            {
                List<ExtraDrops.Entry> l = ExtraDrops.For(old);
                Assert.IsFalse(l != null && l.Any(e => e.Item == kv.Value && !e.QuestOnly), old + " 에 " + kv.Value + " 잔존");
            }
        }
        foreach (string id in Ids)
        {
            List<ExtraDrops.Entry> l = ExtraDrops.For(id);
            if (l != null)
            {
                foreach (ExtraDrops.Entry e in l)
                {
                    Assert.IsTrue(items.Contains(e.Item), id + " 드롭 아이템 없음: " + e.Item);
                }
            }
        }
    }
}
