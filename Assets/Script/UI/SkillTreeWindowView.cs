using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>스킬창 뷰 (코드 생성 UI). 모든 항목이 Button 이라 마우스로도 조작하고, 키보드·패드는 SkillTreeWindow 가 직접 읽는다.</summary>
public sealed class SkillTreeWindowView
{
    public const int Rows = 8;
    const float RowH = 58f;

    public GameObject Panel;
    public readonly Button[] Tabs = new Button[4];
    public readonly TextMeshProUGUI[] TabText = new TextMeshProUGUI[4];
    public readonly Button[] RowButtons = new Button[Rows];
    public readonly TextMeshProUGUI[] RowName = new TextMeshProUGUI[Rows];
    public readonly TextMeshProUGUI[] RowEffect = new TextMeshProUGUI[Rows];
    public readonly Button[] RowPlus = new Button[Rows];
    public readonly Image[] RowBg = new Image[Rows];
    public TextMeshProUGUI Header, Points, Hint;
    public Button Respec, Close;
    public TextMeshProUGUI RespecText;

    public void Build(GameObject host, Action<int> onTab, Action<int> onRow, Action<int> onPlus, Action onRespec, Action onClose)
    {
        UiTheme theme = UiTheme.Get();
        var canvas = host.AddComponent<Canvas>();
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

        TextMeshProUGUI title = QuestWindowView.NewText("Title", pr, theme, 38f);
        QuestWindowView.Place(title.rectTransform, new Vector2(30f, -18f), new Vector2(160f, 54f));
        title.text = "스킬";
        title.color = theme.unique;
        float[] tabX = { 200f, 450f, 640f, 830f };
        float[] tabW = { 240f, 180f, 180f, 180f };
        for (int i = 0; i < Tabs.Length; i++)
        {
            int idx = i;
            Tabs[i] = QuestWindowView.NewButton("Tab" + i, pr, theme, new Vector2(tabX[i], -16f), new Vector2(tabW[i], 48f), 26f, out TabText[i], () => onTab(idx));
            TabText[i].alignment = TextAlignmentOptions.Center;
        }
        Close = QuestWindowView.NewButton("Close", pr, theme, new Vector2(1040f, -16f), new Vector2(110f, 48f), 24f, out TextMeshProUGUI closeText, onClose);
        closeText.text = "닫기";
        closeText.alignment = TextAlignmentOptions.Center;

        Header = QuestWindowView.NewText("Header", pr, theme, 26f);
        QuestWindowView.Place(Header.rectTransform, new Vector2(30f, -84f), new Vector2(760f, 40f));
        Points = QuestWindowView.NewText("Points", pr, theme, 26f);
        QuestWindowView.Place(Points.rectTransform, new Vector2(790f, -84f), new Vector2(360f, 40f));
        Points.alignment = TextAlignmentOptions.TopRight;

        for (int i = 0; i < Rows; i++)
        {
            int idx = i;
            Button b = QuestWindowView.NewButton("Row" + i, pr, theme, new Vector2(30f, -134f - i * RowH), new Vector2(1120f, RowH - 4f), 26f, out RowName[i], () => onRow(idx));
            RowButtons[i] = b;
            RowBg[i] = b.GetComponent<Image>();
            RowName[i].rectTransform.offsetMin = new Vector2(12f, 0f);
            RowName[i].rectTransform.offsetMax = new Vector2(-790f, 0f);
            RowEffect[i] = QuestWindowView.NewText("Effect", b.transform, theme, 24f);
            RectTransform er = RowEffect[i].rectTransform;
            er.anchorMin = Vector2.zero;
            er.anchorMax = Vector2.one;
            er.offsetMin = new Vector2(340f, 0f);
            er.offsetMax = new Vector2(-90f, 0f);
            RowEffect[i].verticalAlignment = VerticalAlignmentOptions.Middle;
            RowPlus[i] = QuestWindowView.NewButton("Plus", b.transform, theme, new Vector2(1040f, -6f), new Vector2(70f, RowH - 16f), 30f, out TextMeshProUGUI plus, () => onPlus(idx));
            plus.text = "+";
            plus.alignment = TextAlignmentOptions.Center;
            b.gameObject.SetActive(false);
        }

        Respec = QuestWindowView.NewButton("Respec", pr, theme, new Vector2(30f, -604f), new Vector2(260f, 46f), 24f, out RespecText, onRespec);
        RespecText.alignment = TextAlignmentOptions.Center;
        Hint = QuestWindowView.NewText("Hint", pr, theme, 22f);
        QuestWindowView.Place(Hint.rectTransform, new Vector2(30f, -658f), new Vector2(1120f, 30f));
        Hint.color = new Color(1f, 1f, 1f, 0.6f);
        Panel.SetActive(false);
    }
}
