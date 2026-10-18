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

    KeyAction action;
    TextMeshPro prompt;
    bool playerNear;

    public string Conversation
    {
        get => _conversation;
        set => _conversation = value;
    }

    void Awake()
    {
        action = new KeyAction();
        var go = new GameObject("TalkPrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, _promptHeight, 0f);
        prompt = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            prompt.font = theme.pixelFont;
        }
        prompt.text = "UP  TALK";
        prompt.fontSize = 3f;
        prompt.alignment = TextAlignmentOptions.Center;
        prompt.color = theme.unique;
        prompt.rectTransform.sizeDelta = new Vector2(6f, 1f);
        var sr = GetComponentInChildren<SpriteRenderer>();
        prompt.sortingLayerID = sr != null ? sr.sortingLayerID : 0;
        prompt.sortingOrder = 500;
        go.SetActive(false);
    }

    void OnEnable()
    {
        action.UI.Up.Enable();
    }

    void OnDisable()
    {
        action.UI.Up.Disable();
    }

    void OnDestroy()
    {
        action?.Dispose();
    }

    void Update()
    {
        bool canTalk = playerNear && !DatabaseManager.isOpenUI && !PixelCrushers.DialogueSystem.DialogueManager.isConversationActive;
        if (prompt.gameObject.activeSelf != canTalk)
        {
            prompt.gameObject.SetActive(canTalk);
        }
        if (canTalk && action.UI.Up.triggered)
        {
            FacePlayer();
            if (_portrait != null && !string.IsNullOrEmpty(_actorName))
            {
                StoryDialogues.SetPortrait(_actorName, _portrait);
            }
            DialogueBoot.Talk(_conversation, transform);
        }
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
