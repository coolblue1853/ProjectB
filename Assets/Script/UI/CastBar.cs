using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 범용 시전 바(화면 하단 중앙): 채워지는 바 + 이름. Show(이름, 진행 콜백) → 완료면 Hide(true), 취소면 Hide(false)(빨갛게 깜박이고 사라짐).
/// 지금은 귀환 주문서(RecallCast 이벤트 구독)만 쓴다.
/// </summary>
public sealed class CastBar : MonoBehaviour
{
    private const float BarWidth = 360f;
    private const float BarHeight = 26f;
    private static readonly Color FillColor = new Color(0.45f, 0.78f, 1f, 1f);
    private static readonly Color FailColor = new Color(1f, 0.2f, 0.2f, 1f);

    private static CastBar _instance;

    private GameObject _root;
    private Image _fill;
    private TextMeshProUGUI _label;
    private Func<float> _progress;
    private Tween _blink;

    public static bool IsVisible => _instance != null && _instance._root != null && _instance._root.activeSelf;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[CastBar]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<CastBar>();
    }

    public static void Show(string label, Func<float> progress)
    {
        if (_instance != null)
        {
            _instance.DoShow(label, progress);
        }
    }

    public static void Hide(bool success)
    {
        if (_instance != null)
        {
            _instance.DoHide(success);
        }
    }

    private void Awake()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 49;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _root = new GameObject("Box", typeof(RectTransform), typeof(Image));
        _root.transform.SetParent(transform, false);
        var bg = _root.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.7f);
        bg.raycastTarget = false;
        var rt = (RectTransform)_root.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 220f);
        rt.sizeDelta = new Vector2(BarWidth + 8f, BarHeight + 8f);

        var fgo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fgo.transform.SetParent(_root.transform, false);
        _fill = fgo.GetComponent<Image>();
        _fill.color = FillColor;
        _fill.raycastTarget = false;
        _fill.type = Image.Type.Filled;
        _fill.fillMethod = Image.FillMethod.Horizontal;
        _fill.fillOrigin = 0;
        var fr = (RectTransform)fgo.transform;
        fr.anchorMin = Vector2.zero;
        fr.anchorMax = Vector2.one;
        fr.offsetMin = new Vector2(4f, 4f);
        fr.offsetMax = new Vector2(-4f, -4f);

        var tgo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        tgo.transform.SetParent(_root.transform, false);
        _label = tgo.GetComponent<TextMeshProUGUI>();
        UiTheme theme = UiTheme.Get();
        if (theme != null && theme.pixelFont != null)
        {
            _label.font = theme.pixelFont;
        }
        _label.fontSize = 20f;
        _label.alignment = TextAlignmentOptions.Center;
        _label.raycastTarget = false;
        _label.color = Color.white;
        var lr = _label.rectTransform;
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = lr.offsetMax = Vector2.zero;

        _root.SetActive(false);
    }

    private void OnEnable()
    {
        RecallCast.Started += OnRecallStarted;
        RecallCast.Ended += OnRecallEnded;
    }

    private void OnDisable()
    {
        RecallCast.Started -= OnRecallStarted;
        RecallCast.Ended -= OnRecallEnded;
        _blink?.Kill();
        _blink = null;
    }

    private void OnRecallStarted()
    {
        DoShow(RecallCast.CastLabel, GetRecallProgress);
    }

    private void OnRecallEnded(bool completed)
    {
        DoHide(completed);
    }

    private static float GetRecallProgress()
    {
        return RecallCast.Progress;
    }

    private void DoShow(string label, Func<float> progress)
    {
        _blink?.Kill();
        _blink = null;
        _progress = progress;
        _label.text = label;
        _fill.color = FillColor;
        _fill.fillAmount = 0f;
        _root.SetActive(true);
    }

    private void DoHide(bool success)
    {
        _progress = null;
        _blink?.Kill();
        _blink = null;
        if (success)
        {
            _root.SetActive(false);
            return;
        }
        _fill.fillAmount = Mathf.Max(_fill.fillAmount, 0.12f);
        _fill.color = FailColor;
        var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        for (int i = 0; i < 3; i++)
        {
            seq.Append(_fill.DOFade(0.15f, 0.07f));
            seq.Append(_fill.DOFade(1f, 0.07f));
        }
        seq.OnComplete(() => _root.SetActive(false));
        _blink = seq;
    }

    private void Update()
    {
        if (_progress != null)
        {
            _fill.fillAmount = _progress();
        }
    }
}
