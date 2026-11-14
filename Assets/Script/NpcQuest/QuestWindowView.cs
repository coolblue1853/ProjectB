using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 창(Q) 화면 구성 (REWORK-5 P2, U6). 코드 생성 UI: 탭 4개(전체·받을 수 있는·진행 중·완료)·목록 9줄(줄마다 아이콘)·상세·보상 아이콘·버튼 3개.
/// 모든 항목은 Button 이라 마우스로도 조작한다(키보드·패드는 NpcQuestWindow 가 직접 처리).
/// </summary>
public sealed class QuestWindowView
{
    public const int Rows = 9;
    public const float RowH = 56f;
    public static readonly int TabCount = QuestListRules.Order.Length; // 탭 수 = 화면 순서 배열 길이(어긋나지 않게)
    private const float ListW = 440f;
    private const float DetailX = 496f;
    private const float DetailW = 660f;

    public GameObject Panel;
    public TextMeshProUGUI Title, Hint, Detail, RewardHeader, RewardText, Guide, ListInfo;
    /// <summary>탭 배열은 QuestListRules.Tab 정수로 찾는다 (화면 위치는 QuestListRules.Order 순서)</summary>
    public readonly Button[] Tabs = new Button[TabCount];
    public readonly TextMeshProUGUI[] TabText = new TextMeshProUGUI[TabCount];
    public readonly Button[] RowButtons = new Button[Rows];
    public readonly Image[] RowBg = new Image[Rows];
    public readonly TextMeshProUGUI[] RowText = new TextMeshProUGUI[Rows];
    public readonly Image[] RowIcon = new Image[Rows];
    public Button Primary, Track, Abandon, Close;
    public TextMeshProUGUI PrimaryText, TrackText, AbandonText;
    public Image RewardIcon;

    public void Build(GameObject host, Action<int> onTab, Action<int> onRow, Action onPrimary, Action onTrack, Action onAbandon, Action onClose)
    {
        UiTheme theme = UiTheme.Get();
        var canvas = host.AddComponent<Canvas>(); // Transform 이 RectTransform 으로 바뀌므로 host.transform 은 그 뒤에 다시 읽는다
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 70;
        var scaler = host.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        host.AddComponent<GraphicRaycaster>();

        Panel = theme.NewPanel("Panel", host.transform).gameObject;
        Panel.GetComponent<Image>().raycastTarget = true; // 창 뒤 클릭 차단
        var pr = (RectTransform)Panel.transform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1180f, 700f);

        Title = NewText("Title", pr, theme, 34f);
        Place(Title.rectTransform, new Vector2(30f, -18f), new Vector2(210f, 54f));
        Title.text = "퀘스트";
        Title.color = theme.unique;
        Title.enableAutoSizing = true; // 입구 이름(떠돌이 탐험가 이브 등)이 길어도 칸 안에
        Title.fontSizeMin = 20f;
        Title.fontSizeMax = 34f;
        float[] tabW = { 140f, 240f, 190f, 170f }; // Order 순서: 전체 · 받을 수 있는 · 진행 중 · 완료
        float x = 250f;
        for (int i = 0; i < TabCount; i++)
        {
            int idx = (int)QuestListRules.Order[i];
            Tabs[idx] = NewButton("Tab" + idx, pr, theme, new Vector2(x, -16f), new Vector2(tabW[i], 48f), 26f, out TabText[idx], () => onTab(idx));
            TabText[idx].alignment = TextAlignmentOptions.Center;
            x += tabW[i] + 6f;
        }
        Close = NewButton("Close", pr, theme, new Vector2(1040f, -16f), new Vector2(110f, 48f), 24f, out TextMeshProUGUI closeText, onClose);
        closeText.text = "닫기";
        closeText.alignment = TextAlignmentOptions.Center;

        Image left = theme.NewPanel("ListBg", pr, true);
        Place(left.rectTransform, new Vector2(30f, -76f), new Vector2(ListW, Rows * RowH + 14f));
        for (int i = 0; i < Rows; i++)
        {
            int idx = i;
            Button b = NewButton("Row" + i, left.transform, theme, new Vector2(6f, -7f - i * RowH), new Vector2(ListW - 12f, RowH - 2f), 24f, out RowText[i], () => onRow(idx));
            b.transition = Selectable.Transition.None;
            RowButtons[i] = b;
            RowBg[i] = b.GetComponent<Image>();
            RowText[i].enableWordWrapping = false;
            RowText[i].overflowMode = TextOverflowModes.Truncate;
            RowText[i].alignment = TextAlignmentOptions.Left;
            RowText[i].rectTransform.offsetMin = new Vector2(52f, 0f);
            var rowIconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            rowIconGo.transform.SetParent(b.transform, false);
            RowIcon[i] = rowIconGo.GetComponent<Image>();
            RowIcon[i].raycastTarget = false;
            RowIcon[i].preserveAspect = true;
            Place(RowIcon[i].rectTransform, new Vector2(8f, -8f), new Vector2(38f, 38f));
            RowIcon[i].gameObject.SetActive(false); // 스프라이트가 없으면 흰 네모가 되므로 그릴 때만 켠다
        }
        ListInfo = NewText("ListInfo", pr, theme, 20f);
        Place(ListInfo.rectTransform, new Vector2(30f, -(76f + Rows * RowH + 18f)), new Vector2(ListW, 26f));
        ListInfo.alignment = TextAlignmentOptions.Right;
        ListInfo.color = new Color(1f, 1f, 1f, 0.5f);

        Detail = NewText("Detail", pr, theme, 24f);
        Place(Detail.rectTransform, new Vector2(DetailX, -80f), new Vector2(DetailW, 250f));
        Detail.enableWordWrapping = true;
        Detail.overflowMode = TextOverflowModes.Truncate;

        RewardHeader = NewText("RewardHeader", pr, theme, 24f);
        Place(RewardHeader.rectTransform, new Vector2(DetailX, -340f), new Vector2(DetailW, 32f));
        RewardHeader.text = "<color=#88AACC>-- 보상 --</color>";
        var iconGo = new GameObject("RewardIcon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(pr, false);
        RewardIcon = iconGo.GetComponent<Image>();
        RewardIcon.raycastTarget = false;
        RewardIcon.preserveAspect = true;
        Place(RewardIcon.rectTransform, new Vector2(DetailX, -376f), new Vector2(40f, 40f));
        RewardText = NewText("RewardText", pr, theme, 24f);
        Place(RewardText.rectTransform, new Vector2(DetailX + 50f, -382f), new Vector2(DetailW - 50f, 36f));
        RewardText.enableWordWrapping = false;

        Guide = NewText("Guide", pr, theme, 24f);
        Place(Guide.rectTransform, new Vector2(DetailX, -432f), new Vector2(DetailW, 150f));
        Guide.enableWordWrapping = true;
        Guide.overflowMode = TextOverflowModes.Truncate;

        Primary = NewButton("Primary", pr, theme, new Vector2(DetailX, -588f), new Vector2(180f, 46f), 24f, out PrimaryText, onPrimary);
        Track = NewButton("Track", pr, theme, new Vector2(DetailX + 190f, -588f), new Vector2(180f, 46f), 24f, out TrackText, onTrack);
        Abandon = NewButton("Abandon", pr, theme, new Vector2(DetailX + 380f, -588f), new Vector2(180f, 46f), 24f, out AbandonText, onAbandon);
        foreach (TextMeshProUGUI t in new[] { PrimaryText, TrackText, AbandonText })
        {
            t.alignment = TextAlignmentOptions.Center;
        }

        Hint = NewText("Hint", pr, theme, 19f);
        Place(Hint.rectTransform, new Vector2(30f, -642f), new Vector2(1120f, 50f));
        Hint.enableWordWrapping = true;
        Hint.color = new Color(1f, 1f, 1f, 0.55f);

        Panel.SetActive(false);
    }

    public static Button NewButton(string name, Transform parent, UiTheme theme, Vector2 pos, Vector2 size, float fontSize, out TextMeshProUGUI label, Action onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Place((RectTransform)go.transform, pos, size);
        var img = go.GetComponent<Image>();
        img.color = Color.white;
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.10f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.26f);
        colors.pressedColor = new Color(1f, 0.85f, 0.4f, 0.4f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.03f);
        btn.colors = colors;
        btn.navigation = new Navigation { mode = Navigation.Mode.None }; // 화살표·Enter 가 UI 포커스로 새지 않게
        btn.onClick.AddListener(() => onClick());
        label = NewText("Label", go.transform, theme, fontSize);
        var lr = label.rectTransform;
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = new Vector2(6f, 0f);
        lr.offsetMax = new Vector2(-6f, 0f);
        label.alignment = TextAlignmentOptions.Left;
        label.verticalAlignment = VerticalAlignmentOptions.Middle;
        return btn;
    }

    public static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.raycastTarget = false;
        t.richText = true;
        t.enableWordWrapping = false;
        t.outlineWidth = 0.2f;
        t.outlineColor = new Color32(0, 0, 0, 200);
        return t;
    }

    public static void Place(RectTransform rt, Vector2 topLeft, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = topLeft;
        rt.sizeDelta = size;
    }
}
