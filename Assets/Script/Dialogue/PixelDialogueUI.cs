using System;
using System.Collections.Generic;
using Febucci.UI;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Dialogue System 용 코드 생성 대사창 (AssetIntegrationPlan W1). 공용 픽셀 프레임 + Text Animator 타자기.
/// 선택(Select/Enter): 타이핑 중이면 끝까지 표시, 다 나왔으면 다음 줄. 선택지는 위/아래로 고른다.
/// 열린 동안 UiLock "Dialogue" 로 플레이어 조작을 막는다.
/// </summary>
public class PixelDialogueUI : MonoBehaviour, IDialogueUI
{
    const int MaxResponses = 4;

    public event EventHandler<SelectedResponseEventArgs> SelectedResponseHandler;

    KeyAction action;
    InputAction up, down, select;
    GameObject root, responsePanel;
    Image portrait;
    TextMeshProUGUI nameText, bodyText, continueMark, alertText;
    TextAnimatorPlayer typewriter;
    readonly List<(Image bg, TextMeshProUGUI text)> responseRows = new List<(Image, TextMeshProUGUI)>();
    Response[] responses;
    int cursor;
    bool open, typing, waitingContinue;
    float shownAt, alertUntil, savedMusicVolume = -1f;
    const float DuckRatio = 0.45f; // 대사 중 BGM 덕킹 비율

    public bool IsOpen => open;

    void Awake()
    {
        action = new KeyAction();
        up = action.UI.UPInventory;
        down = action.UI.DownInventory;
        select = action.UI.Select;
        Build();
    }

    void OnDestroy()
    {
        action?.Dispose();
    }

    public void Open()
    {
        open = true;
        root.SetActive(true);
        responsePanel.SetActive(false);
        action.UI.Enable();
        UiLock.Set("Dialogue", true);
        Sfx.Play(SfxId.UiOpen, 0.6f);
        if (DarkTonic.MasterAudio.MasterAudio.SafeInstance != null && savedMusicVolume < 0f)
        {
            savedMusicVolume = DarkTonic.MasterAudio.MasterAudio.PlaylistMasterVolume;
            DarkTonic.MasterAudio.MasterAudio.PlaylistMasterVolume = savedMusicVolume * DuckRatio;
        }
    }

    public void Close()
    {
        open = false;
        root.SetActive(false);
        action.UI.Disable();
        UiLock.Set("Dialogue", false);
        if (savedMusicVolume >= 0f && DarkTonic.MasterAudio.MasterAudio.SafeInstance != null)
        {
            DarkTonic.MasterAudio.MasterAudio.PlaylistMasterVolume = savedMusicVolume;
        }
        savedMusicVolume = -1f;
    }

    public void ShowSubtitle(Subtitle subtitle)
    {
        string text = subtitle.formattedText != null ? subtitle.formattedText.text : "";
        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        PixelCrushers.DialogueSystem.CharacterInfo who = subtitle.speakerInfo;
        nameText.text = who != null ? who.Name : "";
        nameText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(nameText.text));
        Sprite face = who != null ? who.portrait : null;
        portrait.sprite = face;
        portrait.transform.parent.gameObject.SetActive(face != null);
        bodyText.rectTransform.offsetMin = new Vector2(face != null ? 250f : 40f, bodyText.rectTransform.offsetMin.y);
        typing = true;
        waitingContinue = true;
        shownAt = Time.unscaledTime;
        continueMark.gameObject.SetActive(false);
        typewriter.ShowText(text);
    }

    public void HideSubtitle(Subtitle subtitle)
    {
        waitingContinue = false;
        continueMark.gameObject.SetActive(false);
    }

    public void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
    {
        this.responses = responses;
        cursor = 0;
        responsePanel.SetActive(true);
        waitingContinue = false;
        if (subtitle != null && subtitle.formattedText != null && !string.IsNullOrEmpty(subtitle.formattedText.text))
        {
            // 선택지 직전 대사를 화면에 남겨 둔다(타자기 없이)
            typewriter.ShowText(subtitle.formattedText.text);
            typewriter.SkipTypewriter();
        }
        var rp = (RectTransform)responsePanel.transform;
        int n = Mathf.Min(responses.Length, MaxResponses);
        rp.sizeDelta = new Vector2(rp.sizeDelta.x, 30f + n * 52f);
        shownAt = Time.unscaledTime;
        RedrawResponses();
    }

    public void HideResponses()
    {
        responsePanel.SetActive(false);
        responses = null;
    }

    public void ShowQTEIndicator(int index) { }

    public void HideQTEIndicator(int index) { }

    public void ShowAlert(string message, float duration)
    {
        PickupLog.Report(message, 1, "R");
    }

    public void HideAlert() { }

    void Update()
    {
        if (!open || Time.unscaledTime - shownAt < 0.12f)
        {
            return;
        }
        Keyboard kb = Keyboard.current;
        bool confirm = select.triggered || (kb != null && (kb.enterKey.wasPressedThisFrame || kb.zKey.wasPressedThisFrame));
        if (responses != null)
        {
            UpdateResponses(confirm);
            return;
        }
        if (!waitingContinue || !confirm)
        {
            return;
        }
        if (typing)
        {
            typewriter.SkipTypewriter();
            return;
        }
        waitingContinue = false;
        Sfx.Play(SfxId.UiMove, 0.5f);
        DialogueManager.instance.conversationView?.HandleContinueButtonClick();
    }

    void UpdateResponses(bool confirm)
    {
        int n = Mathf.Min(responses.Length, MaxResponses);
        if (up.triggered)
        {
            cursor = (cursor - 1 + n) % n;
            Sfx.Play(SfxId.UiMove, 0.6f);
            RedrawResponses();
        }
        else if (down.triggered)
        {
            cursor = (cursor + 1) % n;
            Sfx.Play(SfxId.UiMove, 0.6f);
            RedrawResponses();
        }
        else if (confirm)
        {
            Response r = responses[cursor];
            Sfx.Play(SfxId.UiOpen, 0.5f);
            HideResponses();
            SelectedResponseHandler?.Invoke(this, new SelectedResponseEventArgs(r));
        }
    }

    void RedrawResponses()
    {
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < responseRows.Count; i++)
        {
            bool has = responses != null && i < responses.Length && i < MaxResponses;
            responseRows[i].bg.gameObject.SetActive(has);
            if (!has)
            {
                continue;
            }
            bool on = i == cursor;
            responseRows[i].bg.color = on ? Color.white : new Color(1f, 1f, 1f, 0f);
            responseRows[i].text.color = on ? theme.unique : Color.white;
            responseRows[i].text.text = (on ? "> " : "  ") + responses[i].formattedText.text;
        }
    }

    void OnTextShowed()
    {
        typing = false;
        if (waitingContinue)
        {
            continueMark.gameObject.SetActive(true);
        }
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        root = new GameObject("Root", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        Stretch((RectTransform)root.transform);

        // 대사 상자 (화면 아래 가운데)
        Image box = theme.NewPanel("Box", root.transform);
        var br = box.rectTransform;
        br.anchorMin = br.anchorMax = br.pivot = new Vector2(0.5f, 0f);
        br.anchoredPosition = new Vector2(0f, 40f);
        br.sizeDelta = new Vector2(1500f, 250f);

        // 초상 (왼쪽)
        Image faceFrame = theme.NewPanel("FaceFrame", br, true);
        Place(faceFrame.rectTransform, new Vector2(24f, -25f), new Vector2(200f, 200f));
        var faceGo = new GameObject("Face", typeof(RectTransform), typeof(Image));
        faceGo.transform.SetParent(faceFrame.transform, false);
        portrait = faceGo.GetComponent<Image>();
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        var fr = portrait.rectTransform;
        fr.anchorMin = Vector2.zero;
        fr.anchorMax = Vector2.one;
        fr.offsetMin = new Vector2(12f, 12f);
        fr.offsetMax = new Vector2(-12f, -12f);

        // 이름표 (상자 위쪽 왼편)
        Image tag = theme.NewPanel("NameTag", br, true);
        var tr = tag.rectTransform;
        tr.anchorMin = tr.anchorMax = tr.pivot = new Vector2(0f, 1f);
        tr.anchoredPosition = new Vector2(250f, 30f);
        tr.sizeDelta = new Vector2(320f, 56f);
        nameText = NewText("Name", tag.transform, theme, 30f);
        Stretch(nameText.rectTransform);
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = theme.unique;

        // 본문
        bodyText = NewText("Body", br, theme, 32f);
        var bt = bodyText.rectTransform;
        bt.anchorMin = Vector2.zero;
        bt.anchorMax = Vector2.one;
        bt.offsetMin = new Vector2(250f, 30f);
        bt.offsetMax = new Vector2(-40f, -40f);
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.enableWordWrapping = true;
        bodyText.lineSpacing = 12f;
        bodyText.gameObject.AddComponent<TextAnimator>();
        typewriter = bodyText.gameObject.AddComponent<TextAnimatorPlayer>();
        typewriter.onTextShowed = new UnityEngine.Events.UnityEvent();
        typewriter.onTextShowed.AddListener(OnTextShowed);

        continueMark = NewText("Continue", br, theme, 28f);
        var cr = continueMark.rectTransform;
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(1f, 0f);
        cr.anchoredPosition = new Vector2(-28f, 18f);
        cr.sizeDelta = new Vector2(60f, 36f);
        continueMark.text = "<wave>v</wave>";
        continueMark.gameObject.AddComponent<TextAnimator>();
        continueMark.color = theme.unique;
        continueMark.gameObject.SetActive(false);

        // 선택지 (상자 위 오른쪽)
        Image rp = theme.NewPanel("Responses", root.transform);
        var rr = rp.rectTransform;
        rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0.5f, 0f);
        rr.anchoredPosition = new Vector2(430f, 310f);
        rr.sizeDelta = new Vector2(620f, 240f);
        responsePanel = rp.gameObject;
        for (int i = 0; i < MaxResponses; i++)
        {
            Image row = theme.NewPanel("Row" + i, rr, true, 0f);
            var rowRt = row.rectTransform;
            rowRt.anchorMin = rowRt.anchorMax = rowRt.pivot = new Vector2(0f, 1f);
            rowRt.anchoredPosition = new Vector2(16f, -16f - i * 52f);
            rowRt.sizeDelta = new Vector2(588f, 48f);
            var text = NewText("Text", row.transform, theme, 28f);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(14f, 0f);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            responseRows.Add((row, text));
        }
        responsePanel.SetActive(false);
        root.SetActive(false);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        return t;
    }
}
