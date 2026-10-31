using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>화면 아래 메이플식 EXP 바 + 레벨 표시 (ClassicMapleDesign.md). 씬을 건드리지 않고 런타임에 만든다.</summary>
public class LevelHud : MonoBehaviour
{
    const float BarHeight = 24f;

    static LevelHud instance;
    Image fill;
    TextMeshProUGUI label;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
        {
            return;
        }
        var go = new GameObject("[LevelHud]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<LevelHud>();
        instance.Build();
        LevelSystem.Changed += instance.Refresh;
        instance.Refresh();
    }

    void OnDestroy()
    {
        LevelSystem.Changed -= Refresh;
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 49;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var bg = NewImage("Bar", transform, new Color(0f, 0f, 0f, 0.7f));
        RectTransform br = bg.rectTransform;
        br.anchorMin = new Vector2(0f, 0f);
        br.anchorMax = new Vector2(1f, 0f);
        br.pivot = new Vector2(0.5f, 0f);
        br.anchoredPosition = Vector2.zero;
        br.sizeDelta = new Vector2(0f, BarHeight);

        fill = NewImage("Fill", bg.transform, new Color(0.95f, 0.78f, 0.25f, 0.95f));
        RectTransform fr = fill.rectTransform;
        fr.anchorMin = Vector2.zero;
        fr.anchorMax = new Vector2(0f, 1f);
        fr.pivot = new Vector2(0f, 0.5f);
        fr.offsetMin = fr.offsetMax = Vector2.zero;

        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(bg.transform, false);
        label = go.GetComponent<TextMeshProUGUI>();
        if (theme != null && theme.pixelFont != null)
        {
            label.font = theme.pixelFont;
        }
        label.fontSize = 20f;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        label.outlineWidth = 0.25f;
        label.outlineColor = new Color32(0, 0, 0, 220);
        RectTransform lr = label.rectTransform;
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = lr.offsetMax = Vector2.zero;
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

    void Refresh()
    {
        if (fill == null || label == null)
        {
            return;
        }
        int level = LevelSystem.Level;
        int need = LevelRules.ExpToNext(level);
        float ratio = need > 0 ? Mathf.Clamp01((float)LevelSystem.Exp / need) : 1f;
        fill.rectTransform.anchorMax = new Vector2(ratio, 1f);
        label.text = need > 0
            ? "Lv." + level + "   " + Loc.T("EXP") + " " + LevelSystem.Exp + " / " + need + "  (" + (ratio * 100f).ToString("0.0") + "%)"
            : "Lv." + level + "   MAX";
    }
}
