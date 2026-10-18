using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 타이틀 오버레이 — 게임 시작 시 "TO THE BEYOND" 로고 + 부제 + PRESS ANY KEY. 아무 키나 누르면 걷히고 조작이 풀린다.
/// 별도 씬 없이 현재 씬 위에 덮는다(세이브·로딩 흐름 불변). 세션당 1회, 에디터 프로브 테스트 중엔 TitleScreen.Skip 으로 건너뜀.
/// </summary>
public class TitleScreen : MonoBehaviour
{
    public static bool Skip; // 자동화 테스트가 true 로 두면 표시하지 않음
    static TitleScreen instance;

    CanvasGroup group;
    TextMeshProUGUI prompt;
    bool dismissed;
    float shownAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null || Skip)
        {
            return;
        }
        var go = new GameObject("[TitleScreen]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<TitleScreen>();
        instance.Build();
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        group = gameObject.AddComponent<CanvasGroup>();

        // 어두운 덮개 (게임 화면이 비치게 85%)
        var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        dim.transform.SetParent(transform, false);
        var dr = (RectTransform)dim.transform;
        dr.anchorMin = Vector2.zero;
        dr.anchorMax = Vector2.one;
        dr.offsetMin = dr.offsetMax = Vector2.zero;
        var dimImg = dim.GetComponent<Image>();
        dimImg.color = new Color(0.02f, 0.02f, 0.05f, 0.85f);
        dimImg.raycastTarget = false;

        // 로고 패널
        Image panel = theme.NewPanel("Panel", transform, false, 0.95f);
        var pr = panel.rectTransform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.56f);
        pr.sizeDelta = new Vector2(980f, 300f);

        var small = NewText("Small", pr, theme, 30f, new Vector2(0f, 95f), new Color(0.75f, 0.72f, 0.8f));
        small.text = "A TOWER ROSE FROM THE EARTH.  AN EXPLORER CLIMBS.";
        var title = NewText("Title", pr, theme, 104f, new Vector2(0f, 10f), theme.unique);
        title.characterSpacing = 14f;
        title.outlineWidth = 0.15f;
        title.gameObject.AddComponent<TextAnimator>().SetText("<wave a=0.15 f=0.6>TO THE BEYOND</wave>", false);
        var sub = NewText("Sub", pr, theme, 34f, new Vector2(0f, -80f), Color.white);
        sub.text = "FLOOR 1  ·  THE FOREST";

        prompt = NewText("Prompt", transform, theme, 34f, new Vector2(0f, -300f), new Color(1f, 0.92f, 0.7f));
        prompt.text = "PRESS ANY KEY";
        prompt.DOFade(0.25f, 0.7f).SetLoops(-1, LoopType.Yoyo).SetLink(prompt.gameObject);

        UiLock.Set("Title", true);
        shownAt = Time.unscaledTime;
        GameLog.Info(GameLog.Cat.Player, "타이틀 표시");
    }

    void Update()
    {
        if (dismissed || Time.unscaledTime - shownAt < 0.6f)
        {
            return;
        }
        bool any = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame))
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
        if (any)
        {
            Dismiss();
        }
    }

    public static void DismissNow()
    {
        if (instance != null)
        {
            instance.Dismiss();
        }
    }

    void Dismiss()
    {
        if (dismissed)
        {
            return;
        }
        dismissed = true;
        Sfx.Play(SfxId.UiOpen);
        group.DOFade(0f, 0.6f).SetUpdate(true).SetLink(gameObject).OnComplete(() =>
        {
            UiLock.Set("Title", false);
            gameObject.SetActive(false);
        });
    }

    static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size, Vector2 pos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(10, 5, 0, 230);
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1600f, size * 1.4f);
        rt.anchoredPosition = pos;
        return t;
    }
}
