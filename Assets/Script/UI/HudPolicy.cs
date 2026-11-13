using PixelCrushers.QuestMachine;
using SRDebugger.UI.Other;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD 표시·배치 정책 한 곳(11-02). 큰 창(인벤·창고·상점·제작·퀘스트·일시정지 등 = UiLock 이 하나라도 잡힌 상태)이 열리면
/// 창 위에 겹치던 HUD(퀘스트 추적·아티팩트 칩·SR 치트 줄)를 숨기고, 평소에는 치트 줄을 우하단에 놓는다.
/// 씬에 남은 Quest Machine 추적 HUD 는 REWORK-5 P5 부터 항상 숨긴다(퀘스트는 NpcQuestTracker 하나로 표시).
/// 개별 HUD(AscentHud·CraftPinHud·MinimapHud·NpcQuestTracker)는 <see cref="Hidden"/> 만 본다.
/// </summary>
public static class HudPolicy
{
    /// <summary>1920x1080 기준 우측 추적 열: 오른쪽 여백 / 폭 / 중앙선에서 위로 올라간 상단 / 간격 / 추적 상자 최대 높이.</summary>
    public const float ColumnMargin = 16f;
    public const float ColumnWidth = 380f;
    public const float ColumnTopAboveMiddle = 125f;
    public const float ColumnGap = 8f;
    public const float TrackerMaxHeight = 330f;

    /// <summary>큰 창이 열려 있으면 true. 창 하나라도 UiLock 을 잡으면 DatabaseManager.isOpenUI 가 true(대화·일시정지·타이틀·미니게임 포함 — 기존 HUD 들이 이미 그렇게 숨었다).</summary>
    public static bool Hidden => DatabaseManager.isOpenUI;

    /// <summary>NpcQuestTracker 가 매 갱신마다 기록하는 자기 상자 높이(1080 기준, 숨김·없음이면 0).</summary>
    public static float TrackerHeight { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        TrackerHeight = 0f;
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
    private float _nextResolve;
    private float _nextFit;

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
        SetCanvas(_qmCanvas, false); // REWORK-5 P5: Quest Machine 추적은 쓰지 않는다
        if (Time.unscaledTime >= _nextFit)
        {
            _nextFit = Time.unscaledTime + FitInterval;
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
}
