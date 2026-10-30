using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>정밀 단조 패널(코드 생성, 강화 창 오른쪽 영역). <see cref="ForgeSession"/> 을 읽기만 한다.</summary>
public sealed class ForgeHammerView
{
    private const float BarWidth = 420f;

    private readonly GameObject _root;
    private readonly TextMeshProUGUI _title;
    private readonly TextMeshProUGUI _scores;
    private readonly TextMeshProUGUI _hint;
    private readonly RectTransform _good;
    private readonly RectTransform _great;
    private readonly RectTransform _perfect;
    private readonly RectTransform _cursor;
    private float _shownTarget = -1f;
    private readonly System.Text.StringBuilder _sb = new System.Text.StringBuilder(128);
    private int _shownKey = -1;   // 점수 텍스트 캐시 키(판정 3개·합계·환불량)

    public ForgeHammerView(RectTransform parent, UiTheme theme)
    {
        _root = new GameObject("ForgeHammer", typeof(RectTransform));
        var rt = (RectTransform)_root.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(510f, -86f);
        rt.sizeDelta = new Vector2(460f, 330f);

        _title = NewText("Title", rt, theme, 30f, new Vector2(0f, 0f));
        _title.color = theme.unique;
        _title.text = Loc.T("Precision forging");

        RectTransform track = NewImage("Track", rt, new Color(0.08f, 0.08f, 0.1f, 0.95f)).rectTransform;
        track.anchorMin = track.anchorMax = track.pivot = new Vector2(0f, 1f);
        track.anchoredPosition = new Vector2(20f, -90f);
        track.sizeDelta = new Vector2(BarWidth, 36f);
        _good = Zone("Good", track, new Color(0.45f, 0.75f, 0.9f, 0.4f));
        _great = Zone("Great", track, new Color(0.9f, 0.8f, 0.4f, 0.6f));
        _perfect = Zone("Perfect", track, new Color(0.55f, 0.9f, 0.45f, 0.95f));
        _cursor = NewImage("Cursor", track, Color.white).rectTransform;
        _cursor.anchorMin = new Vector2(0f, 0f);
        _cursor.anchorMax = new Vector2(0f, 1f);
        _cursor.offsetMin = new Vector2(-3f, -6f);
        _cursor.offsetMax = new Vector2(3f, 6f);

        _scores = NewText("Scores", rt, theme, 26f, new Vector2(0f, -140f));
        _hint = NewText("Hint", rt, theme, 22f, new Vector2(0f, -318f));
        _hint.color = new Color(1f, 1f, 1f, 0.6f);
        _hint.text = Loc.T("Hit with ATTACK    BACK cancel");
        _root.SetActive(false);
    }

    public bool Active => _root.activeSelf;

    public void SetActive(bool on)
    {
        _root.SetActive(on);
        _shownTarget = -1f;
        _shownKey = -1;
    }

    public void Refresh(ForgeSession s, int stonesRefundIfNine)
    {
        if (!Mathf.Approximately(s.Target, _shownTarget))
        {
            _shownTarget = s.Target;
            SetSpan(_good, s.Target - AdvancedEnhanceRules.GoodHalf, s.Target + AdvancedEnhanceRules.GoodHalf);
            SetSpan(_great, s.Target - AdvancedEnhanceRules.GreatHalf, s.Target + AdvancedEnhanceRules.GreatHalf);
            SetSpan(_perfect, s.Target - AdvancedEnhanceRules.PerfectHalf, s.Target + AdvancedEnhanceRules.PerfectHalf);
        }
        _cursor.gameObject.SetActive(!s.Waiting && !s.Done);
        float a = s.Cursor / GatherRules.CursorMax;
        _cursor.anchorMin = new Vector2(a, 0f);
        _cursor.anchorMax = new Vector2(a, 1f);

        int key = stonesRefundIfNine;
        for (int i = 0; i < AdvancedEnhanceRules.Strikes; i++)
        {
            key = key * 5 + (s.ScoreAt(i) + 1);
        }
        if (key == _shownKey)
        {
            return;
        }
        _shownKey = key;
        StringBuilder sb = _sb;
        sb.Length = 0;
        for (int i = 0; i < AdvancedEnhanceRules.Strikes; i++)
        {
            int sc = s.ScoreAt(i);
            sb.Append(sc < 0 ? "[ - ]" : "[ " + JudgeName(sc) + " ]").Append(i < AdvancedEnhanceRules.Strikes - 1 ? "  " : "");
        }
        sb.Append("\n\n").Append(Loc.F("Forge score {0}", s.Total)).Append("  ").Append(Loc.F("Forging up to +{0}%p", AdvancedEnhanceRules.ForgeBonusPct(AdvancedEnhanceRules.MaxScore)));
        sb.Append("\n9  ").Append(Loc.F("Refund {0} crystals", stonesRefundIfNine));
        _scores.text = sb.ToString();
    }

    public static string JudgeName(int score)
    {
        switch (score)
        {
            case 3: return Loc.T("PERFECT");
            case 2: return Loc.T("GREAT");
            case 1: return Loc.T("GOOD");
            default: return Loc.T("MISS");
        }
    }

    private static RectTransform Zone(string name, RectTransform parent, Color color)
    {
        RectTransform rt = NewImage(name, parent, color).rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    private static void SetSpan(RectTransform rt, float from, float to)
    {
        rt.anchorMin = new Vector2(Mathf.Clamp01(from / GatherRules.CursorMax), 0f);
        rt.anchorMax = new Vector2(Mathf.Clamp01(to / GatherRules.CursorMax), 1f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static Image NewImage(string name, Transform parent, Color color)
    {
        var img = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        img.transform.SetParent(parent, false);
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size, Vector2 pos)
    {
        var t = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        t.transform.SetParent(parent, false);
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(460f, 170f);
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.raycastTarget = false;
        return t;
    }
}
