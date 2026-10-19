using PixelCrushers;
using PixelCrushers.QuestMachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마을 퀘스트 게시판 (G11, Quest Machine). QuestCatalog 체인을 QuestBuilder로 코드 생성해 QuestGiver에 등록하고,
/// 플레이어가 범위 안에서 ↑를 누르면 Quest Machine 대화 UI를 연다. 퀘스트 성공 시 골드·강화석·아이템을 지급한다.
/// 선행 퀘스트는 QuestStateQuestCondition(offer 조건)으로 건다 — 완료 전에는 게시판에 뜨지 않는다.
/// </summary>
[RequireComponent(typeof(QuestGiver))]
public class QuestBoard : MonoBehaviour, IMessageHandler
{
    /// <summary>퀘스트 출처: 게시판(체인·부가) / 촌장(스토리, 대화로 열림)</summary>
    public enum Source { Board, Elder }

    [SerializeField] private SpriteRenderer _questMark;
    [SerializeField] private Source _source = Source.Board;
    [Tooltip("범위 안에서 ↑ 로 연다(게시판). 촌장은 Dialogue System 대화 선택지로 연다")]
    [SerializeField] private bool _useUpKey = true;

    const string SaveBoardKey = "qm.board", SaveElderKey = "qm.elder", SaveJournalKey = "qm.journal";

    /// <summary>촌장 퀘스트 제공자 (대화 선택지 → DialogueBoot.OpenElderQuests)</summary>
    public static QuestBoard ElderBoard { get; private set; }

    string GiverKey => _source == Source.Board ? SaveBoardKey : SaveElderKey;
    System.Collections.Generic.IEnumerable<QuestCatalog.Def> Catalog => _source == Source.Board ? QuestCatalog.All() : QuestCatalog.Story;
    string ReportTo => _source == Source.Board ? "the quest board" : "the Village Elder";

    KeyAction action;
    InputAction upAction;
    QuestGiver giver;
    QuestJournal journal;
    bool playerNear, lockedUi;

    void Awake()
    {
        action = new KeyAction();
        upAction = action.UI.Up;
        giver = GetComponent<QuestGiver>();
    }

    void OnEnable()
    {
        upAction.Enable();
        MessageSystem.AddListener(this, QuestMachineMessages.QuestStateChangedMessage, string.Empty);
        MessageSystem.AddListener(this, QuestMachineMessages.QuestCounterChangedMessage, string.Empty);
    }

    void OnDisable()
    {
        upAction.Disable();
        MessageSystem.RemoveListener(this);
    }

    void OnDestroy()
    {
        action?.Dispose();
    }

    System.Collections.IEnumerator Start()
    {
        // QuestGiver.Start 가 자기 목록을 초기화하므로 한 프레임 뒤에 등록한다
        yield return null;
        journal = PlayerController.instance != null ? PlayerController.instance.GetComponent<QuestJournal>() : null;
        giver.includeInSavedGameData = true;
        if (journal != null)
        {
            journal.includeInSavedGameData = true;
        }
        Load();
        foreach (QuestCatalog.Def d in Catalog)
        {
            // 저장본에 없는 퀘스트(새로 추가된 것 포함)만 생성 — 플레이어가 이미 받은 퀘스트는 게시판 목록에서 빠져 있다
            bool inJournal = journal != null && journal.ContainsQuest(d.id);
            if (giver.ContainsQuest(d.id) || inJournal)
            {
                continue;
            }
            giver.AddQuest(Build(d));
        }
        RefreshMark();
        GameLog.Info(GameLog.Cat.Item, "퀘스트 " + _source + ": " + giver.questList.Count + "개 제공 대기");
    }

    Quest Build(QuestCatalog.Def d)
    {
        string title = Loc.T(d.title);
        var qb = new QuestBuilder(title, d.id, title);
        qb.quest.isTrackable = true;
        qb.quest.showInTrackHUD = true;
        qb.AddOfferContents(qb.CreateTitleContent(), qb.CreateBodyContent(Loc.T(d.offer)),
            qb.CreateBodyContent(Loc.T("Reward: ") + d.gold + " " + Loc.T("gold") + (d.stones > 0 ? ", " + d.stones + " " + EnhanceRules.StoneDisplay + (d.stones > 1 && !Loc.IsKorean ? "s" : "") : "")));

        if (!string.IsNullOrEmpty(d.requires))
        {
            var cond = ScriptableObject.CreateInstance<QuestStateQuestCondition>();
            cond.requiredQuestID = new StringField(d.requires);
            cond.requiredState = QuestState.Successful;
            qb.quest.offerConditionSet.conditionList.Add(cond);
        }

        var stateDialogue = qb.quest.GetStateInfo(QuestState.Active).GetContentList(QuestContentCategory.Dialogue);
        qb.AddContents(stateDialogue, qb.CreateTitleContent());
        qb.AddContents(qb.quest.GetStateInfo(QuestState.Active).GetContentList(QuestContentCategory.Journal), qb.CreateTitleContent());
        qb.AddContents(qb.quest.GetStateInfo(QuestState.Active).GetContentList(QuestContentCategory.HUD), qb.CreateTitleContent());
        qb.AddContents(qb.quest.GetStateInfo(QuestState.Successful).GetContentList(QuestContentCategory.Journal), qb.CreateTitleContent());

        // 처치 카운터: "Kill" 메시지의 파라미터가 대상 몬스터 이름이면 +1
        var counter = qb.AddCounter("kills", 0, 0, d.count, false, QuestCounterUpdateMode.Messages);
        foreach (string target in d.targets)
        {
            counter.messageEventList.Add(new QuestCounterMessageEvent(null, new StringField(d.message), new StringField(target),
                QuestCounterMessageEvent.Operation.ModifyByLiteralValue, 1));
        }

        var hunt = qb.AddConditionNode(qb.GetStartNode(), "hunt", title, ConditionCountMode.All);
        qb.AddCounterCondition(hunt, "kills", CounterValueConditionMode.AtLeast, d.count);
        qb.AddContents(hunt.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.Dialogue), qb.CreateBodyContent(Loc.T(d.journal)));
        qb.AddContents(hunt.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.Journal), qb.CreateBodyContent(Loc.T(d.journal)));
        qb.AddContents(hunt.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.HUD), qb.CreateBodyContent("{#kills}/" + d.count));

        // 게시판에 돌아와 보고
        var report = qb.AddDiscussQuestNode(hunt, QuestMessageParticipant.QuestGiver, giver.id);
        qb.AddContents(report.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.Dialogue), qb.CreateBodyContent(Loc.T(d.report ?? "Well done! Here is your reward.")));
        qb.AddContents(report.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.Journal), qb.CreateBodyContent(Loc.T("Report back to " + ReportTo + ".")));
        qb.AddContents(report.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.HUD), qb.CreateBodyContent(Loc.T("Report to " + ReportTo)));
        report.GetStateInfo(QuestNodeState.Active).actionList.Add(qb.CreateAlertAction(title + Loc.T(" complete! Report to " + ReportTo + ".")));
        qb.AddSuccessNode(report);
        return qb.ToQuest();
    }

    public void OnMessage(MessageArgs messageArgs)
    {
        if (messageArgs.message == QuestMachineMessages.QuestCounterChangedMessage)
        {
            Save(); // 처치 카운터 진행 저장
            return;
        }
        // values = [노드 id(퀘스트 자체면 null), 상태]
        if (messageArgs.values == null || messageArgs.values.Length < 2 || messageArgs.values[0] != null)
        {
            return;
        }
        if (!(messageArgs.values[1] is QuestState state))
        {
            return;
        }
        string questId = messageArgs.parameter;
        if (messageArgs.message == QuestMachineMessages.QuestStateChangedMessage && state == QuestState.Successful && !QuestMachine.isLoadingGame && Owns(questId))
        {
            GrantReward(QuestCatalog.Find(questId)); // 게시판·촌장이 같은 메시지를 받으므로 자기 출처 퀘스트만 보상(중복 지급 방지)
        }
        RefreshMark();
        Save();
    }

    /// <summary>퀘스트 진행을 ES3에 저장 (상태·카운터가 바뀔 때마다 — 데이터가 작아 비용 무시 가능)</summary>
    void Save()
    {
        if (QuestMachine.isLoadingGame || journal == null)
        {
            return;
        }
        ES3.Save(GiverKey, giver.RecordData());
        ES3.Save(SaveJournalKey, journal.RecordData());
    }

    void Load()
    {
        if (ES3.KeyExists(GiverKey))
        {
            giver.ApplyData(ES3.Load<string>(GiverKey));
        }
        if (journal != null && ES3.KeyExists(SaveJournalKey))
        {
            journal.ApplyData(ES3.Load<string>(SaveJournalKey));
        }
        GameLog.Info(GameLog.Cat.Save, "퀘스트 로드: 게시판 " + giver.questList.Count + " / 저널 " + (journal != null ? journal.questList.Count : 0));
    }

    void GrantReward(QuestCatalog.Def d)
    {
        if (d == null)
        {
            return;
        }
        DatabaseManager.money += d.gold;
        if (d.gold > 0 && PlayerController.instance != null)
        {
            HitFeedback.SpawnText(null, PlayerController.instance.transform.position + Vector3.up, "+" + d.gold + " G", HitFeedback.TextKind.Gold);
        }
        for (int i = 0; i < d.stones; i++)
        {
            InventoryManager.instance.CreatItem(EnhanceRules.StoneName);
        }
        ArtifactSystem.Unlock(d.artifact);
        if (!string.IsNullOrEmpty(d.rewardItem))
        {
            InventoryManager.instance.CreatItem(d.rewardItem);
        }
        InventoryManager.instance.RebuildItemStackCache();
        InventoryManager.instance.SaveInventory();
        if (d.stones > 0)
        {
            PickupLog.Report(EnhanceRules.StoneName, d.stones, "R");
        }
        Sfx.Play(SfxId.Victory, 0.6f);
        GameLog.Info(GameLog.Cat.Item, "퀘스트 보상 " + d.id + ": 골드 " + d.gold + ", 강화석 " + d.stones);
    }

    bool Owns(string questId)
    {
        foreach (QuestCatalog.Def d in Catalog)
        {
            if (d.id == questId)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 이 제공자가 준 진행 중 퀘스트의 '보고' 노드를 완료시킨다: 수행자(플레이어 저널) → 제공자 "Discussed Quest" 메시지.
    /// 촌장은 Dialogue System 대화를 마치면 보고로 친다(퀘스트 창을 거치지 않아도 되게 — 창은 활성 퀘스트가 하나일 때만 이 메시지를 보낸다).
    /// </summary>
    public int ReportActiveQuests()
    {
        if (giver == null || journal == null)
        {
            return 0;
        }
        int n = 0;
        foreach (Quest q in journal.questList.ToArray())
        {
            if (q != null && q.GetState() == QuestState.Active && StringField.Equals(q.questGiverID, giver.id))
            {
                QuestMachineMessages.DiscussedQuest(journal.gameObject, giver, giver.id, q.id);
                n++;
            }
        }
        return n;
    }

    /// <summary>Quest Machine 퀘스트 대화창을 연다(촌장: 대화 선택지 뒤)</summary>
    public void OpenDialogue()
    {
        if (giver != null)
        {
            giver.StartDialogueWithPlayer();
        }
    }

    /// <summary>촌장(TalkNpc 대화 = Village/Elder)에 퀘스트 제공자를 런타임으로 붙인다. 씬 저장 없음</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AttachElder()
    {
        ElderBoard = null;
        foreach (TalkNpc npc in FindObjectsByType<TalkNpc>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (npc.Conversation != StoryDialogues.Elder || npc.GetComponent<QuestGiver>() != null)
            {
                continue;
            }
            var g = npc.gameObject.AddComponent<QuestGiver>();
            g.id = new StringField(npc.gameObject.name); // 메시지 발신자 식별이 오브젝트 이름 기준이라 ID 를 같게(게시판도 QuestBoard=QuestBoard)
            g.displayName = new StringField("Village Elder");
            var board = npc.gameObject.AddComponent<QuestBoard>();
            board._source = Source.Elder;
            board._useUpKey = false;
            ElderBoard = board;
            GameLog.Info(GameLog.Cat.Item, "촌장 퀘스트 제공자 부착: " + npc.name);
            return;
        }
    }

    /// <summary>받을 수 있는 퀘스트가 있으면 게시판 위에 ! 표시</summary>
    void RefreshMark()
    {
        if (_questMark == null || giver == null)
        {
            return;
        }
        bool offerable = false;
        foreach (Quest q in giver.questList)
        {
            if (q != null && q.GetState() == QuestState.WaitingToStart && q.canOffer)
            {
                offerable = true;
                break;
            }
        }
        _questMark.enabled = offerable;
    }

    void Update()
    {
        // 대화창이 열린 동안 플레이어 조작 잠금 (기존 UI 와 같은 isOpenUI 플래그)
        bool dialogueOpen = QuestMachine.defaultQuestDialogueUI != null && QuestMachine.defaultQuestDialogueUI.isVisible;
        if (dialogueOpen != lockedUi)
        {
            lockedUi = dialogueOpen;
            UiLock.Set("QuestDialog", dialogueOpen);
        }
        if (_useUpKey && playerNear && upAction.triggered && DatabaseManager.isOpenUI == false)
        {
            giver.StartDialogueWithPlayer();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = true;
            RefreshMark();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
}
