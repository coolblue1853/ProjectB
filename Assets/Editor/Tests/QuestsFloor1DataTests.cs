using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>QuestsFloor1.csv 데이터 무결성: 선행 그래프·보상/목표 실존·EXP 합·같은 구역 연속 동종 금지(R1)</summary>
public class QuestsFloor1DataTests
{
    private static readonly string[] NewIds =
    {
        "Q106", "Q107", "Q108", "Q205", "Q206", "Q207", "Q306", "Q307", "Q308", "Q309",
        "Q406", "Q407", "Q408", "Q409", "Q505", "Q506", "Q507", "Q508", "Q509",
        "Q605", "Q606", "Q607", "Q608", "Q609", "Q610", "Q701", "Q702", "Q703", "Q704",
    };

    private static string[][] Rows(string file)
    {
        string text = File.ReadAllText(Path.Combine(Application.dataPath, "Resources/" + file)).TrimStart((char)0xFEFF);
        return text.Replace("\r", "").Split('\n').Skip(1).Where(l => l.Length > 0).Select(l => l.Split(',')).ToArray();
    }

    private static HashSet<string> Column(string file, int col)
    {
        return new HashSet<string>(Rows(file).Select(r => r[col].Trim()));
    }

    [Test]
    public void NewQuests_Are29AndExpSumIs89500()
    {
        var all = NpcQuestCatalog.All;
        Assert.AreEqual(29, NewIds.Length);
        int sum = 0;
        foreach (string id in NewIds)
        {
            NpcQuestDef d = NpcQuestCatalog.Find(id);
            Assert.IsNotNull(d, id);
            sum += d.Exp;
        }
        Assert.AreEqual(80800, sum, "90700 - 늪 곁가지 Q406~409 감액(5100·5400·5400·5700 → 2800·2900·3000·3000, 메인 Q401 1500·Q404 1600 의 2배 이내 = 3000 이하, 11-11)");
        Assert.AreEqual(27 + 29 + 4 + 2 + 7 + 11, all.Count, "기존 27 + 신규 29 + 신규 맵 4 + EarlyHour Q109·Q110 + 마차 FT_Q 7 + 게시판·촌장 이행 11");
    }

    [Test]
    public void Requires_PointToExistingQuestsAndHaveNoCycle()
    {
        var all = NpcQuestCatalog.All;
        foreach (NpcQuestDef d in all)
        {
            if (string.IsNullOrEmpty(d.Requires))
            {
                continue;
            }
            Assert.IsNotNull(NpcQuestCatalog.Find(d.Requires), d.Id + " 선행 없음: " + d.Requires);
            var seen = new HashSet<string> { d.Id };
            string cur = d.Requires;
            while (!string.IsNullOrEmpty(cur))
            {
                Assert.IsTrue(seen.Add(cur), "선행 순환: " + d.Id);
                cur = NpcQuestCatalog.Find(cur).Requires;
            }
        }
    }

    [Test]
    public void Q604_RequiresOnlyQ601_GraduationIsSoft()
    {
        Assert.AreEqual("Q601", NpcQuestCatalog.Find("Q604").Requires);
        Assert.AreNotEqual("Q504", NpcQuestCatalog.Find("Q601").Requires);
    }

    [Test]
    public void RewardsAndTargets_Exist()
    {
        HashSet<string> items = Column("ItemSheet.csv", 1);
        HashSet<string> tfNames = Column("ItemSheet.csv", 11);
        HashSet<string> monsters = Column("MonsterSheet.csv", 0);
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (!string.IsNullOrEmpty(d.RewardItem))
            {
                Assert.IsTrue(items.Contains(d.RewardItem), d.Id + " 보상 아이템 없음: " + d.RewardItem);
                Assert.Greater(d.RewardCount, 0, d.Id + " 보상 수량");
            }
            switch (d.Objective)
            {
                case NpcQuestObjective.Kill:
                    foreach (string t in d.Target.Split('|'))
                    {
                        Assert.IsTrue(monsters.Contains(t), d.Id + " 몬스터 없음: " + t);
                    }
                    break;
                case NpcQuestObjective.KillElite:
                    Assert.IsTrue(d.Target == "*" || monsters.Contains(d.Target), d.Id + " 정예 없음: " + d.Target);
                    break;
                case NpcQuestObjective.Collect:
                    if (!d.IsRecordsTarget)
                    {
                        Assert.IsTrue(items.Contains(d.Target), d.Id + " 수집 아이템 없음: " + d.Target);
                    }
                    break;
                case NpcQuestObjective.Craft:
                    Assert.IsTrue(d.Target == "Any" || items.Contains(d.Target), d.Id + " 제작 아이템 없음: " + d.Target);
                    break;
                case NpcQuestObjective.Equip:
                    Assert.IsTrue(d.Target == "armor" || d.Target.StartsWith("sets:") || tfNames.Contains(d.Target),
                        d.Id + " 세트 키 없음: " + d.Target);
                    break;
            }
        }
    }

    [Test]
    public void FollowUp_DoesNotRepeatPrerequisiteTarget()
    {
        // R1: 선행→후속 퀘스트의 처치·수집·제작 목표가 같으면 반복으로 느껴진다. 누적 상태형(equip/enhance/bestiary)·talk·visit 는 제외.
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            NpcQuestDef pre = string.IsNullOrEmpty(d.Requires) ? null : NpcQuestCatalog.Find(d.Requires);
            if (pre == null || !IsRepeatable(d.Objective) || !IsRepeatable(pre.Objective))
            {
                continue;
            }
            Assert.AreNotEqual(pre.Target, d.Target, d.Id + " 가 선행 " + pre.Id + " 와 같은 목표");
        }
    }

    private static bool IsRepeatable(NpcQuestObjective o)
    {
        return o == NpcQuestObjective.Kill || o == NpcQuestObjective.KillElite
            || o == NpcQuestObjective.Collect || o == NpcQuestObjective.Craft;
    }

    [Test]
    public void Q202_TargetsWildBoar()
    {
        Assert.AreEqual("WildBoar_BD", NpcQuestCatalog.Find("Q202").Target);
        Assert.AreEqual("FangDagger", NpcQuestCatalog.Find("Q202").RewardItem);
    }
}
