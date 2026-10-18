using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 획득 로그 뷰 (좌측 하단): PickupLogModel 상태를 줄 6개 풀로 그린다.
/// R/U 등급은 Text Animator로 이름이 출렁이고(&lt;wave&gt;) 테두리 글로우가 붙는다. 인벤토리가 가득 차면 빨간 경고(2초 스로틀).
/// 사용: PickupLog.Report(itemName, count, rarity) / PickupLog.ReportFull()
/// </summary>
public class PickupLog : MonoBehaviour
{
    const int PoolSize = PickupLogModel.MaxLines + 1;
    const float RowHeight = 34f, BottomOffset = 120f, LeftOffset = 24f, FullThrottle = 2f;

    class Row
    {
        public RectTransform rect;
        public CanvasGroup group;
        public Image icon;
        public Image glow;
        public TextMeshProUGUI text;
        public TextAnimator animator;
        public int lineId = -1, version = -1;
    }

    static PickupLog instance;
    readonly PickupLogModel model = new PickupLogModel();
    Row[] rows;
    TextMeshProUGUI fullText;
    float lastFull = -99f;
    int maxShownId; // 처음 보이는 줄만 등장 연출 (밀려 올라가는 줄은 제외)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        instance = null;
    }

    public static void Report(string itemName, int count, string rarity)
    {
        PickupLog log = Ensure();
        PickupLogModel.Line line = log.model.Add(itemName, count, rarity ?? "C", Time.time);
        log.Refresh();
        GameLog.Info(GameLog.Cat.Item, "획득 로그: " + itemName + " x" + line.count);
    }

    public static void ReportFull()
    {
        PickupLog log = Ensure();
        if (Time.time - log.lastFull < FullThrottle)
        {
            return;
        }
        log.lastFull = Time.time;
        log.fullText.DOKill();
        log.fullText.alpha = 1f;
        log.fullText.gameObject.SetActive(true);
        log.fullText.DOFade(0f, 0.6f).SetDelay(1.4f).SetLink(log.fullText.gameObject)
            .OnComplete(() => log.fullText.gameObject.SetActive(false));
        Sfx.Play(SfxId.CraftFail, 0.5f);
        GameLog.Info(GameLog.Cat.Item, "인벤토리 가득 참 — 획득 거절");
    }

    static PickupLog Ensure()
    {
        if (instance == null)
        {
            var go = new GameObject("[PickupLog]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<PickupLog>();
            instance.Build();
        }
        return instance;
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        rows = new Row[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            rows[i] = CreateRow(i, theme);
        }

        fullText = CreateText("Full", transform, theme, 30f);
        var fr = fullText.rectTransform;
        fr.anchorMin = fr.anchorMax = new Vector2(0.5f, 0f);
        fr.pivot = new Vector2(0.5f, 0f);
        fr.anchoredPosition = new Vector2(0f, 220f);
        fr.sizeDelta = new Vector2(600f, 50f);
        fullText.alignment = TextAlignmentOptions.Center;
        fullText.color = theme.warning;
        fullText.text = "Inventory Full";
        fullText.gameObject.SetActive(false);
    }

    Row CreateRow(int index, UiTheme theme)
    {
        var go = new GameObject("Row" + index, typeof(RectTransform), typeof(CanvasGroup));
        var rect = (RectTransform)go.transform;
        rect.SetParent(transform, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.sizeDelta = new Vector2(460f, RowHeight);

        Image glow = theme.NewPanel("Glow", rect, true);
        var glowRect = glow.rectTransform;
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(-4f, -2f);
        glowRect.offsetMax = new Vector2(4f, 2f);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        var iconRect = (RectTransform)iconGo.transform;
        iconRect.SetParent(rect, false);
        iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.sizeDelta = new Vector2(30f, 30f);
        iconRect.anchoredPosition = new Vector2(2f, 0f);
        var icon = iconGo.GetComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        TextMeshProUGUI text = CreateText("Text", rect, theme, 26f);
        var tr = text.rectTransform;
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(40f, 0f);
        tr.offsetMax = Vector2.zero;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        // 긴 문구(포털 봉인 토스트·긴 아이템 이름)가 칸 밖으로 넘치지 않게 줄 폭에 맞춰 줄이고, 그래도 넘치면 자름(픽셀 폰트에 말줄임 글자가 없어 Ellipsis 는 경고를 낸다)
        text.enableAutoSizing = true;
        text.fontSizeMin = 16f;
        text.fontSizeMax = 26f;
        text.overflowMode = TextOverflowModes.Truncate;
        var animator = text.gameObject.AddComponent<TextAnimator>();

        go.SetActive(false);
        return new Row { rect = rect, group = go.GetComponent<CanvasGroup>(), icon = icon, glow = glow, text = text, animator = animator };
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, UiTheme theme, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            text.font = theme.pixelFont;
        }
        text.fontSize = size;
        text.raycastTarget = false;
        text.enableWordWrapping = false;
        text.outlineWidth = 0.2f;
        text.outlineColor = new Color32(0, 0, 0, 200);
        return text;
    }

    void Update()
    {
        if (model.Tick(Time.time) > 0)
        {
            Refresh();
        }
        float now = Time.time;
        var lines = model.Lines;
        for (int i = 0; i < lines.Count && i < rows.Length; i++)
        {
            rows[i].group.alpha = lines[i].Alpha(now);
        }
    }

    /// <summary>표시 이름(ItemSheet itemNameT). 아이템이 아닌 문구(승급 알림 등)는 그대로</summary>
    static string DisplayName(string key)
    {
        DatabaseManager db = DatabaseManager.instance;
        int idx = db != null ? db.FindItemDataIndex(key) : -1;
        if (idx < 0)
        {
            return key;
        }
        string t = db.LoadItemData(idx).itemNameT;
        return string.IsNullOrEmpty(t) ? key : t;
    }

    /// <summary>모델 줄 순서대로 행을 배치: 최신 줄이 맨 아래, 위로 갈수록 오래된 줄</summary>
    void Refresh()
    {
        UiTheme theme = UiTheme.Get();
        var lines = model.Lines;
        for (int i = 0; i < rows.Length; i++)
        {
            Row row = rows[i];
            if (i >= lines.Count)
            {
                row.rect.gameObject.SetActive(false);
                row.lineId = -1;
                continue;
            }
            PickupLogModel.Line line = lines[i];
            int fromBottom = lines.Count - 1 - i;
            row.rect.anchoredPosition = new Vector2(LeftOffset, BottomOffset + fromBottom * RowHeight);
            if (row.lineId == line.id && row.version == line.version)
            {
                continue;
            }
            bool isNew = line.id > maxShownId;
            bool sameLine = row.lineId == line.id; // 같은 줄의 수량 증가
            maxShownId = Mathf.Max(maxShownId, line.id);
            row.lineId = line.id;
            row.version = line.version;
            row.rect.gameObject.SetActive(true);
            row.icon.sprite = Resources.Load<Sprite>("Item/" + line.itemName);
            row.icon.enabled = row.icon.sprite != null;
            Color c = theme.RarityColor(line.rarity);
            row.glow.enabled = line.emphasis;
            row.glow.color = new Color(c.r * 0.6f + 0.4f, c.g * 0.6f + 0.4f, c.b * 0.6f + 0.4f, 0.95f);
            string hex = ColorUtility.ToHtmlStringRGB(c);
            string shown = DisplayName(line.itemName);
            string name = line.emphasis ? "<wave a=0.4 f=0.8>" + shown + "</wave>" : shown;
            string countText = line.count > 1 ? " <color=#DDDDDD>x" + line.count + "</color>" : "";
            row.animator.SetText("<color=#" + hex + ">" + name + "</color>" + countText, false);
            row.rect.DOKill(true);
            if (isNew)
            {
                row.rect.localScale = Vector3.one;
                row.rect.DOPunchScale(new Vector3(0.08f, 0.08f, 0f), 0.2f, 4, 0.5f).SetLink(row.rect.gameObject);
            }
            else if (sameLine)
            {
                row.rect.DOPunchScale(new Vector3(0.12f, 0.12f, 0f), 0.15f, 3, 0.5f).SetLink(row.rect.gameObject);
            }
        }
    }
}
