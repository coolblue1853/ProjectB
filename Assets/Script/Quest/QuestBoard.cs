using PixelCrushers.QuestMachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마을 의뢰 게시판 입구 (REWORK-5 P1). 게시판 퀘스트(토끼 소동~심연의 마법사·땔감·치유사·대장장이 제자)는
/// QuestsFloor1.csv(giver=questboard)로 옮겨졌고 NPC 퀘스트와 같은 대화·진행·보상 규칙을 쓴다.
/// 이 컴포넌트는 범위 안에서 ↑(키보드 ↑ / 패드 D패드 위 — UI Up 액션)를 누르면 NpcQuestGiver("questboard") 대화를 여는 입구이고,
/// 로드 때 구 Quest Machine 저장(qm.journal)을 한 번 npcq.state 로 이행한다(QuestMigration).
/// Quest Machine 퀘스트 생성·HUD·보상은 더 하지 않는다.
/// </summary>
[RequireComponent(typeof(QuestGiver))]
public class QuestBoard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _questMark;
    [Tooltip("범위 안에서 ↑ 로 연다")]
    [SerializeField] private bool _useUpKey = true;

    public const string BoardNpcId = "questboard";
    private const string SaveJournalKey = QuestMigration.LegacyJournalKey;
    private const int MaxWaitFrames = 900;

    private KeyAction _action;
    private InputAction _upAction;
    private bool _playerNear;

    private void Awake()
    {
        _action = new KeyAction();
        _upAction = _action.UI.Up;
    }

    private void OnEnable()
    {
        _upAction.Enable();
    }

    private void OnDisable()
    {
        _upAction.Disable();
    }

    private void OnDestroy()
    {
        _action?.Dispose();
    }

    private System.Collections.IEnumerator Start()
    {
        if (_questMark != null)
        {
            _questMark.enabled = false; // 머리 위 !/? 는 NpcQuestGiver 가 같은 규칙으로 그린다
        }
        yield return null; // QuestGiver·QuestJournal 의 Start 가 끝난 뒤
        if (!QuestMigration.Needed())
        {
            yield break;
        }
        QuestJournal journal = PlayerController.instance != null ? PlayerController.instance.GetComponent<QuestJournal>() : null;
        if (journal == null)
        {
            yield break; // 다음 시작에 재시도
        }
        if (ES3.KeyExists(SaveJournalKey))
        {
            journal.ApplyData(ES3.Load<string>(SaveJournalKey)); // 구 저장을 읽기만 한다(이후 디스크에는 쓰지 않는다)
        }
        // npcq.state 를 디스크에서 읽은 뒤에 넣어야 덮어써지지 않는다
        int waited = 0;
        while (!NpcQuestLog.Loaded && waited++ < MaxWaitFrames)
        {
            yield return null;
        }
        if (NpcQuestLog.Loaded)
        {
            QuestMigration.RunOnce(journal);
        }
    }

    private void Update()
    {
        if (!_useUpKey || !_playerNear || !_upAction.triggered || DatabaseManager.isOpenUI)
        {
            return;
        }
        NpcQuestGiver giver = GetComponent<NpcQuestGiver>();
        if (giver != null && giver.HasQuest)
        {
            giver.TalkQuest();
        }
        else
        {
            PickupLog.Notice("지금 받을 수 있는 의뢰가 없습니다.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _playerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _playerNear = false;
        }
    }
}
