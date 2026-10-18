using System.Collections.Generic;
using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 대장장이 강화 창 (코드 생성 UI). 대장장이 앞에서 ↓로 연다.
/// 왼쪽: 인벤토리의 장비 목록(↑↓ 선택) / 오른쪽: 성공률·강화석·골드·실패 규칙 / 선택 키 = 강화, 뒤로 = 닫기.
/// </summary>
public class EnhanceWindow : MonoBehaviour
{
    const int VisibleRows = 9;
    const float RowH = 44f;

    static EnhanceWindow instance;

    KeyAction action;
    InputAction upAction, downAction, selectAction, backAction;

    GameObject panel;
    readonly List<(Image bg, Image icon, TextMeshProUGUI text)> rows = new List<(Image, Image, TextMeshProUGUI)>();
    TextMeshProUGUI detail, resultText, emptyText;
    TextAnimator resultAnimator;
    readonly List<ItemCheck> items = new List<ItemCheck>();
    int cursor, scroll;
    bool open;
    float openedAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        instance = null;
    }

    public static bool IsOpen => instance != null && instance.open;

    public static void Open()
    {
        if (instance == null)
        {
            var go = new GameObject("[EnhanceWindow]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<EnhanceWindow>();
            instance.Build();
        }
        instance.Show();
    }

    void Awake()
    {
        action = new KeyAction();
        upAction = action.UI.UPInventory;
        downAction = action.UI.DownInventory;
        selectAction = action.UI.Select;
        backAction = action.UI.Back;
    }

    void OnDestroy()
    {
        action?.Dispose();
    }

    void Show()
    {
        open = true;
        openedAt = Time.unscaledTime;
        UiLock.Set("Enhance", true);
        action.UI.Enable();
        panel.SetActive(true);
        resultText.text = "";
        cursor = 0;
        scroll = 0;
        Collect();
        Redraw();
        Sfx.Play(SfxId.UiOpen);
        GameLog.Info(GameLog.Cat.Item, "강화 창 열기: 장비 " + items.Count + "개");
    }

    void Close()
    {
        open = false;
        UiLock.Set("Enhance", false);
        action.UI.Disable();
        panel.SetActive(false);
    }

    void Collect()
    {
        items.Clear();
        foreach (ItemCheck c in InventoryManager.instance.EnumerateInventoryItems())
        {
            if (c.itemData != null && c.itemData.type == "Equip")
            {
                items.Add(c);
            }
        }
        cursor = Mathf.Clamp(cursor, 0, Mathf.Max(0, items.Count - 1));
    }

    void Update()
    {
        if (!open)
        {
            return;
        }
        // 여는 키 입력이 같은 프레임에 선택으로 읽히지 않도록 잠깐 무시
        if (Time.unscaledTime - openedAt < 0.15f)
        {
            return;
        }
        if (backAction.triggered)
        {
            Close();
            return;
        }
        if (items.Count == 0)
        {
            return;
        }
        if (upAction.triggered)
        {
            Move(-1);
        }
        else if (downAction.triggered)
        {
            Move(1);
        }
        else if (selectAction.triggered)
        {
            DoEnhance();
        }
    }

    void Move(int delta)
    {
        cursor = (cursor + delta + items.Count) % items.Count;
        if (cursor < scroll)
        {
            scroll = cursor;
        }
        else if (cursor >= scroll + VisibleRows)
        {
            scroll = cursor - VisibleRows + 1;
        }
        Sfx.Play(SfxId.UiMove, 0.6f);
        Redraw();
    }

    void DoEnhance()
    {
        ItemCheck item = items[cursor];
        int before = item.itemData.upgrade;
        if (!EnhanceService.TryEnhance(item, out EnhanceRules.Result result, out int after))
        {
            ShowResult("<shake>" + Reason(EnhanceService.CanTry(item)) + "</shake>", UiTheme.Get().warning);
            Redraw();
            return;
        }
        UiTheme theme = UiTheme.Get();
        switch (result)
        {
            case EnhanceRules.Result.Success:
                ShowResult("<bounce>SUCCESS  +" + after + "</bounce>", after >= 5 ? theme.unique : theme.rare);
                HitFeedback.Shake(after >= 5 ? HitFeedback.ShakeTier.Medium : HitFeedback.ShakeTier.Light);
                break;
            case EnhanceRules.Result.FailDown:
                ShowResult("<shake>FAILED  +" + before + " → +" + after + "</shake>", theme.warning);
                break;
            default:
                ShowResult("<shake>FAILED</shake>", theme.warning);
                break;
        }
        Redraw();
    }

    static string Reason(EnhanceService.Check c)
    {
        switch (c)
        {
            case EnhanceService.Check.MaxLevel: return "MAX LEVEL";
            case EnhanceService.Check.NoStones: return "NOT ENOUGH CRYSTALS";
            case EnhanceService.Check.NoGold: return "NOT ENOUGH GOLD";
            default: return "CANNOT ENHANCE";
        }
    }

    void ShowResult(string text, Color color)
    {
        resultText.color = color;
        resultAnimator.SetText(text, false);
        resultText.transform.DOKill(true);
        resultText.transform.localScale = Vector3.one;
        resultText.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 5, 0.6f).SetLink(resultText.gameObject).SetUpdate(true);
    }

    void Redraw()
    {
        UiTheme theme = UiTheme.Get();
        emptyText.gameObject.SetActive(items.Count == 0);
        for (int r = 0; r < rows.Count; r++)
        {
            int i = scroll + r;
            var row = rows[r];
            bool show = i < items.Count;
            row.bg.gameObject.SetActive(show);
            if (!show)
            {
                continue;
            }
            ItemData d = items[i].itemData;
            row.bg.color = i == cursor ? Color.white : new Color(1f, 1f, 1f, 0f);
            row.icon.sprite = Resources.Load<Sprite>("Item/" + d.name);
            row.icon.enabled = row.icon.sprite != null;
            row.text.text = "<color=#" + ColorUtility.ToHtmlStringRGB(theme.RarityColor(d.rarity)) + ">" + EnhanceRules.DisplayName(string.IsNullOrEmpty(d.itemNameT) ? d.name : d.itemNameT, d.upgrade) + "</color>";
        }
        detail.text = items.Count == 0 ? "" : DetailText(items[cursor].itemData, theme);
    }

    static string DetailText(ItemData d, UiTheme theme)
    {
        int lv = d.upgrade;
        string ok = "#8CFF8C", bad = "#" + ColorUtility.ToHtmlStringRGB(theme.warning);
        var sb = new System.Text.StringBuilder();
        sb.Append("<size=120%>").Append(EnhanceRules.DisplayName(string.IsNullOrEmpty(d.itemNameT) ? d.name : d.itemNameT, lv)).Append("</size>\n\n");
        if (!EnhanceRules.CanEnhance(lv))
        {
            sb.Append("<color=#FFC640>MAX ENHANCED</color>");
            return sb.ToString();
        }
        bool weapon = d.equipArea == "Weapon";
        float now = weapon ? EnhanceRules.WeaponMultiplier(lv) : EnhanceRules.GearMultiplier(lv);
        float next = weapon ? EnhanceRules.WeaponMultiplier(lv + 1) : EnhanceRules.GearMultiplier(lv + 1);
        sb.Append("+").Append(lv).Append("  →  <color=#FFC640>+").Append(lv + 1).Append("</color>\n");
        sb.Append(weapon ? "Damage " : "Stats ").Append(Pct(now)).Append(" → ").Append(Pct(next)).Append("\n\n");
        int streak = EnhanceService.FailStreak;
        bool chance = streak >= EnhanceRules.PityFails;
        sb.Append("Success  ").Append(chance ? "<color=#FFC640>100% (CHANCE!)</color>" : Mathf.RoundToInt(EnhanceRules.Rate(lv) * 100f) + "%").Append("\n");
        sb.Append("On fail  ").Append(EnhanceRules.FailDrops(lv) ? "<color=" + bad + ">-1 level</color>" : "keep level").Append("\n\n");
        int haveStone = InventoryManager.instance.CountItem(EnhanceRules.StoneName), needStone = EnhanceRules.Stones(lv);
        int needGold = EnhanceRules.Gold(lv);
        sb.Append("Crystals  <color=").Append(haveStone >= needStone ? ok : bad).Append(">").Append(haveStone).Append(" / ").Append(needStone).Append("</color>\n");
        sb.Append("Gold    <color=").Append(DatabaseManager.money >= needGold ? ok : bad).Append(">").Append(DatabaseManager.money).Append(" / ").Append(needGold).Append("</color>");
        return sb.ToString();
    }

    static string Pct(float mul)
    {
        return "+" + Mathf.RoundToInt((mul - 1f) * 100f) + "%";
    }

    // ── UI 생성 ──
    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 70;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        panel = theme.NewPanel("Panel", transform).gameObject;
        var pr = (RectTransform)panel.transform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1000f, 560f);

        var title = NewText("Title", pr, theme, 40f);
        Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(30f, -20f), new Vector2(940f, 50f));
        title.text = "ENHANCE";
        title.color = theme.unique;

        var hint = NewText("Hint", pr, theme, 22f);
        Place(hint.rectTransform, new Vector2(0f, 0f), new Vector2(30f, 18f), new Vector2(940f, 30f));
        hint.text = "UP/DOWN select    SELECT enhance    BACK close";
        hint.color = new Color(1f, 1f, 1f, 0.6f);

        for (int r = 0; r < VisibleRows; r++)
        {
            Image bg = theme.NewPanel("Row" + r, pr, true, 0f);
            Place((RectTransform)bg.transform, new Vector2(0f, 1f), new Vector2(30f, -86f - r * RowH), new Vector2(440f, RowH - 4f));
            Image icon = NewImage("Icon", bg.transform, Color.white);
            Place((RectTransform)icon.transform, new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(34f, 34f), new Vector2(0f, 0.5f));
            icon.preserveAspect = true;
            var text = NewText("Text", bg.transform, theme, 26f);
            Place(text.rectTransform, new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(380f, RowH), new Vector2(0f, 0.5f));
            text.alignment = TextAlignmentOptions.MidlineLeft;
            rows.Add((bg, icon, text));
        }

        emptyText = NewText("Empty", pr, theme, 26f);
        Place(emptyText.rectTransform, new Vector2(0f, 1f), new Vector2(30f, -90f), new Vector2(440f, 80f));
        emptyText.text = "No equipment in inventory";
        emptyText.color = new Color(1f, 1f, 1f, 0.6f);

        detail = NewText("Detail", pr, theme, 26f);
        Place(detail.rectTransform, new Vector2(0f, 1f), new Vector2(510f, -86f), new Vector2(460f, 330f));
        detail.alignment = TextAlignmentOptions.TopLeft;
        detail.enableWordWrapping = true;

        resultText = NewText("Result", pr, theme, 40f);
        Place(resultText.rectTransform, new Vector2(0f, 0f), new Vector2(510f, 60f), new Vector2(460f, 60f));
        resultText.alignment = TextAlignmentOptions.Center;
        resultAnimator = resultText.gameObject.AddComponent<TextAnimator>();

        panel.SetActive(false);
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot ?? new Vector2(0f, 1f);
        if (pivot == null && anchor.y == 0f)
        {
            rt.pivot = Vector2.zero;
        }
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        return t;
    }
}
