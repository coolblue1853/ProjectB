using TMPro;
using UnityEngine;

/// <summary>
/// 씬에 영문으로 박힌 정적 버튼 문구(close·divide·Sell·Buy)를 한글로 바꾼다(REWORK-0 2차, 창고·인벤 수량 창).
/// 씬을 고치지 않고 창 루트의 Start 에서 한 번만 돈다. 글자가 정확히 같은 것만 바꾸므로 코드가 채우는 문구는 건드리지 않는다.
/// </summary>
public static class StaticLabelKo
{
    public static string Translate(string text)
    {
        switch (text)
        {
            case "close": return "닫기";
            case "divide": return "나누기";
            case "Sell": return "판매";
            case "Buy": return "구매";
            default: return null;
        }
    }

    public static void Apply(Transform root)
    {
        if (root == null)
        {
            return;
        }
        foreach (TextMeshProUGUI t in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            string ko = Translate(t.text);
            if (ko != null)
            {
                t.text = ko;
            }
        }
    }
}
