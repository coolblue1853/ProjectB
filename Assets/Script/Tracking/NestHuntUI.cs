using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 정예 둥지 추적 HUD(코드 생성, 화면 위 가운데 작은 패널). NestHuntSite.Latest 를 읽기만 한다.
/// 진행 중: 제목(흔적 n/3 또는 둥지 찾기), 다음 표식의 방향·거리 힌트(플레이어 기준 5m 단위), 남은 시간 막대. 살피는 중에는 진행률.
/// 끝나면 결과 문구(정예 출현 / 흔적을 놓침)만 몇 초 보인다.
/// </summary>
public class NestHuntUI : MonoBehaviour
{
    private const float PanelWidth = 460f;
    private const float PanelHeight = 96f;
    private const float BarWidth = 400f;

    private static NestHuntUI _instance;

    private GameObject _panel;
    private TextMeshProUGUI _message;
    private TextMeshProUGUI _sub;
    private RectTransform _timeTrack;
    private RectTransform _timeFill;
    private string _lastMessage;
    private string _lastSub;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[NestHuntUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<NestHuntUI>();
        _instance.Build();
    }

    private void Update()
    {
        NestHuntSite site = NestHuntSite.Latest;
        bool show = site != null && site.isActiveAndEnabled && site.Active;
        if (_panel.activeSelf != show)
        {
            _panel.SetActive(show);
        }
        if (!show)
        {
            return;
        }
        NestHuntState s = site.State;
        if (s.Finished)
        {
            _timeTrack.gameObject.SetActive(false);
            Set(ref _lastMessage, _message, ResultText(site));
            Set(ref _lastSub, _sub, string.Empty);
            return;
        }
        _timeTrack.gameObject.SetActive(true);
        _timeFill.sizeDelta = new Vector2(BarWidth * Mathf.Clamp01(s.Remaining / NestHuntRules.TimeLimit), _timeFill.sizeDelta.y);
        Set(ref _lastMessage, _message, s.Phase == NestPhase.NestRevealed
            ? Loc.T("Nest hunt: find the nest")
            : string.Format(Loc.T("Nest hunt: trail {0}/{1}"), s.Found + 1, NestHuntRules.Trails));
        Set(ref _lastSub, _sub, SubText(site));
    }

    private static string ResultText(NestHuntSite site)
    {
        if (site.State.Phase != NestPhase.Success)
        {
            return Loc.T("The nest was lost");
        }
        return site.SummonedName != null
            ? string.Format(Loc.T("Elite {0} emerged from the nest!"), site.SummonedName)
            : Loc.T("The nest was empty");
    }

    private static string SubText(NestHuntSite site)
    {
        int next = site.State.Found;
        NestMark mark = site.Mark(next);
        if (mark != null && mark.Investigating)
        {
            return Loc.T("Investigating...") + " " + Mathf.RoundToInt(mark.Progress * 100f) + "%";
        }
        PlayerController pc = PlayerController.instance;
        if (mark == null || pc == null)
        {
            return string.Empty;
        }
        NestHuntRules.Hint(pc.transform.position.x, mark.Position.x, out int dir, out int meters);
        if (dir == 0)
        {
            return Loc.T("Right here - investigate (UP)");
        }
        string side = Loc.T(dir < 0 ? "Left" : "Right");
        string key = next >= NestHuntRules.Trails ? "Nest: {0} about {1}m" : "Next sign: {0} about {1}m";
        return string.Format(Loc.T(key), side, meters);
    }

    private static void Set(ref string last, TextMeshProUGUI label, string text)
    {
        if (last != text)
        {
            last = text;
            label.text = text;
        }
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 54;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image panel = theme.NewPanel("NestHuntPanel", transform, false, 0.9f);
        _panel = panel.gameObject;
        RectTransform rt = panel.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -100f);
        rt.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        _message = TrackingUI.NewText(theme, "Message", rt, new Vector2(0f, -12f), 22f);
        _sub = TrackingUI.NewText(theme, "Sub", rt, new Vector2(0f, -44f), 18f);

        _timeTrack = TrackingUI.NewImage("TimeTrack", rt, new Color(0.08f, 0.08f, 0.1f, 0.9f)).rectTransform;
        _timeTrack.anchorMin = _timeTrack.anchorMax = new Vector2(0.5f, 0f);
        _timeTrack.anchoredPosition = new Vector2(0f, 14f);
        _timeTrack.sizeDelta = new Vector2(BarWidth, 6f);
        _timeFill = TrackingUI.NewImage("Fill", _timeTrack, theme.unique).rectTransform;
        _timeFill.anchorMin = new Vector2(0f, 0f);
        _timeFill.anchorMax = new Vector2(0f, 1f);
        _timeFill.pivot = new Vector2(0f, 0.5f);
        _timeFill.anchoredPosition = Vector2.zero;
        _timeFill.sizeDelta = new Vector2(BarWidth, 0f);

        _panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
