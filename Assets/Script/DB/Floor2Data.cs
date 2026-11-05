using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 2층계 초안 데이터 로드 스위치 (AUTO-22, Docs/Floor2Data/MigrationPlan.md).
/// Enabled=false(기본)면 모든 로더가 기존 CSV 그대로 읽는다. true 면 Resources/Floor2Preview/ 의 초안 데이터 행을 끝에 이어 붙인다.
/// 이어 붙일 때 초안 머리글 1행은 버린다. 로더가 처음 읽기 전에 켜야 한다(로드 결과는 캐시된다).
/// </summary>
public static class Floor2Data
{
    public static bool Enabled { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Enabled = false;
    }

    /// <summary>스위치 OFF 면 baseText 를 그대로, ON 이면 previewName 의 데이터 행을 이어 붙여 돌려준다.</summary>
    public static string Merge(string baseText, string previewName)
    {
        return Enabled ? MergeRows(baseText, PreviewText(previewName)) : baseText;
    }

    /// <summary>순수 규칙: 초안 머리글 1행을 버리고 데이터 행을 baseText 끝에 이어 붙인다(테스트용으로 공개).</summary>
    public static string MergeRows(string baseText, string previewText)
    {
        if (string.IsNullOrEmpty(previewText))
        {
            return baseText;
        }
        string[] lines = previewText.Replace("\r", "").TrimStart('\uFEFF').Split('\n');
        var sb = new StringBuilder(baseText.TrimEnd('\r', '\n'));
        for (int i = 1; i < lines.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                sb.Append('\n').Append(lines[i]);
            }
        }
        return sb.ToString();
    }

    private static string PreviewText(string previewName)
    {
        TextAsset t = Resources.Load<TextAsset>("Floor2Preview/" + previewName);
        return t != null ? t.text : "";
    }
}
