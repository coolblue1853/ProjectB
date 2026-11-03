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

    private bool _waiting;
    // 진행 중 Transition 의 이동(아직 안 불렸으면)·완료 콜백. 다른 암전이 끊어도 Kill 이 대신 불러 포털 플래그가 남지 않게 한다
    private System.Action _pendingMid, _pendingDone;
    // 전환 내내(암전 시작~완전히 밝아질 때까지) 입력 잠금. 끊겨도 Kill 이 done 을 부르며 풀어 준다
    private System.IDisposable _inputLock;

    /// <summary>지금 암전 중인가(Out~In 사이)</summary>
    public static bool Busy => instance != null && (instance._waiting || (instance._seq != null && instance._seq.IsActive()));

    /// <summary>어두워진 뒤 midAction 을 부르고, 잠깐 유지했다가 밝아진다. done 은 다 밝아진 뒤</summary>
    public static void Transition(System.Action midAction, System.Action done = null)
    {
        ScreenFader f = Ensure();
        f.Kill();
        f._black.gameObject.SetActive(true);
        f._pendingMid = midAction;
        f._pendingDone = done;
        f._inputLock = PlayerStatus.Acquire(StatusFlag.InputLock, "ScreenFader");
        f._seq = f._black.DOFade(1f, OutTime)
            .OnComplete(() =>
            {
                try
                {
                    f.RunPendingMid();
                }
                finally
                {
                    f.FadeIn(); // 이동이 예외를 던져도 화면이 검게 남지 않는다
                }
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
        f.FadeIn();
    }

    /// <summary>검은 화면 유지 → 밝아짐. 맵을 켜는 무거운 프레임이 지난 뒤(2프레임) 시작한다 — 그 프레임의 긴 unscaled 시간이 유지·페이드를 건너뛰던 문제(11-01 캡처)</summary>
    void FadeIn()
    {
        _waiting = true;
        StartCoroutine(FadeInAfterFrames());
    }

    void RunPendingMid()
    {
        System.Action mid = _pendingMid;
        _pendingMid = null;
        mid?.Invoke();
    }

    void RunPendingDone()
    {
        _inputLock?.Dispose();
        _inputLock = null;
        System.Action done = _pendingDone;
        _pendingDone = null;
        done?.Invoke();
    }

    System.Collections.IEnumerator FadeInAfterFrames()
    {
        yield return null;
        yield return null;
        _waiting = false;
        _seq = DOTween.Sequence()
            .AppendInterval(HoldTime)
            .Append(_black.DOFade(0f, InTime))
            .OnComplete(() => { _black.gameObject.SetActive(false); RunPendingDone(); })
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    /// <summary>진행 중 암전을 끊는다. 끊긴 Transition 의 이동·완료 콜백은 바로 부른다(웨이포인트가 포털 암전을 끊어도 이동·플래그 해제가 빠지지 않게)</summary>
    void Kill()
    {
        StopAllCoroutines();
        _waiting = false;
        _seq?.Kill();
        _black.DOKill();
        try
        {
            RunPendingMid();
        }
        finally
        {
            RunPendingDone();
        }
    }

    void OnDestroy()
    {
        _inputLock?.Dispose();
        _inputLock = null;
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
