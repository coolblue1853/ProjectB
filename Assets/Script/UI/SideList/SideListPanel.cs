using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤·상점 창 옆에 붙는 읽기 전용 목록 패널 공용 틀(BasicFeatures.md 13절). 코드 생성 HUD(정렬 58).
/// 창(인벤 장비칸·상점 진열칸) 안쪽 왼쪽 아래 빈 자리에 붙인다 — 인벤·상점 창이 화면을 거의 다 덮어 옆에는 자리가 없다(화면 좌표 기준이라 창이 다른 캔버스여도 맞는다).
/// 글자는 Changed() 가 참일 때만 다시 만든다(매 프레임 문자열을 만들지 않게).
/// </summary>
public abstract class SideListPanel : MonoBehaviour
{
    private const float Margin = 40f;

    private RectTransform _panel;
    private TextMeshProUGUI _title, _body;
    private Canvas _canvas;
    private Canvas _anchorCanvas;
    private RectTransform _anchorRect;
    private readonly Vector3[] _corners = new Vector3[4];

    protected abstract float Width { get; }

    /// <summary>패널을 띄울 때인가</summary>
    protected abstract bool Visible();

    /// <summary>붙일 창(인벤 장비칸·상점 진열칸). 왼쪽 아래 안쪽에 놓인다</summary>
    protected abstract RectTransform Anchor();

    /// <summary>마지막으로 그린 뒤 내용이 바뀌었나(모델 버전·선택·골드 등을 각각 비교 — 해시로 합치면 겹칠 수 있다). 참이면 기준값을 지금 값으로 바꾼다</summary>
    protected abstract bool Changed();

    protected abstract void Render(TextMeshProUGUI title, TextMeshProUGUI body);

    public bool IsShowing => _panel != null && _panel.gameObject.activeSelf;

    /// <summary>프로브·테스트용: 지금 본문 글자</summary>
    public string BodyText => _body != null ? _body.text : "";

    protected virtual void Awake()
    {
        Build();
    }

    private void LateUpdate()
    {
        RectTransform anchor = Visible() ? Anchor() : null;
        if (anchor == null)
        {
            if (_panel.gameObject.activeSelf)
            {
                _panel.gameObject.SetActive(false);
            }
            return;
        }
        bool changed = Changed(); // 기준값 갱신을 위해 항상 부른다
        if (!_panel.gameObject.activeSelf || changed)
        {
            Render(_title, _body);
            _body.ForceMeshUpdate();
            _panel.sizeDelta = new Vector2(Width, 74f + _body.preferredHeight);
            _panel.gameObject.SetActive(true);
        }
        if (anchor != _anchorRect)
        {
            _anchorRect = anchor;
            _anchorCanvas = anchor.GetComponentInParent<Canvas>();
        }
        PlaceInside(anchor);
    }

    /// <summary>창(anchor) 안쪽 왼쪽 아래 구석에 붙인다(인벤 장비칸·상점 진열칸 아래 빈 자리). 화면 좌표로 맞춘다</summary>
    private void PlaceInside(RectTransform anchor)
    {
        anchor.GetWorldCorners(_corners);
        Camera cam = _anchorCanvas != null && _anchorCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? _anchorCanvas.worldCamera : null;
        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(cam, _corners[0]);
        float scale = _canvas.scaleFactor > 0f ? _canvas.scaleFactor : 1f;
        _panel.anchoredPosition = new Vector2(bottomLeft.x / scale + Margin, bottomLeft.y / scale + Margin + _panel.sizeDelta.y);
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 58;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _panel = theme.NewPanel("Panel", transform, false, 0.95f).rectTransform;
        _panel.anchorMin = _panel.anchorMax = Vector2.zero;
        _panel.pivot = new Vector2(0f, 1f);
        _panel.sizeDelta = new Vector2(Width, 200f);

        _title = NewText("Title", theme, 26f);
        _title.color = theme.unique;
        Place(_title.rectTransform, new Vector2(20f, -16f), new Vector2(Width - 40f, 36f));
        _body = NewText("Body", theme, 23f);
        Place(_body.rectTransform, new Vector2(20f, -56f), new Vector2(Width - 40f, 600f));
        _panel.gameObject.SetActive(false);
    }

    private TextMeshProUGUI NewText(string name, UiTheme theme, float size)
    {
        var t = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        t.transform.SetParent(_panel, false);
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.outlineWidth = 0.2f;
        t.outlineColor = new Color32(0, 0, 0, 200);
        t.raycastTarget = false;
        return t;
    }

    private static void Place(RectTransform rt, Vector2 topLeft, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = topLeft;
        rt.sizeDelta = size;
    }

    /// <summary>긴 이름은 잘라 가격·키 힌트와 겹치지 않게</summary>
    protected static string Shorten(string name, int max)
    {
        return name == null || name.Length <= max ? name : name.Substring(0, max - 1) + "…";
    }

    /// <summary>시트 이름 → 아이템 데이터(없으면 null). 내용이 바뀐 때(Render)에만 부른다</summary>
    protected static ItemData LookUp(string id)
    {
        DatabaseManager db = DatabaseManager.instance;
        int index = db != null ? db.FindItemDataIndex(id) : -1;
        return index >= 0 ? db.LoadItemData(index) : null;
    }
}
