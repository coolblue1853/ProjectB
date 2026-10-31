using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>화면 우측 작은 퀘스트 추적 표시: 진행 중 최대 3개의 제목과 목표 x/y (완료 가능이면 노란색). 없으면 숨김.</summary>
public class NpcQuestTracker : MonoBehaviour
{
    private const int MaxShown = 3;
    private const float RefreshInterval = 0.5f;

    private TextMeshProUGUI _text;
    private GameObject _root;
    private float _nextRefresh;
    private bool _dirty = true;

    private void Awake()
    {
        UiTheme theme = UiTheme.Get();
        var go = new GameObject("[NpcQuestTracker]");
        DontDestroyOnLoad(go);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 48;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _root = new GameObject("Box", typeof(RectTransform), typeof(Image));
        _root.transform.SetParent(go.transform, false);
        var bg = _root.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.45f);
        bg.raycastTarget = false;
        var rt = (RectTransform)_root.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-16f, 40f);
        rt.sizeDelta = new Vector2(380f, 170f);

        var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        tgo.transform.SetParent(_root.transform, false);
        _text = tgo.GetComponent<TextMeshProUGUI>();
        if (theme != null && theme.pixelFont != null)
        {
            _text.font = theme.pixelFont;
        }
        _text.fontSize = 20f;
        _text.alignment = TextAlignmentOptions.TopLeft;
        _text.richText = true;
        _text.raycastTarget = false;
        _text.enableWordWrapping = true;
        _text.overflowMode = TextOverflowModes.Truncate;
        var tr = _text.rectTransform;
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(10f, 6f);
        tr.offsetMax = new Vector2(-10f, -6f);
        _root.SetActive(false);
    }

    private void OnEnable()
    {
        NpcQuestLog.Changed += MarkDirty;
    }

    private void OnDisable()
    {
        NpcQuestLog.Changed -= MarkDirty;
    }

    private void MarkDirty()
    {
        _dirty = true;
    }

    private void Update()
    {
        if (!_dirty && Time.unscaledTime < _nextRefresh)
        {
            return;
        }
        _dirty = false;
        _nextRefresh = Time.unscaledTime + RefreshInterval;
        var active = NpcQuestLog.ActiveQuests();
        if (active.Count == 0)
        {
            if (_root.activeSelf)
            {
                _root.SetActive(false);
            }
            return;
        }
        var sb = new StringBuilder();
        for (int i = 0; i < active.Count && i < MaxShown; i++)
        {
            NpcQuestDef d = active[i];
            bool ready = NpcQuestLog.StatusOf(d) == NpcQuestStatus.Ready;
            sb.Append(ready ? "<color=#FFD633>" : "<color=#FFFFFF>").Append(d.Title).Append("</color>\n");
            sb.Append("<color=#BBBBBB>  ").Append(ready ? "완료! " + NpcQuestCatalog.NpcName(d.Giver) + "에게 보고" : NpcQuestWindow.ObjectiveText(d)).Append("</color>\n");
        }
        _text.text = sb.ToString();
        if (!_root.activeSelf)
        {
            _root.SetActive(true);
        }
    }
}
