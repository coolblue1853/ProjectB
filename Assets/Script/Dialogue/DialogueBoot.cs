using PixelCrushers.DialogueSystem;
using UnityEngine;

/// <summary>
/// Dialogue System 런타임 구성 (AssetIntegrationPlan W1): 컨트롤러 + 코드 생성 DB + 픽셀 대사창.
/// 씬에 Dialogue Manager 프리팹을 두지 않는다. 첫 실행 때 타이틀이 닫히면 오프닝 독백을 한 번 재생한다(ES3 "story.opening").
/// </summary>
public class DialogueBoot : MonoBehaviour
{
    const string OpeningKey = "story.opening";
    static DialogueBoot instance;
    bool openingDone;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null || FindFirstObjectByType<DialogueSystemController>() != null)
        {
            return;
        }
        var go = new GameObject("[Dialogue]");
        go.SetActive(false); // 필드를 채운 뒤 Awake 가 돌도록
        DontDestroyOnLoad(go);

        var uiGo = new GameObject("PixelDialogueUI");
        uiGo.transform.SetParent(go.transform, false);
        uiGo.AddComponent<PixelDialogueUI>();

        var ctrl = go.AddComponent<DialogueSystemController>();
        ctrl.initialDatabase = StoryDialogues.Build();
        ctrl.dontDestroyOnLoad = false; // 루트에서 직접 DontDestroyOnLoad
        ctrl.displaySettings.dialogueUI = uiGo;
        ctrl.displaySettings.subtitleSettings.continueButton = DisplaySettings.SubtitleSettings.ContinueButtonMode.Always;
        ctrl.displaySettings.subtitleSettings.showPCSubtitlesDuringLine = false; // 고른 선택지를 다시 읽지 않는다
        ctrl.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine = true;
        ctrl.displaySettings.inputSettings.alwaysForceResponseMenu = true;
        ctrl.debugLevel = DialogueDebug.DebugLevel.Warning;

        instance = go.AddComponent<DialogueBoot>();
        instance.openingDone = ES3.KeyExists(OpeningKey) && ES3.Load<bool>(OpeningKey);
        go.SetActive(true);
        GameLog.Info(GameLog.Cat.Player, "대화 시스템 준비: 대화 " + ctrl.initialDatabase.conversations.Count + "개");
    }

    void Update()
    {
        if (openingDone || TitleScreen.IsShowing || DatabaseManager.isOpenUI || PlayerController.instance == null)
        {
            return;
        }
        openingDone = true;
        ES3.Save(OpeningKey, true);
        Talk(StoryDialogues.Opening, null);
    }

    /// <summary>대화 시작 (플레이어 ↔ 상대). 이미 대화 중이거나 다른 창이 열려 있으면 false</summary>
    public static bool Talk(string conversation, Transform npc)
    {
        if (DialogueManager.isConversationActive || PlayerController.instance == null)
        {
            return false;
        }
        Transform player = PlayerController.instance.transform;
        DialogueManager.StartConversation(conversation, player, npc != null ? npc : player);
        return DialogueManager.isConversationActive;
    }
}
