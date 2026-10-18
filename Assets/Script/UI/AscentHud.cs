using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>좌상단 탐험가 칭호 + 상승 부하/저항 표시. 부족분이 있으면 붉게 (TowerWorld.md 7절)</summary>
public class AscentHud : MonoBehaviour
{
    static AscentHud instance;
    TextMeshProUGUI text;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
        {
            return;
        }
        var go = new GameObject("[AscentHud]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AscentHud>();
        instance.Build();
        AscentLoad.Changed += instance.Refresh;
        instance.Refresh();
    }

    void OnDestroy()
    {
        AscentLoad.Changed -= Refresh;
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(transform, false);
        text = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            text.font = theme.pixelFont;
        }
        text.fontSize = 24f;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        text.outlineWidth = 0.25f;
        text.outlineColor = new Color32(0, 0, 0, 220);
        var rt = text.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(24f, -196f); // HP·스태미나 바·횃불 아이콘 아래
        rt.sizeDelta = new Vector2(600f, 60f);
    }

    void Refresh()
    {
        if (text == null)
        {
            return;
        }
        UiTheme theme = UiTheme.Get();
        int d = AscentLoad.Deficit;
        string loadColor = d > 0 ? "#" + ColorUtility.ToHtmlStringRGB(theme.warning) : "#9ED0FF";
        text.text = "<color=#" + ColorUtility.ToHtmlStringRGB(theme.unique) + ">" + AscentLoad.RankName(AscentLoad.Rank).ToUpperInvariant() + " EXPLORER</color>\n"
            + "<color=" + loadColor + ">LOAD " + AscentLoad.CurrentLoad + " / RES " + AscentLoad.Resistance + (d > 0 ? "  -" + Mathf.RoundToInt(d * AscentLoad.PenaltyPerPoint * 100f) + "% DMG" : "") + "</color>";
    }
}
