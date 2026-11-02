using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>화면 우측 작은 퀘스트 추적 표시: 추적 지정한(NpcQuestTrack, 최대 3개) 퀘스트의 제목·목표 x/y·목표 구역 (완료 가능이면 노란색). 없으면 숨김.</summary>
public class NpcQuestTracker : MonoBehaviour
{
    private const float RefreshInterval = 0.5f;

    private TextMeshProUGUI _text;
    private GameObject _root;
    private float _nextRefresh;
    private bool _dirty = true;
    private bool _wasHidden;

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
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-HudPolicy.ColumnMargin, HudPolicy.ColumnTopAboveMiddle);
        rt.sizeDelta = new Vector2(HudPolicy.ColumnWidth, HudPolicy.TrackerMaxHeight);

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
        HudPolicy.TrackerHeight = 0f;
    }

    private void MarkDirty()
    {
        _dirty = true;
    }

    private void Update()
    {
        bool hidden = HudPolicy.Hidden;
        if (hidden != _wasHidden)
        {
            _wasHidden = hidden;
            _dirty = true; // 창이 열리고 닫히는 즉시 반영
        }
        if (!_dirty && Time.unscaledTime < _nextRefresh)
        {
            return;
        }
        _dirty = false;
        _nextRefresh = Time.unscaledTime + RefreshInterval;
        if (HudPolicy.Hidden)
        {
            // 큰 창이 열려 있으면 창 위에 겹치지 않게 숨긴다(HudPolicy)
            if (_root.activeSelf)
            {
                _root.SetActive(false);
            }
            HudPolicy.TrackerHeight = 0f;
            return;
        }
        var active = NpcQuestTrack.Tracked(NpcQuestLog.ActiveQuests());
        if (active.Count == 0)
        {
            if (_root.activeSelf)
            {
                _root.SetActive(false);
            }
            HudPolicy.TrackerHeight = 0f;
            return;
        }
        var sb = new StringBuilder();
        for (int i = 0; i < active.Count; i++)
        {
            NpcQuestDef d = active[i];
            bool ready = NpcQuestLog.StatusOf(d) == NpcQuestStatus.Ready;
            sb.Append(ready ? "<color=#FFD633>" : "<color=#FFFFFF>").Append(d.Title).Append("</color>\n");
            string zone = NpcQuestZone.Of(d);
            sb.Append("<color=#BBBBBB>  ").Append(ready ? "완료! " + NpcQuestCatalog.NpcName(d.Giver) + "에게 보고" : NpcQuestWindow.ObjectiveText(d)).Append("</color>");
            if (!ready && zone.Length > 0)
            {
                sb.Append("  <color=#7FA8C8>@").Append(zone).Append("</color>");
            }
            sb.Append('\n');
        }
        _text.text = sb.ToString();
        float height = _text.GetPreferredValues(_text.text, HudPolicy.ColumnWidth - 20f, 0f).y + 12f;
        height = Mathf.Clamp(height, 44f, HudPolicy.TrackerMaxHeight);
        ((RectTransform)_root.transform).sizeDelta = new Vector2(HudPolicy.ColumnWidth, height);
        HudPolicy.TrackerHeight = height;
        if (!_root.activeSelf)
        {
            _root.SetActive(true);
        }
    }
}
