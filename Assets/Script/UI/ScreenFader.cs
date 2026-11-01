using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 암전(11-01 사용자: 포털·순간이동은 화면이 어두워졌다 밝아지는 게임다운 전환으로).
/// 검은 판 하나를 맨 위 캔버스에 두고, 어둡게(Out) → 콜백(이동) → 잠깐 유지 → 밝게(In). 시간 정지와 무관하게 실제 시간으로 돈다.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public const float OutTime = 0.18f, HoldTime = 0.12f, InTime = 0.3f;

    static ScreenFader instance;
    Image _black;
    Tween _seq;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        instance = null;
    }

    static ScreenFader Ensure()
    {
        if (instance == null)
        {
            var go = new GameObject("[ScreenFader]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ScreenFader>();
            instance.Build();
        }
        return instance;
    }

    /// <summary>지금 암전 중인가(Out~In 사이)</summary>
    bool _waiting;

    public static bool Busy => instance != null && (instance._waiting || (instance._seq != null && instance._seq.IsActive()));

    /// <summary>어두워진 뒤 midAction 을 부르고, 잠깐 유지했다가 밝아진다. done 은 다 밝아진 뒤</summary>
    public static void Transition(System.Action midAction, System.Action done = null)
    {
        ScreenFader f = Ensure();
        f.Kill();
        f._black.gameObject.SetActive(true);
        f._seq = f._black.DOFade(1f, OutTime)
            .OnComplete(() =>
            {
                midAction?.Invoke();
                f.FadeIn(done);
            })
            .SetUpdate(true)
            .SetLink(f.gameObject);
    }

    /// <summary>이미 이동이 끝난 경우(사망 귀환 등): 바로 검게 덮었다가 밝아진다(카메라가 따라오는 모습을 가린다)</summary>
    public static void FadeInFromBlack()
    {
        ScreenFader f = Ensure();
        f.Kill();
        f._black.gameObject.SetActive(true);
        Color c = f._black.color;
        c.a = 1f;
        f._black.color = c;
        f.FadeIn(null);
    }

    /// <summary>검은 화면 유지 → 밝아짐. 맵을 켜는 무거운 프레임이 지난 뒤(2프레임) 시작한다 — 그 프레임의 긴 unscaled 시간이 유지·페이드를 건너뛰던 문제(11-01 캡처)</summary>
    void FadeIn(System.Action done)
    {
        _waiting = true;
        StartCoroutine(FadeInAfterFrames(done));
    }

    System.Collections.IEnumerator FadeInAfterFrames(System.Action done)
    {
        yield return null;
        yield return null;
        _waiting = false;
        _seq = DOTween.Sequence()
            .AppendInterval(HoldTime)
            .Append(_black.DOFade(0f, InTime))
            .OnComplete(() => { _black.gameObject.SetActive(false); done?.Invoke(); })
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    void Kill()
    {
        StopAllCoroutines();
        _waiting = false;
        _seq?.Kill();
        _black.DOKill();
    }

    void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90; // HUD·배너 위, 다만 지역 배너는 밝아진 뒤에 보인다
        var go = new GameObject("Black", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        _black = go.GetComponent<Image>();
        _black.color = new Color(0f, 0f, 0f, 0f);
        _black.raycastTarget = false;
        go.SetActive(false);
    }
}
