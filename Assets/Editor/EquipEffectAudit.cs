using System.Text;
using UnityEngine;

/// <summary>REWORK-12 P7: 아이템·세트 효과 점검 표(마크다운) 생성. 프로브 `efx_p7_audit.cs` 가 호출해 Docs/Playtest/EquipEffectAudit.md 에 쓴다</summary>
public static class EquipEffectAudit
{
    public static string Build()
    {
        EffectTable.ResetCache();
        EffectTable.Load();
        var sb = new StringBuilder();
        sb.Append("# 장비 효과 점검 (자동 생성, REWORK-12 P7)\n\n");
        sb.Append("점수는 `EffectScore`(설계 §3.2), 예산 R10·U24·L45, 판정 ±30%. none = 등급 기준 미달(효과 없음).\n\n## 아이템\n\n");
        sb.Append("| 아이템 | 등급 | 효과 id | 점수 | 판정 |\n|---|---|---|---|---|\n");
        string[] lines = Resources.Load<TextAsset>("ItemSheet").text.Replace("\r", "").Split('\n');
        int none = 0;
        for (int i = 2; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 20 || c[2] != "Equip")
            {
                continue;
            }
            string grade = c[14];
            float score = EffectScore.OfIds(c[19]);
            string j = EffectScore.Judge(grade, score);
            if (j == "none")
            {
                none++;
            }
            if (grade == "C" && score <= 0f)
            {
                continue; // C 등급 효과 없음은 정상이라 표에서 생략
            }
            sb.Append("| ").Append(c[1]).Append(" | ").Append(grade).Append(" | ").Append(c[19]).Append(" | ").Append(score.ToString("0.#")).Append(" | ").Append(j).Append(" |\n");
        }
        sb.Append("\nR 이상 효과 없음(none): ").Append(none).Append("건\n\n## 세트 단계 (SetItem.csv)\n\n| 키 | 효과 id | 점수 |\n|---|---|---|\n");
        string[] sets = Resources.Load<TextAsset>("SetItem").text.Replace("\r", "").Split('\n');
        for (int i = 2; i < sets.Length; i++)
        {
            string[] c = sets[i].Split(',');
            if (c.Length < 3 || string.IsNullOrWhiteSpace(c[c.Length - 1]))
            {
                continue;
            }
            string ids = c[c.Length - 1];
            sb.Append("| ").Append(c[0]).Append(" | ").Append(ids).Append(" | ").Append(EffectScore.OfIds(ids).ToString("0.#")).Append(" |\n");
        }
        return sb.ToString();
    }
}
