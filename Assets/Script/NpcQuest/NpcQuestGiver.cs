using TMPro;
using UnityEngine;

/// <summary>
/// 퀘스트를 주는 NPC (머리 위 표시 + 상호작용). 노란 ! = 수락 가능, 노란 ? = 완료 가능, 회색 ! = 레벨 부족, 회색 ? = 진행 중 (메이플식).
/// 상호작용은 NPC ↑ → 선택지 창(NpcInteraction)에서 "퀘스트 이야기" 를 골랐을 때만 TalkQuest 가 대화를 연다(REWORK-8).
/// </summary>
public class NpcQuestGiver : MonoBehaviour
{
    private const float MarkWorldHeight = 3.3f;
    private const float RefreshInterval = 0.5f;
    private static readonly Color Gold = new Color(1f, 0.85f, 0.2f);
    private static readonly Color Grey = new Color(0.62f, 0.62f, 0.62f);

    [SerializeField] private string _npcId;

    private TextMeshPro _mark;
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

    /// <summary>현재 표시 상태 (테스트·프로브용): "", "!", "?", "!grey", "?grey"</summary>
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
            case NpcQuestStatus.Active: state = "?grey"; break; // 진행 중: 회색 ?
        }
        MarkState = state;
        bool show = state.Length > 0;
        if (_mark.gameObject.activeSelf != show)
        {
            _mark.gameObject.SetActive(show);
        }
        if (show)
        {
            _mark.text = state[0] == '?' ? "?" : "!";
            _mark.color = state.EndsWith("grey") ? Grey : Gold;
        }
    }

    /// <summary>선택지에 "퀘스트 이야기" 를 넣을지: 수락 가능·진행 중·완료 가능일 때</summary>
    public bool HasQuest
    {
        get
        {
            NpcQuestStatus s = NpcQuestLog.BestFor(_npcId, out _);
            return s == NpcQuestStatus.Available || s == NpcQuestStatus.Active || s == NpcQuestStatus.Ready;
        }
    }

    /// <summary>NPC 선택지에서 "퀘스트 이야기" 를 골랐을 때 퀘스트 대화를 시작한다 (REWORK-8: 더는 말만 걸어서 자동 시작하지 않는다)</summary>
    public bool TalkQuest()
    {
        if (PixelCrushers.DialogueSystem.DialogueManager.isConversationActive)
        {
            return true;
        }
        NpcQuestLog.OnTalk(_npcId);
        NpcQuestStatus s = NpcQuestLog.BestFor(_npcId, out NpcQuestDef q);
        string conv;
        if (s == NpcQuestStatus.Ready)
        {
            if (!NpcQuestLog.RewardCapacity(q))
            {
                if (_npcId == "merchant")
                {
                    PickupLog.Warn(NpcQuestRewards.ShopInventoryFullHint);
                    return true;
                }
                return DialogueBoot.Talk(StoryDialogues.InventoryFullConversation(q.Id), transform);
            }
            conv = StoryDialogues.ReportConversation(q.Id);
        }
        else if (s == NpcQuestStatus.Available)
        {
            conv = StoryDialogues.OfferConversation(q.Id);
        }
        else if (s == NpcQuestStatus.Active)
        {
            conv = StoryDialogues.ProgressConversation(q.Id);
        }
        else
        {
            return false;
        }
        return DialogueBoot.Talk(conv, transform);
    }
}
