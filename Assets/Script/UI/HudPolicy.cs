using System.Collections.Generic;
using PixelCrushers.QuestMachine;
using SRDebugger.UI.Other;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD 표시·배치 정책 한 곳(11-02). 큰 창(인벤·창고·상점·제작·퀘스트·일시정지 등 = UiLock 이 하나라도 잡힌 상태)이 열리면
/// 창 위에 겹치던 HUD(퀘스트 추적·Quest Machine 추적·아티팩트 칩·SR 치트 줄)를 숨기고,
/// 평소에는 우측 퀘스트 열(NpcQuestTracker 아래에 Quest Machine 추적)과 치트 줄(우하단)을 겹치지 않게 놓는다.
/// 개별 HUD(AscentHud·CraftPinHud·MinimapHud·NpcQuestTracker)는 <see cref="Hidden"/> 만 본다.
/// </summary>
public static class HudPolicy
{
    /// <summary>1920x1080 기준 우측 추적 열: 오른쪽 여백 / 폭 / 중앙선에서 위로 올라간 상단 / 간격 / 추적 상자 최대 높이.</summary>
    public const float ColumnMargin = 16f;
    public const float ColumnWidth = 380f;
    public const float ColumnTopAboveMiddle = 125f;
    public const float ColumnGap = 8f;
    public const float TrackerMaxHeight = 190f;

    /// <summary>큰 창이 열려 있으면 true. 창 하나라도 UiLock 을 잡으면 DatabaseManager.isOpenUI 가 true(대화·일시정지·타이틀·미니게임 포함 — 기존 HUD 들이 이미 그렇게 숨었다).</summary>
    public static bool Hidden => DatabaseManager.isOpenUI;

    /// <summary>NpcQuestTracker 가 매 갱신마다 기록하는 자기 상자 높이(1080 기준, 숨김·없음이면 0).</summary>
    public static float TrackerHeight { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        TrackerHeight = 0f;
    }

    /// <summary>Quest Machine 추적 상단의 중앙선 기준 y(1080 기준, 위가 +). 추적 상자가 있으면 그 아래.</summary>
    public static float QuestHudTop(float trackerHeight)
    {
        return ColumnTopAboveMiddle - (trackerHeight > 0f ? trackerHeight + ColumnGap : 0f);
    }

    /// <summary>Quest Machine 캔버스(기준 높이 600 + 높이 맞춤이라 k 는 해상도와 무관하게 약 0.556)의 폰트 크기를 1080 기준 제목 26/본문 24(QM 픽셀 글꼴은 Galmuri 보다 글리프가 작다)로 환산한다. 이미 맞춘 값은 그대로 둔다.</summary>
    public static int QuestHudFontSize(int current, float k)
    {
        int title = Mathf.Max(8, Mathf.RoundToInt(26f * k));
        int body = Mathf.Max(8, Mathf.RoundToInt(24f * k));
        if (current <= title)
        {
            return current;
        }
        return current >= 28 ? title : body;
    }
}

/// <summary>HudPolicy 를 실제로 적용하는 상주 객체(씬 로드 때 자동 생성). 대상은 1초마다 다시 찾는다(맵 전환·재생성 대비).</summary>
public sealed class HudPolicyDriver : MonoBehaviour
{
    private const float ResolveInterval = 1f;
    private const float FitInterval = 0.25f;
    private static readonly Vector2 CheatAnchorMin = new Vector2(0.56f, 0f);

    private Canvas _artifact;
    private PinnedUIRoot _sr;
    private UnityUIQuestHUD _qm;
    private Canvas _qmCanvas;
    private RectTransform _qmPanel;
    private int _qmSignature = -1;
    private float _nextResolve;
    private float _nextFit;
    private readonly List<Text> _texts = new List<Text>(8);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindFirstObjectByType<HudPolicyDriver>() != null)
        {
            return;
        }
        var go = new GameObject("[HudPolicy]");
        DontDestroyOnLoad(go);
        go.AddComponent<HudPolicyDriver>();
    }

    private void Update()
    {
        if (Time.unscaledTime >= _nextResolve)
        {
            _nextResolve = Time.unscaledTime + ResolveInterval;
            Resolve();
        }
        bool visible = !HudPolicy.Hidden;
        SetCanvas(_artifact, visible);
        if (_sr != null)
        {
            SetCanvas(_sr.Canvas, visible);
        }
        SetCanvas(_qmCanvas, visible);
        if (Time.unscaledTime >= _nextFit)
        {
            _nextFit = Time.unscaledTime + FitInterval;
            FitQuestHud();
            if (_sr != null)
            {
                PlaceCheats(_sr); // SR 이 레이아웃을 다시 계산해 덮어쓴 경우 복구(이미 배치됐으면 비교만)
            }
        }
    }

    private void Resolve()
    {
        if (_artifact == null)
        {
            var sys = FindFirstObjectByType<ArtifactSystem>();
            _artifact = sys != null ? sys.GetComponent<Canvas>() : null;
        }
        if (_sr == null)
        {
            _sr = FindFirstObjectByType<PinnedUIRoot>(FindObjectsInactive.Include);
            if (_sr != null && !PlaceCheats(_sr))
            {
                _sr = null; // 아직 준비 안 됨 → 다음 해결 때 재시도
            }
        }
        if (_qm == null)
        {
            _qm = FindFirstObjectByType<UnityUIQuestHUD>(FindObjectsInactive.Include);
            _qmPanel = null;
            _qmSignature = -1;
            // 하위 캔버스를 얹어 껐다 켠다(Quest Machine 캔버스에는 저널·대화 UI 도 있어 캔버스 전체를 끌 수 없다)
            _qmCanvas = null;
            if (_qm != null)
            {
                _qmCanvas = _qm.GetComponent<Canvas>();
                if (_qmCanvas == null)
                {
                    _qmCanvas = _qm.gameObject.AddComponent<Canvas>();
                }
            }
        }
    }

    private static void SetCanvas(Canvas canvas, bool visible)
    {
        if (canvas != null && canvas.enabled != visible)
        {
            canvas.enabled = visible;
        }
    }

    /// <summary>SR 고정 치트 줄을 체력 HUD 가 있는 좌상단 대신 우하단 두 줄로 옮긴다(경험치 바 위, 퀵슬롯 오른쪽). 부모 레이아웃은 무시시킨다.</summary>
    private static bool PlaceCheats(PinnedUIRoot root)
    {
        var rt = root.Options != null ? root.Options.transform as RectTransform : null;
        if (rt == null)
        {
            return false;
        }
        if (rt.anchorMin == CheatAnchorMin && rt.pivot == Vector2.right)
        {
            return true; // 이미 배치됨
        }
        var le = rt.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = rt.gameObject.AddComponent<LayoutElement>();
        }
        le.ignoreLayout = true;
        rt.anchorMin = CheatAnchorMin;
        rt.anchorMax = Vector2.right;
        rt.pivot = Vector2.right;
        rt.anchoredPosition = new Vector2(-8f, 30f);
        rt.sizeDelta = new Vector2(0f, 96f);
        if (root.OptionsLayoutGroup != null)
        {
            root.OptionsLayoutGroup.childAlignment = TextAnchor.LowerRight;
        }
        return true;
    }

    /// <summary>Quest Machine 추적 패널: 우측 열에서 추적 상자 아래, 고정 폭으로 줄바꿈, 폰트는 우리 HUD 크기에 맞춤.</summary>
    private void FitQuestHud()
    {
        if (_qm == null)
        {
            return;
        }
        if (_qmPanel == null && _qm.transform.childCount > 0)
        {
            _qmPanel = _qm.transform.GetChild(0) as RectTransform;
        }
        // 우리가 얹은 하위 캔버스(꺼지면 높이 0)를 건너뛰고 부모 쪽 루트 캔버스를 기준으로 삼는다
        Canvas parentCanvas = _qm.transform.parent != null ? _qm.transform.parent.GetComponentInParent<Canvas>() : null;
        if (_qmPanel == null || parentCanvas == null)
        {
            return;
        }
        float height = ((RectTransform)parentCanvas.rootCanvas.transform).rect.height;
        if (height < 100f)
        {
            return; // 스케일러가 아직 적용되기 전
        }
        float k = height / 1080f;
        // 추적 객체(HUD)는 캔버스 우상단 점이므로 위에서부터 (중앙선 - 상단) 만큼 내려온다
        float y = -height * 0.5f + HudPolicy.QuestHudTop(HudPolicy.TrackerHeight) * k;
        var pos = new Vector2(-HudPolicy.ColumnMargin * k, y);
        var size = new Vector2(HudPolicy.ColumnWidth * k, _qmPanel.sizeDelta.y);
        if (_qmPanel.anchorMin != new Vector2(1f, 1f) || _qmPanel.pivot != new Vector2(1f, 1f)
            || (_qmPanel.anchoredPosition - pos).sqrMagnitude > 0.01f || !Mathf.Approximately(_qmPanel.sizeDelta.x, size.x))
        {
            _qmPanel.anchorMin = _qmPanel.anchorMax = new Vector2(1f, 1f);
            _qmPanel.pivot = new Vector2(1f, 1f);
            _qmPanel.anchoredPosition = pos;
            _qmPanel.sizeDelta = size;
            var fitter = _qmPanel.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            var vlg = _qmPanel.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.childControlWidth = true;
                vlg.childForceExpandWidth = true;
            }
        }
        int signature = _qmPanel.childCount * 31 + Mathf.RoundToInt(k * 1000f);
        if (signature == _qmSignature)
        {
            return;
        }
        _qmSignature = signature;
        _qmPanel.GetComponentsInChildren(true, _texts);
        for (int i = 0; i < _texts.Count; i++)
        {
            Text t = _texts[i];
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.fontSize = HudPolicy.QuestHudFontSize(t.fontSize, k);
        }
        _texts.Clear();
        LayoutRebuilder.MarkLayoutForRebuild(_qmPanel);
    }
}
