using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>장비 효과 P2 (EquipEffectDesign §2.8·§5): effectIds 열 로드 무결성, 신규 Mod op 적용·원복</summary>
public class EquipEffectDataTests
{
    static string[] Lines(string file)
    {
        string text = File.ReadAllText(Path.Combine(Application.dataPath, "Resources/" + file)).TrimStart((char)0xFEFF);
        return text.Replace("\r", "").Split('\n').Where(l => l.Length > 0).ToArray();
    }

    /// <summary>머리글 이름으로 effectIds 열을 찾아 (키, 효과 id 문자열) 목록. 열 번호에 의존하지 않는다</summary>
    static List<KeyValuePair<string, string>> IdColumn(string file, int keyCol, int headerLine)
    {
        string[] lines = Lines(file);
        string[] head = lines[headerLine].Split(',');
        int col = System.Array.IndexOf(head, "effectIds");
        Assert.GreaterOrEqual(col, 0, file + " 에 effectIds 열 없음");
        var list = new List<KeyValuePair<string, string>>();
        for (int i = headerLine + 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            Assert.AreEqual(head.Length, c.Length, file + " 열 수 불일치 행 " + (i + 1));
            list.Add(new KeyValuePair<string, string>(c[keyCol].Trim(), c[col].Trim()));
        }
        return list;
    }

    static List<KeyValuePair<string, string>> AllSources()
    {
        var all = IdColumn("ItemSheet.csv", 1, 1);
        all.AddRange(IdColumn("SetItem.csv", 0, 1));
        return all;
    }

    static List<EffectEntry> Table()
    {
        return EffectTable.ParseEntries(File.ReadAllText(Path.Combine(Application.dataPath, "Resources/EquipEffects.csv")));
    }

    [TearDown]
    public void Cleanup()
    {
        SkillEffectService.ClearAll();
        EffectTable.ResetCache();
    }

    [Test]
    public void EffectIds_AllExistInEquipEffects()
    {
        var ids = new HashSet<string>(Table().Select(e => e.id));
        var bad = AllSources().SelectMany(s => SkillEffectService.SplitIds(s.Value).Select(id => new { s.Key, id }))
            .Where(x => !ids.Contains(x.id)).Select(x => x.Key + "→" + x.id).ToArray();
        Assert.IsEmpty(bad, "존재하지 않는 효과 id: " + string.Join(", ", bad));
    }

    [Test]
    public void EffectIds_AtMostThreePerSource_NoDuplicates()
    {
        var bad = new List<string>();
        foreach (var s in AllSources())
        {
            string[] raw = s.Value.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (raw.Length > SkillEffectService.MaxIdsPerSource || raw.Distinct().Count() != raw.Length)
            {
                bad.Add(s.Key);
            }
        }
        Assert.IsEmpty(bad, string.Join(", ", bad));
    }

    [Test]
    public void EffectTable_EntryNumbersContiguous_PerIdSumWithinCaps()
    {
        var bad = new List<string>();
        foreach (var g in Table().GroupBy(e => e.id))
        {
            int[] nums = g.Select(e => e.entry).OrderBy(n => n).ToArray();
            if (!nums.SequenceEqual(Enumerable.Range(1, nums.Length)))
            {
                bad.Add(g.Key + " entry 번호 불연속");
            }
            foreach (var h in g.Where(e => e.kind == EffectKind.Mod).GroupBy(e => e.selector + "|" + e.op))
            {
                float sum = h.Sum(e => e.value);
                if (sum > Cap(h.First().op))
                {
                    bad.Add(g.Key + " " + h.Key + " 합 " + sum + " > 상한");
                }
            }
        }
        Assert.IsEmpty(bad, string.Join("; ", bad));
    }

    static float Cap(EffectOp op)
    {
        switch (op)
        {
            case EffectOp.CooldownPct: return EffectCaps.CooldownPct;
            case EffectOp.DamagePct: return EffectCaps.DamagePct;
            case EffectOp.RangePct: return EffectCaps.RangePct;
            case EffectOp.ProjSpeedPct: return EffectCaps.ProjSpeedPct;
            case EffectOp.DurationPct: return EffectCaps.DurationPct;
            case EffectOp.StaminaPct: return EffectCaps.StaminaPct;
            case EffectOp.ProcChance: return EffectCaps.ProcChance;
            case EffectOp.ProjectileAdd: return EffectCaps.ProjectileAdd;
            case EffectOp.HitCountAdd: return EffectCaps.HitCountAdd;
            case EffectOp.PierceAdd: return EffectCaps.PierceAdd;
            case EffectOp.SummonAdd: return EffectCaps.SummonAdd;
            default: return EffectCaps.CastCharge;
        }
    }

    [Test]
    public void Floor2Preview_HeadersMatchBase()
    {
        string it = File.ReadAllText(Path.Combine(Application.dataPath, "Resources/ItemSheet.csv"));
        string set = File.ReadAllText(Path.Combine(Application.dataPath, "Resources/SetItem.csv"));
        Assert.IsTrue(Floor2Data.HeaderMatches(it, Lines("Floor2Preview/Floor2ItemSheet.csv")[0]));
        Assert.IsTrue(Floor2Data.HeaderMatches(set, Lines("Floor2Preview/Floor2SetItem.csv")[0]));
    }

    [Test]
    public void OnlyTestItems_HaveEffectIds()
    {
        string[] withEffects = AllSources().Where(s => s.Value.Length > 0).Select(s => s.Key).OrderBy(x => x).ToArray();
        Assert.AreEqual(new[] { "BearClaw", "LeafAdae", "LeafStaff" }, withEffects);
    }

    [Test]
    public void SplitIds_TrimsAndLimits()
    {
        Assert.AreEqual(new[] { "A", "B", "C" }, SkillEffectService.SplitIds(" A ; B;;C ; D ").ToArray());
        Assert.IsEmpty(SkillEffectService.SplitIds(""));
        Assert.IsEmpty(SkillEffectService.SplitIds(null));
    }

    [Test]
    public void AttachIds_AppliesNewOps_DetachRestores()
    {
        EffectTable.Build(Table());
        SkillEffectService.AttachIds("Item:Weapon", "EF_LeafStaff", "R");
        SkillMods m = SkillEffectService.Query("Scattering_Leaves", WeaponKind.Staff);
        Assert.AreEqual(10f, m.damagePct);
        Assert.AreEqual(10f, m.rangePct);
        Assert.AreEqual(0f, SkillEffectService.Query("Haste", WeaponKind.Staff).damagePct, "다른 스킬은 무영향");
        SkillEffectService.Detach("Item:Weapon");
        m = SkillEffectService.Query("Scattering_Leaves", WeaponKind.Staff);
        Assert.AreEqual(0f, m.damagePct);
        Assert.AreEqual(0f, m.rangePct);
    }

    [Test]
    public void DevTestEffect_StaminaProjSpeedDuration_AndCap()
    {
        EffectTable.Build(Table());
        SkillEffectService.AttachIds("Dev:A", "EF_DEV_Test");
        SkillEffectService.AttachIds("Dev:B", "EF_DEV_Test"); // 다른 출처 같은 효과: 합산되어 상한(StaminaPct 50)
        SkillMods m = SkillEffectService.Query("Anything", WeaponKind.None);
        Assert.AreEqual(40f, m.staminaPct);
        Assert.AreEqual(40f, m.projSpeedPct);
        Assert.AreEqual(40f, m.durationPct);
        SkillEffectService.AttachIds("Dev:C", "EF_DEV_Test");
        Assert.AreEqual(50f, SkillEffectService.Query("Anything", WeaponKind.None).staminaPct, "상한 50");
        Assert.AreEqual(50f, SkillEffectService.StaminaPct("Anything", WeaponKind.None));
        SkillEffectService.ClearAll();
        Assert.AreEqual(0f, SkillEffectService.StaminaPct("Anything", WeaponKind.None), "원복(캐시 무효화)");
    }

    [Test]
    public void UnknownEffectId_IsIgnored_NoThrow()
    {
        EffectTable.Build(Table());
        SkillEffectService.AttachIds("Item:X", "EF_DOES_NOT_EXIST");
        Assert.AreEqual(0, SkillEffectService.ActiveCount);
    }

    [Test]
    public void SkillModApplier_ScaleDamageAndStamina()
    {
        int[] src = { 100, 50 };
        Assert.AreSame(src, SkillModApplier.ScaleDamage(src, 0f), "효과 없으면 같은 참조(수치 불변)");
        Assert.AreEqual(new[] { 120, 60 }, SkillModApplier.ScaleDamage(src, 20f));
        Assert.AreEqual(100, src[0], "원본 배열 불변");
        Assert.AreEqual(10, SkillModApplier.Stamina(10, 0f));
        Assert.AreEqual(8, SkillModApplier.Stamina(10, 20f));
        Assert.AreEqual(0, SkillModApplier.Stamina(0, 20f));
    }

    [Test]
    public void ItemWithoutEffectIds_QueryUnchanged()
    {
        EffectTable.Build(Table());
        SkillEffectService.AttachIds("Item:Weapon", "", "C");
        SkillMods m = SkillEffectService.Query("Scattering_Leaves", WeaponKind.Staff);
        Assert.AreEqual(0f, m.damagePct);
        Assert.AreEqual(0f, m.rangePct);
        Assert.AreEqual(0, SkillEffectService.ActiveCount);
    }
}
