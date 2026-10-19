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
    [SerializeField] private SpriteRenderer _questMark;

    const string SaveBoardKey = "qm.board", SaveJournalKey = "qm.journal";

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
        foreach (QuestCatalog.Def d in QuestCatalog.All())
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
        GameLog.Info(GameLog.Cat.Item, "퀘스트 게시판: 체인 " + QuestCatalog.Chain.Count + " + 부가 " + QuestCatalog.Side.Count);
    }

    Quest Build(QuestCatalog.Def d)
    {
        var qb = new QuestBuilder(d.title, d.id, d.title);
        qb.quest.isTrackable = true;
        qb.quest.showInTrackHUD = true;
        qb.AddOfferContents(qb.CreateTitleContent(), qb.CreateBodyContent(d.offer),
            qb.CreateBodyContent("Reward: " + d.gold + " gold" + (d.stones > 0 ? ", " + d.stones + " " + EnhanceRules.StoneDisplay + (d.stones > 1 ? "s" : "") : "")));

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

        var hunt = qb.AddConditionNode(qb.GetStartNode(), "hunt", d.title, ConditionCountMode.All);
        qb.AddCounterCondition(hunt, "kills", CounterValueConditionMode.AtLeast, d.count);
        qb.AddContents(hunt.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.Dialogue), qb.CreateBodyContent(d.journal));
        qb.AddContents(hunt.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.Journal), qb.CreateBodyContent(d.journal));
        qb.AddContents(hunt.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.HUD), qb.CreateBodyContent("{#kills}/" + d.count));

        // 게시판에 돌아와 보고
        var report = qb.AddDiscussQuestNode(hunt, QuestMessageParticipant.QuestGiver, giver.id);
        qb.AddContents(report.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.Dialogue), qb.CreateBodyContent("Well done! Here is your reward."));
        qb.AddContents(report.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.Journal), qb.CreateBodyContent("Report back to the quest board."));
        qb.AddContents(report.GetStateInfo(QuestNodeState.Active).GetContentList(QuestContentCategory.HUD), qb.CreateBodyContent("Report to the quest board"));
        report.GetStateInfo(QuestNodeState.Active).actionList.Add(qb.CreateAlertAction(d.title + " complete! Report to the quest board."));
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
        if (messageArgs.message == QuestMachineMessages.QuestStateChangedMessage && state == QuestState.Successful && !QuestMachine.isLoadingGame)
        {
            GrantReward(QuestCatalog.Find(questId));
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
        ES3.Save(SaveBoardKey, giver.RecordData());
        ES3.Save(SaveJournalKey, journal.RecordData());
    }

    void Load()
    {
        if (ES3.KeyExists(SaveBoardKey))
        {
            giver.ApplyData(ES3.Load<string>(SaveBoardKey));
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
        if (playerNear && upAction.triggered && DatabaseManager.isOpenUI == false)
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
