using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 우측 퀘스트 추적 HUD (REWORK-5 P3, 3계통 단일): 헤더 "▼ 퀘스트 (n)" + 추적 중인 퀘스트(최대 NpcQuestTrack.Max, 현상 포함)를
/// 전부 같은 2줄 형식 `[유형] 제목` / `  목표 현재/필요 @구역`(완료 가능이면 노란 제목 + "완료! ○○에게 보고")로 보여 준다.
/// 초과분은 "…외 n건", 구역 세트 재료 한 줄은 목록 아래. 헤더를 클릭하면(또는 Q 창의 H·LT) 접고 ES3 quest.hudFold 에 저장. 없으면 숨김.
/// </summary>
public class NpcQuestTracker : MonoBehaviour
{
    private const float RefreshInterval = 0.5f;

    private TextMeshProUGUI _text;
    private GameObject _root;
    private float _nextRefresh;
    private string _lastText; // 마지막으로 적용한 추적 문구 — 같으면 TMP 높이 재계산을 건너뛴다(LP-3)
    private bool _dirty = true;
    private bool _wasHidden;
    private bool _lastFold;

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
        // 헤더 줄 클릭 = 접기/펼치기 (마우스). 키·패드는 Q 창에서(H · LT)
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        var hgo = new GameObject("FoldHeader", typeof(RectTransform), typeof(Image), typeof(Button));
        hgo.transform.SetParent(_root.transform, false);
        var himg = hgo.GetComponent<Image>();
        himg.color = new Color(1f, 1f, 1f, 0f);
        var hbtn = hgo.GetComponent<Button>();
        hbtn.targetGraphic = himg;
        hbtn.transition = Selectable.Transition.None;
        hbtn.navigation = new Navigation { mode = Navigation.Mode.None };
        hbtn.onClick.AddListener(OnHeaderClick);
        var hr = (RectTransform)hgo.transform;
        hr.anchorMin = new Vector2(0f, 1f);
        hr.anchorMax = new Vector2(1f, 1f);
        hr.pivot = new Vector2(0.5f, 1f);
        hr.sizeDelta = new Vector2(0f, 28f);
        hr.anchoredPosition = Vector2.zero;
        _root.SetActive(false);
    }

    private void OnHeaderClick()
    {
        NpcQuestTrack.SetHudFolded(!NpcQuestTrack.HudFolded);
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
        _dirty = true;
    }

    private void OnEnable()
    {
        NpcQuestLog.Changed += MarkDirty;
        _dirty = true;
    }

    private void OnDisable()
    {
        NpcQuestLog.Changed -= MarkDirty;
        HudPolicy.TrackerHeight = 0f;
        if (_root != null)
        {
            _root.SetActive(false); // 컴포넌트가 꺼지면 HUD 도 숨긴다(Update 가 멈춰 남는 것 방지)
        }
    }

    private void MarkDirty()
    {
        _dirty = true;
    }

    private void Update()
    {
        bool hidden = HudPolicy.Hidden;
        if (hidden != _wasHidden || NpcQuestTrack.HudFolded != _lastFold)
        {
            _wasHidden = hidden;
            _lastFold = NpcQuestTrack.HudFolded;
            _dirty = true; // 창이 열리고 닫히는 즉시, 접기가 바뀌면 바로 반영
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
            _lastText = null;
            HudPolicy.TrackerHeight = 0f;
            return;
        }
        List<NpcQuestDef> all = QuestService.ActiveAll();
        List<NpcQuestDef> active = NpcQuestTrack.Tracked(all);
        string setLine = ZoneProgress.TrackerLine(); // 현재 구역 세트 재료 n/m 한 줄(L6)
        if (all.Count == 0 && setLine.Length == 0)
        {
            if (_root.activeSelf)
            {
                _root.SetActive(false);
            }
            _lastText = null;
            HudPolicy.TrackerHeight = 0f;
            return;
        }
        string built = BuildText(all, active, setLine, NpcQuestTrack.HudFolded);
        if (built == _lastText && _root.activeSelf)
        {
            return; // 값이 그대로면 텍스트·높이 갱신 생략
        }
        _lastText = built;
        _text.text = built;
        float height = _text.GetPreferredValues(built, HudPolicy.ColumnWidth - 20f, 0f).y + 12f;
        height = Mathf.Clamp(height, 44f, HudPolicy.TrackerMaxHeight);
        ((RectTransform)_root.transform).sizeDelta = new Vector2(HudPolicy.ColumnWidth, height);
        HudPolicy.TrackerHeight = height;
        if (!_root.activeSelf)
        {
            _root.SetActive(true);
        }
    }

    /// <summary>추적 HUD 본문 (순수 함수 — 테스트가 형식을 확인한다). all = 진행 중 전부, tracked = 표시할 것</summary>
    public static string BuildText(IList<NpcQuestDef> all, IList<NpcQuestDef> tracked, string setLine, bool folded)
    {
        var sb = new StringBuilder();
        bool anyReady = false;
        foreach (NpcQuestDef d in all)
        {
            anyReady |= QuestService.IsReady(d);
        }
        if (all.Count > 0)
        {
            sb.Append("<color=#88AACC>").Append(folded ? "▶" : "▼").Append(" 퀘스트 (").Append(all.Count).Append(")</color>");
            sb.Append(folded && anyReady ? "  <color=#FFD633>완료 가능!</color>" : "").Append('\n');
        }
        if (folded)
        {
            return sb.ToString();
        }
        for (int i = 0; i < tracked.Count; i++)
        {
            NpcQuestDef d = tracked[i];
            bool ready = QuestService.IsReady(d);
            sb.Append(ready ? "<color=#FFD633>" : "<color=#FFFFFF>").Append('[').Append(QuestService.KindTag(d)).Append("] ").Append(d.Title).Append("</color>\n");
            string zone = QuestService.ZoneOf(d);
            sb.Append("<color=#BBBBBB>  ").Append(ready ? "완료! " + QuestService.ReportTarget(d) + "에게 보고" : QuestService.Objective(d)).Append("</color>");
            if (!ready && !string.IsNullOrEmpty(zone))
            {
                sb.Append("  <color=#7FA8C8>@").Append(zone).Append("</color>");
            }
            sb.Append('\n');
        }
        int extra = tracked.Count >= NpcQuestTrack.Max ? all.Count - tracked.Count : 0;
        if (extra > 0)
        {
            sb.Append("<color=#777777>…외 ").Append(extra).Append("건</color>\n");
        }
        sb.Append(setLine);
        return sb.ToString();
    }
}
