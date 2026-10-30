using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 요리 HUD(코드 생성, 낚시·채집·발굴 HUD 와 같은 자리·테마). CookingStove.Current 를 읽기만 한다.
/// 선택(◀ 레시피 ▶ + 재료) → 굽기(왕복 커서 + 목표 구역) / 끓이기(화력 막대 + 안전 구역) → 결과(등급 + 얻은 것).
/// </summary>
public class CookingUI : MonoBehaviour
{
    private const float PanelWidth = 460f;
    private const float BarWidth = 380f;
    private const float PlayHeight = 235f;
    private const float ResultHeight = 170f;

    private static CookingUI _instance;

    private readonly Color _track = new Color(0.08f, 0.08f, 0.1f, 0.95f);
    private readonly Color _perfectZone = new Color(0.35f, 0.8f, 0.4f, 0.9f);
    private readonly Color _goodZone = new Color(0.45f, 0.75f, 0.95f, 0.75f);
    private readonly Color _safeZone = new Color(0.4f, 0.75f, 0.95f, 0.6f);
    private readonly Color _dangerZone = new Color(0.8f, 0.2f, 0.18f, 0.55f);
    private readonly Color _cursorColor = new Color(1f, 0.85f, 0.35f);
    private readonly Color _heatNormal = new Color(1f, 0.6f, 0.25f);
    private readonly Color _heatHot = new Color(0.95f, 0.2f, 0.15f);

    private GameObject _panel;
    private RectTransform _panelRect;
    private TextMeshProUGUI _message;
    private TextMeshProUGUI _sub;
    private TextMeshProUGUI _line;
    private TextMeshProUGUI _help;
    private GameObject _bar;
    private RectTransform _perfectRect;
    private RectTransform _goodRect;
    private RectTransform _safeRect;
    private RectTransform _dangerRect;
    private RectTransform _cursor;
    private RectTransform _timeFill;
    private Image _cursorImage;
    private CookingSession _shown;
    private CookPhase _shownPhase = (CookPhase)(-1);
    private int _shownIndex = -1;
    private int _shownRound = -1;
    private int _shownMisses = -1;
    private int _shownPercent = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[CookingUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<CookingUI>();
        _instance.Build();
    }

    private void Update()
    {
        CookingStove stove = CookingStove.Current;
        CookingSession s = stove != null ? stove.Session : null;
        if (s == null)
        {
            if (_panel.activeSelf)
            {
                _panel.SetActive(false);
            }
            _shown = null;
            return;
        }
        if (!_panel.activeSelf)
        {
            _panel.SetActive(true);
        }
        if (s != _shown || s.Phase != _shownPhase)
        {
            _shown = s;
            _shownPhase = s.Phase;
            _shownIndex = _shownRound = _shownMisses = _shownPercent = -1;
            EnterPhase(stove, s);
        }
        switch (s.Phase)
        {
            case CookPhase.Select:
                RefreshSelect(s);
                break;
            case CookPhase.Play:
                if (s.Recipe.Method == CookMethod.Grill)
                {
                    RefreshGrill(s);
                }
                else
                {
                    RefreshBoil(s);
                }
                break;
        }
    }

    private void EnterPhase(CookingStove stove, CookingSession s)
    {
        _panelRect.sizeDelta = new Vector2(PanelWidth, s.Phase == CookPhase.Result ? ResultHeight : PlayHeight);
        _bar.SetActive(s.Phase == CookPhase.Play);
        _timeFill.parent.gameObject.SetActive(s.Phase == CookPhase.Play);
        switch (s.Phase)
        {
            case CookPhase.Select:
                _help.text = Loc.T(s.Recipes.Count > 0 ? "LEFT RIGHT pick  ·  X cook  ·  ESC close" : "ESC close");
                break;
            case CookPhase.Play:
                bool grill = s.Recipe.Method == CookMethod.Grill;
                _message.text = ItemText.DisplayName(s.Recipe.Id);
                _help.text = Loc.T(grill ? "X flip when the cursor hits the mark  ·  ESC stop" : "Hold X for heat  ·  keep it in the blue  ·  ESC stop");
                BuildZones(grill);
                break;
            default:
                ShowResult(stove, s);
                break;
        }
    }

    private void RefreshSelect(CookingSession s)
    {
        if (s.Index == _shownIndex)
        {
            return;
        }
        _shownIndex = s.Index;
        if (s.Recipes.Count == 0)
        {
            _message.text = Loc.T("No ingredients");
            _sub.text = Loc.T("Nothing to cook with what you carry");
            _line.text = Loc.T("Gather, hunt and fish for ingredients");
            return;
        }
        CookRecipe r = s.Recipes[s.Index];
        _message.text = (s.Index > 0 ? "< " : "  ") + ItemText.DisplayName(r.Id) + (s.Index < s.Recipes.Count - 1 ? " >" : "  ");
        _sub.text = Loc.T(r.Method == CookMethod.Grill ? "Grill - flip on time" : "Boil - hold the heat") + "  (" + (s.Index + 1) + "/" + s.Recipes.Count + ")";
        _line.text = IngredientText(r);
    }

    private static string IngredientText(CookRecipe r)
    {
        InventoryManager inv = InventoryManager.instance;
        var sb = new StringBuilder();
        foreach (CookIngredient i in r.Ingredients)
        {
            if (sb.Length > 0)
            {
                sb.Append("  ");
            }
            int have = inv != null ? inv.CountItem(i.Id) : 0;
            sb.Append(ItemText.DisplayName(i.Id)).Append(' ').Append(have).Append('/').Append(i.Count);
        }
        return sb.ToString();
    }

    private void BuildZones(bool grill)
    {
        _perfectRect.gameObject.SetActive(grill);
        _goodRect.gameObject.SetActive(grill);
        _safeRect.gameObject.SetActive(!grill);
        _dangerRect.gameObject.SetActive(!grill);
        if (!grill)
        {
            SetZone(_safeRect, CookingRules.SafeMin, CookingRules.SafeMax);
            SetZone(_dangerRect, CookingRules.OverheatLevel, 100f);
            _cursorImage.color = _heatNormal;
        }
        else
        {
            _cursorImage.color = _cursorColor;
        }
    }

    private void RefreshGrill(CookingSession s)
    {
        _timeFill.sizeDelta = new Vector2(BarWidth * Mathf.Clamp01(1f - s.Elapsed / CookingRules.GrillTotalTime), _timeFill.sizeDelta.y);
        _cursor.gameObject.SetActive(s.WindowOpen);
        _perfectRect.gameObject.SetActive(s.WindowOpen);
        _goodRect.gameObject.SetActive(s.WindowOpen);
        if (s.WindowOpen)
        {
            SetZone(_goodRect, s.GrillTarget - CookingRules.GoodAverage, s.GrillTarget + CookingRules.GoodAverage);
            SetZone(_perfectRect, s.GrillTarget - CookingRules.PerfectError, s.GrillTarget + CookingRules.PerfectError);
            SetCursor(s.GrillCursor);
        }
        int err = Mathf.RoundToInt(s.LastError);
        if (s.Round != _shownRound || s.Misses != _shownMisses || err != _shownPercent)
        {
            _shownRound = s.Round;
            _shownMisses = s.Misses;
            _shownPercent = err;
            _sub.text = string.Format(Loc.T("Flip {0}/{1}"), Mathf.Min(s.Round + 1, CookingRules.GrillRounds), CookingRules.GrillRounds);
            _line.text = s.LastError < 0f ? (s.Misses > 0 ? Loc.T("Missed a flip!") : Loc.T("Wait for the sizzle")) : string.Format(Loc.T("Off by {0}%"), err);
        }
    }

    private void RefreshBoil(CookingSession s)
    {
        _timeFill.sizeDelta = new Vector2(BarWidth * Mathf.Clamp01(1f - s.Elapsed / CookingRules.BoilTime), _timeFill.sizeDelta.y);
        SetCursor(s.Heat);
        _cursorImage.color = s.Heat >= CookingRules.OverheatLevel ? _heatHot : _heatNormal;
        int pct = Mathf.RoundToInt(s.InZoneRatio * 100f);
        if (pct != _shownPercent || (s.OverheatTime > 0f))
        {
            _shownPercent = pct;
            _sub.text = string.Format(Loc.T("Steady {0}%"), pct);
            _line.text = s.OverheatTime > 0f ? string.Format(Loc.T("Too hot! {0}s"), (CookingRules.OverheatFail - s.OverheatTime).ToString("F1")) : Loc.T("Keep the heat in the blue");
        }
    }

    private void ShowResult(CookingStove stove, CookingSession s)
    {
        if (s.Cancelled)
        {
            _message.text = Loc.T("Stopped cooking");
            _sub.text = stove.Consumed ? Loc.T("The ingredients were lost") : "";
            _line.text = "";
            return;
        }
        _message.text = Loc.T(GradeKey(s.Grade, s.Overheated));
        UiTheme theme = UiTheme.Get();
        if (stove.BagFull)
        {
            _sub.text = Loc.T("Bag full - left it");
        }
        else if (!string.IsNullOrEmpty(stove.ResultId))
        {
            string rarity = s.Grade == CookGrade.Fail ? "C" : s.Recipe.Rarity.ToString();
            _sub.text = Loc.T("Got: ") + "<color=#" + ColorUtility.ToHtmlStringRGB(theme.RarityColor(rarity)) + ">"
                + ItemText.DisplayName(stove.ResultId) + "</color> x" + stove.ResultCount;
        }
        _line.text = "";
    }

    private static string GradeKey(CookGrade grade, bool overheated)
    {
        switch (grade)
        {
            case CookGrade.Perfect:
                return "Perfect dish!";
            case CookGrade.Good:
                return "Well cooked";
            case CookGrade.Normal:
                return "Edible";
            default:
                return overheated ? "Boiled over - burnt!" : "Burnt!";
        }
    }

    private void SetZone(RectTransform zone, float from, float to)
    {
        zone.anchorMin = new Vector2(Mathf.Clamp01(from / 100f), 0f);
        zone.anchorMax = new Vector2(Mathf.Clamp01(to / 100f), 1f);
        zone.offsetMin = Vector2.zero;
        zone.offsetMax = Vector2.zero;
    }

    private void SetCursor(float pos)
    {
        float x = Mathf.Clamp01(pos / 100f);
        _cursor.anchorMin = new Vector2(x, 0f);
        _cursor.anchorMax = new Vector2(x, 1f);
        _cursor.anchoredPosition = Vector2.zero;
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 55;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image panel = theme.NewPanel("CookingPanel", transform, false, 0.9f);
        _panel = panel.gameObject;
        RectTransform pr = panel.rectTransform;
        _panelRect = pr;
        pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 1f); // 다른 미니게임 HUD 와 같은 자리(동시에 열리지 않음)
        pr.anchoredPosition = new Vector2(0f, -100f);
        pr.sizeDelta = new Vector2(PanelWidth, PlayHeight);

        _message = NewText(theme, "Message", pr, new Vector2(0f, -14f), 24f);
        _sub = NewText(theme, "Sub", pr, new Vector2(0f, -50f), 18f);
        _line = NewText(theme, "Line", pr, new Vector2(0f, -80f), 16f);

        RectTransform track = NewImage("BarTrack", pr, _track).rectTransform;
        _bar = track.gameObject;
        track.anchorMin = track.anchorMax = new Vector2(0.5f, 1f);
        track.pivot = new Vector2(0.5f, 1f);
        track.anchoredPosition = new Vector2(0f, -112f);
        track.sizeDelta = new Vector2(BarWidth, 30f);
        _goodRect = NewImage("Good", track, _goodZone).rectTransform;
        _perfectRect = NewImage("Perfect", track, _perfectZone).rectTransform;
        _safeRect = NewImage("Safe", track, _safeZone).rectTransform;
        _dangerRect = NewImage("Danger", track, _dangerZone).rectTransform;
        _cursorImage = NewImage("Cursor", track, _cursorColor);
        _cursor = _cursorImage.rectTransform;
        _cursor.sizeDelta = new Vector2(6f, 12f);

        RectTransform timeTrack = NewImage("TimeTrack", pr, _track).rectTransform;
        timeTrack.anchorMin = timeTrack.anchorMax = new Vector2(0.5f, 1f);
        timeTrack.pivot = new Vector2(0.5f, 1f);
        timeTrack.anchoredPosition = new Vector2(0f, -156f);
        timeTrack.sizeDelta = new Vector2(BarWidth, 6f);
        _timeFill = NewImage("Fill", timeTrack, theme.unique).rectTransform;
        _timeFill.anchorMin = new Vector2(0f, 0f);
        _timeFill.anchorMax = new Vector2(0f, 1f);
        _timeFill.pivot = new Vector2(0f, 0.5f);
        _timeFill.anchoredPosition = Vector2.zero;
        _timeFill.sizeDelta = new Vector2(BarWidth, 0f);

        _help = NewText(theme, "Help", pr, Vector2.zero, 14f);
        RectTransform hr = _help.rectTransform;
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0f);
        hr.pivot = new Vector2(0.5f, 0.5f);
        hr.anchoredPosition = new Vector2(0f, 24f);
        hr.sizeDelta = new Vector2(PanelWidth - 24f, 20f);

        _panel.SetActive(false);
    }

    private static Image NewImage(string name, Transform parent, Color color)
    {
        var img = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        img.transform.SetParent(parent, false);
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI NewText(UiTheme theme, string name, RectTransform parent, Vector2 pos, float size)
    {
        var t = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        t.transform.SetParent(parent, false);
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(-24f, 34f);
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(0, 0, 0, 220);
        t.raycastTarget = false;
        return t;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
