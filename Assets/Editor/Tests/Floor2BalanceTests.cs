using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>2층계 기존 몹 수치와 실행에 등록하지 않은 퀘스트 초안의 구조·예산을 검증한다.</summary>
public class Floor2BalanceTests
{
    [Test]
    public void ExistingMonsters_ParseWithoutDuplicatesAndStayInZoneBand()
    {
        var csv = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/MonsterSheet.csv");
        Assert.IsNotNull(csv);
        var ids = new HashSet<string>();
        int count = 0;
        foreach (string line in csv.text.Split('\n'))
        {
            string[] c = line.TrimEnd('\r').Split(',');
            if (c[0] == "id" || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            Assert.AreEqual(15, c.Length, c[0]);
            Assert.IsTrue(ids.Add(c[0]), "중복 id: " + c[0]);
            if (c[2] != "2")
            {
                continue;
            }
            count++;
            var row = MonsterTable.Get(c[0]);
            Assert.IsNotNull(row, c[0]);
            int level = int.Parse(c[12]);
            bool rampart = c[3].StartsWith("Broken Rampart", StringComparison.Ordinal);
            Assert.That(level, Is.InRange(rampart ? 18 : 16, rampart ? 19 : 17), c[0]);
            Assert.AreEqual(level, row.level, c[0]);
            Assert.AreEqual(int.Parse(c[13]), row.exp, c[0]);
            Assert.AreEqual(int.Parse(c[14]), row.contactDamage, c[0]);
            Assert.Greater(row.exp.Value, 0, c[0]);
            double ratio = row.contactDamage.Value / (200.0 + LevelRules.HpBonus(level));
            Assert.That(ratio, Is.InRange(c[6] == "1" ? 0.12 : 0.06, c[6] == "1" ? 0.18 : 0.101), c[0]);
        }
        Assert.AreEqual(11, count, "미등록 몹을 실행 CSV에 추가하지 않는다");
    }

    [Test]
    public void DraftQuests_HaveSameSchemaValidDependenciesAndFixedBudget()
    {
        string root = Path.GetDirectoryName(Application.dataPath);
        string[] lines = File.ReadAllLines(Path.Combine(root, "Docs/Data/floor2_quests_draft.csv"));
        string header = File.ReadAllLines(Path.Combine(Application.dataPath, "Resources/QuestsFloor1.csv"))[0];
        Assert.AreEqual(header, lines[0]);
        Assert.AreEqual(21, lines.Length);
        var seen = new HashSet<string> { "Q604" };
        int exp = 0;
        int main = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            Assert.AreEqual(13, c.Length, "초안도 기존 단순 CSV 로더와 호환");
            Assert.IsFalse(seen.Contains(c[0]), "중복 퀘스트: " + c[0]);
            foreach (string dependency in c[4].Split(';'))
            {
                Assert.IsTrue(seen.Contains(dependency), c[0] + " 선행: " + dependency);
            }
            seen.Add(c[0]);
            Assert.That(int.Parse(c[3]), Is.InRange(15, 25));
            Assert.Greater(int.Parse(c[7]), 0);
            Assert.IsNotEmpty(c[2]);
            Assert.IsNotEmpty(c[6]);
            Assert.IsNotEmpty(c[12]);
            Assert.IsTrue(c[5] == "kill" || c[5] == "visit");
            exp += int.Parse(c[8]);
            if (c[12].StartsWith("메인:", StringComparison.Ordinal))
            {
                main++;
            }
        }
        Assert.AreEqual(6, main);
        Assert.AreEqual(181684, exp);
        Assert.IsFalse(File.Exists(Path.Combine(Application.dataPath, "Resources/QuestsFloor2.csv")));
    }

    [Test]
    public void CurveAndQuestTiming_KeepLevel25RewardsOutOfLevel25Arrival()
    {
        int to25 = 0;
        for (int level = 15; level < 25; level++)
        {
            to25 += LevelRules.ExpToNext(level);
        }
        Assert.AreEqual(595582, to25);
        int to26 = to25 + LevelRules.ExpToNext(25);
        Assert.AreEqual(726734, to26);
        string root = Path.GetDirectoryName(Application.dataPath);
        string[] lines = File.ReadAllLines(Path.Combine(root, "Docs/Data/floor2_quests_draft.csv"));
        int earlyRewards = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (int.Parse(c[3]) < 25)
            {
                earlyRewards += int.Parse(c[8]);
            }
        }
        Assert.AreEqual(132000, earlyRewards);
        Assert.That(181684.0 / to26, Is.InRange(0.24999, 0.25001));
    }
}
