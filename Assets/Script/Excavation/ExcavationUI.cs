using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 발굴 HUD(코드 생성, 낚시·채집 HUD 와 같은 자리·테마). ExcavationSite.Current 를 읽기만 한다.
/// 5×5 칸(안 판 돌 / 위험 균열 / 판 칸 숫자 / 유물 / 무너짐) + 커서 테두리, 곡괭이·유물 수, 붕괴 막대, 남은 시간 막대, 조작 안내.
/// 결과 때는 칸·막대를 숨기고 결과 문구와 얻은 것, 기록이 나왔으면 기록 본문을 보여 준다.
/// </summary>
public class ExcavationUI : MonoBehaviour
{
    private const float PanelWidth = 460f;
    private const float BarWidth = 360f;
    private const float Cell = 46f;
    private const float Gap = 4f;
    private const float GridTop = -78f;
    private const float PlayHeight = 445f;
    private const float ResultHeight = 250f;

    private static ExcavationUI _instance;

    private readonly Color _stone = new Color(0.36f, 0.33f, 0.31f);
    private readonly Color _hazardHidden = new Color(0.46f, 0.25f, 0.23f);
    private readonly Color _hazardMark = new Color(0.9f, 0.45f, 0.4f);
    private readonly Color _hole = new Color(0.1f, 0.09f, 0.09f);
    private readonly Color _relic = new Color(0.82f, 0.66f, 0.28f);
    private readonly Color _relicMark = new Color(0.3f, 0.18f, 0.06f);
    private readonly Color _hazardDug = new Color(0.6f, 0.14f, 0.12f);
    private readonly Color[] _hintColors =
    {
        new Color(0.5f, 0.5f, 0.52f), new Color(0.55f, 0.85f, 0.95f), new Color(0.95f, 0.82f, 0.4f), new Color(1f, 0.55f, 0.3f),
    };

    private GameObject _panel;
    private RectTransform _panelRect;
    private TextMeshProUGUI _message;
    private TextMeshProUGUI _sub;
    private TextMeshProUGUI _record;
    private GameObject _play;
    private RectTransform _cursor;
    private Image[] _cells;
    private TextMeshProUGUI[] _cellTexts;
    private RectTransform _collapseFill;
    private RectTransform _timeFill;
    private TextMeshProUGUI _collapseLabel;
    private TextMeshProUGUI _help;
    private ExcavationSession _shown;
    private bool _shownDone;
    private int _shownCursor = -1;
    private int _shownPicks = -1;

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
        var go = new GameObject("[ExcavationUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<ExcavationUI>();
        _instance.Build();
    }

    private void Update()
    {
        ExcavationSite site = ExcavationSite.Current;
        ExcavationSession s = site != null ? site.Session : null;
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
        if (s != _shown || s.Done != _shownDone)
        {
            _shown = s;
            _shownDone = s.Done;
            _shownCursor = -1;
            _shownPicks = -1;
            _play.SetActive(!s.Done);
            _record.gameObject.SetActive(s.Done);
            _panelRect.sizeDelta = new Vector2(PanelWidth, s.Done ? ResultHeight : PlayHeight); // 결과 때는 문구만이라 줄인다
            if (s.Done)
            {
                ShowResult(site, s);
                return;
            }
            _collapseLabel.text = Loc.T("Collapse"); // 언어가 바뀌었을 수 있다(판마다 한 번)
            _help.text = Loc.T("ARROWS move  ·  X dig  ·  ESC stop");
            _message.text = Loc.T(s.Spec.Rare ? "Rare ruin! Read the signs, find the relics" : "Read the signs, find the relics");
        }
        if (s.Done)
        {
            return;
        }
        _timeFill.sizeDelta = new Vector2(BarWidth * Mathf.Clamp01(1f - s.Elapsed / s.Spec.TimeLimit), _timeFill.sizeDelta.y);
        if (s.Cursor != _shownCursor || s.PicksLeft != _shownPicks)
        {
            _shownCursor = s.Cursor;
            _shownPicks = s.PicksLeft;
            RefreshBoard(s);
        }
    }

    private void RefreshBoard(ExcavationSession s)
    {
        _sub.text = string.Format(Loc.T("Picks {0}  ·  Relics {1}/{2}"), s.PicksLeft, s.RelicsFound, s.Spec.Relics);
        for (int i = 0; i < _cells.Length; i++)
        {
            PaintCell(s, i);
        }
        _cursor.anchoredPosition = CellPos(s.Cursor, s.Size);
        float collapse = Mathf.Clamp01((float)s.Collapse / ExcavationRules.CollapseLimit);
        _collapseFill.sizeDelta = new Vector2(BarWidth * collapse, _collapseFill.sizeDelta.y);
    }

    private void PaintCell(ExcavationSession s, int i)
    {
        DigContent c = s.ContentIfVisible(i);
        TextMeshProUGUI t = _cellTexts[i];
        if (!s.IsDug(i))
        {
            bool hazard = c == DigContent.Hazard; // 위험 칸은 파기 전에도 붉은 균열로 보인다
            _cells[i].color = hazard ? _hazardHidden : _stone;
            t.text = hazard ? "!" : "";
            t.color = _hazardMark;
            return;
        }
        switch (c)
        {
            case DigContent.Relic:
                _cells[i].color = _relic;
                t.text = "+";
                t.color = _relicMark;
                break;
            case DigContent.Hazard:
                _cells[i].color = _hazardDug;
                t.text = "X";
                t.color = Color.white;
                break;
            default:
                int h = s.HintAt(i);
                _cells[i].color = _hole;
                t.text = h.ToString();
                t.color = _hintColors[Mathf.Clamp(h, 0, _hintColors.Length - 1)];
                break;
        }
    }

    private void ShowResult(ExcavationSite site, ExcavationSession s)
    {
        _message.text = ResultHead(s.Result);
        _sub.text = FoundText(site, s);
        if (site.RecordFound >= 0)
        {
            FathersRecords.Record r = FathersRecords.All[site.RecordFound];
            _record.text = "<color=#" + ColorUtility.ToHtmlStringRGB(UiTheme.Get().unique) + ">" + Loc.T("New record: ")
                + Loc.T(r.Title) + "</color>\n\n" + Loc.T(r.Body);
        }
        else
        {
            _record.text = site.RecordReplaced ? Loc.T("All records found - took a gear core instead") : "";
        }
    }

    private static string ResultHead(DigResult r)
    {
        switch (r)
        {
            case DigResult.Success:
                return Loc.T("Excavation complete!");
            case DigResult.OutOfPicks:
                return Loc.T("Out of picks");
            case DigResult.Collapsed:
                return Loc.T("The ruin collapsed");
            case DigResult.TimedOut:
                return Loc.T("Time ran out");
            default:
                return Loc.T("Stopped excavating");
        }
    }

    /// <summary>이번 판에 얻은 아이템(기록 제외)을 한 줄로. 가방이 가득 찼으면 그 안내</summary>
    private static string FoundText(ExcavationSite site, ExcavationSession s)
    {
        if (site.BagFull)
        {
            return Loc.T("Bag full - left it");
        }
        var sb = new StringBuilder();
        UiTheme theme = UiTheme.Get();
        foreach (DigFind f in s.Finds)
        {
            if (f.Def.Id == ExcavationRules.RecordId)
            {
                continue;
            }
            if (sb.Length > 0)
            {
                sb.Append("  ");
            }
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(theme.RarityColor(f.Def.Rarity.ToString()))).Append('>')
                .Append(ItemText.DisplayName(f.Def.Id)).Append("</color> x").Append(f.Count);
        }
        if (sb.Length > 0)
        {
            return Loc.T("Got: ") + sb;
        }
        return site.RecordFound >= 0 ? "" : Loc.T("Nothing found");
    }

    private static Vector2 CellPos(int index, int size)
    {
        float span = size * Cell + (size - 1) * Gap;
        float x = -span / 2f + Cell / 2f + (index % size) * (Cell + Gap);
        float y = -(Cell / 2f) - (index / size) * (Cell + Gap);
        return new Vector2(x, y);
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

        Image panel = theme.NewPanel("ExcavationPanel", transform, false, 0.9f);
        _panel = panel.gameObject;
        RectTransform pr = panel.rectTransform;
        _panelRect = pr;
        pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 1f); // 낚시·채집 HUD 와 같은 자리(동시에 열리지 않음)
        pr.anchoredPosition = new Vector2(0f, -100f);
        pr.sizeDelta = new Vector2(PanelWidth, PlayHeight);

        _message = NewText(theme, "Message", pr, new Vector2(0f, -14f), 24f);
        _sub = NewText(theme, "Sub", pr, new Vector2(0f, -46f), 18f);
        _record = NewText(theme, "Record", pr, new Vector2(0f, -92f), 18f);
        _record.textWrappingMode = TextWrappingModes.Normal;
        _record.alignment = TextAlignmentOptions.Top;
        _record.rectTransform.sizeDelta = new Vector2(-48f, ResultHeight - 100f);

        RectTransform play = NewRect("Play", pr); // 결과 때는 칸·막대를 통째로 숨긴다
        _play = play.gameObject;
        BuildGrid(theme, play);
        _collapseFill = Fill(Bar("CollapseTrack", play, 64f, 10f), new Color(0.75f, 0.25f, 0.2f));
        _collapseLabel = BottomText(theme, play, "CollapseLabel", 86f, BarWidth, TextAlignmentOptions.Left);
        _timeFill = Fill(Bar("TimeTrack", play, 44f, 6f), theme.unique);
        _timeFill.sizeDelta = new Vector2(BarWidth, 0f);
        _help = BottomText(theme, play, "Help", 26f, PanelWidth - 24f, TextAlignmentOptions.Center);

        _record.gameObject.SetActive(false);
        _panel.SetActive(false);
    }

    private void BuildGrid(UiTheme theme, RectTransform play)
    {
        RectTransform grid = NewRect("Grid", play);
        grid.anchorMin = grid.anchorMax = new Vector2(0.5f, 1f);
        grid.anchoredPosition = new Vector2(0f, GridTop);
        grid.sizeDelta = Vector2.zero;
        _cursor = NewImage("Cursor", grid, theme.unique).rectTransform;
        _cursor.sizeDelta = new Vector2(Cell + 6f, Cell + 6f);
        int n = ExcavationRules.Basic.Size;
        _cells = new Image[n * n];
        _cellTexts = new TextMeshProUGUI[n * n];
        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i] = NewImage("Cell" + i, grid, _stone);
            RectTransform rt = _cells[i].rectTransform;
            rt.sizeDelta = new Vector2(Cell, Cell);
            rt.anchoredPosition = CellPos(i, n);
            TextMeshProUGUI t = NewText(theme, "Hint", rt, Vector2.zero, 26f);
            RectTransform tr = t.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            t.alignment = TextAlignmentOptions.Center;
            _cellTexts[i] = t;
        }
    }

    private static TextMeshProUGUI BottomText(UiTheme theme, RectTransform parent, string name, float y, float width, TextAlignmentOptions align)
    {
        TextMeshProUGUI t = NewText(theme, name, parent, Vector2.zero, 14f);
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(width, 20f);
        t.alignment = align;
        return t;
    }

    private static RectTransform Fill(RectTransform track, Color color)
    {
        RectTransform rt = NewImage("Fill", track, color).rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        return rt;
    }

    private static RectTransform Bar(string name, RectTransform parent, float y, float height)
    {
        RectTransform rt = NewImage(name, parent, new Color(0.08f, 0.08f, 0.1f, 0.9f)).rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(BarWidth, height);
        return rt;
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
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
