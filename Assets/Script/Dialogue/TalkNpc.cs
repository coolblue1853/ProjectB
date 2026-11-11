using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 말 걸 수 있는 NPC: 가까이 가면 머리 위에 "UP  TALK" 표시, 위 방향키로 대화(퀘스트 게시판과 같은 조작).
/// 대화 중에는 플레이어 쪽을 바라본다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TalkNpc : MonoBehaviour
{
    [SerializeField] private string _conversation = StoryDialogues.Elder;
    [SerializeField] private float _promptHeight = 1.9f;
    [Tooltip("대사창 초상 (대화 상대 Actor 이름과 함께)")]
    [SerializeField] private Sprite _portrait;
    [SerializeField] private string _actorName;

    /// <summary>U31: 말 걸기 범위 최소 폭(월드 단위). 씬에 박힌 NPC 도 런타임 배치 NPC(2.2)와 같게 맞춘다. 넓은 것은 줄이지 않는다.</summary>
    public const float MinTalkWidth = 2.2f;

    KeyAction action;
    TextMeshPro prompt;
    bool _promptPad;
    bool playerNear;
    public bool HasServiceLabel { get; set; }

    public string Conversation
    {
        get => _conversation;
        set => _conversation = value;
    }

    /// <summary>런타임으로 만든 NPC 용 설정 (대화 이름·프롬프트 높이). Awake 전이면 Awake 가 이 높이를 쓴다</summary>
    public void Configure(string conversation, float promptHeight)
    {
        _conversation = conversation;
        _promptHeight = promptHeight;
        if (prompt != null)
        {
            prompt.transform.localPosition = new Vector3(0f, _promptHeight, 0f);
        }
    }

    void Awake()
    {
        action = new KeyAction();
        EnsureTalkRange();
        var go = new GameObject("TalkPrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, _promptHeight, 0f);
        prompt = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            prompt.font = theme.pixelFont;
        }
        RefreshPromptText();
        prompt.fontSize = 3f;
        prompt.alignment = TextAlignmentOptions.Center;
        prompt.color = theme.unique;
        prompt.rectTransform.sizeDelta = new Vector2(6f, 1f);
        var sr = GetComponentInChildren<SpriteRenderer>();
        prompt.sortingLayerID = sr != null ? sr.sortingLayerID : 0;
        prompt.sortingOrder = 500;
        go.SetActive(false);
    }

    void EnsureTalkRange()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        float sx = Mathf.Abs(transform.lossyScale.x);
        if (box == null || !box.isTrigger || sx < 0.0001f)
        {
            return;
        }
        float minLocal = MinTalkWidth / sx;
        if (box.size.x < minLocal)
        {
            box.size = new Vector2(minLocal, box.size.y);
        }
    }

    void OnEnable()
    {
        action.UI.Up.Enable();
    }

    void OnDisable()
    {
        action.UI.Up.Disable();
    }

    /// <summary>입력 장치에 맞춰 프롬프트 문구를 바꾼다(키보드 ↑, 패드 십자키 ↑). 바뀔 때만 문자열을 만든다.</summary>
    void RefreshPromptText()
    {
        bool pad = InputDeviceHint.UsingGamepad;
        if (prompt == null || (_promptPad == pad && !string.IsNullOrEmpty(prompt.text)))
        {
            return;
        }

        _promptPad = pad;
        prompt.text = (pad ? "D-PAD " : "") + Loc.T("UP  TALK"); // 번역은 기존 키를 재사용(패드는 접두만)
    }

    void OnDestroy()
    {
        action?.Dispose();
    }

    void Update()
    {
        bool canTalk = playerNear && !DatabaseManager.isOpenUI && !NpcMenu.IsOpen && !PixelCrushers.DialogueSystem.DialogueManager.isConversationActive;
        bool showPrompt = canTalk && !HasServiceLabel;
        if (prompt.gameObject.activeSelf != showPrompt)
        {
            prompt.gameObject.SetActive(showPrompt);
        }
        if (showPrompt)
        {
            RefreshPromptText();
        }
        bool mine = InteractArbiter.Contend(gameObject, transform.position, canTalk);
        if (mine && action.UI.Up.triggered)
        {
            FacePlayer();
            NpcInteraction.Run(gameObject, null, null, null, TalkConversation);
        }
    }

    void TalkConversation()
    {
        if (_portrait != null && !string.IsNullOrEmpty(_actorName))
        {
            StoryDialogues.SetPortrait(_actorName, _portrait);
        }
        DialogueBoot.Talk(_conversation, transform);
    }

    void FacePlayer()
    {
        if (PlayerController.instance == null)
        {
            return;
        }
        float dx = PlayerController.instance.transform.position.x - transform.position.x;
        Vector3 s = transform.localScale;
        // 마을 NPC 도트는 정면을 본다 → 왼쪽(-) / 오른쪽(+) 으로만 뒤집는다(오른쪽이 기본)
        s.x = Mathf.Abs(s.x) * (dx >= 0f ? 1f : -1f);
        transform.localScale = s;
        // 프롬프트 글자는 뒤집히지 않게
        Vector3 ps = prompt.transform.localScale;
        ps.x = Mathf.Abs(ps.x) * Mathf.Sign(s.x);
        prompt.transform.localScale = ps;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
}
