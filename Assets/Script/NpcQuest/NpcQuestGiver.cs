using TMPro;
using UnityEngine;

/// <summary>
/// 퀘스트를 주는 NPC (머리 위 표시 + 상호작용). 노란 ! = 수락 가능, 노란 ? = 완료 가능, 회색 ! = 레벨 부족 (메이플식).
/// 상호작용은 NPC 기존 ↑ 처리(TalkNpc / 상점 UI)가 TryTalk 를 먼저 불러서 퀘스트 대화를 우선한다.
/// 수락 대화를 한 번 본 뒤 거절하면 그 NPC 곁에 머무는 동안은 다시 묻지 않는다(상점 NPC 가 막히지 않게).
/// </summary>
public class NpcQuestGiver : MonoBehaviour
{
    private const float MarkWorldHeight = 3.3f;
    private const float RefreshInterval = 0.5f;
    private static readonly Color Gold = new Color(1f, 0.85f, 0.2f);
    private static readonly Color Grey = new Color(0.62f, 0.62f, 0.62f);

    [SerializeField] private string _npcId;

    private TextMeshPro _mark;
    private bool _offerShown;
    private float _nextRefresh;

    public string NpcId => _npcId;

    public void Init(string npcId)
    {
        _npcId = npcId;
    }

    private void Awake()
    {
        var go = new GameObject("QuestMark");
        go.transform.SetParent(transform, false);
        float sy = Mathf.Abs(transform.lossyScale.y);
        go.transform.localPosition = new Vector3(0f, MarkWorldHeight / (sy > 0.01f ? sy : 1f), 0f);
        // NPC 를 좌우로 뒤집어도(scale.x < 0) 글자는 정상으로
        go.transform.localScale = new Vector3(Mathf.Sign(transform.lossyScale.x) < 0f ? -1f : 1f, 1f, 1f);
        _mark = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme != null && theme.pixelFont != null)
        {
            _mark.font = theme.pixelFont;
        }
        _mark.fontSize = 7f;
        _mark.alignment = TextAlignmentOptions.Center;
        _mark.rectTransform.sizeDelta = new Vector2(2f, 2f);
        _mark.outlineWidth = 0.3f;
        _mark.outlineColor = new Color32(0, 0, 0, 255);
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        _mark.sortingLayerID = sr != null ? sr.sortingLayerID : 0;
        _mark.sortingOrder = 501;
        go.SetActive(false);
    }

    private void OnEnable()
    {
        NpcQuestLog.Changed += RefreshMark;
        RefreshMark();
    }

    private void OnDisable()
    {
        NpcQuestLog.Changed -= RefreshMark;
    }

    private void Update()
    {
        // 수집 개수·플레이어 레벨처럼 이벤트 없이 바뀌는 값 때문에 가끔 다시 본다
        if (Time.unscaledTime >= _nextRefresh)
        {
            _nextRefresh = Time.unscaledTime + RefreshInterval;
            RefreshMark();
        }
    }

    /// <summary>현재 표시 상태 (테스트·프로브용): "", "!", "?", "!grey"</summary>
    public string MarkState { get; private set; } = "";

    private void RefreshMark()
    {
        if (_mark == null)
        {
            return;
        }
        NpcQuestStatus s = NpcQuestLog.BestFor(_npcId, out _);
        string state = "";
        switch (s)
        {
            case NpcQuestStatus.Available: state = "!"; break;
            case NpcQuestStatus.Ready: state = "?"; break;
            case NpcQuestStatus.LevelLow: state = "!grey"; break;
        }
        MarkState = state;
        bool show = state.Length > 0;
        if (_mark.gameObject.activeSelf != show)
        {
            _mark.gameObject.SetActive(show);
        }
        if (show)
        {
            _mark.text = state == "?" ? "?" : "!";
            _mark.color = state == "!grey" ? Grey : Gold;
        }
    }

    /// <summary>NPC 오브젝트에 퀘스트 대화를 시작한다. 시작했으면 true (호출한 쪽은 기존 상호작용을 건너뛴다)</summary>
    public static bool TryTalk(GameObject npc)
    {
        NpcQuestGiver g = npc != null ? npc.GetComponent<NpcQuestGiver>() : null;
        return g != null && g.Interact();
    }

    private bool Interact()
    {
        if (PixelCrushers.DialogueSystem.DialogueManager.isConversationActive)
        {
            return true; // 대화 중에 ↑ 로 선택지를 고르는 입력이 상점을 열지 않게 먹는다
        }
        NpcQuestLog.OnTalk(_npcId);
        NpcQuestStatus s = NpcQuestLog.BestFor(_npcId, out NpcQuestDef q);
        string conv;
        if (s == NpcQuestStatus.Ready)
        {
            conv = StoryDialogues.ReportConversation(q.Id);
        }
        else if (s == NpcQuestStatus.Available && !_offerShown)
        {
            conv = StoryDialogues.OfferConversation(q.Id);
            _offerShown = true;
        }
        else
        {
            return false;
        }
        return DialogueBoot.Talk(conv, transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _offerShown = false;
        }
    }
}
